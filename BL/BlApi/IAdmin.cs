namespace BlApi;
public interface IAdmin
{
    DateTime GetSystemClock();
    void AdvanceSystemClock(BO.TimeUnit timeUnit);
    TimeSpan GetRiskTimeRange();
    void SetRiskTimeRange(TimeSpan riskTimeRange);
    void ResetDatabase();
    void InitializeDatabase();
}
