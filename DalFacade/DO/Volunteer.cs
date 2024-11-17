// Module Volunteer.cs
namespace DAL;
using DO;
using System.Data;


/// <summary>
/// Represents a volunteer with its properties
/// </summary>
/// <param name="Id">Unique volunteer ID (national ID)</param>
/// <param name="FullName">Full name of the volunteer</param>
/// <param name="PhoneNumber">Valid phone number</param>
/// <param name="Email">Valid email address</param>
/// <param name="Password">Password (nullable, encrypted later)</param>
/// <param name="CurrentAddress">Current address</param>
/// <param name="Latitude">Latitude (nullable)</param>
/// <param name="Longitude">Longitude (nullable)</param>
/// <param name="Role">Role (Manager/Volunteer)</param>
/// <param name="IsActive">Indicates if active</param>
/// <param name="MaxDistance">Max distance for requests (nullable)</param>
/// <param name="DistanceType">Distance calculation type (Aerial/Walking/Driving)</param>
public record Volunteer
(
    int Id,
    string FullName,
    string PhoneNumber,
    string Email,
    string? Password = null,
    string? CurrentAddress = null,
    double? Latitude = null,
    double? Longitude = null,
    VolunteerRole Role = VolunteerRole.Volunteer,
    bool IsActive = true,
    double? MaxDistance = null,
    DistanceType DistanceType = DistanceType.Aerial
)

{
    /// <summary>
    /// Default constructor for initializing a new volunteer with default values.
    /// </summary>
    public Volunteer() : this(0, string.Empty, string.Empty, string.Empty) { }
}