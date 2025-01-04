using Helpers;
namespace BO;
/// <summary>
/// Represents a simplified view of a volunteer for list displays, summarizing key details and statistics.
/// </summary>
/// <remarks>
/// Properties:
/// - <see cref="Id"/>: Unique identifier for the volunteer.
/// - <see cref="FullName"/>: Full name of the volunteer.
/// - <see cref="IsActive"/>: Indicates if the volunteer is currently active in the system.
/// - <see cref="TotalCallsHandled"/>: Total number of calls the volunteer has successfully handled.
/// - <see cref="TotalCallsCanceled"/>: Total number of calls the volunteer has canceled.
/// - <see cref="TotalCallsExpired"/>: Total number of calls assigned to the volunteer that expired.
/// - <see cref="ActiveCallId"/>: The ID of the volunteer's current active call (if any).
/// - <see cref="Type"/>: The type of the current or most recent call assigned to the volunteer.
/// </remarks>
public class VolunteerInList
{
    public int Id { get; init; }
    public string FullName { get; set; }
    public bool IsActive { get; set; }
    public int TotalCallsHandled { get; set; }
    public int TotalCallsCanceled { get; set; }
    public int TotalCallsExpired { get; set; }
    public int? ActiveCallId { get; set; }
    public CallType Type { get; set; }

    public override string ToString() => this.ToStringProperty();
}
