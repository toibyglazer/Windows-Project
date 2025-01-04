using BlApi;
using System.Diagnostics;
using Dal;
using BO;
using Helpers;
using DalTest;
using System.Linq.Expressions;
using DO;

namespace BlImplementation;

/// <summary>
/// Implementation of the IAdmin interface providing administrative functionalities.
/// This class contains methods for advancing the system clock, managing the database, 
/// configuring risk time range, and retrieving system clock information.
/// </summary>
/// <remarks>
/// The available methods include:
/// 1. AdvanceSystemClock - Advances the system clock by a specified time unit (minute, hour, day, month, or year).
/// 2. GetRiskTimeRange - Retrieves the currently configured risk time range.
/// 3. GetSystemClock - Retrieves the current system clock.
/// 4. InitializeDatabase - Initializes the database with default data and handles any initialization errors.
/// 5. ResetDatabase - Resets the database to its initial state by clearing all data and configurations.
/// 6. SetRiskTimeRange - Updates the risk time range configuration.
/// </remarks>
public class AdminImplementation : IAdmin
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    public void AdvanceSystemClock(BO.TimeUnit timeUnit)
    {
        // Determine the time increment based on the selected time unit
        TimeSpan increment = timeUnit switch
        {
            BO.TimeUnit.MINUTE => TimeSpan.FromMinutes(1),
            BO.TimeUnit.HOUR => TimeSpan.FromHours(1),
            BO.TimeUnit.DAY => TimeSpan.FromDays(1),
            BO.TimeUnit.MONTH => TimeSpan.FromDays(30),
            BO.TimeUnit.YEAR => TimeSpan.FromDays(365),
            _ => throw new BlArgumentException("Invalid time unit") // Handle invalid time unit
        };

        // Update the system clock with the increment
        DateTime newClock = ClockManager.Now.Add(increment);
        ClockManager.UpdateClock(newClock);
    }

    public TimeSpan GetRiskTimeRange()
    {
        // Return the configured risk time range
        return _dal.Config.RiskRangelock;
    }

    public DateTime GetSystemClock()
    {
        // Return the current system clock
        return _dal.Config.Clock;
    }

    public void InitializeDatabase()
    {
        try
        {
            // Initialize the database with default data
            Initialization.Do();
        }
        catch (DalAlreadyExistsException ex)
        {
            // Handle error if initialization already exists
            throw new BlAlreadyExistsException("Error occurred while initializing the database.", ex);
        }
        catch (Exception ex)
        {
            // Handle general error during initialization
            throw new DatabaseInitializationException("Error occurred while initializing the database.", ex);
        }
    }

    public void ResetDatabase()
    {
        try
        {
            // Reset all data and configurations
            _dal.Config.Reset();

            _dal.Call.DeleteAll();
            _dal.Assignment.DeleteAll();
            _dal.Volunteer.DeleteAll();

            _dal.Config.Reset();
        }
        catch (Exception ex)
        {
            // Handle error during database reset
            Console.WriteLine($"Error resetting database: {ex.Message}");
        }
    }

    public void SetRiskTimeRange(TimeSpan riskTimeRange)
    {
        // Set the new risk time range configuration
        _dal.Config.RiskRangelock = riskTimeRange;
    }

}
