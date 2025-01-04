using Helpers;
namespace BO;
/// <summary>
/// Represents an open call in a list, including essential details and properties.
/// </summary>
/// <remarks>
/// Properties:
/// - <see cref="Id"/>: Unique identifier for the open call.
/// - <see cref="Type"/>: Specifies the type of the call (e.g., logistics, medical).
/// - <see cref="Description"/>: Optional additional details about the call.
/// - <see cref="FullAddress"/>: The exact location of the call.
/// - <see cref="OpeningTime"/>: Timestamp when the call was created.
/// - <see cref="MaxCompletionTime"/>: Latest possible completion time for the call (optional).
/// - <see cref="DistanceToActiveCall"/>: Distance in kilometers from the current active call.
/// </remarks>
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
