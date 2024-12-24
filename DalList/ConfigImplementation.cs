namespace Dal;
using DalApi;
using System;
/// <summary>
/// Implementation of the IConfig interface, managing configuration values such as system clock, IDs, and risk range lock.
/// </summary>
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

    //Resets the configuration to its default values.
    public void Reset()
    {
        Config.Reset();
    }
}
