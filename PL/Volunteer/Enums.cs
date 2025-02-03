namespace PL.Volunteer;

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

// DistanceTypeCollection - אוסף סוגי המרחק
public class DistanceTypeCollection : IEnumerable
{
    static readonly IEnumerable<BO.DistanceType> s_enums =
        Enum.GetValues(typeof(BO.DistanceType)) as IEnumerable<BO.DistanceType> ?? throw new InvalidOperationException();

    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();

    public BO.DistanceType type { get; set; } = BO.DistanceType.Aerial;  // אפשר להניח ערך ברירת מחדל
}

public class VolunteerRoleCollection : IEnumerable
{
    // הגדרת אוסף ערכי VolunteerRole
    static readonly IEnumerable<BO.VolunteerRole> s_enums =
        Enum.GetValues(typeof(BO.VolunteerRole)) as IEnumerable<BO.VolunteerRole> ?? throw new InvalidOperationException();

    // איטרטור עבור האוסף
    public IEnumerator GetEnumerator() => s_enums.GetEnumerator();

    // ערך ברירת מחדל לתפקיד
    public BO.VolunteerRole Role { get; set; } = BO.VolunteerRole.Volunteer;
}
