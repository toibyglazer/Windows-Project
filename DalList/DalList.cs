using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dal;
using DalApi;
using DO;
sealed internal class DalList : IDal
{
    /// <summary>
    /// A thread-safe, lazy-initialized singleton instance of the IDal interface.
    /// The Lazy<IDal> ensures that the DalList object is only created when first accessed.
    /// At this point, the DalList object has not yet been actually created.
    /// I have only defined the mechanism for "how to create it".
    /// </summary>

    private static Lazy<IDal> _instance = new Lazy<IDal>(() => new DalList());

    /// <summary>
    /// Provides access to the singleton instance of the IDal interface.
    /// Accessing this property triggers the initialization of the DalList object if it hasn't been created yet.
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
    //                    // Create the instance of DalList if it hasn't been created yet
    //                    _instance = new DalList();
    //                }
    //            }
    //        }
    //        // Return the created instance
    //        return _instance;
    //    }
    //}



    /// <summary>
    /// Provides implementations for data operations related to Volunteers, Calls, Assignments, and Configurations.
    /// Each property exposes a corresponding implementation for handling CRUD operations on the respective data entities.
    /// </summary>
    public IVolunteer Volunteer { get; } = new VolunteerImplementation();

    public ICall Call { get; } = new CallImplementation();

    public IAssignment Assignment { get; } = new AssignmentImplementation();

    public IConfig Config { get; } = new ConfigImplementation();

    public void ResetDB()
    {
        Volunteer.DeleteAll();
        Call.DeleteAll();
        Assignment.DeleteAll();
        Config.Reset();
    }
}
