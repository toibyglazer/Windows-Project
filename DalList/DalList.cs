using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dal;
using DalApi;
using DO;

/// <summary>
/// Implementation of the IDal interface using in-memory data structures.
/// This sealed class provides access to the data modules (Volunteer, Call, Assignment, Config)
/// and includes a method to reset the database by clearing all data.
/// </summary>
sealed public class DalList : IDal
{

    /// <summary>
    /// Provides implementations for data operations related to Volunteers, Calls, Assignments, and Configurations.
    /// Each property exposes a corresponding implementation for handling CRUD operations on the respective data entities.
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
