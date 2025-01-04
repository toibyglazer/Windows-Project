using BO;
using DalApi;
using System.Linq;
using System.Text.RegularExpressions;
namespace Helpers;

using BlApi;
using DO;


internal static class VolunteerManager
{
    private static IDal s_dal = DalApi.Factory.Get; //stage 4

    private static int CountAssignmentsByType(int volunteerId, DO.AssignmentCompletionType completionType)
    {
        return s_dal.Assignment.ReadAll()
            .Count(a => a.VolunteerId == volunteerId && a.CompletionType == completionType);
    }
    public static int GetTotalHandledCalls(int volunteerId) =>
    CountAssignmentsByType(volunteerId, DO.AssignmentCompletionType.Treated);
    public static int GetTotalCancelledCalls(int volunteerId) =>
        CountAssignmentsByType(volunteerId, DO.AssignmentCompletionType.VolunteerCancelled);
    public static int GetTotalExpiredCalls(int volunteerId) =>
        CountAssignmentsByType(volunteerId, DO.AssignmentCompletionType.Expired);

    public static BO.Volunteer CreateVolunteer(DO.Volunteer volunteer, BO.CallInProgress? callInProgress)
    {
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
            TotalHandledCalls = Helpers.VolunteerManager.GetTotalHandledCalls(volunteer.Id),
            TotalCancelledCalls = Helpers.VolunteerManager.GetTotalCancelledCalls(volunteer.Id),
            TotalExpiredCalls = Helpers.VolunteerManager.GetTotalExpiredCalls(volunteer.Id),
            ActiveCall = callInProgress
        };
    }
    public static int? GetActiveCallId(int volunteerId)
    {
        DO.Assignment? activeAssignment = s_dal.Assignment.ReadAll()
            .FirstOrDefault(a => a.VolunteerId == volunteerId && a.CompletionTime == null);

        return activeAssignment?.CallId;
    }
    public static BO.CallType GetOpenCallType(int volunteerId)
    {
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


    public static BO.VolunteerInList ConvertToBOVolunteerInList(DO.Volunteer volunteer)
    {
        return new BO.VolunteerInList
        {
            Id = volunteer.Id,
            FullName = volunteer.FullName,
            IsActive = volunteer.IsActive,
            TotalCallsHandled = GetTotalHandledCalls(volunteer.Id),
            TotalCallsCanceled = GetTotalCancelledCalls(volunteer.Id),
            TotalCallsExpired = GetTotalExpiredCalls(volunteer.Id),
            ActiveCallId = GetActiveCallId(volunteer.Id),
            Type = GetOpenCallType(volunteer.Id)
        };
    }
    public static DO.Volunteer ConvertToDOVolunteer(BO.Volunteer volunteer)
    {
        double? longitude = null;
        double? latitude = null;
        if (!string.IsNullOrEmpty(volunteer.CurrentAddress))
        {
            GeoCoordinates geoCoordinates = Tools.CallOpenStreetMapApi(volunteer.CurrentAddress);
            longitude = geoCoordinates.Longitude;
            latitude = geoCoordinates.Latitude;
        }
        var hashedPassword = volunteer.Password;
        if (!(Tools.PasswordHasher.IsHashed(volunteer.Password)))
        {
            hashedPassword = Tools.PasswordHasher.HashPassword(volunteer.Password);
        }
        
        return new DO.Volunteer
        {
            Id = volunteer.Id,
            FullName = volunteer.FullName,
            PhoneNumber = volunteer.PhoneNumber,
            Email = volunteer.Email,
            Password = hashedPassword,
            CurrentAddress = volunteer.CurrentAddress,
            Latitude = latitude,
            Longitude = longitude,
            Role = (DO.VolunteerRole)volunteer.Role,
            IsActive = volunteer.IsActive,
            MaxDistance = volunteer.MaxDistance,
            DistanceType = (DO.DistanceType)volunteer.DistanceType
        };
    }
 

    #region Creating CallInProgress
    public static BO.CallInProgress CreateCallInProgress(DO.Volunteer volunteer, DO.Call activeCall, int activeCallId, int volunteerId)
    {
        double distance = CalculateDistance(volunteer, activeCall, volunteer.DistanceType);
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
    public static int GetActiveAssignmentId(int volunteerId)
    {
        var activeAssignment = s_dal.Assignment.ReadAll()
        .FirstOrDefault(a => a.VolunteerId == volunteerId && a.CompletionTime == null);

        return activeAssignment.Id;
    }
    public static DateTime GetActiveCallEntryTime(int volunteerId)
    {
        DO.Assignment? activeAssignment = s_dal.Assignment.ReadAll()
            .FirstOrDefault(a => a.VolunteerId == volunteerId && a.CompletionTime == null);

        return activeAssignment.EntryTime;
    }
    public static ActiveCallStatus DetermineCallStatus(DateTime? maxCompletionTime)
    {
        if (maxCompletionTime.HasValue)
        {
            DateTime currentTime = s_dal.Config.Clock;
            DateTime maxCompletionDate = maxCompletionTime.Value;

            TimeSpan timeRemaining = maxCompletionDate - currentTime;


            if (timeRemaining <= s_dal.Config.RiskRangelock)
            {
                return ActiveCallStatus.AtRisk;
            }
            else
            {
                return ActiveCallStatus.InProgress;
            }
        }
        else
        {
            return ActiveCallStatus.InProgress;
        }
    }
    #endregion

    #region Validate BO Volunteer Data
    public static void ValidateVolunteerData(BO.Volunteer volunteer)
    {
        try
        {
            ValidateId(volunteer.Id);
            ValidateFullName(volunteer.FullName);
            ValidatePhoneNumber(volunteer.PhoneNumber);
            ValidateEmail(volunteer.Email);
            ValidatePassword(volunteer.Password);
            ValidateAddress(volunteer.CurrentAddress);
            TryParseAndValidateRole(volunteer.Role.ToString());
            ValidateMaxDistance(volunteer.MaxDistance);
            ValidateDistanceType(volunteer.DistanceType.ToString());
        }

        catch (Exception ex)
        {
            throw HandleValidationException(ex);
        }

    }
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
    public static void ValidateId(int id)
    {
        if (id <= 0)
            throw new InvalidIdException("ID must be a positive number.");
        if (!IsValidIsraeliId(id))
            throw new InvalidIdException("ID is not valid.");
    }
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
    public static void ValidateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new InvalidFullNameException("Full name cannot be empty.");
    }
    public static void ValidatePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new InvalidPhoneNumberException("Phone number cannot be empty.");
        if (!Regex.IsMatch(phoneNumber, @"^0\d{1,2}[- ]?\d{3}[- ]?\d{4}$"))
            throw new InvalidPhoneNumberException("Phone number must start with 0 and have 10 digits.");

    }
    public static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidEmailException("Email cannot be empty.");

        int atIndex = email.IndexOf('@');
        int dotIndex = email.LastIndexOf('.');

        if (atIndex <= 0 || dotIndex <= atIndex + 1 || dotIndex == email.Length - 1)
            throw new InvalidEmailException("Email is not valid.");
    }
    public static void ValidatePassword(string? password)
    {
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
    public static void ValidateAddress(string? address)
    {
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
    public static void ValidateLatitudeLongitude(double? latitude, double? longitude)
    {
        if (latitude.HasValue && (latitude < -90 || latitude > 90))
            throw new InvalidCoordinatesException("Latitude must be between -90 and 90.");
        if (longitude.HasValue && (longitude < -180 || longitude > 180))
            throw new InvalidCoordinatesException("Longitude must be between -180 and 180.");
    }
    public static void TryParseAndValidateRole(string roleString)
    {
        if (!Enum.TryParse(roleString, true, out BO.VolunteerRole role))
            throw new InvalidRoleException($"Invalid role string: {roleString}");
    }
    public static void ValidateMaxDistance(double? maxDistance)
    {
        if (maxDistance.HasValue && maxDistance <= 0)
            throw new InvalidMaxDistanceException("Max distance must be a positive number.");
    }
    public static void ValidateDistanceType(string input)
    {
        if (!Enum.TryParse(input, true, out BO.DistanceType distanceType))
        {
            throw new InvalidDistanceTypeException($"Invalid input '{input}' for DistanceType.");
        }
    }

    #endregion

    #region Distance calculation according to distance type
    public static double CalculateDistance(DO.Volunteer volunteer, DO.Call call, DO.DistanceType distanceType)
    {
        double distance = 0;
        double latitude = volunteer.Latitude ?? throw new InvalidOperationException("Latitude is null");
        double longitude = volunteer.Longitude ?? throw new InvalidOperationException("Longitude is null");

        switch (distanceType)
        {
            case DO.DistanceType.Aerial:

                distance = CalculateAerialDistance(latitude, longitude, call.Latitude, call.Longitude);
                break;

            case DO.DistanceType.Driving:
                distance = Tools.GetWalkingRouteDistance(latitude, longitude, call.Latitude, call.Longitude);
                break;

            case DO.DistanceType.Walking:
                distance = Tools.GetDrivingRouteDistance(latitude, longitude, call.Latitude, call.Longitude);
                break;

            default:
                throw new ArgumentException("Unsupported distance type");
        }

        return distance;
    }
    private static double CalculateAerialDistance(double? lat1, double? lon1, double lat2, double lon2)
    {
        const double EarthRadiusKm = 6371.0;

        double startLatitude = lat1.Value;
        double startLongitude = lon1.Value;

        double lat1Rad = DegreesToRadians(startLatitude);
        double lon1Rad = DegreesToRadians(startLongitude);
        double lat2Rad = DegreesToRadians(lat2);
        double lon2Rad = DegreesToRadians(lon2);

        double dLat = lat2Rad - lat1Rad;
        double dLon = lon2Rad - lon1Rad;


        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));


        double distance = EarthRadiusKm * c;

        return distance;
    }
    private static double DegreesToRadians(double degrees)
    {
        return degrees * (Math.PI / 180.0);
    }
    #endregion
}









