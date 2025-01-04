namespace BlImplementation;
using BlApi;
using BO;
using System.Collections.Generic;
using System.Data;
using BCrypt.Net;
using Helpers;



internal class VolunteerImplementation : IVolunteer
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    private static Dictionary<int, (int FailedAttempts, DateTime? LockoutEndTime)> loginAttempts
    = new Dictionary<int, (int FailedAttempts, DateTime? LockoutEndTime)>();
    public BO.VolunteerRole Login(int username, string password)
    {
       
        if (loginAttempts.ContainsKey(username))
        {
            var (failedAttempts, lockoutEndTime) = loginAttempts[username];

            if (lockoutEndTime.HasValue && lockoutEndTime > DateTime.Now)
            {
                throw new BO.BlLockoutException($"Volunteer with ID {username} is locked out until {lockoutEndTime.Value}.");
            }

            if (lockoutEndTime.HasValue && lockoutEndTime <= DateTime.Now)
            {
                loginAttempts[username] = (0, null); 
            }
        }

        DO.Volunteer volunteer = _dal.Volunteer.Read(username)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {username} does not exist.");

        bool isPasswordValid = Tools.PasswordHasher.VerifyPassword(password, volunteer.Password);

        if (!isPasswordValid)
        {
          
            if (!loginAttempts.ContainsKey(username))
            {
                loginAttempts[username] = (1, null); 
            }
            else
            {
                var (failedAttempts, lockoutEndTime) = loginAttempts[username];
                failedAttempts++;

                if (failedAttempts >= 5) 
                {
                    lockoutEndTime = DateTime.Now.AddMinutes(3); 
                    loginAttempts[username] = (failedAttempts, lockoutEndTime);

                    throw new BO.BlLockoutException($"Volunteer with ID {username} is now locked out until {lockoutEndTime.Value}.");
                }
                else
                {
                    loginAttempts[username] = (failedAttempts, null);
                }
            }

            throw new BO.BlInvalidPasswordException($"Invalid password for Volunteer with ID {username}");
        }

        if (loginAttempts.ContainsKey(username))
        {
            loginAttempts.Remove(username);
        }

        return (BO.VolunteerRole)volunteer.Role;
    }


   
    public void AddVolunteer(BO.Volunteer volunteer)
    {
        Helpers.VolunteerManager.ValidateVolunteerData(volunteer);
        DO.Volunteer DOvolunteer = Helpers.VolunteerManager.ConvertToDOVolunteer(volunteer);

        try
        {
            _dal.Volunteer.Create(DOvolunteer);
        }

        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException($"Volunteer retrieval failed: {ex.Message}");
        }
    }
    public BO.Volunteer GetVolunteerDetails(int volunteerId)
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

                    callInProgress = Helpers.VolunteerManager.CreateCallInProgress(volunteer, activeCall, activeCallId.Value, volunteerId);
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
    public IEnumerable<BO.VolunteerInList> GetVolunteersList(bool? isActiveFilter = null, BO.VolunteerSortField? sortField = null)
    {
        IEnumerable<DO.Volunteer> volunteers = _dal.Volunteer.ReadAll();
        if (isActiveFilter.HasValue)
            volunteers = volunteers.Where(v => v.IsActive == isActiveFilter);

        IEnumerable<BO.VolunteerInList> BOvolunteer = volunteers.Select(volunteer => Helpers.VolunteerManager.ConvertToBOVolunteerInList(volunteer));


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
    public void UpdateVolunteer(int volunteerId, BO.Volunteer volunteer, string managerPassword = null)
    {

        DO.Volunteer DOvolunteer = _dal.Volunteer.Read(volunteerId)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} does not exist.");

       
        if (DOvolunteer.Role == DO.VolunteerRole.Volunteer && volunteer.Id != volunteerId)
            throw new BO.BlAccessDeniedException("You are not authorized to update another volunteer's details.");

       
        if (!(Tools.PasswordHasher.IsHashed(volunteer.Password)) && DOvolunteer.Role == DO.VolunteerRole.Volunteer && volunteer.Id == volunteerId)
        {
            if (!(Tools.PasswordHasher.VerifyPassword(volunteer.Password,DOvolunteer.Password)))
                throw new BO.BlAccessDeniedException("The password you entered is incorrect.");
        }

        if (DOvolunteer.Role == DO.VolunteerRole.Manager)
        {
            if (string.IsNullOrEmpty(managerPassword) || !Tools.PasswordHasher.VerifyPassword(managerPassword,DOvolunteer.Password))
            {
                throw new BO.BlAccessDeniedException("Manager password is incorrect. You cannot update the volunteer's details.");
            }
        }

        if (DOvolunteer.Role != (DO.VolunteerRole)volunteer.Role && DOvolunteer.Role == DO.VolunteerRole.Volunteer)
            throw new BO.BlAccessDeniedException("You are not allowed to change your role, only the manager.");

        if (DOvolunteer.IsActive != volunteer.IsActive && DOvolunteer.Role == DO.VolunteerRole.Volunteer)
            throw new BO.BlAccessDeniedException("You are not allowed to change your activity, only the manager.");

        Helpers.VolunteerManager.ValidateVolunteerData(volunteer);
        DOvolunteer = Helpers.VolunteerManager.ConvertToDOVolunteer(volunteer);

        try
        {
            _dal.Volunteer.Update(DOvolunteer);
        }

        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Volunteer retrieval failed: {ex.Message}", ex);
        }
    }
    public void DeleteVolunteer(int volunteerId)
    {

        DO.Volunteer DOvolunteer = _dal.Volunteer.Read(volunteerId)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} does not exist.");

        try
        {
            bool hasHandledCalls = Helpers.VolunteerManager.GetTotalHandledCalls(volunteerId) > 0;
            bool hasActiveCall = Helpers.VolunteerManager.GetActiveCallId(volunteerId) != null;

            if (!hasHandledCalls && !hasActiveCall)
                _dal.Volunteer.Delete(volunteerId);

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
            throw new BO.BlDoesNotExistException($"Volunteer retrieval failed: {ex.Message}", ex);
        }
    }
    public void UpdatePassword(int volunteerId, string oldPassword, string newPassword)
    {
        // 1. Retrieve the volunteer from the database by ID
        DO.Volunteer DOvolunteer = _dal.Volunteer.Read(volunteerId)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} does not exist.");

        string storedPassword = DOvolunteer.Password; // This contains the Salt and Hash together

        // 3. Verify the old password using the PasswordHasher
        bool isOldPasswordValid = Tools.PasswordHasher.VerifyPassword(oldPassword, storedPassword);
        if (!isOldPasswordValid)
        {
            throw new IncorrectPasswordException("The old password you entered is incorrect. Please try again.");
        }

        Helpers.VolunteerManager.ValidatePassword(newPassword);
        // 4. Encrypt the new password using the HashPassword method
        string newHashedPassword = Tools.PasswordHasher.HashPassword(newPassword);
        DOvolunteer = DOvolunteer with { Password = newHashedPassword };
        _dal.Volunteer.Update(DOvolunteer);
    }
}
