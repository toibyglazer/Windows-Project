namespace DO;

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
    Driving       // Driving distance (via roads)
}

// Enum for call types 
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
    SpecialEventSupport      // תמיכה באירועים מיוחדים
}

public enum AssignmentCompletionType
{
    Treated,          
    VolunteerCancelled,
    AdminCancelled,   
    Expired
}


