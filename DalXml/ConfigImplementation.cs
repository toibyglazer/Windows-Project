namespace Dal;

using DalApi;
using System;
using System.Runtime.CompilerServices; // לסנכרון מתודות ה־get/set/Reset

/// <summary>
/// Implementation of the IConfig interface, managing configuration values such as system clock, IDs, and risk range lock.
/// </summary>
internal class ConfigImplementation : IConfig
{
    public DateTime Clock
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.Clock;

        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.Clock = value;
    }

    public int NextCallId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.NextCallId;
    }

    public int NextAssignmentId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.NextAssignmentId;
    }

    public TimeSpan RiskRangelock
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => Config.RiskRangelock;

        [MethodImpl(MethodImplOptions.Synchronized)]
        set => Config.RiskRangelock = value;
    }

    /// <summary>
    /// Resets the configuration to its default values.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Reset()
    {
        Config.Reset();
    }
}
