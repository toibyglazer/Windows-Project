using Helpers;
namespace BO;

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
