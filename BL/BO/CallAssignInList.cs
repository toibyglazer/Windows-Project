using Helpers;
namespace BO;
/// <summary>
/// Represents an assignment in the call list.
/// - Id: A unique identifier for the assignment.
/// - FullName: The full name of the person assigned to the call.
/// - EntryTime: The time when the person was assigned to the call.
/// - CompletionTime: The time when the assignment was completed, if applicable.
/// - CompletionType: The type of completion for the assignment (e.g., successful, failed).
/// </summary>
public class CallAssignInList
{
    public int Id { get; init; }
    public string FullName { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? CompletionTime { get; set; }
    public AssignmentCompletionType? CompletionType { get; set; }
    public override string ToString() => this.ToStringProperty();

}
