namespace DO;


/// <summary>
/// Represents a call entity with all its properties.
/// </summary>
/// <param name="Id">Unique identifier for the call (non-nullable).</param>
/// <param name="Type">Type of the call (e.g., food preparation, delivery, etc.) (non-nullable).</param>
/// <param name="FullAddress">Full address of the call's location (non-nullable).</param>
/// <param name="Latitude">Latitude of the call's location (non-nullable).</param>
/// <param name="Longitude">Longitude of the call's location (non-nullable).</param>
/// <param name="OpeningTime">Time when the call was opened (non-nullable).</param>
/// <param name="MaxCompletionTime">Maximum time by which the call should be completed (nullable).</param>
/// <param name="Description">Detailed description of the call (nullable).</param>
public record Call
(
    int Id ,
    CallType Type,
    string FullAddress,
    double Latitude,
    double Longitude,
    DateTime OpeningTime,
    DateTime? MaxCompletionTime = null,
    string? Description = null
)

{
    /// <summary>
    ///  Default constructor for initializing a new call with default values.
    /// </summary>
    public Call() : this(0, CallType.EquipmentDelivery, string.Empty, 0.0, 0.0, DateTime.Now) { }

    public object? EntryTime { get; set; }
}


