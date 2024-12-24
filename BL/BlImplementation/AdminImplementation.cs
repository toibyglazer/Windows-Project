using BlApi;
using System.Diagnostics;
using Dal;
using BO;
using Helpers;
using DalTest;
using System.Linq.Expressions;
using DO;

namespace BlImplementation;

public class AdminImplementation : IAdmin
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;
    public void AdvanceSystemClock(BO.TimeUnit timeUnit)
    {
        TimeSpan increment = timeUnit switch
        {
            BO.TimeUnit.MINUTE => TimeSpan.FromMinutes(1),
            BO.TimeUnit.HOUR => TimeSpan.FromHours(1),
            BO.TimeUnit.DAY => TimeSpan.FromDays(1),
            BO.TimeUnit.MONTH => TimeSpan.FromDays(30), // הערכה עבור חודש
            BO.TimeUnit.YEAR => TimeSpan.FromDays(365), // הערכה עבור שנה
            _ => throw new BlArgumentException("Invalid time unit")
        };

        DateTime newClock = ClockManager.Now.Add(increment);
        ClockManager.UpdateClock(newClock); 
    }

    public TimeSpan GetRiskTimeRange()
    {
        return _dal.Config.RiskRangelock;
    }

    public DateTime GetSystemClock()
    {
        return _dal.Config.Clock;
    }

    public void InitializeDatabase()
    {
        try
        {
            Initialization.Do();
        }

        catch (DalAlreadyExistsException ex)
        {
            // In case of any failure during the initialization, throw a custom exception
            throw new BlAlreadyExistsException("Error occurred while initializing the database.",ex);
        }
        catch (Exception ex)
        {
            // In case of any failure during the initialization, throw a custom exception
            throw new DatabaseInitializationException("Error occurred while initializing the database.", ex);
        }
    }

    public void ResetDatabase()
    {
        try
        {
            _dal.Config.Reset();

            _dal.Call.DeleteAll();          
            _dal.Assignment.DeleteAll();     
            _dal.Volunteer.DeleteAll();      
  
            _dal.Config.Reset();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error resetting database: {ex.Message}");
        }
    }

    public void SetRiskTimeRange(TimeSpan riskTimeRange)
    {
        _dal.Config.RiskRangelock = riskTimeRange;
    }
}
