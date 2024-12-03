/// <summary>
/// The `DalXml` class serves as the main entry point to the Data Access Layer (DAL) implemented using XML.
/// It provides access to implementations of data modules such as Volunteers, Calls, Assignments, and Configurations.
/// </summary>
using DalApi;

namespace Dal;

sealed public class DalXml : IDal
{
    /// <summary>
    /// Provides access to the data modules for Volunteers, Calls, Assignments, and Configurations.
    /// </summary>
    public IVolunteer Volunteer { get; } = new VolunteerImplementation();
    public ICall Call { get; } = new CallImplementation();

    public IAssignment Assignment { get; } = new AssignmentImplementation();

    public IConfig Config { get; } = new ConfigImplementation();

    /// <summary>
    /// Resets the database by clearing all data from each module.
    /// </summary>
    public void ResetDB()
    {
        Volunteer.DeleteAll();
        Call.DeleteAll();
        Assignment.DeleteAll();
        Config.Reset();
    }
}