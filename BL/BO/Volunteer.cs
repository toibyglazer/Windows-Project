using Helpers;
namespace BO;
/// <summary>
/// Represents a volunteer in the system with relevant personal and operational details.
/// </summary>
/// <remarks>
/// Properties:
/// - <see cref="Id"/>: Unique identifier for the volunteer.
/// - <see cref="FullName"/>: The full name of the volunteer (required).
/// - <see cref="PhoneNumber"/>: Contact phone number for the volunteer (required).
/// - <see cref="Email"/>: Email address of the volunteer (required).
/// - <see cref="Password"/>: Password for the volunteer's account.
/// - <see cref="CurrentAddress"/>: The current address of the volunteer (optional).
/// - <see cref="Latitude"/>: Geographical latitude of the volunteer's location (optional).
/// - <see cref="Longitude"/>: Geographical longitude of the volunteer's location (optional).
/// - <see cref="Role"/>: The role assigned to the volunteer (e.g., Manager, Volunteer).
/// - <see cref="IsActive"/>: Indicates whether the volunteer is active in the system.
/// - <see cref="MaxDistance"/>: Maximum distance the volunteer is willing to travel (optional).
/// - <see cref="DistanceType"/>: Type of distance calculation (e.g., aerial, driving).
/// - <see cref="TotalHandledCalls"/>: Total number of calls the volunteer has handled.
/// - <see cref="TotalCancelledCalls"/>: Total number of calls the volunteer has canceled.
/// - <see cref="TotalExpiredCalls"/>: Total number of calls that expired while assigned.
/// - <see cref="ActiveCall"/>: The current active call assigned to the volunteer (optional).
/// </remarks>
public class Volunteer
{
    public int Id { get; init; }
    public required string FullName { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Email { get; set; }
    public string? Password { get; set; }
    public string? CurrentAddress { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public VolunteerRole Role { get; set; } 
    public bool IsActive { get; set; }
    public double? MaxDistance { get; set; }
    public DistanceType DistanceType { get; set; }
    public int TotalHandledCalls { get; set; }
    public int TotalCancelledCalls { get; set; }
    public int TotalExpiredCalls { get; set; }
    public BO.CallInProgress? ActiveCall { get; set; }

    //public string ImageProfilePath { get; set; } =
    //  Path.Combine(AppContext.BaseDirectory, "ProfileImage", "default.jpg");


    public override string ToString() => this.ToStringProperty();
}

