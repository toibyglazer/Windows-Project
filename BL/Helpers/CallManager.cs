using Bl;
using BlApi;
using BlImplementation;
using BO;
using DalApi;
using DO;
using System;

namespace Helpers;

internal static class CallManager
{
    private static IDal s_dal = DalApi.Factory.Get; //stage 4

    public static BO.Call ConvertCallToBO(DO.Call doCall)
    {
        return new BO.Call
        {
            Id = doCall.Id,
            Type = (BO.CallType)doCall.Type,
            FullAddress = doCall.FullAddress,
            Latitude = doCall.Latitude,
            Longitude = doCall.Longitude,
            OpeningTime = doCall.OpeningTime,
            MaxCompletionTime = doCall.MaxCompletionTime,
            Description = doCall.Description,
            Status = CalculateCallStatus(doCall),
            callAssignInList = GetCallAssignments(doCall.Id)?.ToList()
        };
    }
    public static IEnumerable<BO.CallAssignInList> GetCallAssignments(int callId)
    {
        try
        {
            IEnumerable<DO.Assignment> allAssignments = s_dal.Assignment.ReadAll();

            var filteredAssignments = allAssignments
                .Where(assignment => assignment.CallId == callId);

            if (!filteredAssignments.Any())
                return null;

            var callAssignList = filteredAssignments
                .Select(assignment => ConvertToCallAssignInList(assignment));

            return callAssignList;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while getting call assignments: {ex.Message}");
            return null;
        }
    }
    private static BO.CallAssignInList ConvertToCallAssignInList(DO.Assignment assignment)
    {
        var volunteerFullName = s_dal.Volunteer.Read(assignment.VolunteerId)
            ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {assignment.VolunteerId} does not exist.");

        return new BO.CallAssignInList
        {
            Id = assignment.Id,
            FullName = volunteerFullName.FullName,
            EntryTime = assignment.EntryTime,
            CompletionTime = assignment.CompletionTime,
            CompletionType = (BO.AssignmentCompletionType)assignment.CompletionType,
        };
    }
    private static BO.CallStatus CalculateCallStatus(DO.Call doCall)
    {
        DO.Assignment assignment = s_dal.Assignment.Read(doCall.Id)
            ?? throw new BO.BlDoesNotExistException($"Call with ID {doCall.Id} does not exist.");

        AdminImplementation admin = new AdminImplementation();
        var riskTimeRange = admin.GetRiskTimeRange();
        var systemClock = admin.GetSystemClock();

        if (assignment == null)
        {
            if ((doCall.MaxCompletionTime - systemClock) <= riskTimeRange)
                return CallStatus.OpenAtRisk;
            return CallStatus.Open;
        }
        if (assignment.CompletionTime == null)
        {
            if ((doCall.MaxCompletionTime - systemClock) <= riskTimeRange)
                return CallStatus.InTreatmentAtRisk;
            return CallStatus.InProgress;
        }
        if (assignment.CompletionType == DO.AssignmentCompletionType.Treated
            || assignment.CompletionType == DO.AssignmentCompletionType.VolunteerCancelled
            || assignment.CompletionType == DO.AssignmentCompletionType.AdminCancelled)
            return CallStatus.Closed;
        if (systemClock > doCall.MaxCompletionTime)
        {
            return CallStatus.Expired;
        }

        throw new InvalidOperationException("Unknown call status.");
    }
    internal static void PeriodicCallUpdates()
    {
        IEnumerable<DO.Call> allCalls = s_dal.Call.ReadAll();
        AdminImplementation admin = new AdminImplementation();
        var riskTimeRange = admin.GetRiskTimeRange();
        var systemClock = admin.GetSystemClock();

        foreach (var call in allCalls)
        {
            if (call.MaxCompletionTime == null || call.MaxCompletionTime.Value >= systemClock)
                continue;

            BO.Call currentCall = ConvertCallToBO(call);
            if (currentCall.Status != CallStatus.Closed)
            {
                var assignments = s_dal.Assignment.ReadAll().Where(a => a.CallId == call.Id).ToList();
                if (assignments.Count == 0)
                {
                    var newAssignment = new DO.Assignment
                    {
                        CallId = call.Id,
                        VolunteerId = 0,
                        EntryTime = systemClock,
                        CompletionTime = systemClock,
                        CompletionType = DO.AssignmentCompletionType.Expired
                    };
                    s_dal.Assignment.Create(newAssignment);
                }
                else
                {
                    // Update existing assignment with "Expired Cancellation"
                    var assignment = assignments.LastOrDefault(a => !a.CompletionTime.HasValue);
                    if (assignment != null)
                    {
                        var updatedAssignment = assignment with
                        {
                            CompletionTime = systemClock,
                            CompletionType = DO.AssignmentCompletionType.Expired
                        };

                        s_dal.Assignment.Update(updatedAssignment);
                    }
                }
            }

        }





    }
    public static TimeSpan? CalculateRemainingTime(DO.Call call)
    {
        if (call.MaxCompletionTime.HasValue)
        {
            var remaining = call.MaxCompletionTime.Value - DateTime.Now;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero; // ensures non-negative time
        }
        return null; // If MaxCompletionTime is null, return null
    }


    public static TimeSpan? CalculateCompletionTime(DO.Call call)
    {
        if (call.MaxCompletionTime.HasValue)
        {
            return call.MaxCompletionTime.Value - call.OpeningTime;
        }
        return null;
    }
    public static int GetAssignmentCount(DO.Call call)
    {
        return s_dal.Assignment.ReadAll(a => a.CallId == call.Id).Count();
    }
    public static CallInList ConvertToCallInList(DO.Call call)
    {
        var assignment = s_dal.Assignment.ReadAll().FirstOrDefault(v => v.CallId == call.Id);
        var BoCall = ConvertCallToBO(call);

        if (assignment != null)
        {
            var lastAssignment = s_dal.Assignment
                .ReadAll(v => v.CallId == call.Id)
                .OrderByDescending(v => v.Id)
                .FirstOrDefault();

            int lastVolunteerId = lastAssignment != null ? lastAssignment.VolunteerId : -1;

            string lastVolunteerName = s_dal.Volunteer.Read(lastVolunteerId).FullName;
            var callInList = new CallInList
            {
                Id = assignment.Id,
                CallId = assignment.CallId,
                Type = (BO.CallType)call.Type,
                OpeningTime = call.OpeningTime,
                RemainingTime = CalculateRemainingTime(call),
                LastVolunteer = lastVolunteerName,
                TotalCompletionTime = CalculateCompletionTime(call),
                Status = BoCall.Status,
                TotalAssignments = GetAssignmentCount(call)
            };
            return callInList;
        }
        else
        {
            var callInList = new CallInList
            {
                Id = 0,
                CallId = call.Id,
                Type = (BO.CallType)call.Type,
                OpeningTime = call.OpeningTime,
                RemainingTime = CalculateRemainingTime(call),
                LastVolunteer = null,
                TotalCompletionTime = CalculateCompletionTime(call),
                Status = BoCall.Status,
                TotalAssignments = 0
            };
            return callInList;
        }
       
    }
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
            _ => calls.OrderBy(call => call.CallId) // מיון ברירת מחדל
        };
    }
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




    public static DO.Call ConvertToDOCall(BO.Call call)
    {
        GeoCoordinates geoCoordinates = Tools.CallOpenStreetMapApi(call.FullAddress);
        double longitude = geoCoordinates.Longitude;
        double latitude = geoCoordinates.Latitude;

        return new DO.Call(
              Id: call.Id,
              Type: (DO.CallType)call.Type,
              FullAddress: call.FullAddress,
              Latitude: latitude,
              Longitude: longitude,
              OpeningTime: call.OpeningTime,
              MaxCompletionTime: call.MaxCompletionTime,
              Description: call.Description
          );
    }
    public static void ValidateCallData(BO.Call call)
    {
        try
        {
            if (call.Latitude == null || call.Longitude == null)
                throw new InvalidOperationException("Latitude and Longitude must not be null. This indicates an unexpected issue.");

            TryParseAndValidateCallType(call.Type.ToString());  
            ValidateAddress(call.FullAddress);                          
            ValidateMaxEndTime(call.OpeningTime, call.MaxCompletionTime);    
                     
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
            InvalidCallTypeException => new InvalidCallTypeException($"Call Type Validation Error: {ex.Message}"),
            AddressValidationException => new AddressValidationException($"Address Validation Error: {ex.Message}"),
            InvalidOperationException => new InvalidOperationException($"Max End Time Validation Error: {ex.Message}"),
            _ => new Exception($"Unexpected Error: {ex.Message}", ex),
        };
    }
    public static void TryParseAndValidateCallType(string callTypeString)
    {
        if (!Enum.TryParse(callTypeString, true, out DO.CallType callType))
            throw new InvalidCallTypeException($"Invalid call type string: {callTypeString}");
    }
    public static void ValidateAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return;  // No address provided, it's valid as null or empty.

        var parts = address.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
            throw new AddressValidationException("Address must include Street, City, and Country");

        string street = parts[0].Trim();
        string city = parts[1].Trim();
        string country = parts[2].Trim();

        if (street.Length < 2 || city.Length < 2 || country.Length < 2)
            throw new AddressValidationException("Each part of the address must be at least 2 characters long");

        if (!GeocodingService.VerifyAddressExists(address))
            throw new AddressValidationException("Address does not exist in reality");
    }
    public static void ValidateMaxEndTime(DateTime openTime, DateTime? maxEndTime)
    {
        if (maxEndTime <= DateTime.Now)
        {
            throw new InvalidOperationException("Max end time must be later than the current time.");
        }
        if (maxEndTime <= openTime)
        {
            throw new InvalidOperationException("Max end time must be later than the open time.");
        }
    }
    public static void ValidateCallStatus(string statusString)
    {
        // מנסה להמיר את המחרוזת לסטטוס מתוך האינאמ CallStatus
        if (!Enum.TryParse(statusString, true, out CallStatus status))
        {
            throw new InvalidOperationException($"Invalid status string: {statusString}. Valid values are: Open, InProgress, Closed, Expired, OpenAtRisk, InTreatmentAtRisk.");
        }
    }




    public static OpenCallInList ConvertToOpenCallInList(BO.Call call, DO.Volunteer volunteer, DO.DistanceType distanceType)
    {
        double distance = CalculateDistance(volunteer, call, distanceType);

        return new OpenCallInList
        {
            Id = call.Id,
            Type = call.Type,
            Description = call.Description,
            FullAddress = call.FullAddress,
            OpeningTime = call.OpeningTime,
            MaxCompletionTime = call.MaxCompletionTime,
            DistanceToActiveCall = distance
        };
    }
    public static ClosedCallInList ConvertToClosedCallInList(DO.Assignment assignment)
    {
        var newCall= s_dal.Call.Read(assignment.CallId)
            ?? throw new BO.BlDoesNotExistException($"Call with ID {assignment.CallId} does not exist.");

        return new ClosedCallInList
        {
            Id = assignment.CallId,
            Type = (BO.CallType)newCall.Type,
            FullAddress = newCall.FullAddress,
            OpeningTime = newCall.OpeningTime,
            EntryTime = assignment.EntryTime,
            CompletionTime = assignment.CompletionTime,
            CompletionType = (BO.AssignmentCompletionType)assignment.CompletionType
        };
    }
    public static double CalculateDistance(DO.Volunteer volunteer, BO.Call call, DO.DistanceType distanceType)
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



}








