using Helpers;
namespace BO;

public class VolunteerInList
{
    public int Id { get; init; }
    public string FullName { get; set; }
    public bool IsActive { get; set; }
    public int TotalCallsHandled { get; set; }
    public int TotalCallsCanceled { get; set; }
    public int TotalCallsExpired { get; set; }
    public int? ActiveCallId { get; set; }
    public CallType Type { get; set; }

    public override string ToString() => this.ToStringProperty();
}
