namespace BlApi;
public interface ICall
{
    int[] GetCallStatusesCount();
    IEnumerable<BO.CallInList> GetCalls(BO.CallInListField? filterField = null, object? filterValue = null, BO.CallInListField? sortField = null);
    BO.Call GetCallDetailsById(int callId);

    void UpdateCallDetails(BO.Call call);
    void DeleteCall(int callId);

    void AddCall(BO.Call call);
    IEnumerable<BO.ClosedCallInList> GetClosedCallsByVolunteer(int volunteerId, BO.CallType? callTypeFilter = null, BO.ClosedCallSortField? sortByField = null);
    IEnumerable<BO.OpenCallInList> GetOpenCallsByVolunteer(int volunteerId, BO.CallType? callTypeFilter = null, BO.OpenCallSortField? sortByField = null);

    void CompleteAssignment(int volunteerId, int assignmentId);
    void CancelAssignment(int volunteerId, int assignmentId);
    void AssignCallToVolunteer(int volunteerId, int callId);
   
}
