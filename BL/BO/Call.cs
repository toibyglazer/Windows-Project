using Helpers;
namespace BO;
/// <summary>
/// Represents a call in the system. 
/// - Id: A unique identifier for the call.
/// - Type: The type of the call (e.g., emergency, regular).
/// - Description: A description of the call.
/// - FullAddress: The full address of the call location.
/// - Latitude: The latitude coordinate of the call location.
/// - Longitude: The longitude coordinate of the call location.
/// - OpeningTime: The time when the call was opened or received.
/// - MaxCompletionTime: The maximum allowed time for completing the call.
/// - Status: The current status of the call (e.g., pending, completed).
/// - callAssignInList: A list of assignments associated with the call (e.g., volunteers assigned).
/// </summary>
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
