using Helpers;
namespace BO;

public class Call
{
    public int Id { get; init; }
    public CallType Type { get; set; }
    public string? Description { get; set; }
    public string FullAddress { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime OpeningTime { get; set; }
    public DateTime? MaxCompletionTime { get; set; }
    public CallStatus Status { get; set; }
    public List<BO.CallAssignInList>? callAssignInList { get; set; }
    public override string ToString() => this.ToStringProperty();
}
