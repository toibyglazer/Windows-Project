namespace BlImplementation;
using BlApi;
using BO;
using System.Collections.Generic;

internal class CallImplementation : ICall
{
    public void AddCall(Call call)
    {
        throw new NotImplementedException();
    }

    public void AssignCallToVolunteer(int volunteerId, int callId)
    {
        throw new NotImplementedException();
    }

    public void CancelAssignment(int volunteerId, int assignmentId)
    {
        throw new NotImplementedException();
    }

    public void CompleteAssignment(int volunteerId, int assignmentId)
    {
        throw new NotImplementedException();
    }

    public void DeleteCall(int callId)
    {
        throw new NotImplementedException();
    }

    public Call GetCallDetailsById(int callId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<CallInList> GetCalls(CallInListFilterField? filterField = null, object? filterValue = null, CallInListSortField? sortField = null)
    {
        throw new NotImplementedException();
    }

    public int[] GetCallStatusesCount()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ClosedCallInList> GetClosedCallsByVolunteer(int volunteerId, CallType? callTypeFilter = null, ClosedCallSortField? sortByField = null)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<OpenCallInList> GetOpenCallsByVolunteer(int volunteerId, CallType? callTypeFilter = null, OpenCallSortField? sortByField = null)
    {
        throw new NotImplementedException();
    }

    public void UpdateCallDetails(Call call)
    {
        throw new NotImplementedException();
    }
}
