using Helpers;
namespace BO;
/// <summary>
/// Represents a closed call in the call list.
/// - Id: A unique identifier for the closed call.
/// - Type: The type of the call (e.g., emergency, non-emergency).
/// - FullAddress: The full address where the call took place.
/// - OpeningTime: The time when the call was opened.
/// - EntryTime: The time when the volunteer entered the call.
/// - CompletionTime: The time when the call was completed, if applicable.
/// - CompletionType: The type of completion (e.g., successful, failed).
/// </summary>
public class ClosedCallInList
{
    public int Id { get; init; }
    public CallType Type { get; set; }
    public string FullAddress { get; set; }
    public DateTime OpeningTime { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? CompletionTime { get; set; }
    public AssignmentCompletionType? CompletionType { get; set; }
    public override string ToString() => this.ToStringProperty();
}
