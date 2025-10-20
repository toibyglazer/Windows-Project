using BO;
using Helpers;
using System.Threading.Tasks;

namespace BlApi;
/// <summary>
/// Interface for managing call-related operations in the BL layer.
/// </summary>
/// 
/// Explanations for the methods:
/// - GetCallStatusesCount: Retrieves the count of calls by their statuses.
/// - GetCalls: Retrieves a list of calls with optional filtering and sorting.
/// - GetCallDetailsById: Fetches the details of a specific call by its ID.
/// - UpdateCallDetails: Updates the details of an existing call.
/// - DeleteCall: Deletes a call by its ID.
/// - AddCall: Adds a new call to the system.
/// - GetClosedCallsByVolunteer: Retrieves a list of closed calls associated with a specific volunteer, with optional filtering and sorting.
/// - GetOpenCallsByVolunteer: Retrieves a list of open calls associated with a specific volunteer, with optional filtering and sorting.
/// - CompleteAssignment: Marks an assignment as completed by a volunteer.
/// - CancelAssignment: Cancels an assignment for a volunteer.
/// - AssignCallToVolunteer: Assigns a specific call to a volunteer.
public interface ICall :IObservable //stage 5 הרחבת ממשק

{
    int[] GetCallStatusesCount();
    IEnumerable<BO.CallInList> GetCalls(BO.CallInListField? filterField = null, object? filterValue = null, BO.CallInListField? sortField = null);
    BO.Call GetCallDetailsById(int callId);

    Task UpdateCallDetailsAsync(BO.Call call);
    void DeleteCall(int callId);

    Task AddCallAsync(BO.Call call);
    IEnumerable<BO.ClosedCallInList> GetClosedCallsByVolunteer(int volunteerId, BO.CallType? callTypeFilter = null, BO.ClosedCallSortField? sortByField = null);
    Task<IEnumerable<BO.OpenCallInList>> GetOpenCallsByVolunteerAsync(int volunteerId, BO.CallType? callTypeFilter = null, BO.OpenCallSortField? sortByField = null);
    Task<Dictionary<int, GeoCoordinates>> GetCoordinatesForCallsAsync(IEnumerable<BO.OpenCallInList> calls);



    void CompleteAssignment(int volunteerId, int assignmentId);
    Task CancelAssignmentAsync(int volunteerId, int assignmentId);
    void AssignCallToVolunteer(int volunteerId, int callId);

    
}