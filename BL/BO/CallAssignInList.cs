using Helpers;
namespace BO;

public class CallAssignInList
{
    public int Id { get; init; }
    public string FullName { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime? CompletionTime { get; set; }
    public AssignmentCompletionType? CompletionType { get; set; }
    public override string ToString() => this.ToStringProperty();

}
