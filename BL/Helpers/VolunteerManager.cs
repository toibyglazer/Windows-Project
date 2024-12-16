using BO;
using DalApi;
using System.Linq;
using System.Text.RegularExpressions;
namespace Helpers;


using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

internal static class VolunteerManager
{
    private static IDal s_dal = Factory.Get; //stage 4


    private static int CountAssignmentsByType(int volunteerId, DO.AssignmentCompletionType completionType)
    {
        return s_dal.Assignment.ReadAll()
            .Count(a => a.Id == volunteerId && a.CompletionType == completionType);
    }
    public static int GetTotalHandledCalls(int volunteerId) =>
    CountAssignmentsByType(volunteerId, DO.AssignmentCompletionType.Treated);

    public static int GetTotalCancelledCalls(int volunteerId) =>
        CountAssignmentsByType(volunteerId, DO.AssignmentCompletionType.VolunteerCancelled);

    public static int GetTotalExpiredCalls(int volunteerId) =>
        CountAssignmentsByType(volunteerId, DO.AssignmentCompletionType.Expired);

    public static int? GetActiveCallId(int volunteerId)
    {
        DO.Assignment? activeAssignment = s_dal.Assignment.ReadAll()
            .FirstOrDefault(a => a.Id == volunteerId && a.CompletionTime == null);

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
    public static int GetActiveAssignmentId(int volunteerId)
    {
        var activeAssignment = s_dal.Assignment.ReadAll()
        .FirstOrDefault(a => a.Id == volunteerId && a.CompletionTime == null);

        return activeAssignment.Id;
    }

    public static DateTime GetActiveCallEntryTime(int volunteerId)
    {
        DO.Assignment? activeAssignment = s_dal.Assignment.ReadAll()
            .FirstOrDefault(a => a.VolunteerId == volunteerId && a.CompletionTime == null);

        return activeAssignment.EntryTime;
    }

    //מחזירה מרחק בהתאם לסוג המרחק אוויר
    public static double CalculateDistance(double? lat1, double? lon1, double lat2, double lon2, BO.DistanceType distanceType)
    {

        if (lat1 == null || lon1 == null)
            return 0; // ערך ברירת מחדל למרחק במקרה של ערכים חסרים

        double distance = HaversineDistance(lat1, lon1, lat2, lon2);

      
        return distanceType switch
        {
            BO.DistanceType.Aerial => distance,
            BO.DistanceType.Walking => distance * 1.1,   // הערכה של 10% יותר
            BO.DistanceType.Driving => distance * 1.2,   // הערכה של 20% יותר
            _ => distance
        };
    }


    //פונקציית עזר לחישוב מרחק
    private static double HaversineDistance(double? lat1, double? lon1, double lat2, double lon2)
    {
        const int R = 6371;

        double latitude1 = lat1 ?? 0.0;
        double longitude1 = lon1 ?? 0.0;

        double dLat = (lat2 - latitude1) * Math.PI / 180;
        double dLon = (lon2 - longitude1) * Math.PI / 180;

        latitude1 = latitude1 * Math.PI / 180;
        lat2 = lat2 * Math.PI / 180;

        double a = Math.Pow(Math.Sin(dLat / 2), 2) +
                    Math.Cos(latitude1) * Math.Cos(lat2) *
                    Math.Pow(Math.Sin(dLon / 2), 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    //חישוב זמן הגעה לפי סוג המרחק אווירי נסיעה הליכה
    public static TimeSpan CalculateTime(double distance, BO.DistanceType distanceType)
    {
        // מהירויות ממוצעות בקמ"ש
        double speed = distanceType switch
        {
            BO.DistanceType.Aerial => 800,   // מהירות טיסה (קמ"ש)
            BO.DistanceType.Walking => 5,   // מהירות הליכה (קמ"ש)
            BO.DistanceType.Driving => 50,  // מהירות נסיעה (קמ"ש)
            _ => 5
        };

        double timeInHours = distance / speed; // זמן בשעות
        return TimeSpan.FromHours(timeInHours);
    }

    public static ActiveCallStatus DetermineCallStatus(DateTime? maxCompletionTime)
    {
        if (maxCompletionTime.HasValue)
        {
            DateTime currentTime = s_dal.Config.Clock;  // הזמן הנוכחי לפי הגדרת הקונפיגורציה.
            DateTime maxCompletionDate = maxCompletionTime.Value;

            TimeSpan timeRemaining = maxCompletionDate - currentTime;

            // אם הזמן שנותר קטן או שווה לטווח הסיכון - קריאה בסיכון
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
            // אם אין תאריך סיום מוגדר, אין הגבלת זמן, ולכן נשאר InProgress
            return ActiveCallStatus.InProgress;
        }
    }



    
    
        public static void ValidateId(int id)
        {
            if (id <= 0)
                throw new InvalidIdException("ID must be a positive number.");
            if (!IsValidIsraeliId(id))
                throw new InvalidIdException("ID is not valid.");
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
            if (!Regex.IsMatch(phoneNumber, @"^0\d{9}$"))
                throw new InvalidPhoneNumberException("Phone number must start with 0 and have 10 digits.");
        }
    public static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidEmailException("Email cannot be empty.");
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new InvalidEmailException("Email is not valid.");
    }
    
    public static void ValidateAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return;  // No address provided, it's valid as null.

       
        var parts = address.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
            throw new AddressValidationException("Address must include Street, City, and Country");

        string street = parts[0].Trim();
        string city = parts[1].Trim();
        string country = parts[2].Trim();

        if (street.Length < 2 || city.Length < 2 || country.Length < 2)
            throw new AddressValidationException("Each part of the address must be at least 2 characters long");

       
        if (!GeocodingService.CheckAddressExists(address))
            throw new AddressValidationException("Address does not exist in reality");
    }



    public static void ValidateLatitudeLongitude(double? latitude, double? longitude)
        {
            if (latitude.HasValue && (latitude < -90 || latitude > 90))
                throw new InvalidCoordinatesException("Latitude must be between -90 and 90.");
            if (longitude.HasValue && (longitude < -180 || longitude > 180))
                throw new InvalidCoordinatesException("Longitude must be between -180 and 180.");
        }


        public static void ValidatePassword(string? password)
        {
            if (!string.IsNullOrEmpty(password) && password.Length < 6)
                throw new InvalidPasswordException("Password must have at least 6 characters.");
        }

        public static void ValidateMaxDistance(double? maxDistance)
        {
            if (maxDistance.HasValue && maxDistance <= 0)
                throw new InvalidMaxDistanceException("Max distance must be a positive number.");
        }

        public static void ValidateRole(VolunteerRole role)
        {
            if (!Enum.IsDefined(typeof(VolunteerRole), role))
                throw new InvalidRoleException("Role is not valid.");
        }

        public static void ValidateIsActive(bool isActive)
        {
            // אין צורך בבדיקה כי זה שדה בוליאני
        }

        // פונקציות עזר פשוטות:
        private static bool IsValidIsraeliId(int id)
        {
            // לוגיקה פשוטה לבדיקה, לדוגמה חישוב ספרת ביקורת.
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

        private static bool IsRealAddress(string address)
        {
            // סימולציה של בדיקה פשוטה. ניתן להרחיב עם ממשקי API.
            return address.Length > 5;
        }
}




public class GeoCoordinates
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}


public static class GeocodingService
{
    public static GeoCoordinates CallOpenStreetMapApi(string address)
    {
        try
        {
            var url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(address)}";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "C# App");

                // מבצע קריאה סינכרונית
                var response = client.GetStringAsync(url).Result;

                var jsonArray = JArray.Parse(response);

                if (jsonArray.Count > 0)
                {
                    var location = jsonArray[0];
                    return new GeoCoordinates
                    {
                        Latitude = (double)location["lat"],
                        Longitude = (double)location["lon"]
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get geolocation: {ex.Message}");
        }

        return null;
    }
    private static readonly HttpClient httpClient = new HttpClient();

    public static bool CheckAddressExists(string address)
    {
        try
        {
            var url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(address)}";

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "C# Application");

            // שליחה סינכרונית של הבקשה
            var response = httpClient.GetStringAsync(url).Result;

            var jsonArray = JArray.Parse(response);

            // אם יש תוצאה כלשהי, הכתובת קיימת
            return jsonArray.Count > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to check address existence: {ex.Message}");
            return false;
        }
    }



}


