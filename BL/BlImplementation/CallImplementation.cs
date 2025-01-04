namespace BlImplementation;
using BlApi;
using BO;
using DO;
using System.Collections.Generic;
using System.Data;

internal class CallImplementation : ICall
{
    private readonly DalApi.IDal s_dal = DalApi.Factory.Get;
    public void AssignCallToVolunteer(int volunteerId, int callId)
    {
        DO.Volunteer volunteer = s_dal.Volunteer.Read(volunteerId)
          ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} does not exist.");

        DO.Call call = s_dal.Call.Read(callId)
         ?? throw new BO.BlDoesNotExistException($"Call with ID {callId} not found.");
        var assignments = s_dal.Assignment.ReadAll();

        if (assignments.Any(a => a.CallId == callId && a.CompletionType == DO.AssignmentCompletionType.Treated))
        {
            throw new BlInvalidValueException("The call has already been treated.");
        }
        if (assignments.Any(a => a.CallId == callId && a.CompletionType == null))
        {
            throw new BlInvalidValueException("The call is currently being handled by another volunteer.");
        }
        if (call.MaxCompletionTime.HasValue && call.MaxCompletionTime < DateTime.Now)
        {
            throw new BlInvalidValueException("The call is expired and cannot be assigned.");
        }
        if (!volunteer.IsActive)
        {
            throw new BlInvalidValueException("The volunteer is not active and cannot handle calls.");
        }
        var newAssignment = new Assignment
        {
            CallId = callId,
            VolunteerId = volunteerId,
            EntryTime = DateTime.Now,
            CompletionTime = null,
            CompletionType = null
        };
        try
        {
            s_dal.Assignment.Create(newAssignment);
        }
        catch (DalAlreadyExistsException ex)
        {
            throw new BlAlreadyExistsException("This assignment already exists.", ex);
        }

    }
    public IEnumerable<CallInList> GetCalls(CallInListField? filterField = null, object? filterValue = null, CallInListField? sortField = null)
    {

        IEnumerable<DO.Call> allCalls = s_dal.Call.ReadAll() ?? Enumerable.Empty<DO.Call>();
        IEnumerable<BO.CallInList> callsInList = allCalls.Select(v => Helpers.CallManager.ConvertToCallInList(v));
       
        if (filterField.HasValue && filterValue != null)
        {
            callsInList = callsInList.Where(call =>
            {
                var callValue = Helpers.CallManager.GetValueForField(call, filterField.Value);
                return callValue != null && callValue.Equals(filterValue);
            });
        }
        if (sortField.HasValue)
        {
            callsInList = Helpers.CallManager.SortCalls(callsInList, sortField.Value);
        }

        return callsInList;
    }
    public int[] GetCallStatusesCount()
    {
       
        var allCalls = s_dal.Call.ReadAll();

        var statusGroups = allCalls.GroupBy(call => Helpers.CallManager.CalculateCallStatus(call))
                                   .Select(group => new { Status = group.Key, Count = group.Count() })
                                   .ToList();

        int[] callCounts = Enum.GetValues(typeof(CallStatus))
                               .Cast<CallStatus>()
                               .Select(status => statusGroups.FirstOrDefault(group => group.Status == status)?.Count ?? 0)
                               .ToArray();

        int totalCallsCount = allCalls.Count(); 
        int calculatedCount = callCounts.Sum();
        if (calculatedCount != totalCallsCount)
        {
            throw new InvalidOperationException(
                $"Mismatch detected: Total calls in system ({totalCallsCount}) does not match calculated calls ({calculatedCount})."
            );
        }

        return callCounts;
    }

  
    public BO.Call GetCallDetailsById(int callId)
    {
        DO.Call call = s_dal.Call.Read(callId)
              ?? throw new BO.BlDoesNotExistException($"Call with ID {callId} does not exist.");
        
        BO.Call boCall = Helpers.CallManager.ConvertCallToBO(call);
        return boCall;
    }
    public void UpdateCallDetails(BO.Call call)
    {
        DO.Call newCall = s_dal.Call.Read(call.Id)
             ?? throw new BO.BlDoesNotExistException($"Call with ID {call.Id} does not exist.");

        if (newCall.Id != call.Id)
            throw new InvalidOperationException("Cannot update Id. This field is immutable.");
        if (newCall.OpeningTime != call.OpeningTime)
            throw new InvalidOperationException("Cannot update OpeningTime. This field is immutable.");

        Helpers.CallManager.ValidateCallData(call);
        newCall = Helpers.CallManager.ConvertToDOCall(call);

        try
        {
            s_dal.Call.Update(newCall);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Call retrieval failed: {ex.Message}", ex);
        }
    }
    public void DeleteCall(int callId)
    {
        try
        {

            DO.Call newDoCall = s_dal.Call.Read(callId)
                ?? throw new BO.BlDoesNotExistException($"Call with ID {callId} does not exist.");
            BO.Call newBoCall = Helpers.CallManager.ConvertCallToBO(newDoCall);
            IEnumerable<DO.Assignment> assignments = s_dal.Assignment.ReadAll(a => a.CallId == callId);

            if (assignments.Any())
                throw new BO.BlCallAssignedException($"Call with ID {callId} cannot be deleted because it has assignments.");


            if (newBoCall.Status != CallStatus.Open)
                throw new BO.BlInvalidStatusException($"Call with ID {callId} cannot be deleted because its status is not 'Open'.");

            s_dal.Call.Delete(callId);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Call with ID {callId} does not exist in the data layer.", ex);
        }
        
    }
    public void AddCall(BO.Call call)
    {

        Helpers.CallManager.ValidateCallData(call);
        DO.Call newDoCall = Helpers.CallManager.ConvertToDOCall(call);
        try
        {
            s_dal.Call.Create(newDoCall);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Call creation failed: {ex.Message}", ex);
        }
    }
    public IEnumerable<ClosedCallInList> GetClosedCallsByVolunteer(int volunteerId, BO.CallType? callTypeFilter = null, ClosedCallSortField? sortByField = null)
    {
         

        var closedCallsByVolunteer = s_dal.Assignment.ReadAll()
           .Where(assignment => assignment.VolunteerId == volunteerId
           && assignment.CompletionType != null).Select(assignment => Helpers.CallManager.ConvertToClosedCallInList(assignment));



        if (callTypeFilter.HasValue)
        {
            closedCallsByVolunteer = closedCallsByVolunteer.Where(call => call.Type == callTypeFilter.Value);
        }

        closedCallsByVolunteer = sortByField switch
        {
            ClosedCallSortField.Id => closedCallsByVolunteer.OrderBy(call => call.Id),
            ClosedCallSortField.Type => closedCallsByVolunteer.OrderBy(call => call.Type),
            ClosedCallSortField.FullAddress => closedCallsByVolunteer.OrderBy(call => call.FullAddress),
            ClosedCallSortField.OpeningTime => closedCallsByVolunteer.OrderBy(call => call.OpeningTime),
            ClosedCallSortField.EntryTime => closedCallsByVolunteer.OrderBy(call => call.EntryTime),
            ClosedCallSortField.CompletionTime => closedCallsByVolunteer.OrderBy(call => call.CompletionTime),
            ClosedCallSortField.CompletionType => closedCallsByVolunteer.OrderBy(call => call.CompletionType),
            _ => closedCallsByVolunteer.OrderBy(call => call.Id)
        };

        return closedCallsByVolunteer;
    }
    public IEnumerable<OpenCallInList> GetOpenCallsByVolunteer(int volunteerId, BO.CallType? callTypeFilter = null, OpenCallSortField? sortByField = null)
    {
        var volunteer = s_dal.Volunteer.Read(volunteerId)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} not found.");
        if (string.IsNullOrEmpty(volunteer.CurrentAddress) || volunteer.Latitude == null || volunteer.Longitude == null)
            throw new InvalidOperationException("Unable to display open calls list: Volunteer did not provide a valid address.");

        var allCalls = s_dal.Call.ReadAll();
        var allOpenCalls = allCalls.Select(call => Helpers.CallManager.ConvertCallToBO(call));
        allOpenCalls = allOpenCalls.Where(call => call.Status == BO.CallStatus.Open || call.Status == BO.CallStatus.OpenAtRisk);

        if (callTypeFilter.HasValue)
        {
            allOpenCalls = allOpenCalls.Where(call => call.Type == callTypeFilter.Value);
        }
        var allOpenCallInList = allOpenCalls.Select(call => Helpers.CallManager.ConvertToOpenCallInList(call, volunteer, volunteer.DistanceType));

        allOpenCallInList = sortByField switch
        {
            OpenCallSortField.Id => allOpenCallInList.OrderBy(call => call.Id),
            OpenCallSortField.Type => allOpenCallInList.OrderBy(call => call.Type),
            OpenCallSortField.Description => allOpenCallInList.OrderBy(call => call.Description),
            OpenCallSortField.FullAddress => allOpenCallInList.OrderBy(call => call.FullAddress),
            OpenCallSortField.OpeningTime => allOpenCallInList.OrderBy(call => call.OpeningTime),
            OpenCallSortField.MaxCompletionTime => allOpenCallInList.OrderBy(call => call.MaxCompletionTime),
            OpenCallSortField.DistanceToActiveCall => allOpenCallInList.OrderBy(call => call.DistanceToActiveCall),
            _ => allOpenCallInList.OrderBy(call => call.Id),
        };

        return allOpenCallInList;
      

    }

    public void CompleteAssignment(int volunteerId, int assignmentId)
    {
        var assignment = s_dal.Assignment.Read(assignmentId)
          ?? throw new BO.BlDoesNotExistException($"Assignment with ID {assignmentId} not found.");

        if (assignment.VolunteerId != volunteerId)
            throw new BlInvalidRoleException("Only the assigned volunteer can complete this assignment.");
        if (assignment.CompletionType != null || assignment.CompletionTime != null)
            throw new InvalidOperationException("The assignment is already closed or completed.");

        var updatedAssignment = assignment with
        {
            CompletionTime = DateTime.Now,
            CompletionType = DO.AssignmentCompletionType.Treated
        };
        try
        {
            s_dal.Assignment.Update(updatedAssignment);
        }
        catch (DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("An error occurred while updating the assignment.", ex);
        }
    }
    public void CancelAssignment(int volunteerId, int assignmentId)
    {
        DO.Volunteer volunteer = s_dal.Volunteer.Read(volunteerId)
           ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} does not exist.");
        var assignment = s_dal.Assignment.Read(assignmentId)
          ?? throw new BO.BlDoesNotExistException($"Assignment with ID {assignmentId} not found.");

        if (volunteer.Role != DO.VolunteerRole.Manager && volunteerId != assignment.VolunteerId)
            throw new BlInvalidRoleException("Only an admin or the assigned volunteer can cancel this assignment.");
        if (assignment.CompletionType != null || assignment.CompletionTime != null)
            throw new InvalidOperationException("The assignment is already closed or completed.");

        var cancellationType = volunteerId == assignment.VolunteerId
               ? DO.AssignmentCompletionType.VolunteerCancelled
               : DO.AssignmentCompletionType.AdminCancelled;
        var updatedAssignment = assignment with
        {
            CompletionTime = DateTime.Now,
            CompletionType = cancellationType
        };
        try
        {
            s_dal.Assignment.Update(updatedAssignment);
        }
        catch (DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("An error occurred while updating the assignment.", ex);
        }
    }
    
}
