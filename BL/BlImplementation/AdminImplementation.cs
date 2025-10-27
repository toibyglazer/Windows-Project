using BlApi;
using BO;
using DalApi;
using Helpers;
using DalTest;
using System;

namespace BlImplementation;

/// <summary>
/// Admin BL: clock & config control + DB init/reset.
/// עדכונים לשלב 7:
/// 1) Guard: לא מריצים פעולות משנה-מצב בזמן סימולטור (ThrowOnSimulatorIsRunning)
/// 2) כל שינוי-מצב נעשה בתוך lock(AdminManager.blMutex)
/// 3) שימוש ב-AdminManager.Now / UpdateClock
/// </summary>
public class AdminImplementation : IAdmin
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    public void AdvanceSystemClock(BO.TimeUnit timeUnit)
    {
        // Guard נגד סימולטור
        AdminManager.ThrowOnSimulatorIsRunning();

        TimeSpan increment = timeUnit switch
        {
            BO.TimeUnit.MINUTE => TimeSpan.FromMinutes(1),
            BO.TimeUnit.HOUR => TimeSpan.FromHours(1),
            BO.TimeUnit.DAY => TimeSpan.FromDays(1),
            BO.TimeUnit.MONTH => TimeSpan.FromDays(30),
            BO.TimeUnit.YEAR => TimeSpan.FromDays(365),
            _ => throw new BlArgumentException("Invalid time unit")
        };

        var newClock = AdminManager.Now.Add(increment);

        // שינוי-מצב: עדכון שעון
        lock (AdminManager.blMutex)
        {
            AdminManager.UpdateClock(newClock);
        }
    }

    public TimeSpan GetRiskTimeRange() => AdminManager.GetMaxRange();

    public DateTime GetSystemClock() => AdminManager.GetNow();

    public void InitializeDatabase()
    {
        // Guard נגד סימולטור
        AdminManager.ThrowOnSimulatorIsRunning();

        try
        {
            // שינוי-מצב: אתחול DB + סנכרון שעון/קונפיג
            lock (AdminManager.blMutex)
            {
                Initialization.Do();
                AdminManager.UpdateClock(AdminManager.Now);
                AdminManager.MaxRange = AdminManager.MaxRange; // לצורך טריגר לאובזרברים (אם צריך)
            }
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BlAlreadyExistsException("Error occurred while initializing the database.", ex);
        }
        catch (Exception ex)
        {
            throw new DatabaseInitializationException("Error occurred while initializing the database.", ex);
        }
    }

    public void ResetDatabase()
    {
        // Guard נגד סימולטור
        AdminManager.ThrowOnSimulatorIsRunning();

        try
        {
            // שינוי-מצב: ניקוי כל הישות + Reset קונפיג
            lock (AdminManager.blMutex)
            {
                _dal.Call.DeleteAll();
                _dal.Assignment.DeleteAll();
                _dal.Volunteer.DeleteAll();

                _dal.Config.Reset();

                AdminManager.UpdateClock(AdminManager.Now);
                AdminManager.MaxRange = AdminManager.MaxRange; // טריגר לאובזרברים (אם צריך)
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error resetting database: {ex.Message}");
            throw;
        }
    }

    public void SetRiskTimeRange(TimeSpan riskTimeRange)
    {
        // Guard נגד סימולטור
        AdminManager.ThrowOnSimulatorIsRunning();

        // שינוי-מצב: עדכון קונפיג
        lock (AdminManager.blMutex)
        {
            AdminManager.SetMaxRange(riskTimeRange);
        }
    }

    #region Stage 5 - Observers
    public void AddClockObserver(Action clockObserver) =>
        AdminManager.ClockUpdatedObservers += clockObserver;

    public void RemoveClockObserver(Action clockObserver) =>
        AdminManager.ClockUpdatedObservers -= clockObserver;

    public void AddConfigObserver(Action configObserver) =>
        AdminManager.ConfigUpdatedObservers += configObserver;

    public void RemoveConfigObserver(Action configObserver) =>
        AdminManager.ConfigUpdatedObservers -= configObserver;
    #endregion

    public bool IsSimulatorRunning => AdminManager.IsSimulatorRunning;

    public void StartSimulator(int intervalMinutes)
    {
        // לא מתחילים כשכבר רץ
        if (AdminManager.IsSimulatorRunning)
            throw new BO.BLTemporaryNotAvailableException("Simulator already running.");

        lock (AdminManager.blMutex)
        {
            AdminManager.Start(intervalMinutes);
        }
    }

    public void StopSimulator()
    {
        // עצירה היא פעולה בטוחה גם אם כבר עצור
        lock (AdminManager.blMutex)
        {
            if (AdminManager.IsSimulatorRunning)
                AdminManager.Stop();
        }
    }

}
