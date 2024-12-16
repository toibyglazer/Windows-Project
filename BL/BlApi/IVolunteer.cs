namespace BlApi;

public interface IVolunteer
{
    BO.VolunteerRole Login(int username, string password);
    IEnumerable<BO.VolunteerInList> GetVolunteersList(bool? isActiveFilter = null, BO.VolunteerSortField? sortField = null);
    BO.Volunteer GetVolunteerDetails(int volunteerId);
    void UpdateVolunteer(int volunteerId, BO.Volunteer volunteer);
    void AddVolunteer(BO.Volunteer volunteer);

}
