namespace BlImplementation;
using BlApi;
using BO;
using System.Collections.Generic;
using System.Data;
using BCrypt.Net;



internal class VolunteerImplementation : IVolunteer
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    public BO.VolunteerRole Login(int username, string password)
    {
        DO.Volunteer volunteer = _dal.Volunteer.Read(username)
               ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {username} does not exist.");

        bool isPasswordValid = BCrypt.Verify(password, volunteer.Password);
        if (!isPasswordValid)
        {
            throw new BO.BlInvalidPasswordException($"Invalid password for Volunteer with ID {username}.");
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

        IEnumerable<BO.VolunteerInList> BOvolunteer= volunteers.Select(volunteer => Helpers.VolunteerManager.ConvertToBOVolunteerInList(volunteer));


        BOvolunteer = sortField switch
        {
            BO.VolunteerSortField.Id => BOvolunteer.OrderBy(v => v.Id),
            BO.VolunteerSortField.FullName => BOvolunteer.OrderBy(v => v.FullName),
            BO.VolunteerSortField.IsActive => BOvolunteer.OrderBy(v => v.IsActive),
            BO.VolunteerSortField.TotalCallsHandled => BOvolunteer.OrderBy(v => Helpers.VolunteerManager.GetTotalHandledCalls(v.Id)),
            BO.VolunteerSortField.TotalCallsCanceled => BOvolunteer.OrderBy(v => Helpers.VolunteerManager.GetTotalCancelledCalls(v.Id)),
            BO.VolunteerSortField.TotalCallsExpired => BOvolunteer.OrderBy(v => Helpers.VolunteerManager.GetTotalExpiredCalls(v.Id)),
            BO.VolunteerSortField.ActiveCallId => BOvolunteer.OrderBy(v => Helpers.VolunteerManager.GetActiveCallId(v.Id)),
            BO.VolunteerSortField.Type => BOvolunteer.OrderBy(v => Helpers.VolunteerManager.GetOpenCallType(v.Id)),
            _ => BOvolunteer.OrderBy(v => v.Id) 
        };

        return BOvolunteer;
    }
    public void UpdateVolunteer(int volunteerId, BO.Volunteer volunteer)
    {

        DO.Volunteer DOvolunteer = _dal.Volunteer.Read(volunteerId)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} does not exist.");

        if (DOvolunteer.Role == DO.VolunteerRole.Volunteer && volunteer.Id != volunteerId)
            throw new BO.BlAccessDeniedException("You are not authorized to update another volunteer's details.");

        if (DOvolunteer.Role != (DO.VolunteerRole)volunteer.Role)
            throw new BO.BlAccessDeniedException("You are not allowed to change your role.");


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
                throw new BO.BlVolunteerCannotBeDeletedException(
                    $"Cannot delete volunteer with ID {volunteerId}: The volunteer has handled at least one call in the past.");
        }
        catch (DO.DalDoesNotExistException ex)
        {
            
            throw new BO.BlDoesNotExistException($"Volunteer retrieval failed: {ex.Message}", ex);
        }
    }

}
