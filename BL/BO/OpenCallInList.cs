using Helpers;
namespace BO;

public class OpenCallInList
{
    public int Id { get; init; }
    public CallType Type { get; set; }
    public string? Description { get; set; }
    public string FullAddress { get; set; }
    public DateTime OpeningTime { get; set; }
    public DateTime? MaxCompletionTime { get; set; }
    public double DistanceToActiveCall { get; set; }
    public override string ToString() => this.ToStringProperty();
}
