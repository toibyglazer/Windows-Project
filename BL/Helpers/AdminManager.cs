using BlImplementation;
using BO;
using DalApi;
using System;
using System.Threading;
using System.Threading.Tasks; // ← חדש: בשביל Task.Run
using System.Runtime.CompilerServices;

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

    #region Stage 4
    /// <summary>
    /// Property for providing/setting current configuration variable value for any BL class that may need it
    /// </summary>
    internal static TimeSpan MaxRange
    {
        get => s_dal.Config.RiskRangelock;
        set
        {
            s_dal.Config.RiskRangelock = value;
            ConfigUpdatedObservers?.Invoke(); // stage 5
        }
    }

    public static TimeSpan GetMaxRange() => MaxRange;
    public static void SetMaxRange(TimeSpan maxRange) => MaxRange = maxRange;

    internal static DateTime Now
    {
        get => s_dal.Config.Clock;
        set
        {
            s_dal.Config.Clock = value;
            ClockUpdatedObservers?.Invoke(); // stage 5
        }
    }

    public static DateTime GetNow() => Now;
    public static void SetNow(DateTime newClock) => Now = newClock;

    /// <summary>
    /// Method to perform application's clock update from any BL class as may be required
    /// </summary>
    internal static void UpdateClock(DateTime newClock)
    {
        // שמירת השעון הקודם + עדכון לשעון החדש
        var oldClock = s_dal.Config.Clock;
        s_dal.Config.Clock = newClock;

        // עדכון תצוגות (מיידי, סינכרוני)
        ClockUpdatedObservers?.Invoke();

        // עדכונים מחזוריים כבדים — להרצה אסינכרונית כדי לא לחסום
        _ = Task.Run(() =>
        {
            try
            {
                // בשלב הבא נעביר לקריאה ל-CallManager.PeriodicUpdates(oldClock, newClock)
                CallManager.UpdateExpiredCalls();
            }
            catch
            {
                // אפשר להוסיף לוג/Trace במידת הצורך
            }
        });
    }
    #endregion Stage 4

    #region Stage 7 base
    internal static readonly object blMutex = new();
    private static Thread? s_thread;
    private static int s_interval { get; set; } = 1; // in minutes
    private static volatile bool s_stop = false;
    private static volatile bool s_running = false;
    private static readonly object mutex = new();

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
            if (s_thread == null)
            {
                s_interval = Math.Min(60, Math.Max(1, interval)); // clamp range
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
            if (s_thread != null)
            {
                s_stop = true;
                s_running = false;
                try { s_thread.Interrupt(); } catch { }
                s_thread = null;
            }
    }

    private static void clockRunner()
    {
        while (!s_stop)
        {
            UpdateClock(Now.AddMinutes(s_interval));

            #region Stage 7 simulation placeholder
            // כאן נוסיף בהמשך (בשלבים 4.ז–4.ח)
            // קריאות לסימולציות נוספות
            #endregion

            try
            {
                Thread.Sleep(1000); // 1 second tick
            }
            catch (ThreadInterruptedException) { }
        }
    }
    #endregion Stage 7 base
}
