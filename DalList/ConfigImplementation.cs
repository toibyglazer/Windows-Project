namespace Dal;
using DalApi;
using System;

public class ConfigImplementation : IConfig
{
    public DateTime Clock 
    {
        get => Config.Clock;
        set => Config.Clock = value;
    }

    public int NextCallId => Config.NextCallId;

    public int NextAssignmentId => Config.NextAssignmentId;

    public TimeSpan RiskRangelock 
    {
        get => Config.RiskRangelock;
        set => Config.RiskRangelock = value;
    }

    public void Reset()
    {
        Config.Reset();
    }
}
