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
    Aerial,       // Straight-line (aerial) distance
    Walking,      // Walking distance (via paths)
    Driving       // Driving distance (via roads)
}

// Enum for different call types
public enum CallType
{
    FoodPreparation,         // Food preparation
    FoodDelivery,            // Food delivery
    EquipmentDelivery,       // Equipment delivery
    SoldierTransportation,   // Transportation of soldiers between bases
    ComplexLogisticsTransport, // Complex logistics transport
    MedicalSupport,          // Medical support
    EventCoordination,       // Event coordination
    EmergencyResponse,       // Emergency response
    BaseMaintenance,         // Base maintenance
    KitPreparation,          // Kit preparation for soldiers
    SpecialEventSupport,     // Support for special events
    None,
    All
}

// Enum for assignment completion types
public enum AssignmentCompletionType
{
    Treated,               // Successfully treated
    VolunteerCancelled,    // Canceled by the volunteer
    AdminCancelled,        // Canceled by the administrator
    Expired                // Expired due to time limits
}

// Enum for active call statuses
public enum ActiveCallStatus
{
    InProgress,  // The call is in progress
    AtRisk       // The call is at risk of not being completed
}

// Enum for call statuses
public enum CallStatus
{
    Open,               // The call is open and awaiting action
    InProgress,         // The call is currently being handled
    Closed,             // The call has been successfully closed
    Expired,            // The call expired without resolution
    OpenAtRisk,         // Open call at risk of not being completed
    InTreatmentAtRisk   // Call in treatment but at risk
}

// Enum for sorting fields for volunteers
public enum VolunteerSortField
{
    Id,                    // Sort by volunteer ID
    FullName,              // Sort by full name
    IsActive,              // Sort by activity status
    TotalCallsHandled,     // Sort by total handled calls
    TotalCallsCanceled,    // Sort by total canceled calls
    TotalCallsExpired,     // Sort by total expired calls
    ActiveCallId,          // Sort by active call ID
    Type                   // Sort by volunteer type
}

// Enum for filtering fields in a call list
public enum CallInListFilterField
{
    Id,                    // Filter by call ID
    CallId,                // Filter by assignment ID
    Type,                  // Filter by call type
    OpeningTime,           // Filter by opening time
    RemainingTime,         // Filter by remaining time
    LastVolunteer,         // Filter by the last volunteer
    TotalCompletionTime,   // Filter by total completion time
    TotalAssignments       // Filter by total number of assignments
}

// Enum for sorting fields in a call list
public enum CallInListSortField
{
    Id,                    // Sort by call ID
    CallId,                // Sort by assignment ID
    Type,                  // Sort by call type
    OpeningTime,           // Sort by opening time
    RemainingTime,         // Sort by remaining time
    LastVolunteer,         // Sort by the last volunteer
    TotalCompletionTime,   // Sort by total completion time
    TotalAssignments       // Sort by total number of assignments
}

// Enum for sorting fields in a closed call list
public enum ClosedCallSortField
{
    Id,                    // Sort by call ID
    Type,                  // Sort by call type
    FullAddress,           // Sort by full address
    OpeningTime,           // Sort by opening time
    EntryTime,             // Sort by entry time
    CompletionTime,        // Sort by completion time
    CompletionType         // Sort by type of completion
}

// Enum for sorting fields in an open call list
public enum OpenCallSortField
{
    Id,                    // Sort by call ID
    Type,                  // Sort by call type
    Description,           // Sort by call description
    FullAddress,           // Sort by full address
    OpeningTime,           // Sort by opening time
    MaxCompletionTime,     // Sort by maximum completion time
    DistanceToActiveCall   // Sort by distance to the active call
}

// Enum for time units
public enum TimeUnit
{
    MINUTE,  // Represents a minute
    HOUR,    // Represents an hour
    DAY,     // Represents a day
    MONTH,   // Represents a month
    YEAR     // Represents a year
}

// Enum for fields in a call-in-list
public enum CallInListField
{
    AssignmentId,          // Field for assignment ID
    CallId,                // Field for call ID
    CallType,              // Field for call type
    OpenTime,              // Field for opening time
    RemainingTime,         // Field for remaining time
    LastVolunteerName,     // Field for the last volunteer's name
    CompletionTime,        // Field for completion time
    Status,                // Field for the call's status
    TotalAllocations       // Field for the total number of allocations
}

// Enum for fields in an open call-in-list
public enum OpenCallInListField
{
    Id,                    // Field for call ID
    Type,                  // Field for call type
    Description,           // Field for call description
    FullAddress,           // Field for full address
    OpeningTime,           // Field for opening time
    MaxCompletionTime,     // Field for maximum completion time
    DistanceToActiveCall   // Field for distance to the active call
}
public enum Activity
{
    All,
    Active,
    Inactive
}
