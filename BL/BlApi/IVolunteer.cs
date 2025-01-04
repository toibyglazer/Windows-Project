namespace BlApi;
/// <summary>
/// Interface for managing volunteer-related operations in the BL layer.
/// </summary>
/// 
/// Explanations for the methods:
/// - Login: Allows a volunteer to log in using their credentials.
/// - GetVolunteersList: Retrieves a list of volunteers, with optional filtering and sorting.
/// - GetVolunteerDetails: Fetches detailed information about a specific volunteer by ID.
/// - UpdateVolunteer: Updates the details of an existing volunteer, requiring manager authorization.
/// - DeleteVolunteer: Deletes a volunteer from the system by ID.
/// - AddVolunteer: Adds a new volunteer to the system.
/// - UpdatePassword: Updates the password of a volunteer, ensuring proper validation of the old password.
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
