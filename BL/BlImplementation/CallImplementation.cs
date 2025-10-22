using BlApi;
using BO;
using Dal;
using DalApi;
using DO;
using Helpers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using static Helpers.CallManager;
using BlCall = BlApi.ICall;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlImplementation;
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
internal class CallImplementation : BlCall
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


        IConfig config = new ConfigImplementation();
        // Check if the call's maximum completion time has passed (call expired).
        if (call.MaxCompletionTime.HasValue && call.MaxCompletionTime < config.Clock)
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
            CallManager.Observers.NotifyItemUpdated(callId);
            CallManager.Observers.NotifyListUpdated();
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
        IEnumerable<BO.CallInList> callsInList = allCalls.Select(v => ConvertToCallInList(v));

        // Apply filtering if a filter field and value are provided.
        if (filterField.HasValue && filterValue != null)
        {
            callsInList = callsInList.Where(call =>
            {
                // Retrieve the value of the specified field in the call and compare it with the filter value.
                var callValue = GetValueForField(call, filterField.Value);

                return callValue != null && callValue.Equals(filterValue);
            });
        }

        // Apply sorting if a sort field is provided.
        if (sortField.HasValue)
        {
            callsInList = SortCalls(callsInList, sortField.Value);
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
        // NOTE: removed NotifyItemUpdated here — reads should NOT notify observers.
        return boCall;

    }
    public async Task UpdateCallDetailsAsync(BO.Call call)
    {
        DO.Call newCall = s_dal.Call.Read(call.Id)
            ?? throw new BO.BlDoesNotExistException($"Call with ID {call.Id} does not exist.");

        // Check if the ID or OpeningTime fields are being modified, which is not allowed.
        if (newCall.Id != call.Id)
            throw new InvalidOperationException("Cannot update Id. This field is immutable.");
        if (newCall.OpeningTime != call.OpeningTime)
            throw new InvalidOperationException("Cannot update OpeningTime. This field is immutable.");

        // Validate the new call data asynchronously
        await Helpers.CallManager.ValidateCallDataAsync(call);

        // Convert to DO.Call asynchronously
        newCall = await Helpers.CallManager.ConvertToDOCallAsync(call);

        call.Latitude = newCall.Latitude;
        call.Longitude = newCall.Longitude;
        call.OpeningTime = AdminManager.Now;

        try
        {
            s_dal.Call.Update(newCall);
            CallManager.Observers.NotifyItemUpdated(newCall.Id);
            CallManager.Observers.NotifyListUpdated();

            if (call.FullAddress != newCall.FullAddress)
                await CallManager.EmailService.SendEmailToVolunteersForNewCallAsync(call); // שים לב ל-await כאן
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
            CallManager.Observers.NotifyListUpdated();

        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Call with ID {callId} does not exist in the data layer.", ex);
        }
    }
    public async Task AddCallAsync(BO.Call call)
    {
        await Helpers.CallManager.ValidateCallDataAsync(call);

        DO.Call newDoCall = await Helpers.CallManager.ConvertToDOCallAsync(call);

        call.Latitude = newDoCall.Latitude;
        call.Longitude = newDoCall.Longitude;
        call.OpeningTime = AdminManager.Now;

        IEnumerable<DO.Call> allCalls = s_dal.Call.ReadAll();
        var existingCall = allCalls.FirstOrDefault(c => c.Type == newDoCall.Type &&
                                                 c.Latitude == newDoCall.Latitude &&
                                                 c.Longitude == newDoCall.Longitude);

        if (existingCall != null)
        {
            throw new BO.BlAlreadyExistsException($"The call already exists in the system. Call ID: {existingCall.Id}");
        }

        try
        {
            s_dal.Call.Create(newDoCall);
            CallManager.Observers.NotifyListUpdated();
            await CallManager.EmailService.SendEmailToVolunteersForNewCallAsync(call);
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
    public async Task<IEnumerable<OpenCallInList>> GetOpenCallsByVolunteerAsync(int volunteerId, BO.CallType? callTypeFilter = null, OpenCallSortField? sortByField = null)
    {
        // Retrieve the volunteer from the database.
        var volunteer = s_dal.Volunteer.Read(volunteerId)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} not found.");

        // Ensure the volunteer has provided a valid address for displaying the calls.
        if (string.IsNullOrEmpty(volunteer.CurrentAddress) || volunteer.Latitude == null || volunteer.Longitude == null)
            throw new InvalidOperationException("Can't display the open calls list because a valid address hasn't been set. Please enter a valid address first, and then you'll be able to view the open calls list");

        var allCalls = s_dal.Call.ReadAll();
        var allOpenCalls = allCalls.Select(call => Helpers.CallManager.ConvertCallToBO(call))
                                   .Where(call => call.Status == BO.CallStatus.Open || call.Status == BO.CallStatus.OpenAtRisk);

        if (callTypeFilter.HasValue)
        {
            allOpenCalls = allOpenCalls.Where(call => call.Type == callTypeFilter.Value);
        }

        // Convert the filtered calls to OpenCallInList asynchronously
        var allOpenCallInList = await Task.WhenAll(
            allOpenCalls.Select(call => Helpers.CallManager.ConvertToOpenCallInListAsync(call, volunteer, volunteer.DistanceType))
        );

        // Sort the results
        var sortedOpenCallInList = sortByField switch
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

        return sortedOpenCallInList;
    }

    public async Task<Dictionary<int, GeoCoordinates>> GetCoordinatesForCallsAsync(IEnumerable<OpenCallInList> calls)
    {
        var coordinatesDict = new ConcurrentDictionary<int, GeoCoordinates>();
        var tasks = new List<Task>();

        // יצירת משימות לחישוב קואורדינטות במקביל
        foreach (var call in calls)
        {
            tasks.Add(Task.Run(async () =>
            {
                if (!string.IsNullOrEmpty(call.FullAddress))
                {
                    var coordinates = await GetCachedCoordinatesAsync(call.FullAddress);
                    if (coordinates != null)
                    {
                        coordinatesDict[call.Id] = coordinates;
                    }
                }
            }));
        }

        // המתנה לסיום כל המשימות
        await Task.WhenAll(tasks);

        // המרה ל-Dictionary רגיל לפני החזרה
        return new Dictionary<int, GeoCoordinates>(coordinatesDict);
    }

    private static readonly ConcurrentDictionary<string, GeoCoordinates> _coordinatesCache = new ConcurrentDictionary<string, GeoCoordinates>();
    private async Task<GeoCoordinates> GetCachedCoordinatesAsync(string address)
    {
        if (_coordinatesCache.TryGetValue(address, out var cachedCoordinates))
        {
            return cachedCoordinates;
        }

        try
        {

            var coordinates = await Tools.CallOpenStreetMapApiAsync(address);

            if (coordinates != null)
            {

                _coordinatesCache.TryAdd(address, coordinates);
                return coordinates;
            }
        }
        catch (Exception ex)
        {

            System.Diagnostics.Debug.WriteLine($"Error getting coordinates for address {address}: {ex.Message}");
        }

        return new GeoCoordinates { Latitude = 31.895049, Longitude = 34.810181 };
    }
    //public void CompleteAssignment(int volunteerId, int assignmentId)
    //{
    //    // Fetch the assignment data from the database.
    //    var assignment = s_dal.Assignment.Read(assignmentId)
    //        ?? throw new BO.BlDoesNotExistException($"Assignment with ID {assignmentId} not found.");

    //    // Ensure that the volunteer attempting to complete the assignment is the one assigned to it.
    //    if (assignment.VolunteerId != volunteerId)
    //        throw new BlInvalidRoleException("Only the assigned volunteer can complete this assignment.");

    //    // Check if the assignment is already completed or closed.
    //    if (assignment.CompletionType != null || assignment.CompletionTime != null)
    //        throw new InvalidOperationException("The assignment is already closed or completed.");

    //    var updatedAssignment = assignment with
    //    {
    //        CompletionTime = DateTime.Now,
    //        CompletionType = DO.AssignmentCompletionType.Treated
    //    };

    //    try
    //    {
    //        s_dal.Assignment.Update(updatedAssignment);
    //        CallManager.Observers.NotifyItemUpdated(assignmentId);
    //        CallManager.Observers.NotifyListUpdated();
    //    }
    //    catch (DalDoesNotExistException ex)
    //    {
    //        throw new BO.BlDoesNotExistException("An error occurred while updating the assignment.", ex);
    //    }

    //}
    public void CompleteAssignment(int volunteerId, int assignmentId)
    {
        // Fetch the latest OPEN assignment for this volunteer
        var assignment = s_dal.Assignment.ReadAll(a =>
                a.VolunteerId == volunteerId &&
                a.CompletionTime == null &&
                a.CompletionType == null)
            .OrderByDescending(a => a.EntryTime)
            .ThenByDescending(a => a.Id)
            .FirstOrDefault()
            ?? throw new BO.BlDoesNotExistException(
                $"No open assignments found for volunteer {volunteerId}.");

        // Ensure that the volunteer attempting to complete the assignment is the one assigned to it.
        if (assignment.VolunteerId != volunteerId)
            throw new BlInvalidRoleException("Only the assigned volunteer can complete this assignment.");

        // Check if the assignment is already completed or closed (defensive; אמור להיות פתוח ממילא)
        if (assignment.CompletionType != null || assignment.CompletionTime != null)
            throw new InvalidOperationException("The assignment is already closed or completed.");

        var updatedAssignment = assignment with
        {
            // אם אתם עובדים עם שעון סימולטיבי, החליפו ל-AdminManager.Now
            CompletionTime = DateTime.Now,
            CompletionType = DO.AssignmentCompletionType.Treated
        };

        try
        {
            s_dal.Assignment.Update(updatedAssignment);
            CallManager.Observers.NotifyItemUpdated(updatedAssignment.Id);
            CallManager.Observers.NotifyListUpdated();
        }
        catch (DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("An error occurred while updating the assignment.", ex);
        }
    }

    public async Task CancelAssignmentAsync(int volunteerId, int assignmentId)
    {
        // Fetch volunteer data from the database.
        DO.Volunteer volunteer = s_dal.Volunteer.Read(volunteerId)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} does not exist.");

        //// Fetch assignment data from the database.
        //var assignment = s_dal.Assignment.Read(assignmentId)
        //    ?? throw new BO.BlDoesNotExistException($"Assignment with ID {assignmentId} not found.");

        // Fetch the latest OPEN assignment for this volunteer
        var assignment = s_dal.Assignment.ReadAll(a =>
                a.VolunteerId == volunteerId &&
                a.CompletionTime == null &&
                a.CompletionType == null)
            .OrderByDescending(a => a.EntryTime)
            .ThenByDescending(a => a.Id)
            .FirstOrDefault()
            ?? throw new BO.BlDoesNotExistException(
                $"No open assignments found for volunteer {volunteerId}.");


        // Check if the volunteer has the required role (either Manager or the assigned volunteer) to cancel the assignment.
        if (volunteer.Role != DO.VolunteerRole.Manager && volunteerId != assignment.VolunteerId)
            throw new BlInvalidRoleException("Only an admin or the assigned volunteer can cancel this assignment.");

        // Ensure the assignment has not been completed or closed.
        if (assignment.CompletionType != null || assignment.CompletionTime != null)
            throw new InvalidOperationException("The assignment is already closed or completed.");

        if (assignment.VolunteerId == 0)
            throw new InvalidOperationException("The assignment you are trying to update is expired and has never been handled by any volunteer.");


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
            CallManager.Observers.NotifyItemUpdated(assignmentId);
            CallManager.Observers.NotifyListUpdated();

            if (updatedAssignment.CompletionType == DO.AssignmentCompletionType.AdminCancelled)
                await EmailService.SendCancellationEmailToVolunteerAsync(assignmentId); // קריאה אסינכרונית
        }
        catch (DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("An error occurred while updating the assignment.", ex);
        }
    }


    #region Stage 5
    public void AddObserver(Action listObserver) =>
    CallManager.Observers.AddListObserver(listObserver); //stage 5
    public void AddObserver(int id, Action observer) =>
    CallManager.Observers.AddObserver(id, observer); //stage 5
    public void RemoveObserver(Action listObserver) =>
    CallManager.Observers.RemoveListObserver(listObserver); //stage 5
    public void RemoveObserver(int id, Action observer) =>
    CallManager.Observers.RemoveObserver(id, observer); //stage 5
    #endregion Stage 5

}