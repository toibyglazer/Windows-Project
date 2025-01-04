namespace BlApi;
/// <summary>
/// Main interface for the BL layer containing sub-components.
/// </summary>
/// 
/// Explanations for the properties:
/// - Volunteer: Provides access to functionalities related to volunteers.
/// - Call: Provides access to functionalities related to calls.
/// - Admin: Provides access to administrative functionalities.
public interface IBl
{
    IVolunteer Volunteer { get; }
    ICall Call { get; }
    IAdmin Admin { get; }
}
