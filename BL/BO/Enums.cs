namespace BO;

// Enum for volunteer roles
public enum VolunteerRole
{
    Manager,
    Volunteer
}
// Enum for distance calculation types
public enum DistanceType
{
    Aerial,       // Aerial (straight-line) distance
    Walking,      // Walking distance (via paths)
    Driving       // Driving distance (via roads )
}
public enum CallType
{
    FoodPreparation,         // הכנת אוכל
    FoodDelivery,            // משלוח אוכל
    EquipmentDelivery,       // משלוח ציוד
    SoldierTransportation,    // הסעת חיילים בין בסיסים
    ComplexLogisticsTransport, // שינוע לוגיסטי מורכב
    MedicalSupport,          // תמיכה רפואית
    EventCoordination,       // תיאום אירועים
    EmergencyResponse,       // מענה למצבי חירום
    BaseMaintenance,         // תחזוקה בבסיס
    KitPreparation,          // הכנת ערכות לחיילים
    SpecialEventSupport,     // תמיכה באירועים מיוחדים
    None
}

public enum AssignmentCompletionType
{
    Treated,
    VolunteerCancelled,
    AdminCancelled,
    Expired
}

public enum ActiveCallStatus
{
    InProgress,
    AtRisk
}
public enum CallStatus
{
    Open,
    InProgress,
    Closed,
    Expired,
    OpenAtRisk,
    InTreatmentAtRisk
}
public enum VolunteerSortField
{
    Id,
    FullName,
    IsActive,
    TotalCallsHandled,
    TotalCallsCanceled,
    TotalCallsExpired,
    ActiveCallId,
    Type
}
public enum CallInListFilterField
{
    Id,
    CallId,
    Type,
    OpeningTime,
    RemainingTime,
    LastVolunteer,
    TotalCompletionTime,
    TotalAssignments
}

public enum CallInListSortField
{
    Id,
    CallId,
    Type,
    OpeningTime,
    RemainingTime,
    LastVolunteer,
    TotalCompletionTime,
    TotalAssignments
}

public enum ClosedCallSortField
{
    Id,             // מיון לפי מזהה הקריאה
    Type,           // מיון לפי סוג הקריאה
    FullAddress,    // מיון לפי כתובת מלאה
    OpeningTime,    // מיון לפי זמן פתיחה
    EntryTime,      // מיון לפי זמן כניסה
    CompletionTime, // מיון לפי זמן סיום
    CompletionType  // מיון לפי סוג סיום הקריאה
}
public enum OpenCallSortField
{
        Id,                    // מיון לפי מזהה הקריאה
        Type,                  // מיון לפי סוג הקריאה
        Description,           // מיון לפי תיאור הקריאה
        FullAddress,           // מיון לפי כתובת מלאה
        OpeningTime,           // מיון לפי זמן פתיחה
        MaxCompletionTime,     // מיון לפי זמן מקסימלי לסיום
        DistanceToActiveCall   // מיון לפי מרחק מהקריאה הפעילה
}

public enum TimeUnit
{
    MINUTE,
    HOUR,
    DAY,
    MONTH,
    YEAR
}
public enum CallInListField
{ 
    AssignmentId,
    CallId,
    CallType,
    OpenTime,
    RemainingTime,
    LastVolunteerName,
    CompletionTime,
    Status,
    TotalAllocations
}
public enum OpenCallInListField
{
    Id,                     // שדה מזהה הקריאה
    Type,                   // סוג הקריאה
    Description,            // תיאור הקריאה
    FullAddress,            // כתובת מלאה
    OpeningTime,            // זמן פתיחת הקריאה
    MaxCompletionTime,      // זמן סיום מקסימלי
    DistanceToActiveCall    // מרחק לקריאה פעילה
}
