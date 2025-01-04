using Helpers;
namespace BO;

public class Volunteer
{
    public int Id { get; init; }
    public required string FullName { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Email { get; set; }
    public string Password { get; set; }
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
    public override string ToString() => this.ToStringProperty();
}

