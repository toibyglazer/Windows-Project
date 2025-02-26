using BlApi;
using BlImplementation;
using BO;
using DalApi;
using DO;
using System;
using System.Net.Mail;
using System.Net;
using static Helpers.Tools;
using System.Text.RegularExpressions;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Mime;
using System.Text;
using System.Runtime.InteropServices.Marshalling;
namespace Helpers;
/// <summary>
/// Helper class for managing Calls in the system.
/// This class provides helper methods for working with calls, including conversions between objects, distance calculations, and data validation.
/// Methods include: converting BO.Call to DO.Call, calculating distance between a call and a volunteer, and validating call data.
/// </summary>
internal static class CallManager
{
    private static IDal s_dal = DalApi.Factory.Get; //stage 4
    internal static ObserverManager Observers = new(); //stage 5 


    /// <summary>
    /// Converts a DO.Call object to a BO.Call object.
    /// </summary>
    public static BO.Call ConvertCallToBO(DO.Call doCall)
    {
        return new BO.Call
        {
            Id = doCall.Id,
            Type = (BO.CallType)doCall.Type, // Maps the CallType from DO to BO
            FullAddress = doCall.FullAddress,
            Latitude = doCall.Latitude,
            Longitude = doCall.Longitude,
            OpeningTime = doCall.OpeningTime,
            MaxCompletionTime = doCall.MaxCompletionTime,
            Description = doCall.Description,
            Status = CalculateCallStatus(doCall), // Calculates the current status of the call
            callAssignInList = GetCallAssignments(doCall.Id)?.ToList() // Retrieves assignments related to this call
        };
    }

    /// <summary>
    /// Retrieves all call assignments related to a specific call ID.
    /// </summary>
    public static IEnumerable<BO.CallAssignInList>? GetCallAssignments(int callId)
    {
        try
        {
            IEnumerable<DO.Assignment> allAssignments = s_dal.Assignment.ReadAll(); // Fetches all assignments
            var filteredAssignments = allAssignments.Where(assignment => assignment.CallId == callId); // Filters by callId

            if (!filteredAssignments.Any())
                return null; // No assignments found

            // Converts the filtered assignments to BO.CallAssignInList objects
            var callAssignList = filteredAssignments.Select(assignment => ConvertToCallAssignInList(assignment));

            return callAssignList;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while getting call assignments: {ex.Message}"); // Logs the error
            return null;
        }
    }

    /// <summary>
    /// Converts a DO.Assignment object to a BO.CallAssignInList object.
    /// </summary>
    private static BO.CallAssignInList ConvertToCallAssignInList(DO.Assignment assignment)
    {
        if (assignment.VolunteerId == 0)
        {
            return new BO.CallAssignInList
            {
                Id = 0,
                FullName = "",
                EntryTime = assignment.EntryTime,
                CompletionTime = assignment.CompletionTime != null ? assignment.CompletionTime : null,
                CompletionType = assignment.CompletionType != null ? (BO.AssignmentCompletionType)assignment.CompletionType : null,
            };
        }
        var volunteerFullName = s_dal.Volunteer.Read(assignment.VolunteerId)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {assignment.VolunteerId} does not exist."); // Ensures the volunteer exists

        return new BO.CallAssignInList
        {
            Id = assignment.VolunteerId,
            FullName = volunteerFullName.FullName,
            EntryTime = assignment.EntryTime,
            CompletionTime = assignment.CompletionTime != null ? assignment.CompletionTime : null,
            CompletionType = assignment.CompletionType != null ? (BO.AssignmentCompletionType)assignment.CompletionType : null,
        };
    }

    /// <summary>
    /// Calculates the status of a call based on its current data and associated assignments.
    /// </summary>
    public static BO.CallStatus CalculateCallStatus(DO.Call doCall)
    {
        DO.Assignment assignment = s_dal.Assignment.Read(a => a.CallId == doCall.Id); // Fetches the assignment for the call

        AdminImplementation admin = new AdminImplementation();
        var riskTimeRange = admin.GetRiskTimeRange(); // Defines the time range for considering a call "at risk"
        var systemClock = admin.GetSystemClock(); // Gets the current system time

        if (assignment == null) // Case 1: No assignment exists
        {
            if (doCall.MaxCompletionTime == null)
                return CallStatus.Open; // Call is always open if no MaxCompletionTime is defined

            if ((doCall.MaxCompletionTime - systemClock) <= riskTimeRange)
                return CallStatus.OpenAtRisk; // Call is open but at risk

            return CallStatus.Open; // Call is open
        }
        else
        {
            if (assignment.CompletionTime == null) // Case 2: Assignment exists but no CompletionTime
            {
                if (doCall.MaxCompletionTime != null &&
                    (doCall.MaxCompletionTime - systemClock) <= riskTimeRange)
                {
                    return CallStatus.InTreatmentAtRisk; // Call is in treatment but at risk
                }

                return CallStatus.InProgress; // Call is in progress
            }

            // Case 3: Assignment completed
            if (assignment.CompletionType == DO.AssignmentCompletionType.Treated
                || assignment.CompletionType == DO.AssignmentCompletionType.VolunteerCancelled
                || assignment.CompletionType == DO.AssignmentCompletionType.AdminCancelled)
            {
                return CallStatus.Closed; // Call is closed
            }

            // Case 4: Call expired
            if (doCall.MaxCompletionTime != null && systemClock > doCall.MaxCompletionTime)
            {
                return CallStatus.Expired; // Call expired
            }
            if (assignment.CompletionType == DO.AssignmentCompletionType.Expired)
            {
                return CallStatus.Expired; // Call explicitly marked as expired
            }
        }

        throw new InvalidOperationException("Unknown call status."); // Failsafe for unexpected cases
    }

    /// <summary>
    /// Periodically updates the statuses of all calls in the system.
    /// </summary>
    internal static void UpdateExpiredCalls()
    {
        IEnumerable<DO.Call> allCalls = s_dal.Call.ReadAll(); // Fetches all calls
        AdminImplementation admin = new AdminImplementation();
        var riskTimeRange = admin.GetRiskTimeRange();
        var systemClock = admin.GetSystemClock();

        foreach (var call in allCalls)
        {
            if (call.MaxCompletionTime == null || call.MaxCompletionTime.Value >= systemClock)
                continue; // Skip calls that are not expired

            BO.Call currentCall = ConvertCallToBO(call); // Convert the DO.Call to BO.Call
            if (currentCall.Status != CallStatus.Closed) // Process only non-closed calls
            {
                var assignments = s_dal.Assignment.ReadAll().Where(a => a.CallId == call.Id).ToList();
                if (assignments.Count == 0) // Case: No assignments exist
                {
                    var newAssignment = new DO.Assignment
                    {
                        CallId = call.Id,
                        VolunteerId = 0, // No volunteer assigned
                        EntryTime = systemClock,
                        CompletionTime = systemClock,
                        CompletionType = DO.AssignmentCompletionType.Expired // Mark the call as expired
                    };
                    s_dal.Assignment.Create(newAssignment);
                    VolunteerManager.Observers.NotifyListUpdated();//stage 5
                }
                else
                {
                    // Update the last active assignment with "Expired Cancellation"
                    var assignment = assignments.LastOrDefault(a => !a.CompletionTime.HasValue);
                    if (assignment != null)
                    {
                        var updatedAssignment = assignment with
                        {
                            CompletionTime = systemClock,
                            CompletionType = DO.AssignmentCompletionType.Expired
                        };

                        s_dal.Assignment.Update(updatedAssignment);
                        Observers.NotifyItemUpdated(updatedAssignment.Id); //stage 5
                        VolunteerManager.Observers.NotifyListUpdated();//stage 5
                    }
                }
            }
        }
    }





    /// <summary>
    /// Converts a DO.Call object to a BO.CallInList object, including assignment details.
    /// </summary>
    public static CallInList ConvertToCallInList(DO.Call call)
    {
        // Retrieve the most recent assignment for the call
        var lastAssignment = s_dal.Assignment
            .ReadAll(v => v.CallId == call.Id)
            .OrderByDescending(v => v.EntryTime)
            .FirstOrDefault();

        // If there is an assignment, populate the call-in list with relevant data
        if (lastAssignment != null && lastAssignment.VolunteerId != 0)
        {
            int lastVolunteerId = lastAssignment.VolunteerId;

            // Get the volunteer's name by ID
            string lastVolunteerName = s_dal.Volunteer.Read(lastVolunteerId).FullName
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {lastVolunteerId} does not exist.");

            // Create the CallInList object with assignment data
            var callInList = new CallInList
            {
                Id = lastAssignment.Id,
                CallId = lastAssignment.CallId,
                Type = (BO.CallType)call.Type, // Convert the call type
                OpeningTime = call.OpeningTime,
                RemainingTime = CalculateRemainingTime(call), // Calculate remaining time
                LastVolunteer = lastVolunteerName, // Last volunteer's name
                TotalCompletionTime = CalculateCompletionTime(lastAssignment, call), // Calculate completion time
                Status = CalculateCallStatus(call), // Calculate the call status
                TotalAssignments = GetAssignmentCount(call) // Get the total number of assignments for the call
            };
            return callInList;
        }
        else
        {
            // If no assignments exist, return a default CallInList object
            var callInList = new CallInList
            {
                Id = 0, // No assignment ID
                CallId = call.Id,
                Type = (BO.CallType)call.Type,
                OpeningTime = call.OpeningTime,
                RemainingTime = CalculateRemainingTime(call),
                LastVolunteer = null, // No volunteer data
                TotalCompletionTime = null,
                Status = CalculateCallStatus(call),
                TotalAssignments = 0
            };
            return callInList;
        }
    }

    /// <summary>
    /// Calculates the remaining time for a call, based on its MaxCompletionTime.
    /// </summary>
    public static TimeSpan? CalculateRemainingTime(DO.Call call)
    {
        if (call.MaxCompletionTime.HasValue)
        {
            var remaining = call.MaxCompletionTime.Value - DateTime.Now;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero; // ensures non-negative time
        }
        return null; // If MaxCompletionTime is null, return null
    }

    /// <summary>
    /// Calculates the completion time for a specific assignment, based on whether the completion type is 'Treated'.
    /// </summary>
    public static TimeSpan? CalculateCompletionTime(DO.Assignment assignment, DO.Call call)
    {
        if (assignment.CompletionType == DO.AssignmentCompletionType.Treated)
        {
            return assignment.CompletionTime.Value - call.OpeningTime;
        }
        return null;
    }

    /// <summary>
    /// Gets the count of assignments for a specific call.
    /// </summary>
    public static int GetAssignmentCount(DO.Call call)
    {

      return s_dal.Assignment
     .ReadAll(a => a.CallId == call.Id )
     .Count();


    }

    /// <summary>
    /// Sorts a list of CallInList objects based on the specified field.
    /// </summary>
    public static IOrderedEnumerable<CallInList> SortCalls(IEnumerable<CallInList> calls, CallInListField sortField)
    {
        return sortField switch
        {
            CallInListField.AssignmentId => calls.OrderBy(call => call.Id),
            CallInListField.CallId => calls.OrderBy(call => call.CallId),
            CallInListField.CallType => calls.OrderBy(call => call.Type),
            CallInListField.OpenTime => calls.OrderBy(call => call.OpeningTime),
            CallInListField.RemainingTime => calls.OrderBy(call => call.RemainingTime),
            CallInListField.LastVolunteerName => calls.OrderBy(call => call.LastVolunteer),
            CallInListField.CompletionTime => calls.OrderBy(call => call.TotalCompletionTime),
            CallInListField.Status => calls.OrderBy(call => call.Status),
            CallInListField.TotalAllocations => calls.OrderBy(call => call.TotalAssignments),
            _ => calls.OrderBy(call => call.CallId) // Default sorting by CallId
        };
    }

    /// <summary>
    /// Retrieves the value of a specific field from a CallInList object.
    /// </summary>
    public static object? GetValueForField(CallInList call, CallInListField field)
    {
        return field switch
        {
            CallInListField.AssignmentId => call.Id,
            CallInListField.CallId => call.CallId,
            CallInListField.CallType => call.Type,
            CallInListField.OpenTime => call.OpeningTime,
            CallInListField.RemainingTime => call.RemainingTime,
            CallInListField.LastVolunteerName => call.LastVolunteer,
            CallInListField.CompletionTime => call.TotalCompletionTime,
            CallInListField.Status => call.Status,
            CallInListField.TotalAllocations => call.TotalAssignments,
            _ => null
        };
    }




    /// <summary>
    /// Converts a BO.Call object to a DO.Call object, including geolocation data.
    /// </summary>
    public static DO.Call ConvertToDOCall(BO.Call call)
    {
        // Get geolocation (latitude and longitude) for the full address of the call
        GeoCoordinates geoCoordinates = Tools.CallOpenStreetMapApi(call.FullAddress);
        double longitude = geoCoordinates.Longitude;
        double latitude = geoCoordinates.Latitude;

        // Return a new DO.Call object with the relevant data
        return new DO.Call(
            Id: call.Id,
            Type: (DO.CallType)call.Type, // Convert the call type
            FullAddress: call.FullAddress,
            Latitude: latitude,
            Longitude: longitude,
            OpeningTime: DateTime.Now,
            MaxCompletionTime: call.MaxCompletionTime,
            Description: call.Description
        );
    }

    /// <summary>
    /// Validates the data of a BO.Call object, ensuring the latitude, longitude, call type, address, and max end time are valid.
    /// </summary>
    public static void ValidateCallData(BO.Call call)
    {
        try
        {
            //// Validate if latitude and longitude are present
            //if (call.Latitude == null || call.Longitude == null)
            //    throw new InvalidOperationException("Latitude and Longitude must not be null. This indicates an unexpected issue.");

            // Validate the call type string by trying to parse it
            TryParseAndValidateCallType(call.Type.ToString());

            // Validate the address to ensure it has proper format
            ValidateAddress(call.FullAddress);

            // Validate the max completion time to ensure it is later than opening time
            ValidateMaxEndTime(call.OpeningTime, call.MaxCompletionTime);
        }
        catch (Exception ex)
        {
            // Handle any validation exceptions
            throw HandleValidationException(ex);
        }
    }

    /// <summary>
    /// Handles validation exceptions and provides custom error messages based on the exception type.
    /// </summary>
    private static Exception HandleValidationException(Exception ex)
    {
        return ex switch
        {
            InvalidCallTypeException => new InvalidCallTypeException($"Call Type Validation Error: {ex.Message}"),
            AddressValidationException => new AddressValidationException($"Address Validation Error: {ex.Message}"),
            InvalidOperationException => new InvalidOperationException($"Max End Time Validation Error: {ex.Message}"),
            _ => new Exception($"Unexpected Error: {ex.Message}", ex),
        };
    }

    /// <summary>
    /// Tries to parse and validate the call type from its string representation.
    /// </summary>
    public static void TryParseAndValidateCallType(string callTypeString)
    {
        // Try to parse the call type from the string and throw an exception if invalid
        if (!Enum.TryParse(callTypeString, true, out DO.CallType callType))
            throw new InvalidCallTypeException($"Invalid call type string: {callTypeString}");
    }

    /// <summary>
    /// Validates the address to ensure it has a proper format (Street, City, Country) and exists.
    /// </summary>
    public static void ValidateAddress(string? address)
    {
        // If the address is null or empty, consider it valid
        if (string.IsNullOrWhiteSpace(address))
            throw new AddressValidationException("The address field is a required field.");

        // Split the address into components (Street, City, Country)
        var parts = address.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
            throw new AddressValidationException("Address must include Street, City, and Country");

        // Trim and validate the individual components of the address
        string street = parts[0].Trim();
        string city = parts[1].Trim();
        string country = parts[2].Trim();

        if (street.Length < 2 || city.Length < 2 || country.Length < 2)
            throw new AddressValidationException("Each part of the address must be at least 2 characters long");

        // Verify that the address exists in reality
        if (!Tools.VerifyAddressExists(address))
            throw new AddressValidationException("Address does not exist in reality");
    }

    /// <summary>
    /// Validates that the max end time is later than the opening time.
    /// </summary>
    public static void ValidateMaxEndTime(DateTime openTime, DateTime? maxEndTime)
    {
        // If maxEndTime is provided, ensure it is later than the opening time
        if (maxEndTime != null && maxEndTime <= openTime)
        {
            throw new InvalidOperationException("Max end time must be later than the open time.");
        }
    }

    /// <summary>
    /// Validates the call status string to ensure it is a valid status.
    /// </summary>
    public static void ValidateCallStatus(string statusString)
    {
        // Try to parse the status and throw an exception if it is invalid
        if (!Enum.TryParse(statusString, true, out CallStatus status))
        {
            throw new InvalidOperationException($"Invalid status string: {statusString}. Valid values are: Open, InProgress, Closed, Expired, OpenAtRisk, InTreatmentAtRisk.");
        }
    }





    /// <summary>
    /// Converts a BO.Call object and a DO.Volunteer object to an OpenCallInList object, including the calculated distance.
    /// </summary>
    public static OpenCallInList ConvertToOpenCallInList(BO.Call call, DO.Volunteer volunteer, DO.DistanceType distanceType)
    {
        // Calculate the distance between the volunteer and the call based on the specified distance type
        double distance = CalculateDistance(volunteer, call, distanceType);

        // Return a new OpenCallInList object with the relevant data
        return new OpenCallInList
        {
            Id = call.Id,
            Type = call.Type,
            Description = call.Description,
            FullAddress = call.FullAddress,
            OpeningTime = call.OpeningTime,
            MaxCompletionTime = call.MaxCompletionTime,
            DistanceToActiveCall = distance // Add the calculated distance to the active call
        };
    }

    /// <summary>
    /// Converts a DO.Assignment object to a ClosedCallInList object.
    /// Retrieves the associated call from the database and includes completion details.
    /// </summary>
    public static ClosedCallInList ConvertToClosedCallInList(DO.Assignment assignment)
    {
        // Read the call associated with the given assignment
        var newCall = s_dal.Call.Read(assignment.CallId)
            ?? throw new BO.BlDoesNotExistException($"Call with ID {assignment.CallId} does not exist.");

        // Return a new ClosedCallInList object with the relevant data
        return new ClosedCallInList
        {
            Id = newCall.Id,
            Type = (BO.CallType)newCall.Type, // Convert the call type to BO.CallType
            FullAddress = newCall.FullAddress,
            OpeningTime = newCall.OpeningTime,
            EntryTime = assignment.EntryTime, // Time when the assignment was created
            CompletionTime = assignment.CompletionTime, // Time when the call was completed
            CompletionType = (BO.AssignmentCompletionType)assignment.CompletionType // Convert the completion type
        };
    }

    /// <summary>
    /// Calculates the distance between a volunteer and a call based on the specified distance type (Aerial, Driving, Walking).
    /// </summary>
    public static double CalculateDistance(DO.Volunteer volunteer, BO.Call call, DO.DistanceType distanceType)
    {
        double distance = 0;

        // Ensure the volunteer has valid latitude and longitude
        double latitude = volunteer.Latitude ?? throw new InvalidOperationException("Latitude is null");
        double longitude = volunteer.Longitude ?? throw new InvalidOperationException("Longitude is null");

        // Calculate the distance based on the specified distance type
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
                // Throw an exception for unsupported distance types
                throw new ArgumentException("Unsupported distance type");
        }

        return distance;
    }

    /// <summary>
    /// Calculates the aerial (straight-line) distance between two geographic points using the Haversine formula.
    /// </summary>
    private static double CalculateAerialDistance(double? lat1, double? lon1, double lat2, double lon2)
    {
        const double EarthRadiusKm = 6371.0; // Radius of the Earth in kilometers

        // Convert the start latitude and longitude to radians
        double startLatitude = lat1.Value;
        double startLongitude = lon1.Value;

        double lat1Rad = DegreesToRadians(startLatitude);
        double lon1Rad = DegreesToRadians(startLongitude);
        double lat2Rad = DegreesToRadians(lat2);
        double lon2Rad = DegreesToRadians(lon2);

        // Calculate the change in latitude and longitude
        double dLat = lat2Rad - lat1Rad;
        double dLon = lon2Rad - lon1Rad;

        // Haversine formula to calculate the distance
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // Calculate the distance in kilometers
        double distance = EarthRadiusKm * c;

        return distance;
    }

    /// <summary>
    /// Converts degrees to radians, as required by the Haversine formula.
    /// </summary>
    private static double DegreesToRadians(double degrees)
    {
        return degrees * (Math.PI / 180.0);
    }


    /// <summary>
    /// Service for sending emails
    /// </summary>
    public static class EmailService
    {
        private const string SmtpHost = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SenderEmail = "toibymichalminiproject@gmail.com";
        private const string SenderPassword = "iwge idji gbtj lomm";
        public static void SendEmailWithAttachment(string recipientEmail, string subject, string body, string attachmentPath)
        {
            try
            {
                using var smtpClient = new SmtpClient(SmtpHost, SmtpPort)
                {
                    Credentials = new NetworkCredential(SenderEmail, SenderPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(SenderEmail,"Support Team"),
                    Subject = subject,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(recipientEmail);

                if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
                {
                    // צריבת התמונה למייל
                    var inlineImage = new LinkedResource(attachmentPath, "image/png")
                    {
                        ContentId = "MapImage"
                    };

                    // יצירת תוכן HTML עם תמונה מקושרת
                    var alternateView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                    alternateView.LinkedResources.Add(inlineImage);

                    mailMessage.AlternateViews.Add(alternateView);
                }

                // הוספת האייקון לאימייל
                var mapImageGenerator = new MapImageGenerator("", "", "", ""); // זה לא משנה איזה ערכים תעביר כאן
                string iconPath = mapImageGenerator.GetIconPath();
                if (iconPath != null)
                {
                    var iconResource = new LinkedResource(iconPath, "image/png")
                    {
                        ContentId = "MapIcon"
                    };

                    var alternateViewWithIcon = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                    alternateViewWithIcon.LinkedResources.Add(iconResource);
                    mailMessage.AlternateViews.Add(alternateViewWithIcon);
                }

                smtpClient.Send(mailMessage);
                Console.WriteLine($"Email sent successfully to {recipientEmail}");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"SMTP error while sending email to {recipientEmail}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error while sending email to {recipientEmail}: {ex.Message}");
            }
        }

        public static string FormatCallType(BO.CallType callType)
        {
            return Regex.Replace(callType.ToString(), "(?<!^)([A-Z])", " $1");
        }


        public static void SendEmailToVolunteersForNewCall(BO.Call call)
        {
            var volunteers = s_dal.Volunteer.ReadAll();

            foreach (var volunteer in volunteers)
            {
                if (volunteer.Latitude == null || volunteer.Longitude == null || volunteer.MaxDistance == null)
                {
                    continue;
                }

                double distance = CalculateDistance(volunteer, call, volunteer.DistanceType);
                if (distance <= volunteer.MaxDistance && !string.IsNullOrWhiteSpace(volunteer.Email))
                {
                    var mapGenerator = new MapImageGenerator(volunteer.Latitude.ToString(), volunteer.Longitude.ToString(),
                                                             call.Latitude.ToString(), call.Longitude.ToString());
                    string mapImagePath = mapGenerator.GenerateMapImage();

                    string subject = $"New call type: {FormatCallType(call.Type)} is now available in your locale";

                    string body = $@"
<html>
    <head>
        <style>
            body {{
                font-family: Arial, sans-serif;
                line-height: 1.6;
                color: #333;
            }}
            .email-container {{
                width: 100%;
                max-width: 650px; /* הגדלת רוחב הקונטיינר */
                margin: 0 auto;
                padding: 20px;
                background-color: #f9f9f9;
                border-radius: 8px;
                border: 1px solid #ddd;
            }}
            h2 {{
                color: #007BFF;
display: flex;
    align-items: center;
            }}
            p {{
                font-size: 16px;
            }}
            .highlight {{
                font-weight: bold;
                color: #555;
            }}
            .icon {{
                width: 24px;
                 height:auto;
                vertical-align: text-bottom;
                margin-right: 10px;
            }}
           .map-image {{width: 100%; /* מאפשר גמישות בגודל */
    max-width: 400px; /* מגבלת גודל סבירה */
    height: auto;
    margin-top: 10px; /* הפחתת המרווח */
}}
.call-type-highlight {{color: blue;
  
  
  
  
}}
        </style>
    </head>
    <body>
        <div class='email-container'>
        <h2 style='text-align: center; max-width: 90%; word-break: break-word;'>
 <span>
  <img class='icon' src='cid:MapIcon' alt='Map Icon' style='display: inline; vertical-align: text-bottom; margin-right: 10px;' />
  New call of type <span class='call-type-highlight'>{FormatCallType(call.Type)}</span> has been opened in your locale
</span>

</h2>


            <p><span class='highlight'>Exact Address:</span> {call.FullAddress}</p>

         <!-- תמונה מיד אחרי הכתובת -->
         <div style='text-align: center; margin-top: 5px;'>
            <h3>Here's the location map for the call:</h3>
            <img src='cid:MapImage' alt='Call Location Map' class='map-image' /> 
         </div>";

                    if (!string.IsNullOrWhiteSpace(call.Description))
                    {
                        body += $@"
        <p><span class='highlight'>Description:</span> {call.Description}</p>";
                    }

                    body += $@"
        <p><span class='highlight'>Opening Time:</span> {call.OpeningTime}</p>";

                    if (call.MaxCompletionTime.HasValue)
                    {
                        body += $@"
        <p><span class='highlight'>Max Completion Time:</span> {call.MaxCompletionTime.Value.ToString("yyyy-MM-dd HH:mm")}</p>";
                    }

                    body += $@"
        </div>
    </body>
</html>";



                    SendEmailWithAttachment(volunteer.Email, subject, body, mapImagePath);
                }
            }
        }

        public static void SendCancellationEmailToVolunteer(int assignmentId)
        {
            DO.Assignment assignment = s_dal.Assignment.Read(assignmentId)
                 ?? throw new BO.BlDoesNotExistException($"Assignment with ID {assignmentId} does not exist.");

            DO.Call call = s_dal.Call.Read(assignment.CallId)
                ?? throw new BO.BlDoesNotExistException($"Call with ID {assignment.CallId} does not exist.");

            DO.Volunteer volunteer = s_dal.Volunteer.Read(assignment.VolunteerId)
                 ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {assignment.VolunteerId} does not exist.");

            BO.Call callDetailsEmail = ConvertCallToBO(call);

            if (string.IsNullOrWhiteSpace(volunteer.Email))
            {
                Console.WriteLine("The volunteer does not have a current email in the system.");
                return;
            }

            string subject = $"Important Update: Your {FormatCallType(callDetailsEmail.Type)} Call has been Cancelled";

            string body = $@"
<html>
    <head>
        <style>
            body {{
                font-family: Arial, sans-serif;
                line-height: 1.6;
                color: #333;
                background-color: #f9f9f9;
                padding: 20px;
            }}
            .email-container {{
                max-width: 610px;
                margin: 0 auto;
                padding: 20px;
                background-color: #f9f9f9;
                border-radius: 8px;
                border: 1px solid #ddd;
            }}
            h2 {{
                color: #d9534f;
                
            }}
            p {{
                font-size: 16px;
                color: #333;
                margin: 8px 0;
            }}
            .highlight {{
                font-weight: bold;
                color: #555;
                margin-right: 4px;
            }}
            .details {{
                margin: 4px 0;
                font-size: 15px;
            }}
            .icon {{
                width: 30px;
                height: auto;
                vertical-align: text-bottom;
                margin-right: 10px;
            }}
        </style>
    </head>
    <body>
        <div class='email-container'>
            <h2 style='text-align: center; max-width: 90%; word-break: break-word;'>
                <span>
                    <img class='icon' src='cid:WarningIcon' alt='Warning icon' style='display: inline; vertical-align: text-bottom; margin-right: 10px;' />
                    Call Manager Cancellation Message
                </span>
            </h2>
        
        <p style='font-size: 16px; color: #555;'>
            <span class='highlight'>Dear Volunteer,</span> 
            Please be informed that the call you were assigned to has been 
            <span class='highlight' style='color: #d9534f;'>cancelled</span> by the manager.
        </p>
        <br>

        <p class='details'><span class='highlight'>Call Type:</span> {FormatCallType(callDetailsEmail.Type)}</p>
        <p class='details'><span class='highlight'>Full Address:</span> {callDetailsEmail.FullAddress}</p>";

            if (!string.IsNullOrWhiteSpace(callDetailsEmail.Description))
            {
                body += $@"
            <p class='details'><span class='highlight'>Description:</span> {callDetailsEmail.Description}</p>";
            }

            body += $@"
            <p class='details'><span class='highlight'>Call Status:</span> {FormatCallStatus(callDetailsEmail.Status)}</p>

          <p class='highlight' style='font-size: 16px; margin-top: 16px;'>
            We apologize for any inconvenience caused. Thank you for your service.
        </p>
</div>
    </body>
</html>";

            // שליחת המייל למתנדב
            SendEmailWithImage(volunteer.Email, subject, body);
        }
        public static string FormatCallStatus(BO.CallStatus status)
        {
            // המרה למחרוזת
            string statusString = status.ToString();

            // הוספת רווח לפני כל אות גדולה שאינה בתחילת המילה
            return Regex.Replace(statusString, "(?<!^)([A-Z])", " $1");
        }

        public static void SendEmailWithImage(string recipientEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                Console.WriteLine("Invalid recipient email.");
                return;
            }

            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(SenderEmail, "Support Team");
                    mail.To.Add(recipientEmail);
                    mail.Subject = subject;
                    mail.IsBodyHtml = true;

                    // שימוש בנתיב יחסי ליצירת התמונה
                    string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Helpers", "Images");
                    string imagePath = Path.Combine(directoryPath, "cancel-close-10374.png");

                    if (!File.Exists(imagePath))
                    {
                        Console.WriteLine("Image file not found.");
                        return;
                    }

                    var linkedResource = new LinkedResource(imagePath)
                    {
                        ContentId = "WarningIcon",
                        ContentType = new ContentType(MediaTypeNames.Image.Png)
                    };

                    var altView = AlternateView.CreateAlternateViewFromString(body, Encoding.UTF8, MediaTypeNames.Text.Html);
                    altView.LinkedResources.Add(linkedResource);
                    mail.AlternateViews.Add(altView);

                    using (SmtpClient smtp = new SmtpClient(SmtpHost, SmtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(SenderEmail, SenderPassword);
                        smtp.EnableSsl = true;

                        smtp.Send(mail);
                        Console.WriteLine($"Email successfully sent to {recipientEmail}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }


    }
}











