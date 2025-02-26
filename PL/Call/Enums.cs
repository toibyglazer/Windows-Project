using BO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PL.Call;

public class CallTypesCollection : IEnumerable
{
    static readonly IEnumerable<BO.CallType> s_enums =
        Enum.GetValues(typeof(BO.CallType)) as IEnumerable<BO.CallType> ?? throw new InvalidOperationException();

    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();

    public BO.CallType type { get; set; } = BO.CallType.All;
}
public class CallStatusCollection : IEnumerable
{
    static readonly IEnumerable<BO.CallStatus> s_enums =
        Enum.GetValues(typeof(BO.CallStatus)) as IEnumerable<BO.CallStatus> ?? throw new InvalidOperationException();

    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();

    public CallStatus Status { get; set; } = CallStatus.All;
}
