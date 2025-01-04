namespace BlApi;

public interface IVolunteer
{
    BO.VolunteerRole Login(int username, string password);
    IEnumerable<BO.VolunteerInList> GetVolunteersList(bool? isActiveFilter = null, BO.VolunteerSortField? sortField = null);
    BO.Volunteer GetVolunteerDetails(int volunteerId);
    void UpdateVolunteer(int volunteerId, BO.Volunteer volunteer, string? managerPassword);
    void DeleteVolunteer(int volunteerId);
    void AddVolunteer(BO.Volunteer volunteer);

    //Addition: Password update function
    void UpdatePassword(int volunteerId, string oldPassword, string newPassword);
}
