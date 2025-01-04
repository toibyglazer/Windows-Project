using Helpers;
namespace BO;

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
