using System.Security.Cryptography;

namespace DalApi;

public interface IConfig
{
    DateTime Clock { get; set; }
    int NextCallId { get; }
    int NextAssignmentId { get; }
    TimeSpan RiskRangelock { get; set; }
    void Reset();

}
