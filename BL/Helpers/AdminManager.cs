using BlImplementation;
using BO;
using DalApi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;       // ← חדש: List, HashSet
using System.Linq;                      // ← חדש: ToList, OrderByDescending
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;           // ← בשביל Task.Run
using System.Collections.Concurrent;

namespace Helpers;

/// <summary>
/// Internal BL manager for all Application's Clock logic policies
/// </summary>
public static class AdminManager
{
    #region Stage 4
    private static readonly IDal s_dal = Factory.Get;
    #endregion Stage 4

    #region Stage 5
    internal static event Action? ConfigUpdatedObservers; // prepared for stage 5
    internal static event Action? ClockUpdatedObservers;  // prepared for stage 5
    #endregion Stage 5

    #region Stage 4 (Config & Clock) – עם נעילות סביב DAL ו-Notifications מחוץ ל-lock
    /// <summary>
    /// MaxRange (RiskRangelock) – עטוף ב-lock; ה-Notify מחוץ ל-lock
    /// </summary>
    internal static TimeSpan MaxRange
    {
        get
        {
            lock (BlMutex)
                return s_dal.Config.RiskRangelock;
        }
        set
        {
            bool notify = false;
            lock (BlMutex)
            {
                s_dal.Config.RiskRangelock = value;
                notify = true;
            }
            if (notify) ConfigUpdatedObservers?.Invoke(); // stage 5 – מחוץ ל-lock
        }
    }

    public static TimeSpan GetMaxRange() => MaxRange;
    public static void SetMaxRange(TimeSpan maxRange) => MaxRange = maxRange;

    /// <summary>
    /// שעון סימולטור Now – עטוף ב-lock; ה-Notify מחוץ ל-lock
    /// </summary>
    internal static DateTime Now
    {
        get
        {
            lock (BlMutex)
                return s_dal.Config.Clock;
        }
        set
        {
            bool notify = false;
            lock (BlMutex)
            {
                s_dal.Config.Clock = value;
                notify = true;
            }
            if (notify) ClockUpdatedObservers?.Invoke(); // stage 5 – מחוץ ל-lock
        }
    }

    public static DateTime GetNow() => Now;
    public static void SetNow(DateTime newClock) => Now = newClock;

    /// <summary>
    /// עדכון שעון — קורא לעדכונים מחזוריים כבדים אסינכרונית
    /// </summary>
    internal static void UpdateClock(DateTime newClock)
    {
        DateTime oldClock;
        // עדכון השעון – בתוך lock
        lock (BlMutex)
        {
            oldClock = s_dal.Config.Clock;
            s_dal.Config.Clock = newClock;
        }

        // עדכון תצוגות (מיידי, מחוץ ל-lock)
        ClockUpdatedObservers?.Invoke();

        // עדכונים מחזוריים כבדים — להרצה אסינכרונית כדי לא לחסום
        _ = Task.Run(() =>
        {
            try
            {
                // דוגמה לעדכון מחזורי (למשל, פג תוקף לקריאות)
                CallManager.UpdateExpiredCalls();
                // ניתן להוסיף כאן עדכונים תקופתיים נוספים אם יש
            }
            catch
            {
                // לוג שקט; אפשר להרחיב לוגינג לפי צורך
            }
        });
    }
    #endregion Stage 4

    #region Stage 7 base (Simulator infra)
    internal static readonly object blMutex = new();
    public static object BlMutex => blMutex;        // ← בשם המדויק מהמסמך
    private static Thread? s_thread;
    private static int s_interval { get; set; } = 1; // minutes to advance per tick
    private static volatile bool s_stop = false;
    private static volatile bool s_running = false;
    private static readonly object mutex = new();

    private static readonly Random s_rand = new();  // ← רנדום לסימולציה


// --- Simulation knobs ---
private const int MAX_NEW_ASSIGN_PER_TICK = 5;
private const int MAX_CLOSE_PER_TICK = 3;
private const int MAX_VOL_CANCEL_PER_TICK = 2;
private const int MAX_ADMIN_CANCEL_PER_TICK = 1;

private const double P_TRY_ASSIGN = 0.20;   // הסתברות לנסות לקחת קריאה כשאין טיפול
private const double P_VOL_CANCEL = 0.10;   // הסתברות לביטול מתנדב כשעוד לא הגיע זמן
private const double P_ADMIN_CANCEL_OPEN = 0.02; // הסתברות בסיסית לשקול ביטול מנהל לקריאות פתוחות

// state per-assignment (in-memory)
private static readonly ConcurrentDictionary<int, DateTime> s_targetCloseTime = new();
private static readonly ConcurrentDictionary<int, DateTime> s_nextDecisionTime = new();

private static int s_tickCounter = 0;

public static bool IsSimulatorRunning => s_thread != null && s_running;

    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static void ThrowOnSimulatorIsRunning()
    {
        if (IsSimulatorRunning)
            throw new BO.BLTemporaryNotAvailableException(
                "Cannot perform the operation since simulator is running.");
    }

    internal static void Start(int interval)
    {
        lock (mutex)
        {
            if (s_thread != null) return;

            if (interval < 1)
                throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be >= 1 minute");

            s_interval = interval;   // ← ללא הגבלת 60
            s_stop = false;
            s_running = true;
            s_thread = new Thread(clockRunner)
            {
                IsBackground = true,
                Name = "BL.ClockRunner"
            };
            s_thread.Start();
        }
    }

    internal static void Stop()
    {
        lock (mutex)
        {
            if (s_thread == null) return;

            s_stop = true;
            s_running = false;
            try { s_thread.Interrupt(); } catch { }
            s_thread = null;
        }
    }

    internal static void SimulateVolunteerActivity()
    {
        var volunteerIdsToNotify = new HashSet<int>();
        var callIdsToNotify = new HashSet<int>();

        try
        {
            s_tickCounter++;

            List<DO.Volunteer> activeVolunteers;
            DateTime nowSnapshot;

            // 1) snapshot מתנדבים + זמן
            lock (BlMutex)
            {
                nowSnapshot = Now;
                activeVolunteers = Factory.Get.Volunteer
                    .ReadAll(v => v.IsActive == true)
                    .ToList();
            }

            // --- Rate limiting per tick ---
            int newAssignLeft = MAX_NEW_ASSIGN_PER_TICK;
            int closeLeft = MAX_CLOSE_PER_TICK;
            int volCancelLeft = MAX_VOL_CANCEL_PER_TICK;
            int adminCancelLeft = MAX_ADMIN_CANCEL_PER_TICK;

            // דוגמים אקראית תת-קבוצה כדי לפזר לאורך זמן
            // לדוגמה: מטפלים עד 20 מתנדבים בטיק (או כל מינון שתרצי)
            var slice = activeVolunteers
                .OrderBy(_ => s_rand.Next())
                .Take(20)
                .ToList();

            foreach (var vol in slice)
            {
                // 2) בדיקת הקצאה פעילה
                DO.Assignment? activeAssignment;
                lock (BlMutex)
                {
                    activeAssignment = Factory.Get.Assignment
                        .ReadAll(a => a.VolunteerId == vol.Id &&
                                      a.CompletionTime == null &&
                                      a.CompletionType == null)
                        .OrderByDescending(a => a.EntryTime)
                        .FirstOrDefault();
                }

                if (activeAssignment == null)
                {
                    // אין טיפול — מנסים לקחת קריאה רק אם נשאר תקציב לטיק
                    if (newAssignLeft > 0 && s_rand.NextDouble() <= P_TRY_ASSIGN)
                    {
                        List<DO.Call> openCalls;
                        DateTime now2;
                        lock (BlMutex)
                        {
                            now2 = Now;
                            openCalls = Factory.Get.Call
                                .ReadAll(c =>
                                    c.Latitude != 0.0 && c.Longitude != 0.0 &&
                                    (c.MaxCompletionTime == null || c.MaxCompletionTime > now2))
                                .ToList();
                        }

                        if (openCalls.Count > 0 &&
                            vol.Latitude is not null && vol.Longitude is not null && vol.MaxDistance is not null)
                        {
                            var candidates = new List<(DO.Call call, double dist)>();
                            foreach (var call in openCalls.OrderBy(_ => s_rand.Next()).Take(30)) // חיתוך קטן לשיפור ביצועים
                            {
                                BO.CallStatus status;
                                lock (BlMutex)
                                {
                                    status = CallManager.CalculateCallStatus(call);
                                }
                                if (status != BO.CallStatus.Open && status != BO.CallStatus.OpenAtRisk)
                                    continue;

                                double dist = CallManager
                                    .CalculateDistanceAsync(vol, CallManager.ConvertCallToBO(call), vol.DistanceType)
                                    .GetAwaiter().GetResult();

                                if (dist <= vol.MaxDistance)
                                    candidates.Add((call, dist));
                            }

                            if (candidates.Count > 0)
                            {
                                var (pick, dist) = candidates[s_rand.Next(candidates.Count)];

                                int newAssignId;
                                lock (BlMutex)
                                {
                                    var newAssignment = new DO.Assignment
                                    {
                                        CallId = pick.Id,
                                        VolunteerId = vol.Id,
                                        EntryTime = Now,
                                        CompletionTime = null,
                                        CompletionType = null
                                    };
                                    Factory.Get.Assignment.Create(newAssignment);

                                    // מזהים את ה-Id שנוצר (בהנחה שה-DAL תומך בקריאה חוזרת)
                                    newAssignId = Factory.Get.Assignment
                                        .Read(a => a.VolunteerId == vol.Id &&
                                                   a.CallId == pick.Id &&
                                                   a.CompletionTime == null &&
                                                   a.CompletionType == null)?.Id ?? 0;
                                }

                                // קובעים זמן יעד וסשן החלטות
                                if (newAssignId != 0)
                                {
                                    // משך בסיסי: 2 דק׳/ק״מ + 5–20 דק׳
                                    var baseDur = TimeSpan.FromMinutes(2 * dist) +
                                                  TimeSpan.FromMinutes(5 + s_rand.Next(16));

                                    // jitter גדול יחסית כדי לפזר המון: 10–120 דק׳
                                    var jitter = TimeSpan.FromMinutes(s_rand.Next(10, 121));

                                    s_targetCloseTime[newAssignId] = Now + baseDur + jitter;

                                    // חלון החלטה ראשון בעוד 5–45 דק׳
                                    s_nextDecisionTime[newAssignId] = Now + TimeSpan.FromMinutes(s_rand.Next(5, 46));
                                }

                                newAssignLeft--;
                                volunteerIdsToNotify.Add(vol.Id);
                                callIdsToNotify.Add(pick.Id);
                            }
                        }
                    }
                }
                else
                {
                    // יש טיפול — נחליט רק אם הגענו ל-window (Cooldown)
                    if (s_nextDecisionTime.TryGetValue(activeAssignment.Id, out var nextDecision) &&
                        nowSnapshot < nextDecision)
                    {
                        continue; // עוד לא זמן להחליט
                    }

                    // נוודא שיש לנו TargetCloseTime; אם לא—נחשב אחת בסיסית
                    if (!s_targetCloseTime.TryGetValue(activeAssignment.Id, out var targetClose))
                    {
                        double distanceKm = 1.0;
                        if (vol.Latitude is not null && vol.Longitude is not null)
                        {
                            DO.Call theCall;
                            lock (BlMutex)
                            {
                                theCall = Factory.Get.Call.Read(activeAssignment.CallId)
                                          ?? throw new BlDoesNotExistException($"Call {activeAssignment.CallId} not found.");
                            }
                            distanceKm = CallManager
                                .CalculateDistanceAsync(vol, CallManager.ConvertCallToBO(theCall), vol.DistanceType)
                                .GetAwaiter().GetResult();
                        }
                        var baseDur = TimeSpan.FromMinutes(2 * distanceKm) +
                                      TimeSpan.FromMinutes(5 + s_rand.Next(16));
                        var jitter = TimeSpan.FromMinutes(s_rand.Next(10, 121));
                        targetClose = Now + baseDur + jitter;
                        s_targetCloseTime[activeAssignment.Id] = targetClose;
                    }

                    bool timeReached = nowSnapshot >= targetClose;

                    if (timeReached && closeLeft > 0 && s_rand.NextDouble() < 0.5) // לא 100%—כדי לפזר עוד
                    {
                        lock (BlMutex)
                        {
                            var current = Factory.Get.Assignment.Read(activeAssignment.Id) ?? activeAssignment;
                            var updated = current with
                            {
                                CompletionTime = Now,
                                CompletionType = DO.AssignmentCompletionType.Treated
                            };
                            Factory.Get.Assignment.Update(updated);
                        }
                        closeLeft--;
                        volunteerIdsToNotify.Add(vol.Id);
                        callIdsToNotify.Add(activeAssignment.CallId);

                        // מנקים סטייט פנימי
                        s_targetCloseTime.TryRemove(activeAssignment.Id, out _);
                        s_nextDecisionTime.TryRemove(activeAssignment.Id, out _);
                    }
                    else
                    {
                        // לא הגיע זמן/לא נסגר—יש סיכוי קטן לביטול מתנדב, אם נשאר תקציב לטיק
                        if (volCancelLeft > 0 && s_rand.NextDouble() <= P_VOL_CANCEL)
                        {
                            lock (BlMutex)
                            {
                                var current = Factory.Get.Assignment.Read(activeAssignment.Id) ?? activeAssignment;
                                var updated = current with
                                {
                                    CompletionTime = Now,
                                    CompletionType = DO.AssignmentCompletionType.VolunteerCancelled
                                };
                                Factory.Get.Assignment.Update(updated);
                            }
                            volCancelLeft--;
                            volunteerIdsToNotify.Add(vol.Id);
                            callIdsToNotify.Add(activeAssignment.CallId);

                            s_targetCloseTime.TryRemove(activeAssignment.Id, out _);
                            s_nextDecisionTime.TryRemove(activeAssignment.Id, out _);
                        }
                        else
                        {
                            // דוחים את חלון ההחלטה הבא (שוב 5–45 דק׳)
                            s_nextDecisionTime[activeAssignment.Id] = Now + TimeSpan.FromMinutes(s_rand.Next(5, 46));
                        }
                    }
                }
            }

            // --- אירועי מנהל על פתוחות (מעט מאוד בכל טיק) ---
            if (adminCancelLeft > 0 && s_rand.NextDouble() <= P_ADMIN_CANCEL_OPEN)
            {
                List<DO.Call> openSnapshot;
                lock (BlMutex)
                {
                    openSnapshot = Factory.Get.Call.ReadAll().ToList();
                }

                // בוחרים מועמדות פתוחות
                var openCandidates = new List<DO.Call>();
                foreach (var c in openSnapshot.OrderBy(_ => s_rand.Next()).Take(20))
                {
                    BO.CallStatus status;
                    lock (BlMutex)
                    {
                        status = CallManager.CalculateCallStatus(c);
                    }
                    if (status == BO.CallStatus.Open || status == BO.CallStatus.OpenAtRisk)
                        openCandidates.Add(c);
                }

                if (openCandidates.Count > 0)
                {
                    var pick = openCandidates[s_rand.Next(openCandidates.Count)];

                    // מסמנים AdminCancelled דרך יצירת/עדכון Assignment
                    lock (BlMutex)
                    {
                        // אם אין הקצאה פעילה—ניצור רישום "בוטל ע״י מנהל" עם VolunteerId=0
                        var active = Factory.Get.Assignment
                            .Read(a => a.CallId == pick.Id && a.CompletionTime == null && a.CompletionType == null);

                        if (active is null)
                        {
                            Factory.Get.Assignment.Create(new DO.Assignment
                            {
                                CallId = pick.Id,
                                VolunteerId = 0,
                                EntryTime = Now,
                                CompletionTime = Now,
                                CompletionType = DO.AssignmentCompletionType.AdminCancelled
                            });
                        }
                        else
                        {
                            var updated = active with
                            {
                                CompletionTime = Now,
                                CompletionType = DO.AssignmentCompletionType.AdminCancelled
                            };
                            Factory.Get.Assignment.Update(updated);

                            // לניקוי סטייט פנימי אם היה
                            s_targetCloseTime.TryRemove(active.Id, out _);
                            s_nextDecisionTime.TryRemove(active.Id, out _);
                        }
                    }

                    adminCancelLeft--;
                    callIdsToNotify.Add(pick.Id);
                }
            }

            // --- Notifications מחוץ ל-lock ---
            foreach (var vid in volunteerIdsToNotify) VolunteerManager.Observers.NotifyItemUpdated(vid);
            if (volunteerIdsToNotify.Count > 0) VolunteerManager.Observers.NotifyListUpdated();

            foreach (var cid in callIdsToNotify) CallManager.Observers.NotifyItemUpdated(cid);
            if (callIdsToNotify.Count > 0) CallManager.Observers.NotifyListUpdated();
        }
        catch
        {
            // אל תפילו את הסימולטור
        }
    }



    /// <summary>
    /// סימולציית פעילות מתנדבים – עומדת בכללי שלב 7:
    /// * כל גישת DAL עטופה ב-lock(AdminManager.BlMutex)
    /// * שאילתות ToList() בתוך ה-lock ומיד משוחרר
    /// * Notifications רק מחוץ ל-lock
    /// </summary>
    //internal static void SimulateVolunteerActivity()
    //{
    //    // נאסוף מזהים לעדכון (Notifications רק אחרי שיוצאים מכל ה-locks)
    //    var volunteerIdsToNotify = new HashSet<int>();
    //    var callIdsToNotify = new HashSet<int>();

    //    try
    //    {
    //        List<DO.Volunteer> activeVolunteers;
    //        DateTime nowSnapshot;

    //        // 1) שליפת מתנדבים פעילים + זמן נוכחי — ב-lock, ToList מיד
    //        lock (BlMutex)
    //        {
    //            nowSnapshot = Now;
    //            activeVolunteers = Factory.Get.Volunteer
    //                .ReadAll(v => v.IsActive == true)
    //                .ToList();
    //        }

    //        foreach (var vol in activeVolunteers)
    //        {
    //            // 2) בדיקת הקצאה פעילה למתנדב — ב-lock
    //            DO.Assignment? activeAssignment;
    //            lock (BlMutex)
    //            {
    //                activeAssignment = Factory.Get.Assignment
    //                    .ReadAll(a => a.VolunteerId == vol.Id &&
    //                                  a.CompletionTime == null &&
    //                                  a.CompletionType == null)
    //                    .OrderByDescending(a => a.EntryTime)
    //                    .FirstOrDefault();
    //            }

    //            if (activeAssignment == null)
    //            {
    //                // אין קריאה בטיפולו → בהסתברות 20% ננסה לבחור קריאה
    //                if (s_rand.NextDouble() <= 0.20)
    //                {
    //                    // שליפת קריאות פתוחות עם קואורדינטות + לא פג תוקף — לרשימה קונקרטית, ב-lock
    //                    List<DO.Call> openCalls;
    //                    DateTime now2;
    //                    lock (BlMutex)
    //                    {
    //                        now2 = Now;
    //                        openCalls = Factory.Get.Call
    //                            .ReadAll(c =>
    //                                c.Latitude != 0.0 && c.Longitude != 0.0 &&
    //                                (c.MaxCompletionTime == null || c.MaxCompletionTime > now2))
    //                            .ToList();
    //                    }

    //                    if (openCalls.Count > 0 &&
    //                        vol.Latitude is not null && vol.Longitude is not null && vol.MaxDistance is not null)
    //                    {
    //                        // נסנן מועמדות לפי סטטוס פתוח/פתוח-בסיכון ולפי מרחק
    //                        var candidates = new List<(DO.Call call, double dist)>();

    //                        foreach (var call in openCalls)
    //                        {
    //                            // סטטוס (ייתכן DAL בפנים) — נעשה בתוך lock
    //                            BO.CallStatus status;
    //                            lock (BlMutex)
    //                            {
    //                                status = CallManager.CalculateCallStatus(call);
    //                            }
    //                            if (status != BO.CallStatus.Open && status != BO.CallStatus.OpenAtRisk)
    //                                continue;

    //                            // חישוב מרחק – מותר סינכרוני (הרצה עצמה ב-Task.Run)
    //                            double dist = CallManager
    //                                .CalculateDistanceAsync(vol, CallManager.ConvertCallToBO(call), vol.DistanceType)
    //                                .GetAwaiter().GetResult();

    //                            if (dist <= vol.MaxDistance)
    //                                candidates.Add((call, dist));
    //                        }

    //                        if (candidates.Count > 0)
    //                        {
    //                            var pick = candidates[s_rand.Next(candidates.Count)].call;

    //                            // יצירת Assignment חדש — ב-lock, שימוש בשעון הסימולטור
    //                            lock (BlMutex)
    //                            {
    //                                var newAssignment = new DO.Assignment
    //                                {
    //                                    CallId = pick.Id,
    //                                    VolunteerId = vol.Id,
    //                                    EntryTime = Now,
    //                                    CompletionTime = null,
    //                                    CompletionType = null
    //                                };
    //                                Factory.Get.Assignment.Create(newAssignment);
    //                            }

    //                            volunteerIdsToNotify.Add(vol.Id);
    //                            callIdsToNotify.Add(pick.Id);
    //                        }
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                // יש למתנדב קריאה בטיפולו: נבדוק אם עבר "מספיק זמן" כדי לסגור כ"טופל"
    //                double distanceKm = 0.0;
    //                if (vol.Latitude is not null && vol.Longitude is not null)
    //                {
    //                    // שליפת הקריאה — ב-lock
    //                    DO.Call theCall;
    //                    lock (BlMutex)
    //                    {
    //                        theCall = Factory.Get.Call.Read(activeAssignment.CallId)
    //                                  ?? throw new BlDoesNotExistException($"Call {activeAssignment.CallId} not found.");
    //                    }

    //                    // חישוב מרחק (סינכרוני)
    //                    distanceKm = CallManager
    //                        .CalculateDistanceAsync(vol, CallManager.ConvertCallToBO(theCall), vol.DistanceType)
    //                        .GetAwaiter().GetResult();
    //                }

    //                // כלל פשוט: 2 דק' לכל ק"מ + (5–20) דק' רנדומליות
    //                var randomPad = TimeSpan.FromMinutes(5 + s_rand.Next(16)); // 5..20
    //                var minTreat = TimeSpan.FromMinutes(2 * distanceKm) + randomPad;

    //                // ניקח צילום עדכני של Now וה-Assignment — ב-lock קצר
    //                DateTime now4; DO.Assignment current;
    //                lock (BlMutex)
    //                {
    //                    now4 = Now;
    //                    current = Factory.Get.Assignment.Read(activeAssignment.Id) ?? activeAssignment;
    //                }

    //                var elapsed = now4 - current.EntryTime;
    //                bool enough = elapsed >= minTreat;

    //                if (enough)
    //                {
    //                    // סוגרים כמטופל — ב-lock
    //                    lock (BlMutex)
    //                    {
    //                        var updated = current with
    //                        {
    //                            CompletionTime = Now,
    //                            CompletionType = DO.AssignmentCompletionType.Treated
    //                        };
    //                        Factory.Get.Assignment.Update(updated);
    //                    }

    //                    volunteerIdsToNotify.Add(vol.Id);
    //                    callIdsToNotify.Add(current.CallId);
    //                }
    //                else
    //                {
    //                    // לא עבר זמן מספיק — אולי ביטול בהסתברות 10%
    //                    if (s_rand.NextDouble() <= 0.10)
    //                    {
    //                        lock (BlMutex)
    //                        {
    //                            var updated = current with
    //                            {
    //                                CompletionTime = Now,
    //                                CompletionType = DO.AssignmentCompletionType.VolunteerCancelled
    //                            };
    //                            Factory.Get.Assignment.Update(updated);
    //                        }

    //                        volunteerIdsToNotify.Add(vol.Id);
    //                        callIdsToNotify.Add(current.CallId);
    //                    }
    //                }
    //            }
    //        }

    //        // 3) Notifications — רק מחוץ ל-lock
    //        foreach (var vid in volunteerIdsToNotify)
    //            VolunteerManager.Observers.NotifyItemUpdated(vid);
    //        if (volunteerIdsToNotify.Count > 0)
    //            VolunteerManager.Observers.NotifyListUpdated();

    //        foreach (var cid in callIdsToNotify)
    //            CallManager.Observers.NotifyItemUpdated(cid);
    //        if (callIdsToNotify.Count > 0)
    //            CallManager.Observers.NotifyListUpdated();
    //    }
    //    catch
    //    {
    //        // לא מפילים את הסימולטור; ניתן להוסיף לוג לפי צורך
    //    }
    //}

    private static void clockRunner()
    {
        while (!s_stop)
        {
            // מקדמים שעון לפי ה-interval (בדקות סימולטור)
            UpdateClock(Now.AddMinutes(s_interval));

            // מריצים את הסימולציה פעם בשנייה באופן אסינכרוני
            _ = Task.Run(SimulateVolunteerActivity);

            try
            {
                Thread.Sleep(1000); // tick של שנייה
            }
            catch (ThreadInterruptedException) { }
        }
    }
    #endregion Stage 7 base
}
