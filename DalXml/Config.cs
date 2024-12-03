/// <summary>
/// The `Config` class is a static utility class for managing application-level configurations.
/// It provides access to XML-stored configuration values and handles initialization and updates.
/// </summary>
namespace Dal;
internal static class Config
{
    // Paths to XML configuration files
    internal const string s_data_config_xml = "data-config.xml";
    internal const string s_calls_xml = "Calls.xml";
    internal const string s_assignments_xml = "Assignments.xml";
    internal const string s_volunteers_xml = "Volunteers.xml";


    /// <summary>
    /// Gets and increments the next available Call ID.
    /// </summary>
    internal static int NextCallId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextCallId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextCallId", value);
    }
    /// <summary>
    /// Gets and increments the next available Assignment ID.
    /// </summary>
    internal static int NextAssignmentId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextAssignmentId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextAssignmentId", value);
    }
    /// <summary>
    /// Gets or sets the global clock value stored in the configuration.
    /// </summary>
    internal static DateTime Clock
    {
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }
    /// <summary>
    /// Gets or sets the risk range lock duration stored in the configuration.
    /// </summary>
    internal static TimeSpan RiskRangelock
    {
        get => XMLTools.GetConfigRiskRange(s_data_config_xml, "RiskRangelock");
        set => XMLTools.SetConfigTimeSpan(s_data_config_xml, "RiskRangelock", value);
    }
    /// <summary>
    /// Resets the configuration to its default state.
    /// </summary>
    internal static void Reset()
    {
        NextCallId = 1;
        NextAssignmentId = 1;
        Clock = DateTime.Now;
        RiskRangelock = TimeSpan.FromHours(2);
    }
}
