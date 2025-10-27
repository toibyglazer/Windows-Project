namespace BlImplementation;

using BlApi;
using BO;
using System.Collections.Generic;
using Helpers;

public class VolunteerImplementation : IVolunteer
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    /// <summary>
    /// Logs in a volunteer by validating their username and password.
    /// </summary>
    public (string Name, BO.VolunteerRole Role) Login(int username, string password)
    {
        // קריאה בלבד – ללא guard/lock
        DO.Volunteer volunteer = _dal.Volunteer.Read(username)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {username} does not exist.");

        bool isPasswordValid = Tools.PasswordHasher.VerifyPassword(password, volunteer.Password);
        if (!isPasswordValid)
            throw new BO.BlInvalidPasswordException($"Invalid password for Volunteer with ID {username}");

        return (volunteer.FullName, (BO.VolunteerRole)volunteer.Role);
    }

    /// <summary>
    /// Create volunteer (mutating) – guarded & locked
    /// </summary>
    public async Task AddVolunteerAsync(BO.Volunteer volunteer)
    {
        Helpers.AdminManager.ThrowOnSimulatorIsRunning(); // Guard סימולטור

        // ולידציות המרה (לא משנות מצב)
        await Helpers.VolunteerManager.ValidateVolunteerDataAsync(volunteer);
        DO.Volunteer DOvolunteer = await Helpers.VolunteerManager.ConvertToDOVolunteerAddAsync(volunteer);

        try
        {
            // שינוי מצב – בתוך נעילה משותפת של BL
            lock (Helpers.AdminManager.blMutex)
            {
                _dal.Volunteer.Create(DOvolunteer);
                VolunteerManager.Observers.NotifyListUpdated(); // stage 5
            }
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException($"Volunteer create failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Read-only: volunteer details (no guard/lock)
    /// </summary>
    public async Task<BO.Volunteer> GetVolunteerDetailsAsync(int volunteerId)
    {
        try
        {
            DO.Volunteer volunteer = _dal.Volunteer.Read(volunteerId)
                ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} does not exist.");

            int? activeCallId = Helpers.VolunteerManager.GetActiveCallId(volunteerId);

            BO.CallInProgress? callInProgress = null;
            if (activeCallId.HasValue)
            {
                try
                {
                    DO.Call? activeCall = _dal.Call.Read(activeCallId.Value)
                        ?? throw new BO.BlDoesNotExistException($"Call with ID {activeCallId.Value} does not exist.");

                    callInProgress = await Helpers.VolunteerManager.CreateCallInProgressAsync(
                        volunteer, activeCall, activeCallId.Value, volunteerId);
                }
                catch (DO.DalDoesNotExistException ex)
                {
                    throw new BO.BlDoesNotExistException($"Error retrieving active call: {ex.Message}", ex);
                }
            }

            return Helpers.VolunteerManager.CreateVolunteer(volunteer, callInProgress);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Volunteer retrieval failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Read-only: list (no guard/lock)
    /// </summary>
    public IEnumerable<BO.VolunteerInList> GetVolunteersList(bool? isActiveFilter = null, BO.VolunteerSortField? sortField = null)
    {
        IEnumerable<DO.Volunteer> volunteers = _dal.Volunteer.ReadAll();

        if (isActiveFilter.HasValue)
            volunteers = volunteers.Where(v => v.IsActive == isActiveFilter);

        IEnumerable<BO.VolunteerInList> BOvolunteer =
            volunteers.Select(Helpers.VolunteerManager.ConvertToBOVolunteerInList);

        BOvolunteer = sortField switch
        {
            BO.VolunteerSortField.Id => BOvolunteer.OrderBy(v => v.Id),
            BO.VolunteerSortField.FullName => BOvolunteer.OrderBy(v => v.FullName),
            BO.VolunteerSortField.IsActive => BOvolunteer.OrderBy(v => v.IsActive),
            BO.VolunteerSortField.TotalCallsHandled => BOvolunteer.OrderBy(v => v.TotalCallsHandled),
            BO.VolunteerSortField.TotalCallsCanceled => BOvolunteer.OrderBy(v => v.TotalCallsCanceled),
            BO.VolunteerSortField.TotalCallsExpired => BOvolunteer.OrderBy(v => v.TotalCallsExpired),
            BO.VolunteerSortField.ActiveCallId => BOvolunteer.OrderBy(v => v.ActiveCallId),
            BO.VolunteerSortField.Type => BOvolunteer.OrderBy(v => v.Type),
            _ => BOvolunteer.OrderBy(v => v.Id)
        };

        return BOvolunteer;
    }

    /// <summary>
    /// Update volunteer (mutating) – guarded & locked
    /// </summary>
    public async Task UpdateVolunteerAsync(int volunteerId, BO.Volunteer volunteer, string managerPassword = null)
    {
        Helpers.AdminManager.ThrowOnSimulatorIsRunning(); // Guard סימולטור

        DO.Volunteer DOvolunteer = _dal.Volunteer.Read(volunteerId)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} does not exist.");

        // הרשאות (קריאה בלבד)
        if (DOvolunteer.Role != (DO.VolunteerRole)volunteer.Role && DOvolunteer.Role == DO.VolunteerRole.Volunteer)
            throw new BO.BlAccessDeniedException("You are not allowed to change your role, only the manager.");

        if (volunteer.CurrentAddress != DOvolunteer.CurrentAddress)
            await Helpers.VolunteerManager.ValidateAddressAsync(volunteer.CurrentAddress);

        Helpers.VolunteerManager.ValidateVolunteerData(volunteer);
        DOvolunteer = await Helpers.VolunteerManager.ConvertToDOVolunteerAsync(volunteer);

        try
        {
            lock (Helpers.AdminManager.blMutex)
            {
                _dal.Volunteer.Update(DOvolunteer);
                VolunteerManager.Observers.NotifyListUpdated();  // stage 5
                VolunteerManager.Observers.NotifyItemUpdated(DOvolunteer.Id);  // stage 5
            }
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Volunteer update failed: {ex.Message}", ex);
        }
        catch (BO.AddressValidationException ex)
        {
            throw new BO.BlDoesNotExistException($"Address Validation Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete volunteer (mutating) – guarded & locked
    /// </summary>
    public void DeleteVolunteer(int volunteerId)
    {
        Helpers.AdminManager.ThrowOnSimulatorIsRunning(); // Guard סימולטור

        DO.Volunteer DOvolunteer = _dal.Volunteer.Read(volunteerId)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} does not exist.");

        try
        {
            bool hasHandledCalls = Helpers.VolunteerManager.GetTotalHandledCalls(volunteerId) > 0;
            bool hasActiveCall = Helpers.VolunteerManager.GetActiveCallId(volunteerId) != null;
            bool hasCancelledCalls = Helpers.VolunteerManager.GetTotalCancelledCalls(volunteerId) > 0;

            if (!hasHandledCalls && !hasActiveCall && !hasCancelledCalls)
            {
                lock (Helpers.AdminManager.blMutex)
                {
                    _dal.Volunteer.Delete(volunteerId);
                    VolunteerManager.Observers.NotifyListUpdated();  // stage 5
                }
            }
            else
            {
                if (hasHandledCalls)
                    throw new BO.BlVolunteerCannotBeDeletedException(
                        $"Cannot delete volunteer with ID {volunteerId}: The volunteer has handled at least one call in the past.");

                if (hasActiveCall)
                    throw new BO.BlVolunteerCannotBeDeletedException(
                        $"Cannot delete volunteer with ID {volunteerId}: The volunteer is currently handling an active call.");
            }
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Volunteer deletion failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Change password (mutating) – guarded & locked
    /// </summary>
    public void UpdatePassword(int volunteerId, string oldPassword, string newPassword)
    {
        Helpers.AdminManager.ThrowOnSimulatorIsRunning(); // Guard סימולטור

        DO.Volunteer DOvolunteer = _dal.Volunteer.Read(volunteerId)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} does not exist.");

        string storedPassword = DOvolunteer.Password;

        bool isOldPasswordValid = Tools.PasswordHasher.VerifyPassword(oldPassword, storedPassword);
        if (!isOldPasswordValid)
            throw new IncorrectPasswordException("The old password you entered is incorrect. Please try again.");

        Helpers.VolunteerManager.ValidatePassword(newPassword);

        string newHashedPassword = Tools.PasswordHasher.HashPassword(newPassword);
        DOvolunteer = DOvolunteer with { Password = newHashedPassword };

        lock (Helpers.AdminManager.blMutex)
        {
            _dal.Volunteer.Update(DOvolunteer);
        }
    }

    #region Stage 5 Observers
    public void AddObserver(Action listObserver) =>
        VolunteerManager.Observers.AddListObserver(listObserver);

    public void AddObserver(int id, Action observer) =>
        VolunteerManager.Observers.AddObserver(id, observer);

    public void RemoveObserver(Action listObserver) =>
        VolunteerManager.Observers.RemoveListObserver(listObserver);

    public void RemoveObserver(int id, Action observer) =>
        VolunteerManager.Observers.RemoveObserver(id, observer);
    #endregion
}
