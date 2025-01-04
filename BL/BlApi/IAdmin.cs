namespace BlApi;
/// <summary>
/// Interface for administrative functionalities in the BL layer.
/// </summary>
/// 
/// Explanations for the methods:
/// - GetSystemClock: Retrieves the current system clock.
/// - AdvanceSystemClock: Advances the system clock by a specified time unit.
/// - GetRiskTimeRange: Gets the currently defined risk time range.
/// - SetRiskTimeRange: Updates the risk time range with a new value.
/// - ResetDatabase: Resets the database to its initial state.
/// - InitializeDatabase: Initializes the database with default values.
public interface IAdmin
{
    DateTime GetSystemClock();
    void AdvanceSystemClock(BO.TimeUnit timeUnit);
    TimeSpan GetRiskTimeRange();
    void SetRiskTimeRange(TimeSpan riskTimeRange);
    void ResetDatabase();
    void InitializeDatabase();
}
