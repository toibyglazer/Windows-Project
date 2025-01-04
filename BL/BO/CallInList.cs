using Helpers;
namespace BO;
/// <summary>
/// Represents a call in the call list.
/// - Id: An optional unique identifier for the call.
/// - CallId: A unique identifier for the call.
/// - Type: The type of the call (e.g., emergency, non-emergency).
/// - OpeningTime: The time when the call was opened.
/// - RemainingTime: The remaining time until the call must be completed, if applicable.
/// - LastVolunteer: The name of the last volunteer assigned to the call.
/// - TotalCompletionTime: The total time taken to complete the call, if applicable.
/// - TotalAssignments: The total number of assignments related to the call.
/// - Status: The current status of the call (e.g., open, closed, in-progress).
/// </summary>
public class CallInList
{
    public int? Id { get; init; }
    public int CallId { get; init; }
    public CallType Type { get; set; }
    public DateTime OpeningTime { get; set; }
    public TimeSpan? RemainingTime { get; set; }
    public string? LastVolunteer { get; set; }
    public TimeSpan? TotalCompletionTime { get; set; }
    public int TotalAssignments { get; set; }
    public CallStatus Status { get; set; }


    public override string ToString() => this.ToStringProperty();

}
