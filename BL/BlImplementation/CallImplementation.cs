using BlApi;
using BO;
using Dal;
using DalApi;
using DO;
using Helpers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using static Helpers.CallManager;
using BlCall = BlApi.ICall;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlImplementation;

/// <summary>
/// BL implementation for call management (queries + mutating ops).
/// בשינויים הבאים:
/// 1) Guard נגד סימולטור לכל פעולה משנת-מצב.
/// 2) כל שינוי-מצב (Create/Update/Delete/Assign/Cancel/Complete) מתבצע בתוך lock(AdminManager.blMutex).
/// 3) שימוש ב-AdminManager.Now במקום DateTime.Now.
/// </summary>
internal class CallImplementation : BlCall
{
    private readonly DalApi.IDal s_dal = DalApi.Factory.Get;

    public void AssignCallToVolunteer(int volunteerId, int callId)
    {
        // Guard: אין פעולות משנה-מצב בזמן סימולטור
        AdminManager.ThrowOnSimulatorIsRunning();

        DO.Volunteer volunteer = s_dal.Volunteer.Read(volunteerId)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} does not exist.");
        DO.Call call = s_dal.Call.Read(callId)
            ?? throw new BO.BlDoesNotExistException($"Call with ID {callId} not found.");

        var assignments = s_dal.Assignment.ReadAll();

        // Prevent re-assign treated call
        if (assignments.Any(a => a.CallId == callId && a.CompletionType == DO.AssignmentCompletionType.Treated))
            throw new BlInvalidValueException("The call has already been treated.");

        // Prevent assigning when another volunteer is handling
        if (assignments.Any(a => a.CallId == callId && a.CompletionType == null))
            throw new BlInvalidValueException("The call is currently being handled by another volunteer.");

        // Expired?
        if (call.MaxCompletionTime.HasValue && call.MaxCompletionTime < AdminManager.Now)
            throw new BlInvalidValueException("The call is expired and cannot be assigned.");

        // Volunteer must be active
        if (!volunteer.IsActive)
            throw new BlInvalidValueException("The volunteer is not active and cannot handle calls.");

        var newAssignment = new Assignment
        {
            CallId = callId,
            VolunteerId = volunteerId,
            EntryTime = AdminManager.Now,
            CompletionTime = null,
            CompletionType = null
        };

        try
        {
            lock (AdminManager.blMutex)
            {
                s_dal.Assignment.Create(newAssignment);
                CallManager.Observers.NotifyItemUpdated(callId);
                CallManager.Observers.NotifyListUpdated();
            }
        }
        catch (DalAlreadyExistsException ex)
        {
            throw new BlAlreadyExistsException("This assignment already exists.", ex);
        }
    }

    public IEnumerable<CallInList> GetCalls(
        CallInListField? filterField = null,
        object? filterValue = null,
        CallInListField? sortField = null)
    {
        var allCalls = s_dal.Call.ReadAll() ?? Enumerable.Empty<DO.Call>();
        IEnumerable<BO.CallInList> callsInList = allCalls.Select(ConvertToCallInList);

        if (filterField.HasValue && filterValue != null)
        {
            callsInList = callsInList.Where(call =>
            {
                var callValue = GetValueForField(call, filterField.Value);
                return callValue != null && callValue.Equals(filterValue);
            });
        }

        if (sortField.HasValue)
            callsInList = SortCalls(callsInList, sortField.Value);

        return callsInList;
    }

    public int[] GetCallStatusesCount()
    {
        var allCalls = s_dal.Call.ReadAll();

        var statusGroups = allCalls
            .GroupBy(call => Helpers.CallManager.CalculateCallStatus(call))
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToList();

        int[] callCounts = Enum.GetValues(typeof(CallStatus))
            .Cast<CallStatus>()
            .Select(status => statusGroups.FirstOrDefault(g => g.Status == status)?.Count ?? 0)
            .ToArray();

        int totalCallsCount = allCalls.Count();
        int calculatedCount = callCounts.Sum();

        if (calculatedCount != totalCallsCount)
            throw new InvalidOperationException(
                $"Mismatch detected: Total calls in system ({totalCallsCount}) does not match calculated calls ({calculatedCount}).");

        return callCounts;
    }

    public BO.Call GetCallDetailsById(int callId)
    {
        DO.Call call = s_dal.Call.Read(callId)
              ?? throw new BO.BlDoesNotExistException($"Call with ID {callId} does not exist.");

        return Helpers.CallManager.ConvertCallToBO(call);
    }

    public async Task UpdateCallDetailsAsync(BO.Call call)
    {
        // Guard + lock (יש כאן Update)
        AdminManager.ThrowOnSimulatorIsRunning();

        DO.Call current = s_dal.Call.Read(call.Id)
            ?? throw new BO.BlDoesNotExistException($"Call with ID {call.Id} does not exist.");

        if (current.Id != call.Id)
            throw new InvalidOperationException("Cannot update Id. This field is immutable.");
        if (current.OpeningTime != call.OpeningTime)
            throw new InvalidOperationException("Cannot update OpeningTime. This field is immutable.");

        await Helpers.CallManager.ValidateCallDataAsync(call);
        current = await Helpers.CallManager.ConvertToDOCallAsync(call);

        // שדות שנקבעים ב-BL
        call.Latitude = current.Latitude;
        call.Longitude = current.Longitude;
        call.OpeningTime = AdminManager.Now;

        try
        {
            lock (AdminManager.blMutex)
            {
                s_dal.Call.Update(current);
                CallManager.Observers.NotifyItemUpdated(current.Id);
                CallManager.Observers.NotifyListUpdated();
            }

            if (call.FullAddress != current.FullAddress)
                await CallManager.EmailService.SendEmailToVolunteersForNewCallAsync(call);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Call update failed: {ex.Message}", ex);
        }
    }

    public void DeleteCall(int callId)
    {
        // Guard + lock
        AdminManager.ThrowOnSimulatorIsRunning();

        try
        {
            DO.Call doCall = s_dal.Call.Read(callId)
                ?? throw new BO.BlDoesNotExistException($"Call with ID {callId} does not exist.");

            BO.Call boCall = Helpers.CallManager.ConvertCallToBO(doCall);

            IEnumerable<DO.Assignment> assignments = s_dal.Assignment.ReadAll(a => a.CallId == callId);
            if (assignments.Any())
                throw new BO.BlCallAssignedException($"Call with ID {callId} cannot be deleted because it has assignments.");

            if (boCall.Status != CallStatus.Open)
                throw new BO.BlInvalidStatusException($"Call with ID {callId} cannot be deleted because its status is not 'Open'.");

            lock (AdminManager.blMutex)
            {
                s_dal.Call.Delete(callId);
                CallManager.Observers.NotifyListUpdated();
            }
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Call with ID {callId} does not exist in the data layer.", ex);
        }
    }

    public async Task AddCallAsync(BO.Call call)
    {
        // Guard + lock (יש כאן Create)
        AdminManager.ThrowOnSimulatorIsRunning();

        await Helpers.CallManager.ValidateCallDataAsync(call);
        DO.Call newDoCall = await Helpers.CallManager.ConvertToDOCallAsync(call);

        call.Latitude = newDoCall.Latitude;
        call.Longitude = newDoCall.Longitude;
        call.OpeningTime = AdminManager.Now;

        var allCalls = s_dal.Call.ReadAll();
        var existingCall = allCalls.FirstOrDefault(c =>
            c.Type == newDoCall.Type &&
            c.Latitude == newDoCall.Latitude &&
            c.Longitude == newDoCall.Longitude);

        if (existingCall != null)
            throw new BO.BlAlreadyExistsException(
                $"The call already exists in the system. Call ID: {existingCall.Id}");

        try
        {
            lock (AdminManager.blMutex)
            {
                s_dal.Call.Create(newDoCall);
                CallManager.Observers.NotifyListUpdated();
            }

            await CallManager.EmailService.SendEmailToVolunteersForNewCallAsync(call);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Call creation failed: {ex.Message}", ex);
        }
    }

    public IEnumerable<ClosedCallInList> GetClosedCallsByVolunteer(
        int volunteerId, BO.CallType? callTypeFilter = null, ClosedCallSortField? sortByField = null)
    {
        var closedCallsByVolunteer = s_dal.Assignment.ReadAll()
            .Where(a => a.VolunteerId == volunteerId && a.CompletionType != null)
            .Select(Helpers.CallManager.ConvertToClosedCallInList);

        if (callTypeFilter.HasValue)
            closedCallsByVolunteer = closedCallsByVolunteer.Where(call => call.Type == callTypeFilter.Value);

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

    public async Task<IEnumerable<OpenCallInList>> GetOpenCallsByVolunteerAsync(
        int volunteerId, BO.CallType? callTypeFilter = null, OpenCallSortField? sortByField = null)
    {
        var volunteer = s_dal.Volunteer.Read(volunteerId)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} not found.");

        if (string.IsNullOrEmpty(volunteer.CurrentAddress) || volunteer.Latitude == null || volunteer.Longitude == null)
            throw new InvalidOperationException("Can't display the open calls list because a valid address hasn't been set. Please enter a valid address first, and then you'll be able to view the open calls list");

        var allCalls = s_dal.Call.ReadAll();
        var allOpenCalls = allCalls
            .Select(Helpers.CallManager.ConvertCallToBO)
            .Where(call => call.Status == BO.CallStatus.Open || call.Status == BO.CallStatus.OpenAtRisk);

        if (callTypeFilter.HasValue)
            allOpenCalls = allOpenCalls.Where(call => call.Type == callTypeFilter.Value);

        var allOpenCallInList = await Task.WhenAll(
            allOpenCalls.Select(call =>
                Helpers.CallManager.ConvertToOpenCallInListAsync(call, volunteer, volunteer.DistanceType)));

        var sorted = sortByField switch
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

        return sorted;
    }

    public async Task<Dictionary<int, GeoCoordinates>> GetCoordinatesForCallsAsync(IEnumerable<OpenCallInList> calls)
    {
        var coordinatesDict = new ConcurrentDictionary<int, GeoCoordinates>();
        var tasks = new List<Task>();

        foreach (var call in calls)
        {
            tasks.Add(Task.Run(async () =>
            {
                if (!string.IsNullOrEmpty(call.FullAddress))
                {
                    var coordinates = await GetCachedCoordinatesAsync(call.FullAddress);
                    if (coordinates != null)
                        coordinatesDict[call.Id] = coordinates;
                }
            }));
        }

        await Task.WhenAll(tasks);
        return new Dictionary<int, GeoCoordinates>(coordinatesDict);
    }

    private static readonly ConcurrentDictionary<string, GeoCoordinates> _coordinatesCache =
        new ConcurrentDictionary<string, GeoCoordinates>();

    private async Task<GeoCoordinates> GetCachedCoordinatesAsync(string address)
    {
        if (_coordinatesCache.TryGetValue(address, out var cached))
            return cached;

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

        // Fallback – ברירת מחדל בטוחה
        return new GeoCoordinates { Latitude = 31.895049, Longitude = 34.810181 };
    }

    public void CompleteAssignment(int volunteerId, int assignmentId)
    {
        // Guard + lock (Update)
        AdminManager.ThrowOnSimulatorIsRunning();

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

        if (assignment.VolunteerId != volunteerId)
            throw new BlInvalidRoleException("Only the assigned volunteer can complete this assignment.");

        if (assignment.CompletionType != null || assignment.CompletionTime != null)
            throw new InvalidOperationException("The assignment is already closed or completed.");

        var updatedAssignment = assignment with
        {
            CompletionTime = AdminManager.Now,
            CompletionType = DO.AssignmentCompletionType.Treated
        };

        try
        {
            lock (AdminManager.blMutex)
            {
                s_dal.Assignment.Update(updatedAssignment);
                CallManager.Observers.NotifyItemUpdated(updatedAssignment.Id);
                CallManager.Observers.NotifyListUpdated();
            }
        }
        catch (DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("An error occurred while updating the assignment.", ex);
        }
    }

    public async Task CancelAssignmentAsync(int volunteerId, int assignmentId)
    {
        // Guard + lock (Update)
        AdminManager.ThrowOnSimulatorIsRunning();

        DO.Volunteer volunteer = s_dal.Volunteer.Read(volunteerId)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} does not exist.");

        // Latest OPEN assignment for this volunteer
        var assignment = s_dal.Assignment.ReadAll(a =>
                a.VolunteerId == volunteerId &&
                a.CompletionTime == null &&
                a.CompletionType == null)
            .OrderByDescending(a => a.EntryTime)
            .ThenByDescending(a => a.Id)
            .FirstOrDefault()
            ?? throw new BO.BlDoesNotExistException(
                $"No open assignments found for volunteer {volunteerId}.");

        if (volunteer.Role != DO.VolunteerRole.Manager && volunteerId != assignment.VolunteerId)
            throw new BlInvalidRoleException("Only an admin or the assigned volunteer can cancel this assignment.");

        if (assignment.CompletionType != null || assignment.CompletionTime != null)
            throw new InvalidOperationException("The assignment is already closed or completed.");

        if (assignment.VolunteerId == 0)
            throw new InvalidOperationException("The assignment you are trying to update is expired and has never been handled by any volunteer.");

        var cancellationType = volunteerId == assignment.VolunteerId
            ? DO.AssignmentCompletionType.VolunteerCancelled
            : DO.AssignmentCompletionType.AdminCancelled;

        var updatedAssignment = assignment with
        {
            CompletionTime = AdminManager.Now,
            CompletionType = cancellationType
        };

        try
        {
            lock (AdminManager.blMutex)
            {
                s_dal.Assignment.Update(updatedAssignment);
                CallManager.Observers.NotifyItemUpdated(assignmentId);
                CallManager.Observers.NotifyListUpdated();
            }

            if (updatedAssignment.CompletionType == DO.AssignmentCompletionType.AdminCancelled)
                await EmailService.SendCancellationEmailToVolunteerAsync(assignmentId);
        }
        catch (DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("An error occurred while updating the assignment.", ex);
        }
    }

    #region Stage 5 - Observers
    public void AddObserver(Action listObserver) =>
        CallManager.Observers.AddListObserver(listObserver);

    public void AddObserver(int id, Action observer) =>
        CallManager.Observers.AddObserver(id, observer);

    public void RemoveObserver(Action listObserver) =>
        CallManager.Observers.RemoveListObserver(listObserver);

    public void RemoveObserver(int id, Action observer) =>
        CallManager.Observers.RemoveObserver(id, observer);
    #endregion
}
