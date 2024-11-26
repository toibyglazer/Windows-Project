namespace Dal;
/// <summary>
/// Static class managing system-wide configuration settings and ID generators.
/// </summary>

internal static class Config
{
    internal const int startCallId = 1;
    private static int nextCallId = startCallId;
    internal static int NextCallId { get => nextCallId++; }

    internal const int startAssignmentId = 1;
    private static int nextAssignmentId = startAssignmentId;
    internal static int NextAssignmentId { get => nextAssignmentId++; }

    internal static DateTime Clock { get; set; } = DateTime.Now;
    internal static TimeSpan RiskRangelock { get; set; }= TimeSpan.FromHours(2);


    /// <summary>
    /// Resets configuration settings to their default values.
    /// </summary>
    internal static void Reset()
    {
        nextCallId = startCallId;
        nextAssignmentId = startAssignmentId;
        Clock = DateTime.Now;
        RiskRangelock = TimeSpan.FromHours(2);
    }

}
