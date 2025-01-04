using Helpers;
namespace BO;
/// <summary>
/// Represents a call that is currently in progress.
/// - Id: A unique identifier for the call in progress.
/// - CallId: A unique identifier for the call that is in progress.
/// - Type: The type of the call (e.g., emergency, non-emergency).
/// - Description: A description of the call, if provided.
/// - FullAddress: The full address of the call.
/// - OpeningTime: The time when the call was opened.
/// - MaxCompletionTime: The latest time by which the call should be completed, if applicable.
/// - EntryTime: The time when the volunteer entered the call.
/// - DistanceToActiveCall: The distance from the current location to the active call location.
/// - activeCallStatus: The current status of the active call (e.g., in-progress, completed, etc.).
/// </summary>
public class CallInProgress
{
    public int Id { get; init; }
    public int CallId { get; init; }
    public CallType Type { get; set; }
    public string? Description { get; set; }
    public string FullAddress { get; set; }
    public DateTime OpeningTime { get; set; }
    public DateTime? MaxCompletionTime { get; set; }
    public DateTime EntryTime { get; set; }
    public double DistanceToActiveCall { get; set; }
    public ActiveCallStatus activeCallStatus { get; set; }
    public override string ToString() => this.ToStringProperty();
 

}




