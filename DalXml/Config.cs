namespace Dal;
internal static class Config
{
    internal const string s_data_config_xml = "data-config.xml";
    internal const string s_calls_xml = "Calls.xml";
    internal const string s_assignments_xml = "Assignments.xml";
    internal const string s_volunteers_xml = "Volunteers.xml";


    internal static int NextCallId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextCallId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextCallId", value);
    }
    internal static int NextAssignmentId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextAssignmentId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextAssignmentId", value);
    }
    internal static DateTime Clock
    {
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }
    internal static TimeSpan RiskRangelock
    {
        get => XMLTools.GetConfigRiskRange(s_data_config_xml, "RiskRangelock");
        set => XMLTools.SetConfigTimeSpan(s_data_config_xml, "RiskRangelock", value);
    }
    internal static void Reset()
    {
        NextCallId = 1;
        NextAssignmentId = 1;
        Clock = DateTime.Now;
        RiskRangelock = TimeSpan.FromHours(2);
    }
}
