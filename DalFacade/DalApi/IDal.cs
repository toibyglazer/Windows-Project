using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DalApi;
/// <summary>
/// Defines the contract for the Data Access Layer (DAL).
/// Provides access to the main data modules: Volunteers, Calls, Assignments, and Configurations.
/// Includes a method to reset the database to its initial state.
/// </summary>
public interface IDal
{
    IVolunteer Volunteer { get; }
    ICall Call { get; }
    IAssignment Assignment { get; }
    IConfig Config { get; }
    void ResetDB();
}

