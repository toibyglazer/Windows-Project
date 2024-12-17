namespace BlImplementation;
using BlApi;
using BO;
using System.Collections.Generic;
using System.Data;

internal class VolunteerImplementation : IVolunteer
{
    private readonly DalApi.IDal _dal = DalApi.Factory.Get;

    public BO.VolunteerRole Login(int username, string password)
    {
        DO.Volunteer volunteer = _dal.Volunteer.Read(username)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {username} does not exist.");

        if(password != volunteer.Password)
            throw new BO.BlInvalidPasswordException($"Invalid password for Volunteer with ID {username}.");

        return (BO.VolunteerRole)volunteer.Role;
    }
    public void AddVolunteer(BO.Volunteer volunteer)
    {
        throw new NotImplementedException();
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

                    callInProgress = new BO.CallInProgress
                    {
                        Id = Helpers.VolunteerManager.GetActiveAssignmentId(volunteerId),
                        CallId = activeCallId.Value,
                        Type = (BO.CallType)activeCall.Type,
                        FullAddress = activeCall.FullAddress,
                        OpeningTime = activeCall.OpeningTime,
                        MaxCompletionTime = activeCall.MaxCompletionTime,
                        EntryTime = Helpers.VolunteerManager.GetActiveCallEntryTime(volunteerId),
                        DistanceToActiveCall = Helpers.VolunteerManager
                            .CalculateDistance(volunteer.Latitude, volunteer.Longitude, activeCall.Latitude, activeCall.Longitude, (BO.DistanceType)volunteer.DistanceType),
                        activeCallStatus = Helpers.VolunteerManager.DetermineCallStatus(activeCall.MaxCompletionTime)
                    };
                }
                catch (DO.DalDoesNotExistException ex)
                {
                    throw new BO.BlDoesNotExistException($"Error retrieving active call: {ex.Message}", ex);
                }
            }

            return new BO.Volunteer
            {
                Id = volunteer.Id,
                FullName = volunteer.FullName,
                PhoneNumber = volunteer.PhoneNumber,
                Email = volunteer.Email,
                Password = volunteer.Password,
                CurrentAddress = volunteer.CurrentAddress,
                Latitude = volunteer.Latitude,
                Longitude = volunteer.Longitude,
                Role = (BO.VolunteerRole)volunteer.Role,
                IsActive = volunteer.IsActive,
                MaxDistance = volunteer.MaxDistance,
                DistanceType = (BO.DistanceType)volunteer.DistanceType,
                TotalHandledCalls = Helpers.VolunteerManager.GetTotalHandledCalls(volunteer.Id),
                TotalCancelledCalls = Helpers.VolunteerManager.GetTotalCancelledCalls(volunteer.Id),
                TotalExpiredCalls = Helpers.VolunteerManager.GetTotalExpiredCalls(volunteer.Id),
                ActiveCall = callInProgress
            };
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Volunteer retrieval failed: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlInternalLogicException($"An internal error occurred: {ex.Message}", ex);
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

       




    }
}
