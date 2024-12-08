/// <summary>
/// The `DalXml` class serves as the main entry point to the Data Access Layer (DAL) implemented using XML.
/// It provides access to implementations of data modules such as Volunteers, Calls, Assignments, and Configurations.
/// </summary>
using DalApi;
using System.Diagnostics;

namespace Dal;

sealed internal class DalXml : IDal
{
    /// <summary>
    /// A thread-safe, lazy-initialized singleton instance of the IDal interface.
    /// The Lazy<IDal> ensures that the DalXml object is only created when first accessed.
    /// At this point, the DalXml object has not yet been actually created.
    /// I have only defined the mechanism for "how to create it".
    /// </summary>
    private static Lazy<IDal> _instance = new Lazy<IDal>(() => new DalXml());

    /// <summary>
    /// Provides access to the singleton instance of the IDal interface.
    /// Accessing this property triggers the initialization of the DalXml object if it hasn't been created yet.
    /// Run the function in Lazy if this is the first time the instance is needed
    /// </summary>
    public static IDal Instance => _instance.Value;

    /// <summary>
    /// I used the Lazy Initialization method and kept it Thread Safe by manually checking it just for illustration purposes.
    /// </summary>
    //private static IDal _instance;
    //private static readonly object _lock = new object();

    //public static IDal Instance
    //{
    //    get
    //    {
    //        // If the instance is not created yet, we make sure the first thread will create it
    //        if (_instance == null)
    //        {
    //            // Lock on the _lock object to synchronize the threads
    //            lock (_lock)
    //            {
    //                // Check again inside the lock to ensure that another thread didn't already create the instance
    //                if (_instance == null)
    //                {
    //                    // Create the instance of DalXml if it hasn't been created yet
    //                    _instance = new DalXml();
    //                }
    //            }
    //        }
    //        // Return the created instance
    //        return _instance;
    //    }
    //}



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