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

    /// <summary>
    /// Logs in a volunteer by validating their username and password.
    /// Implements a lockout mechanism after 5 failed login attempts, with a 3-minute lockout period.
    /// This is an additional security feature to prevent brute-force attacks and improve system security.
    /// </summary>
    /// <param name="username">The unique identifier for the volunteer.</param>
    /// <param name="password">The password entered by the volunteer.</param>
    /// <returns>The volunteer's role if login is successful.</returns>
    /// <exception cref="BO.BlLockoutException">Thrown if the volunteer is locked out due to too many failed login attempts.</exception>
    /// <exception cref="BO.BlDoesNotExistException">Thrown if the volunteer does not exist in the database.</exception>
    /// <exception cref="BO.BlInvalidPasswordException">Thrown if the password entered is incorrect.</exception>
    private static Dictionary<int, (int FailedAttempts, DateTime? LockoutEndTime)> loginAttempts
        = new Dictionary<int, (int FailedAttempts, DateTime? LockoutEndTime)>();

    public BO.VolunteerRole Login(int username, string password)
    {
        // *** Check for previous failed login attempts ***
        // If there are any past login failures, check if the volunteer is currently locked out.
        if (loginAttempts.ContainsKey(username))
        {
            var (failedAttempts, lockoutEndTime) = loginAttempts[username];

            // *** Lockout Check ***
            // If the volunteer is locked out (the lockout time hasn't passed), throw an exception.
            if (lockoutEndTime.HasValue && lockoutEndTime > DateTime.Now)
            {
                throw new BO.BlLockoutException($"Volunteer with ID {username} is locked out until {lockoutEndTime.Value}.");
            }

            // *** Reset Failed Attempts After Lockout Time Expires ***
            // If the lockout period has expired, reset the failed attempts counter and clear lockout time.
            if (lockoutEndTime.HasValue && lockoutEndTime <= DateTime.Now)
            {
                loginAttempts[username] = (0, null); // Reset failed attempts and clear lockout
            }
        }

        // *** Retrieve the Volunteer from the Database ***
        // Look up the volunteer's details using their ID. If not found, throw an exception.
        DO.Volunteer volunteer = _dal.Volunteer.Read(username)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {username} does not exist.");

        // *** Validate the Password ***
        // Check if the entered password matches the stored password hash.
        bool isPasswordValid = Tools.PasswordHasher.VerifyPassword(password, volunteer.Password);
        if (!isPasswordValid)
        {
            // *** Initialize Failed Attempt Counter ***
            // If it's the first failed attempt, initialize the counter.
            if (!loginAttempts.ContainsKey(username))
            {
                loginAttempts[username] = (1, null); // First failed attempt, no lockout yet
            }
            else
            {
                // *** Increment Failed Attempts ***
                var (failedAttempts, lockoutEndTime) = loginAttempts[username];
                failedAttempts++;

                // *** Lockout After 5 Failed Attempts ***
                // If 5 failed attempts are reached, lock the volunteer out for 3 minutes.
                if (failedAttempts >= 5)
                {
                    lockoutEndTime = DateTime.Now.AddMinutes(3); // Set lockout time for 3 minutes
                    loginAttempts[username] = (failedAttempts, lockoutEndTime);

                    throw new BO.BlLockoutException($"Volunteer with ID {username} is now locked out until {lockoutEndTime.Value}.");
                }
                else
                {
                    // If attempts are less than 5, simply update the counter without lockout.
                    loginAttempts[username] = (failedAttempts, null);
                }
            }

            // *** Incorrect Password Handling ***
            // If the password is invalid, throw an exception.
            throw new BO.BlInvalidPasswordException($"Invalid password for Volunteer with ID {username}");
        }

        // *** Clear Previous Login Attempts on Successful Login ***
        // If login is successful, remove the volunteer from the failed attempts dictionary.
        if (loginAttempts.ContainsKey(username))
        {
            loginAttempts.Remove(username);
        }

        // *** Return the Volunteer Role ***
        // If login is successful, return the role of the volunteer.
        return (BO.VolunteerRole)volunteer.Role;
    }
    public void AddVolunteer(BO.Volunteer volunteer)
    {
        // Validate the input volunteer object to ensure all required data is correct and complete.
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

            // Check if the volunteer has an active call.
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

        // Apply the optional filter for active/inactive volunteers.
        if (isActiveFilter.HasValue)
            volunteers = volunteers.Where(v => v.IsActive == isActiveFilter);

        // Convert data-layer volunteers to business-layer VolunteerInList objects.
        IEnumerable<BO.VolunteerInList> BOvolunteer = volunteers
            .Select(volunteer => Helpers.VolunteerManager.ConvertToBOVolunteerInList(volunteer));

        // Apply the optional sorting based on the specified field.
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
            _ => BOvolunteer.OrderBy(v => v.Id) // Default sorting by ID if no field is specified.
        };

        return BOvolunteer;
    }

    /// <summary>
    /// Updates the details of a volunteer in the system.
    /// Validates permissions and checks the password if necessary.
    /// </summary>
    /// <param name="volunteerId">The ID of the volunteer to update.</param>
    /// <param name="volunteer">The updated volunteer details as a BO object.</param>
    /// <param name="managerPassword">
    /// Optional: The manager's password, required if a manager is performing the update.
    /// </param>
    /// <exception cref="BO.BlDoesNotExistException">Thrown if the volunteer does not exist.</exception>
    /// <exception cref="BO.BlAccessDeniedException">
    /// Thrown if the update is unauthorized or the provided password is incorrect.
    /// </exception>
    public void UpdateVolunteer(int volunteerId, BO.Volunteer volunteer, string managerPassword = null)
    {
        DO.Volunteer DOvolunteer = _dal.Volunteer.Read(volunteerId)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} does not exist.");

        // Validate permissions based on the role and IDs.
        // Volunteer cannot update another volunteer's details.
        if (DOvolunteer.Role == DO.VolunteerRole.Volunteer && volunteer.Id != volunteerId)
            throw new BO.BlAccessDeniedException("You are not authorized to update another volunteer's details.");

        //  Validate password for volunteer updating their own details.
        if (!(Tools.PasswordHasher.IsHashed(volunteer.Password))
            && DOvolunteer.Role == DO.VolunteerRole.Volunteer
            && volunteer.Id == volunteerId)
        {
            if (!(Tools.PasswordHasher.VerifyPassword(volunteer.Password, DOvolunteer.Password)))
                throw new BO.BlAccessDeniedException("The password you entered is incorrect.");
        }

        // Validate manager password if the role is manager.
        if (DOvolunteer.Role == DO.VolunteerRole.Manager)
        {
            if (string.IsNullOrEmpty(managerPassword) ||
                !Tools.PasswordHasher.VerifyPassword(managerPassword, DOvolunteer.Password))
            {
                throw new BO.BlAccessDeniedException("Manager password is incorrect. You cannot update the volunteer's details.");
            }
        }

        // Ensure that volunteers cannot change their role or activity status.
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
        // Step 1: Retrieve the volunteer from the data layer.
        DO.Volunteer DOvolunteer = _dal.Volunteer.Read(volunteerId)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} does not exist.");

        try
        {
            // Step 2: Check if the volunteer has handled calls in the past or has an active call.
            bool hasHandledCalls = Helpers.VolunteerManager.GetTotalHandledCalls(volunteerId) > 0;
            bool hasActiveCall = Helpers.VolunteerManager.GetActiveCallId(volunteerId) != null;

            // Step 3: If the volunteer meets the criteria for deletion, delete them.
            if (!hasHandledCalls && !hasActiveCall)
            {
                _dal.Volunteer.Delete(volunteerId);
            }
            else
            {
                // Step 4: If the volunteer cannot be deleted, throw appropriate exceptions.
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
            // Handle the case where the volunteer could not be deleted due to a data layer issue.
            throw new BO.BlDoesNotExistException($"Volunteer retrieval failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Updates the password of a volunteer, ensuring the old password is correct and the new password is valid.
    /// </summary>
    /// <param name="volunteerId">The ID of the volunteer whose password is being updated.</param>
    /// <param name="oldPassword">The old password entered by the volunteer.</param>
    /// <param name="newPassword">The new password to set for the volunteer.</param>
    /// <exception cref="BO.BlDoesNotExistException">Thrown if the volunteer does not exist.</exception>
    /// <exception cref="IncorrectPasswordException">Thrown if the old password provided is incorrect.</exception>
    /// <exception cref="ArgumentException">Thrown if the new password is invalid.</exception>
    public void UpdatePassword(int volunteerId, string oldPassword, string newPassword)
    {
        DO.Volunteer DOvolunteer = _dal.Volunteer.Read(volunteerId)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} does not exist.");

        string storedPassword = DOvolunteer.Password;

        // Verify the old password using the PasswordHasher.
        bool isOldPasswordValid = Tools.PasswordHasher.VerifyPassword(oldPassword, storedPassword);
        if (!isOldPasswordValid)
        {
            throw new IncorrectPasswordException("The old password you entered is incorrect. Please try again.");
        }

        // Validate the new password's complexity and requirements.
        Helpers.VolunteerManager.ValidatePassword(newPassword);

        // Encrypt the new password using the HashPassword method.
        string newHashedPassword = Tools.PasswordHasher.HashPassword(newPassword);

        // Create an updated copy of the volunteer with the new hashed password.
        DOvolunteer = DOvolunteer with { Password = newHashedPassword };

        // Update the volunteer in the data layer.
        _dal.Volunteer.Update(DOvolunteer);
    }

}
