using Helpers;
namespace BO;

public class CallInList
{
    public int? Id { get; init; }
    public int CallId { get; init; }
    public CallType Type { get; set; }
    public DateTime OpeningTime { get; set; }
    public TimeSpan? RemainingTime { get; set; }//סך זמן שנותר לסיום הקריאה
    public string? LastVolunteer { get; set; }
    public TimeSpan? TotalCompletionTime { get; set; }
    public int TotalAssignments { get; set; }
    public override string ToString() => this.ToStringProperty();

}
