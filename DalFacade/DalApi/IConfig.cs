using System.Security.Cryptography;

namespace DalApi;

/// <summary>
/// Interface for managing global configuration settings in the system.
/// 
/// Properties:
/// - Clock: Gets or sets the current system time.
/// - NextCallId: Retrieves the next unique ID for a Call object.
/// - NextAssignmentId: Retrieves the next unique ID for an Assignment object.
/// - RiskRangelock: Gets or sets the time span for risk range lock.
/// 
/// Methods:
/// - Reset(): Resets configuration settings to their initial values.
public interface IConfig
{
    DateTime Clock { get; set; }
    int NextCallId { get; }
    int NextAssignmentId { get; }
    TimeSpan RiskRangelock { get; set; }
    void Reset();

}
