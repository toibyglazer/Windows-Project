namespace PL;
using System;
using System.Collections;
using System.Collections.Generic;


public class CallTypesCollection : IEnumerable
{
    static readonly IEnumerable<BO.CallType> s_enums =
        Enum.GetValues(typeof(BO.CallType)) as IEnumerable<BO.CallType> ?? throw new InvalidOperationException();

    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();

    public BO.CallType type { get; set; } = BO.CallType.All;
}

public class ActivityCollection : IEnumerable
{
    static readonly IEnumerable<BO.Activity> s_enums =
        Enum.GetValues(typeof(BO.Activity)) as IEnumerable<BO.Activity> ?? throw new InvalidOperationException();

    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();

    public BO.Activity type { get; set; } = BO.Activity.All;
}

