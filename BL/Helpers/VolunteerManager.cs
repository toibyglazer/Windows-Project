using BO;
using DalApi;
using System.Linq;
using System.Text.RegularExpressions;
namespace Helpers;

using System.Drawing;
using System.Net;
using System.Net.Mail;
using BlApi;
using DO;


internal static class VolunteerManager
{
    private static IDal s_dal = DalApi.Factory.Get; //stage 4
    internal static ObserverManager Observers = new(); //stage 5 

    /// <summary>
    /// Counts the number of assignments for a given volunteer and a specific completion type.
    /// </summary>
    /// <param name="volunteerId">The ID of the volunteer.</param>
    /// <param name="completionType">The completion type to filter assignments (e.g., treated, cancelled, expired).</param>
    /// <returns>The count of assignments that match the given volunteer and completion type.</returns>
    private static int CountAssignmentsByType(int volunteerId, DO.AssignmentCompletionType completionType)
    {
        // Reads all assignments and counts those that match the given volunteerId and completionType
        return s_dal.Assignment.ReadAll()
            .Count(a => a.VolunteerId == volunteerId && a.CompletionType == completionType);
    }

    /// <summary>
    /// Gets the total number of handled calls (treated) for a given volunteer.
    /// </summary>
    /// <param name="volunteerId">The ID of the volunteer.</param>
    /// <returns>The total number of treated calls for the volunteer.</returns>
    public static int GetTotalHandledCalls(int volunteerId) =>
        // Calls CountAssignmentsByType with the 'Treated' completion type to get the count of treated assignments
        CountAssignmentsByType(volunteerId, DO.AssignmentCompletionType.Treated);

    /// <summary>
    /// Gets the total number of cancelled calls for a given volunteer.
    /// </summary>
    /// <param name="volunteerId">The ID of the volunteer.</param>
    /// <returns>The total number of cancelled calls for the volunteer.</returns>
    public static int GetTotalCancelledCalls(int volunteerId) =>
        // Calls CountAssignmentsByType with the 'VolunteerCancelled' completion type to get the count of cancelled assignments
        CountAssignmentsByType(volunteerId, DO.AssignmentCompletionType.VolunteerCancelled);

    /// <summary>
    /// Gets the total number of expired calls for a given volunteer.
    /// </summary>
    /// <param name="volunteerId">The ID of the volunteer.</param>
    /// <returns>The total number of expired calls for the volunteer.</returns>
    public static int GetTotalExpiredCalls(int volunteerId) =>
        // Calls CountAssignmentsByType with the 'Expired' completion type to get the count of expired assignments
        CountAssignmentsByType(volunteerId, DO.AssignmentCompletionType.Expired);


    /// <summary>
    /// Converts a DO.Volunteer object to a BO.VolunteerInList object for use in a list display.
    /// </summary>
    /// <param name="volunteer">The data object representing a volunteer.</param>
    /// <returns>A business object representing the volunteer in list format, with total calls and active call details.</returns>
    public static BO.VolunteerInList ConvertToBOVolunteerInList(DO.Volunteer volunteer)
    {
        // Creating a new VolunteerInList business object, populating it with data from DO.Volunteer.
        return new BO.VolunteerInList
        {
            Id = volunteer.Id,
            FullName = volunteer.FullName,
            IsActive = volunteer.IsActive, // Whether the volunteer is active.
            TotalCallsHandled = GetTotalHandledCalls(volunteer.Id), // Total calls handled by the volunteer.
            TotalCallsCanceled = GetTotalCancelledCalls(volunteer.Id), // Total calls canceled by the volunteer.
            TotalCallsExpired = GetTotalExpiredCalls(volunteer.Id), // Total calls expired for the volunteer.
            ActiveCallId = GetActiveCallId(volunteer.Id), // The ID of the active call, if any.
            Type = GetOpenCallType(volunteer.Id) // Type of the open call for the volunteer.
        };
    }
    /// <summary>
    /// Creates a new Volunteer business object (BO.Volunteer) from the data object (DO.Volunteer) and an optional in-progress call.
    /// </summary>
    /// <param name="volunteer">The data object representing a volunteer.</param>
    /// <param name="callInProgress">The current in-progress call, if any.</param>
    /// <returns>A business object representing the volunteer with additional properties like handled calls and active call.</returns>
    public static BO.Volunteer CreateVolunteer(DO.Volunteer volunteer, BO.CallInProgress? callInProgress)
    {
        // Creating a new Volunteer business object and mapping data from the DO.Volunteer to BO.Volunteer.
        return new BO.Volunteer
        {
            Id = volunteer.Id,
            FullName = volunteer.FullName,
            PhoneNumber = volunteer.PhoneNumber,
            Email = volunteer.Email,
            Password = volunteer.Password,
            CurrentAddress = volunteer.CurrentAddress,
            Role = (BO.VolunteerRole)volunteer.Role,
            IsActive = volunteer.IsActive,
            MaxDistance = volunteer.MaxDistance,
            DistanceType = (BO.DistanceType)volunteer.DistanceType,
            TotalHandledCalls = GetTotalHandledCalls(volunteer.Id),
            TotalCancelledCalls = GetTotalCancelledCalls(volunteer.Id),
            TotalExpiredCalls = GetTotalExpiredCalls(volunteer.Id),
            ActiveCall = callInProgress,
            //ImageProfilePath= volunteer.ImageProfilePath
        };
    }

    /// <summary>
    /// Gets the ID of the active call for a given volunteer, if any.
    /// </summary>
    /// <param name="volunteerId">The ID of the volunteer whose active call is to be retrieved.</param>
    /// <returns>The ID of the active call, or null if no active call exists.</returns>
    public static int? GetActiveCallId(int volunteerId)
    {
        DO.Assignment? activeAssignment = s_dal.Assignment.ReadAll()
            .FirstOrDefault(a => a.VolunteerId == volunteerId && a.CompletionTime == null);

        // Returns the CallId of the active assignment, or null if no active assignment is found.
        return activeAssignment?.CallId;
    }

    /// <summary>
    /// Gets the type of the open call for a given volunteer, or 'None' if there is no open call.
    /// </summary>
    /// <param name="volunteerId">The ID of the volunteer whose open call type is to be retrieved.</param>
    /// <returns>The type of the open call, or BO.CallType.None if no open call exists.</returns>
    public static BO.CallType GetOpenCallType(int volunteerId)
    {
        // Looks for the active assignment where the volunteer's completion time is null.
        var activeAssignment = s_dal.Assignment.ReadAll()
            .FirstOrDefault(a => a.VolunteerId == volunteerId && a.CompletionTime == null);

        if (activeAssignment == null)
            return BO.CallType.None;


        DO.Call? call = s_dal.Call.ReadAll()
            .FirstOrDefault(c => c.Id == activeAssignment.CallId);

        if (call == null)
        {
            return BO.CallType.None;
        }
        return (BO.CallType)call.Type;
    }




    /// <summary>
    /// Converts a BO.Volunteer object to a DO.Volunteer object for saving to the database.
    /// </summary>
    /// <param name="volunteer">The business object representing a volunteer.</param>
    /// <returns>A data object representing the volunteer, ready to be saved to the database.</returns>
    public static DO.Volunteer ConvertToDOVolunteer(BO.Volunteer volunteer)
    {
        DO.Volunteer DOvolunteer = s_dal.Volunteer.Read(volunteer.Id);

        bool addressChangeCheck = !string.IsNullOrEmpty(volunteer.CurrentAddress) &&
                           volunteer.CurrentAddress != DOvolunteer.CurrentAddress;

        double? longitude = null;
        double? latitude = null;
        if (addressChangeCheck)
        {
            // If the volunteer has a current address, retrieve geographical coordinates (latitude and longitude).
            if (!string.IsNullOrEmpty(volunteer.CurrentAddress))
            {
                // Calling an external API (OpenStreetMap) to get latitude and longitude based on the address.
                GeoCoordinates geoCoordinates = Tools.CallOpenStreetMapApi(volunteer.CurrentAddress);
                longitude = geoCoordinates.Longitude;
                latitude = geoCoordinates.Latitude;
            }
        }


        // Ensure that the volunteer's password is hashed. If not, hash it before saving.
        var hashedPassword = volunteer.Password;
        if (!(Tools.PasswordHasher.IsHashed(volunteer.Password)))
        {
            hashedPassword = Tools.PasswordHasher.HashPassword(volunteer.Password); // Hash the password if not already hashed.
        }

        // Creating and returning a new DO.Volunteer object, populating it with data from the BO.Volunteer.
        return new DO.Volunteer
        {
            Id = volunteer.Id,
            FullName = volunteer.FullName,
            PhoneNumber = volunteer.PhoneNumber,
            Email = volunteer.Email,
            Password = hashedPassword, // Store the hashed password in the database.
            CurrentAddress = volunteer.CurrentAddress,
            Latitude = addressChangeCheck ? latitude : volunteer.Latitude, // Store latitude.
            Longitude = addressChangeCheck ? longitude : volunteer.Longitude, // Store longitude.
            Role = (DO.VolunteerRole)volunteer.Role, // Convert the volunteer's role to a data object enum.
            IsActive = volunteer.IsActive, // Store the volunteer's activity status.
            MaxDistance = volunteer.MaxDistance, // Store the volunteer's maximum distance for service.
            DistanceType = (DO.DistanceType)volunteer.DistanceType, // Convert the distance type to a data object enum.
            //ImageProfilePath= volunteer.ImageProfilePath
        };
    }
    public static DO.Volunteer ConvertToDOVolunteerAdd(BO.Volunteer volunteer)
    {

        double? longitude = null;
        double? latitude = null;


        // If the volunteer has a current address, retrieve geographical coordinates (latitude and longitude).
        if (!string.IsNullOrEmpty(volunteer.CurrentAddress))
        {
            // Calling an external API (OpenStreetMap) to get latitude and longitude based on the address.
            GeoCoordinates geoCoordinates = Tools.CallOpenStreetMapApi(volunteer.CurrentAddress);
            longitude = geoCoordinates.Longitude;
            latitude = geoCoordinates.Latitude;
        }


        // Ensure that the volunteer's password is hashed. If not, hash it before saving.
        var hashedPassword = volunteer.Password;
        if (!(Tools.PasswordHasher.IsHashed(volunteer.Password)))
        {
            hashedPassword = Tools.PasswordHasher.HashPassword(volunteer.Password); // Hash the password if not already hashed.
        }

        // Creating and returning a new DO.Volunteer object, populating it with data from the BO.Volunteer.
        return new DO.Volunteer
        {
            Id = volunteer.Id,
            FullName = volunteer.FullName,
            PhoneNumber = volunteer.PhoneNumber,
            Email = volunteer.Email,
            Password = hashedPassword, // Store the hashed password in the database.
            CurrentAddress = string.IsNullOrEmpty(volunteer.CurrentAddress) ? null : volunteer.CurrentAddress, // שמירה של null במקום ערך ריק
            Latitude = latitude, // Store latitude.
            Longitude = longitude, // Store longitude.
            Role = (DO.VolunteerRole)volunteer.Role, // Convert the volunteer's role to a data object enum.
            IsActive = volunteer.IsActive, // Store the volunteer's activity status.
            MaxDistance = volunteer.MaxDistance, // Store the volunteer's maximum distance for service.
            DistanceType = (DO.DistanceType)volunteer.DistanceType, // Convert the distance type to a data object enum.
            //ImageProfilePath= volunteer.ImageProfilePath
        };
    }
    //public static DO.Volunteer UpdateImageDoVolunteer(DO.Volunteer volunteer, string path)
    //{
    //    return volunteer with { ImageProfilePath = path };
    //}




    #region Creating CallInProgress

    /// <summary>
    /// Creates a CallInProgress object based on the provided volunteer, active call, and other relevant information.
    /// </summary>
    /// <param name="volunteer">The volunteer who is handling the call.</param>
    /// <param name="activeCall">The active call being assigned to the volunteer.</param>
    /// <param name="activeCallId">The ID of the active call.</param>
    /// <param name="volunteerId">The ID of the volunteer handling the call.</param>
    /// <returns>A CallInProgress object containing details about the active call and its status.</returns>
    public static BO.CallInProgress CreateCallInProgress(DO.Volunteer volunteer, DO.Call activeCall, int activeCallId, int volunteerId)
    {
        // Calculate the distance between the volunteer and the active call based on their settings
        double distance = CalculateDistance(volunteer, activeCall, volunteer.DistanceType);

        // Return a new CallInProgress object with the relevant details
        return new BO.CallInProgress
        {
            Id = GetActiveAssignmentId(volunteerId),
            CallId = activeCallId,
            Type = (BO.CallType)activeCall.Type,
            FullAddress = activeCall.FullAddress,
            OpeningTime = activeCall.OpeningTime,
            MaxCompletionTime = activeCall.MaxCompletionTime,
            EntryTime = GetActiveCallEntryTime(volunteerId),
            DistanceToActiveCall = distance,
            activeCallStatus = DetermineCallStatus(activeCall.MaxCompletionTime)
        };
    }

    /// <summary>
    /// Gets the ID of the active assignment for the given volunteer.
    /// </summary>
    /// <param name="volunteerId">The ID of the volunteer.</param>
    /// <returns>The ID of the active assignment for the volunteer.</returns>
    public static int GetActiveAssignmentId(int volunteerId)
    {
        // Fetch the active assignment for the volunteer where CompletionTime is null (not completed)
        var activeAssignment = s_dal.Assignment.ReadAll()
            .FirstOrDefault(a => a.VolunteerId == volunteerId && a.CompletionTime == null);

        // Return the ID of the active assignment
        return activeAssignment.Id;
    }

    /// <summary>
    /// Gets the entry time of the active call assignment for the given volunteer.
    /// </summary>
    /// <param name="volunteerId">The ID of the volunteer.</param>
    /// <returns>The entry time of the active assignment.</returns>
    public static DateTime GetActiveCallEntryTime(int volunteerId)
    {
        // Fetch the active assignment for the volunteer
        DO.Assignment? activeAssignment = s_dal.Assignment.ReadAll()
            .FirstOrDefault(a => a.VolunteerId == volunteerId && a.CompletionTime == null);

        // Return the entry time of the active assignment
        return activeAssignment.EntryTime;
    }

    /// <summary>
    /// Determines the status of an active call based on its maximum completion time.
    /// </summary>
    /// <param name="maxCompletionTime">The maximum time allowed to complete the call.</param>
    /// <returns>The status of the active call (AtRisk, InProgress, etc.).</returns>
    public static ActiveCallStatus DetermineCallStatus(DateTime? maxCompletionTime)
    {
        // If there is a defined maximum completion time for the call
        if (maxCompletionTime.HasValue)
        {
            // Get the current time from the system configuration (e.g., system clock)
            DateTime currentTime = s_dal.Config.Clock;
            DateTime maxCompletionDate = maxCompletionTime.Value;

            // Calculate the remaining time for the call
            TimeSpan timeRemaining = maxCompletionDate - currentTime;

            // If the time remaining is less than the risk threshold, the call is "At Risk"
            if (timeRemaining <= s_dal.Config.RiskRangelock)
            {
                return ActiveCallStatus.AtRisk;
            }
            else
            {
                return ActiveCallStatus.InProgress; // Otherwise, the call is still in progress
            }
        }
        else
        {
            return ActiveCallStatus.InProgress; // If no maximum completion time is set, assume the call is in progress
        }
    }
    #endregion



    #region Validate BO Volunteer Data
    /// <summary>
    /// Validates the volunteer data to ensure all fields meet the required criteria.
    /// </summary>
    /// <param name="volunteer">The volunteer whose data needs to be validated.</param>
    public static void ValidateVolunteerData(BO.Volunteer volunteer)
    {
        try
        {
            // Each field should pass validation to ensure integrity.
            ValidateId(volunteer.Id); // ID must be positive and valid.
            ValidateFullName(volunteer.FullName); // Full name must not be empty.
            ValidatePhoneNumber(volunteer.PhoneNumber); // Phone number must match a specific pattern.
            ValidateEmail(volunteer.Email); // Email must follow the correct format.
            ValidatePassword(volunteer.Password); // Password must meet complexity requirements.
            ValidateAddress(volunteer.CurrentAddress); // Address must be complete and valid.
            TryParseAndValidateRole(volunteer.Role.ToString()); // Role must be a valid enum.
            ValidateMaxDistance(volunteer.MaxDistance); // Max distance must be a positive number.
            ValidateDistanceType(volunteer.DistanceType.ToString()); // Distance type must be valid.
        }
        catch (Exception ex)
        {
            // Handle all validation errors uniformly by throwing a custom exception.
            throw HandleValidationException(ex);
        }
    }

    /// <summary>
    /// Handles exceptions thrown during the validation process and returns a more specific exception.
    /// </summary>
    /// <param name="ex">The exception to be handled.</param>
    /// <returns>A new exception with a custom message based on the type of the original exception.</returns>
    private static Exception HandleValidationException(Exception ex)
    {
        return ex switch
        {
            InvalidIdException => new InvalidIdException($"ID Validation Error: {ex.Message}"),
            InvalidFullNameException => new InvalidFullNameException($"Full Name Validation Error: {ex.Message}"),
            InvalidPhoneNumberException => new InvalidPhoneNumberException($"Phone Number Validation Error: {ex.Message}"),
            InvalidEmailException => new InvalidEmailException($"Email Validation Error: {ex.Message}"),
            PasswordValidationException => new PasswordValidationException($"Password Validation Error: {ex.Message}"),
            AddressValidationException => new AddressValidationException($"Address Validation Error: {ex.Message}"),
            InvalidCoordinatesException => new InvalidCoordinatesException($"Coordinates Validation Error: {ex.Message}"),
            InvalidRoleException => new InvalidRoleException($"Role Validation Error: {ex.Message}"),
            InvalidMaxDistanceException => new InvalidMaxDistanceException($"Max Distance Validation Error: {ex.Message}"),
            InvalidDistanceTypeException => new InvalidDistanceTypeException($"Distance Type Validation Error: {ex.Message}"),
            _ => new Exception($"Unexpected Error: {ex.Message}", ex),
        };
    }

    /// <summary>
    /// Validates the volunteer's ID, ensuring it is a positive number and a valid Israeli ID.
    /// </summary>
    /// <param name="id">The ID to be validated.</param>
    public static void ValidateId(int id)
    {
        // ID should be positive and pass Luhn validation to be valid.
        if (id <= 0)
            throw new InvalidIdException("ID must be a positive number.");
        if (!IsValidIsraeliId(id))
            throw new InvalidIdException("ID is not valid.");
    }

    /// <summary>
    /// Checks if the provided ID is a valid Israeli ID by performing the Luhn check.
    /// </summary>
    /// <param name="id">The ID to check.</param>
    /// <returns>True if the ID is valid, false otherwise.</returns>
    private static bool IsValidIsraeliId(int id)
    {
        string idString = id.ToString("D9");
        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            int digit = int.Parse(idString[i].ToString());
            if (i % 2 == 1)
                digit *= 2;
            if (digit > 9)
                digit -= 9;
            sum += digit;
        }
        return sum % 10 == 0;
    }

    /// <summary>
    /// Validates the volunteer's full name to ensure it's not empty or null.
    /// </summary>
    /// <param name="fullName">The full name to validate.</param>
    public static void ValidateFullName(string fullName)
    {
        // Full name must not be empty or whitespace.
        if (string.IsNullOrWhiteSpace(fullName))
            throw new InvalidFullNameException("Full name cannot be empty.");
    }

    /// <summary>
    /// Validates the volunteer's phone number using a regular expression to ensure it matches a specific pattern.
    /// </summary>
    /// <param name="phoneNumber">The phone number to validate.</param>
    public static void ValidatePhoneNumber(string phoneNumber)
    {
        // Phone number must follow the specific format for valid local numbers.
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new InvalidPhoneNumberException("Phone number cannot be empty.");
        if (!Regex.IsMatch(phoneNumber, @"^0\d{1,2}[- ]?\d{3}[- ]?\d{4}$"))
            throw new InvalidPhoneNumberException("Phone number must start with 0 and have 10 digits.");
    }

    /// <summary>
    /// Validates the volunteer's email address to ensure it follows a basic valid email format.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    public static void ValidateEmail(string email)
    {
        // Email must follow the correct format with a '@' and '.'.
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidEmailException("Email cannot be empty.");

        int atIndex = email.IndexOf('@');
        int dotIndex = email.LastIndexOf('.');

        if (atIndex <= 0 || dotIndex <= atIndex + 1 || dotIndex == email.Length - 1)
            throw new InvalidEmailException("Email is not valid.");
    }

    /// <summary>
    /// Validates the volunteer's password to ensure it meets complexity requirements (length, uppercase, etc.).
    /// </summary>
    /// <param name="password">The password to validate.</param>
    public static void ValidatePassword(string? password)
    {
        // Password must not be empty and must fulfill complexity requirements: length, case, digits, special characters.
        if (string.IsNullOrEmpty(password))
            throw new PasswordValidationException("Password cannot be empty or null.");
        if (password.Length < 8)
            throw new PasswordValidationException("Password must be at least 8 characters long.");
        if (!Regex.IsMatch(password, "[A-Z]"))
            throw new PasswordValidationException("Password must contain at least one uppercase letter (A-Z).");
        if (!Regex.IsMatch(password, "[a-z]"))
            throw new PasswordValidationException("Password must contain at least one lowercase letter (a-z).");
        if (!Regex.IsMatch(password, "\\d"))
            throw new PasswordValidationException("Password must contain at least one numeric digit (0-9).");
        if (!Regex.IsMatch(password, "[!@#$%^&*(),.?\":{}|<>]"))
            throw new PasswordValidationException("Password must contain at least one special character (e.g., !@#$%^&*).");
    }

    /// <summary>
    /// Validates the volunteer's address to ensure it follows the required structure and exists.
    /// </summary>
    /// <param name="address">The address to validate.</param>
    public static void ValidateAddress(string? address)
    {
        // Address must be structured correctly and match real-world existence.
        if (string.IsNullOrWhiteSpace(address))
            return;

        var parts = address.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                           .Select(p => p.Trim())
                           .ToArray();

        if (parts.Length < 3)
            throw new AddressValidationException("Address must include at least Street, City, and Country.");
        if (parts.Any(p => p.Length < 2))
            throw new AddressValidationException("Each part of the address must be at least 2 characters long.");
        if (!Tools.VerifyAddressExists(address))
            throw new AddressValidationException("Address does not exist in reality.");
    }

    /// <summary>
    /// Validates the coordinates (latitude and longitude) to ensure they fall within valid ranges.
    /// </summary>
    /// <param name="latitude">The latitude to validate.</param>
    /// <param name="longitude">The longitude to validate.</param>
    public static void ValidateLatitudeLongitude(double? latitude, double? longitude)
    {
        // Latitude and longitude must be within valid geographical ranges.
        if (latitude.HasValue && (latitude < -90 || latitude > 90))
            throw new InvalidCoordinatesException("Latitude must be between -90 and 90.");
        if (longitude.HasValue && (longitude < -180 || longitude > 180))
            throw new InvalidCoordinatesException("Longitude must be between -180 and 180.");
    }

    /// <summary>
    /// Tries to parse and validate the volunteer's role.
    /// </summary>
    /// <param name="roleString">The role as a string to parse and validate.</param>
    public static void TryParseAndValidateRole(string roleString)
    {
        // Role must be a valid enum value.
        if (!Enum.TryParse(roleString, true, out BO.VolunteerRole role))
            throw new InvalidRoleException($"Invalid role string: {roleString}");
    }

    /// <summary>
    /// Validates the volunteer's maximum distance to ensure it's a positive number.
    /// </summary>
    /// <param name="maxDistance">The maximum distance to validate.</param>
    public static void ValidateMaxDistance(double? maxDistance)
    {
        // Max distance must be positive if provided.
        if (maxDistance.HasValue && maxDistance < 0)
            throw new InvalidMaxDistanceException("Max distance must be a positive number.");
    }

    /// <summary>
    /// Validates the distance type provided by the volunteer.
    /// </summary>
    /// <param name="input">The distance type as a string to validate.</param>
    public static void ValidateDistanceType(string input)
    {
        // Distance type must be a valid enum value.
        if (!Enum.TryParse(input, true, out BO.DistanceType distanceType))
        {
            throw new InvalidDistanceTypeException($"Invalid input '{input}' for DistanceType.");
        }
    }

    /// <summary>
    /// Validates the image profile path for a volunteer.
    /// Ensures the path exists, points to a valid file, and has the correct extension.
    /// </summary>
    /// <param name="imageProfilePath">The image file path to validate.</param>
    public static void ValidateImageProfilePath(string imageProfilePath)
    {

        if (string.IsNullOrWhiteSpace(imageProfilePath))
        {
            throw new ArgumentException("Image profile path is required.");
        }

        if (!File.Exists(imageProfilePath))
        {
            throw new FileNotFoundException($"The image file '{imageProfilePath}' was not found.");
        }
        string extension = Path.GetExtension(imageProfilePath).ToLower();

        if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
        {
            throw new ArgumentException($"The file '{imageProfilePath}' must be a .jpg, .jpeg, or .png image.");
        }
    }


    #endregion


    #region Distance calculation according to distance type

    /// <summary>
    /// Calculates the distance between a volunteer and a call, based on the specified distance type.
    /// </summary>
    /// <param name="volunteer">The volunteer whose location is being used for the calculation.</param>
    /// <param name="call">The call to which the distance is being calculated.</param>
    /// <param name="distanceType">The type of distance calculation (Aerial, Driving, or Walking).</param>
    /// <returns>The calculated distance between the volunteer and the call.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the volunteer's latitude or longitude is null.</exception>
    /// <exception cref="ArgumentException">Thrown if an unsupported distance type is provided.</exception>
    public static double CalculateDistance(DO.Volunteer volunteer, DO.Call call, DO.DistanceType distanceType)
    {
        double distance = 0;
        double latitude = volunteer.Latitude ?? throw new InvalidOperationException("Latitude is null");
        double longitude = volunteer.Longitude ?? throw new InvalidOperationException("Longitude is null");

        // Based on the distance type, the correct calculation method is called.
        switch (distanceType)
        {
            case DO.DistanceType.Aerial:
                // Calculate aerial distance using the haversine formula.
                distance = CalculateAerialDistance(latitude, longitude, call.Latitude, call.Longitude);
                break;

            case DO.DistanceType.Driving:
                // Calculate the distance using a walking route.
                distance = Tools.GetWalkingRouteDistance(latitude, longitude, call.Latitude, call.Longitude);
                break;

            case DO.DistanceType.Walking:
                // Calculate the distance using a driving route.
                distance = Tools.GetDrivingRouteDistance(latitude, longitude, call.Latitude, call.Longitude);
                break;

            default:
                // Exception is thrown if the distance type is unsupported.
                throw new ArgumentException("Unsupported distance type");
        }

        return distance;
    }

    /// <summary>
    /// Calculates the aerial (straight-line) distance between two geographical points.
    /// </summary>
    /// <param name="lat1">The latitude of the first point.</param>
    /// <param name="lon1">The longitude of the first point.</param>
    /// <param name="lat2">The latitude of the second point.</param>
    /// <param name="lon2">The longitude of the second point.</param>
    /// <returns>The aerial distance in kilometers between the two points.</returns>
    private static double CalculateAerialDistance(double? lat1, double? lon1, double lat2, double lon2)
    {
        const double EarthRadiusKm = 6371.0; // Radius of the Earth in kilometers.

        double startLatitude = lat1.Value;
        double startLongitude = lon1.Value;

        // Convert the latitude and longitude from degrees to radians.
        double lat1Rad = DegreesToRadians(startLatitude);
        double lon1Rad = DegreesToRadians(startLongitude);
        double lat2Rad = DegreesToRadians(lat2);
        double lon2Rad = DegreesToRadians(lon2);

        // Calculate the difference between latitudes and longitudes.
        double dLat = lat2Rad - lat1Rad;
        double dLon = lon2Rad - lon1Rad;

        // Apply the Haversine formula to calculate the distance.
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // Calculate the final distance in kilometers.
        double distance = EarthRadiusKm * c;

        return distance;
    }

    /// <summary>
    /// Converts a given angle in degrees to radians.
    /// </summary>
    /// <param name="degrees">The angle in degrees to be converted.</param>
    /// <returns>The angle in radians.</returns>
    private static double DegreesToRadians(double degrees)
    {
        return degrees * (Math.PI / 180.0);
    }

    #endregion

    public static string GeneratePassword()
    {
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string specialChars = "!@#$%^&*()_+<>?";
        const int passwordLength = 12;

        var allChars = upperChars + lowerChars + digits + specialChars;
        var random = new Random();

        // מוודאים שהסיסמה כוללת לפחות תו אחד מכל סוג
        string password = new string(new[]
        {
        upperChars[random.Next(upperChars.Length)],
        lowerChars[random.Next(lowerChars.Length)],
        digits[random.Next(digits.Length)],
        specialChars[random.Next(specialChars.Length)]
    });

        // משלימים את שאר התווים באורך הרצוי
        password += new string(Enumerable.Range(0, passwordLength - 4)
            .Select(_ => allChars[random.Next(allChars.Length)]).ToArray());

        // ערבוב הסיסמה למניעת תבנית קבועה
        return new string(password.OrderBy(_ => random.Next()).ToArray());
    }


    public class SendEmailNewVolunteer
    {
        private const string SmtpHost = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SenderEmail = "toibymichalminiproject@gmail.com";
        private const string SenderPassword = "iwge idji gbtj lomm";

        public static void SendStaticEmail(string recipientEmail, string volunteerFullName, string generatedPassword)
        {
            if (string.IsNullOrEmpty(recipientEmail))
            {
                throw new InvalidOperationException("Recipient email address is missing.");
            }

            try
            {
                using (var smtpClient = new SmtpClient(SmtpHost, SmtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(SenderEmail, SenderPassword);
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(SenderEmail, "Volunteer Support Team"),
                        Subject = "Welcome to Our Volunteer Family!",
                        IsBodyHtml = true
                    };

                    string body = $@"
<html>
    <head>
        <style>
            body {{
                font-family: 'Arial', sans-serif;
                line-height: 1.6;
                color: #333;
                background-color: #ffffff;
                margin: 0;
                padding: 0;
            }}
            .email-container {{
                width: 100%;
                max-width: 500px; /* הקטנת הרוחב המקסימלי */
                margin: 0 auto;
                padding: 20px; /* הקטנת הריפוד */
                background-image: url('https://pnghq.com/wp-content/uploads/pnghq.com-confetti-png-free-download.png');
                background-repeat: repeat;
                background-size: 150px 150px; /* הקטנת גודל הרקע */
                background-position: center;
                background-color: #f0f0f0;
            }}
            .content {{
                background-color: rgba(255, 255, 255, 0.9);
                padding: 15px; /* הקטנת הריפוד */
                border-radius: 8px; /* הקטנת הרדיוס */
                box-shadow: 0 0 8px rgba(0,0,0,0.1);
            }}
            h2 {{
                color: #4a5568; /* שינוי צבע הכותרת לשחור */
                text-align: center;
                padding: 15px 0;
                font-size: 20px; /* הקטנת גודל הפונט */
                font-weight: bold;
                margin-top: 0;
            }}
            p {{
                font-size: 14px; /* הקטנת גודל הפונט */
                color: #4a5568;
            }}
            .highlight {{
                font-weight: bold;
                color: #1e3a8a;
                background-color: #e2e8f0;
                padding: 4px 8px;
                border-radius: 4px;
            }}
            .footer {{
                font-size: 12px; /* הקטנת גודל הפונט */
                text-align: center;
                margin-top: 20px;
                color: #718096;
                padding: 15px 0;
                background-color: rgba(247, 250, 252, 0.8);
                border-radius: 0 0 8px 8px;
            }}
        </style>
    </head>
    <body>
        <div class='email-container'>
            <div class='content'>
                <h2>Welcome to Our Volunteer Family, {volunteerFullName}!</h2>
                <p>We are thrilled to have you join our team of dedicated volunteers. Your commitment to making a difference is truly inspiring!</p>
                <p>Here are the details for your new account:</p>
                <p><span class='highlight'>Your new password:</span> {generatedPassword}</p>
                <p>Please ensure you keep this password secure and don't share it with anyone.</p>
                <p>We can't wait to see the positive impact you'll make in our community. Remember, your efforts, no matter how small, can create ripples of change!</p>
                <p>If you have any questions or need assistance, don't hesitate to reach out to us. We're here to support you every step of the way.</p>
                
                <div class='footer'>
                    <p>With gratitude,</p>
                    <p>The Volunteer Management Team</p>
                </div>
            </div>
        </div>
    </body>
</html>";

                    mailMessage.Body = body;
                    mailMessage.To.Add(recipientEmail);
                    smtpClient.Send(mailMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send email: {ex.Message}");
            }
        }
    }

}









