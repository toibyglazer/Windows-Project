namespace BlImplementation;
using BlApi;
using BO;
using DO;
using System.Collections.Generic;
using System.Data;

/// <summary>
/// The ICall interface defines methods for managing calls within the system.
/// It includes functionality for retrieving, updating, adding, and deleting calls,
/// as well as managing call assignments to volunteers.
/// 
/// Methods include:
/// 1. GetCallStatusesCount: Retrieves the count of calls grouped by their status.
/// 2. GetCalls: Retrieves a list of calls based on optional filtering and sorting criteria.
/// 3. GetCallDetailsById: Fetches the detailed information of a specific call based on its ID.
/// 4. UpdateCallDetails: Updates the details of a specific call.
/// 5. DeleteCall: Deletes a call by its ID.
/// 6. AddCall: Adds a new call to the system.
/// 7. GetClosedCallsByVolunteer: Retrieves closed calls assigned to a specific volunteer.
/// 8. GetOpenCallsByVolunteer: Retrieves open calls assigned to a specific volunteer.
/// 9. CompleteAssignment: Marks an assignment as completed for a volunteer.
/// 10. CancelAssignment: Cancels an assignment for a volunteer.
/// 11. AssignCallToVolunteer: Assigns a call to a volunteer.
/// </summary>
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
        // Check if the call has already been treated (completed), prevent assigning it again.
        if (assignments.Any(a => a.CallId == callId && a.CompletionType == DO.AssignmentCompletionType.Treated))
        {
            throw new BlInvalidValueException("The call has already been treated.");
        }

        // Check if the call is currently being handled by another volunteer.
        if (assignments.Any(a => a.CallId == callId && a.CompletionType == null))
        {
            throw new BlInvalidValueException("The call is currently being handled by another volunteer.");
        }

        // Check if the call's maximum completion time has passed (call expired).
        if (call.MaxCompletionTime.HasValue && call.MaxCompletionTime < DateTime.Now)
        {
            throw new BlInvalidValueException("The call is expired and cannot be assigned.");
        }

        // Check if the volunteer is active. If not, they cannot be assigned new calls.
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

        // Convert the data layer calls to BO (Business Object) call format for processing.
        IEnumerable<BO.CallInList> callsInList = allCalls.Select(v => Helpers.CallManager.ConvertToCallInList(v));

        // Apply filtering if a filter field and value are provided.
        if (filterField.HasValue && filterValue != null)
        {
            callsInList = callsInList.Where(call =>
            {
                // Retrieve the value of the specified field in the call and compare it with the filter value.
                var callValue = Helpers.CallManager.GetValueForField(call, filterField.Value);
                return callValue != null && callValue.Equals(filterValue);
            });
        }

        // Apply sorting if a sort field is provided.
        if (sortField.HasValue)
        {
            callsInList = Helpers.CallManager.SortCalls(callsInList, sortField.Value);
        }

        return callsInList;
    }
    public int[] GetCallStatusesCount()
    {
        var allCalls = s_dal.Call.ReadAll();

        // Group the calls by their status and count the number of calls per status.
        var statusGroups = allCalls.GroupBy(call => Helpers.CallManager.CalculateCallStatus(call))
                                   .Select(group => new { Status = group.Key, Count = group.Count() })
                                   .ToList();

        // Map the statuses to an array and get the count for each status, defaulting to 0 if the status has no calls.
        int[] callCounts = Enum.GetValues(typeof(CallStatus))
                               .Cast<CallStatus>()
                               .Select(status => statusGroups.FirstOrDefault(group => group.Status == status)?.Count ?? 0)
                               .ToArray();

       
        int totalCallsCount = allCalls.Count();
        int calculatedCount = callCounts.Sum();

        // Check if there is a mismatch between the calculated total and the actual total number of calls.
        if (calculatedCount != totalCallsCount)
        {
            throw new InvalidOperationException(
                $"Mismatch detected: Total calls in system ({totalCallsCount}) does not match calculated calls ({calculatedCount})."
            );
        }
        return callCounts;
    }

    /// <summary>
    /// Retrieves detailed information about a specific call by its unique ID.
    /// This function is useful for obtaining the full details of a call.
    /// </summary>
    /// <param name="callId">The unique identifier of the call to retrieve.</param>
    /// <returns>Returns a BO.Call object containing all details of the call.</returns>
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

        // Check if the ID or OpeningTime fields are being modified, which is not allowed.
        if (newCall.Id != call.Id)
            throw new InvalidOperationException("Cannot update Id. This field is immutable.");
        if (newCall.OpeningTime != call.OpeningTime)
            throw new InvalidOperationException("Cannot update OpeningTime. This field is immutable.");

        // Validate the new call data to ensure it meets the required criteria.
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

            // Retrieve all assignments for the call. If any exist, the call cannot be deleted.
            IEnumerable<DO.Assignment> assignments = s_dal.Assignment.ReadAll(a => a.CallId == callId);
            if (assignments.Any())
                throw new BO.BlCallAssignedException($"Call with ID {callId} cannot be deleted because it has assignments.");

            // Check the call's status. If it's not "Open", prevent deletion.
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
        // Retrieve all assignments for the given volunteer that have a non-null CompletionType, indicating closed calls.
        var closedCallsByVolunteer = s_dal.Assignment.ReadAll()
            .Where(assignment => assignment.VolunteerId == volunteerId
            && assignment.CompletionType != null)
            .Select(assignment => Helpers.CallManager.ConvertToClosedCallInList(assignment));

        // Apply the optional filter for the call type if provided.
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
            _ => closedCallsByVolunteer.OrderBy(call => call.Id),
        };

        return closedCallsByVolunteer;
    }
    public IEnumerable<OpenCallInList> GetOpenCallsByVolunteer(int volunteerId, BO.CallType? callTypeFilter = null, OpenCallSortField? sortByField = null)
    {
        // Retrieve the volunteer from the database.
        var volunteer = s_dal.Volunteer.Read(volunteerId)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} not found.");

        // Ensure the volunteer has provided a valid address for displaying the calls.
        if (string.IsNullOrEmpty(volunteer.CurrentAddress) || volunteer.Latitude == null || volunteer.Longitude == null)
            throw new InvalidOperationException("Unable to display open calls list: Volunteer did not provide a valid address.");

        var allCalls = s_dal.Call.ReadAll();
        var allOpenCalls = allCalls.Select(call => Helpers.CallManager.ConvertCallToBO(call));
        allOpenCalls = allOpenCalls.Where(call => call.Status == BO.CallStatus.Open || call.Status == BO.CallStatus.OpenAtRisk);
        if (callTypeFilter.HasValue)
        {
            allOpenCalls = allOpenCalls.Where(call => call.Type == callTypeFilter.Value);
        }
        // Convert the filtered calls to the OpenCallInList format, using the volunteer's data for further processing.
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
        // Fetch the assignment data from the database.
        var assignment = s_dal.Assignment.Read(assignmentId)
            ?? throw new BO.BlDoesNotExistException($"Assignment with ID {assignmentId} not found.");

        // Ensure that the volunteer attempting to complete the assignment is the one assigned to it.
        if (assignment.VolunteerId != volunteerId)
            throw new BlInvalidRoleException("Only the assigned volunteer can complete this assignment.");

        // Check if the assignment is already completed or closed.
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
        // Fetch volunteer data from the database.
        DO.Volunteer volunteer = s_dal.Volunteer.Read(volunteerId)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} does not exist.");

        // Fetch assignment data from the database.
        var assignment = s_dal.Assignment.Read(assignmentId)
            ?? throw new BO.BlDoesNotExistException($"Assignment with ID {assignmentId} not found.");

        // Check if the volunteer has the required role (either Manager or the assigned volunteer) to cancel the assignment.
        if (volunteer.Role != DO.VolunteerRole.Manager && volunteerId != assignment.VolunteerId)
            throw new BlInvalidRoleException("Only an admin or the assigned volunteer can cancel this assignment.");

        // Ensure the assignment has not been completed or closed.
        if (assignment.CompletionType != null || assignment.CompletionTime != null)
            throw new InvalidOperationException("The assignment is already closed or completed.");

        // Determine the cancellation type based on who is canceling the assignment (volunteer or admin).
        var cancellationType = volunteerId == assignment.VolunteerId
            ? DO.AssignmentCompletionType.VolunteerCancelled
            : DO.AssignmentCompletionType.AdminCancelled;

        // Create an updated assignment with cancellation details.
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
