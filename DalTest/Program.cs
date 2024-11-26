using Dal;
using DalApi;
using DO;


namespace DalTest
{
    public enum MainMenuOptions
    {
        ManageVolunteer = 1,
        ManageCall,
        ManageAssignment,
        ManageConfig,
        InitializeDatabase,
        ResetDatabase,
        DisplayAllData, 
        Exit = 0
    }
    public enum VolunteerMenuOption
    {
        AddVolunteer = 1,
        ViewVolunteerById = 2,
        ViewAllVolunteers = 3,
        UpdateVolunteer = 4,
        DeleteVolunteer = 5,
        DeleteAllVolunteers = 6,
        Exit = 0
    }
    public enum CallMenuOptions
    {
        AddCall = 1,
        ViewCallById = 2,
        ViewAllCalls = 3,
        UpdateCall = 4,
        DeleteCall = 5,
        DeleteAllCalls = 6,
        Exit = 0
    }
    public enum AssignmentMenuOption
    {
        Exit= 0,
        AddAssignment = 1,
        ViewAssignmentById = 2,
        ViewAllAssignments = 3,
        UpdateAssignment = 4,
        DeleteAssignment = 5,
        DeleteAllAssignments = 6
    }
    public enum ConfigMenuOption
    {
        Exit = 0,
        AdvanceByMinute = 1,
        AdvanceByHour = 2,
        AdvanceByDay = 3,
        AdvanceByWeek = 4,
        AdvanceByMonth = 5,
        AdvanceByYear = 6,
        ShowCurrentClock = 7,
        SetConfigValue = 8,
        ShowConfigValue = 9,
        ResetConfig = 10
    }

    internal class Program
    {
        private static IVolunteer? s_dalVolunteer = new VolunteerImplementation();
        private static ICall? s_dalCall = new CallImplementation();
        private static IAssignment? s_dalAssignment = new AssignmentImplementation();
        private static IConfig? s_dalConfig = new ConfigImplementation();


        static void Main(string[] args)
        {
            try
            {
                bool exit = false;
                while (!exit)
                {
                    DisplayMainMenu();
                    exit = HandleUserInput();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex}");
            }
        }
        static bool HandleUserInput()
        {
            try
            {
                MainMenuOptions? selectedOption = GetUserChoice();

                if (selectedOption.HasValue)
                {
                    switch (selectedOption)
                    {
                        case MainMenuOptions.ManageVolunteer:
                            ManageVolunteer();
                            break;

                        case MainMenuOptions.ManageCall:
                            ManageCall();
                            break;

                        case MainMenuOptions.ManageAssignment:
                            ManageAssignment();
                            break;

                        case MainMenuOptions.ManageConfig:
                            ManageConfig();
                            break;

                        case MainMenuOptions.InitializeDatabase:
                            InitializeDatabase();
                            break;

                        case MainMenuOptions.ResetDatabase:
                            ResetDatabase();
                            break;
                        case MainMenuOptions.DisplayAllData:
                            DisplayAllData();
                            break;
                        case MainMenuOptions.Exit:
                            return true; // Exit the loop in Main
                        default:
                            Console.WriteLine("Invalid choice, please try again.");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling input: {ex.Message}");
            }
            return false; // Continue the loop
        }
        static MainMenuOptions? GetUserChoice()
        {
            Console.Write("Enter your choice: ");
            string? input = Console.ReadLine();

            if (Enum.TryParse(input, out MainMenuOptions selectedOption))
            {
                return selectedOption;
            }
            else
            {
                Console.WriteLine("Invalid input, please enter a valid number.");
                return null; // החזרה של ערך ריק אם הקלט לא תקין
            }
        }


        static void ManageVolunteer()
        {
            VolunteerMenuOption choice;

            while (true)
            {
                ManageVolunteersMenu();
                Console.Write("Enter your choice: ");
                string? input = Console.ReadLine();

                // Attempt to parse the input to the corresponding enum value
                if (!Enum.TryParse(input, out choice) || !Enum.IsDefined(typeof(VolunteerMenuOption), choice))
                {
                    Console.WriteLine("Invalid input. Please enter a valid option from the menu.");
                    continue;
                }

                switch (choice)
                {
                    case VolunteerMenuOption.AddVolunteer:
                        AddVolunteer();
                        break;

                    case VolunteerMenuOption.ViewVolunteerById:
                        ViewVolunteerById();
                        break;

                    case VolunteerMenuOption.ViewAllVolunteers:
                        ViewAllVolunteers();
                        break;

                    case VolunteerMenuOption.UpdateVolunteer:
                        UpdateVolunteer();
                        break;

                    case VolunteerMenuOption.DeleteVolunteer:
                        DeleteVolunteer();
                        break;

                    case VolunteerMenuOption.DeleteAllVolunteers:
                        DeleteAllVolunteers();
                        break;

                    case VolunteerMenuOption.Exit:
                        Console.WriteLine("Returning to the main menu...");
                        return;

                    default:
                        Console.WriteLine("Invalid choice, please try again.");
                        break;
                }
            }
        }
        private static void AddVolunteer()
        {
            try
            {

                Console.WriteLine("Enter Volunteer Details:");
                Console.Write("ID: ");
                int id = int.Parse(Console.ReadLine() ?? throw new InvalidOperationException("ID is required."));

                Console.Write("Full Name: ");
                string fullName = Console.ReadLine() ?? string.Empty;

                Console.Write("Phone Number: ");
                string phoneNumber = Console.ReadLine() ?? string.Empty;

                Console.Write("Email: ");
                string email = Console.ReadLine() ?? string.Empty;

                Console.Write("Password (optional): ");
                string? password = Console.ReadLine();

                Console.Write("Current Address (optional): ");
                string? currentAddress = Console.ReadLine();

                Console.Write("Latitude (optional): ");
                double? latitude = double.TryParse(Console.ReadLine(), out var lat) ? lat : null;

                Console.Write("Longitude (optional): ");
                double? longitude = double.TryParse(Console.ReadLine(), out var lon) ? lon : null;

                Console.Write("Role (1 for Manager, 2 for Volunteer): ");
                VolunteerRole role = Enum.TryParse(Console.ReadLine(), out VolunteerRole parsedRole) ? parsedRole : VolunteerRole.Volunteer;

                Console.Write("Is Active (true/false): ");
                bool isActive = bool.TryParse(Console.ReadLine(), out var activeStatus) && activeStatus;

                Console.Write("Max Distance (optional): ");
                double? maxDistance = double.TryParse(Console.ReadLine(), out var dist) ? dist : null;

                Console.Write("Distance Type (1 for Aerial, 2 for Walking, 3 for Driving): ");
                DistanceType distanceType = Enum.TryParse(Console.ReadLine(), out DistanceType parsedDistance) ? parsedDistance : DistanceType.Aerial;


                Volunteer volunteer = new Volunteer(
                    Id: id,
                    FullName: fullName,
                    PhoneNumber: phoneNumber,
                    Email: email,
                    Password: password,
                    CurrentAddress: currentAddress,
                    Latitude: latitude,
                    Longitude: longitude,
                    Role: role,
                    IsActive: isActive,
                    MaxDistance: maxDistance,
                    DistanceType: distanceType
                );

                s_dalVolunteer?.Create(volunteer);
                Console.WriteLine("Volunteer added successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        public static void ViewVolunteerById()
        {
            Console.WriteLine("Please enter the Volunteer ID:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid input. Please enter a valid numeric ID.");
            }
            try
            {
                var volunteer = s_dalVolunteer?.Read(id);
                if (volunteer != null)
                {
                    Console.WriteLine($"Volunteer ID: {volunteer.Id}");
                    Console.WriteLine($"Full Name: {volunteer.FullName}");
                    Console.WriteLine($"Phone Number: {volunteer.PhoneNumber}");
                    Console.WriteLine($"Email: {volunteer.Email}");
                    Console.WriteLine($"Password: {(string.IsNullOrEmpty(volunteer.Password) ? "N/A" : volunteer.Password)}");
                    Console.WriteLine($"Current Address: {(string.IsNullOrEmpty(volunteer.CurrentAddress) ? "N/A" : volunteer.CurrentAddress)}");
                    Console.WriteLine($"Latitude: {(volunteer.Latitude.HasValue ? volunteer.Latitude.Value.ToString() : "N/A")}");
                    Console.WriteLine($"Longitude: {(volunteer.Longitude.HasValue ? volunteer.Longitude.Value.ToString() : "N/A")}");
                    Console.WriteLine($"Role: {volunteer.Role}");
                    Console.WriteLine($"Active Status: {(volunteer.IsActive ? "Active" : "Inactive")}");
                    Console.WriteLine($"Max Distance: {(volunteer.MaxDistance.HasValue ? volunteer.MaxDistance.Value.ToString() : "N/A")}");
                    Console.WriteLine($"Distance Type: {volunteer.DistanceType}");
                }

               
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving the volunteer: {ex.Message}");
            }
        }
        public static void ViewAllVolunteers()
        {
            var volunteers = s_dalVolunteer?.ReadAll();

            if (volunteers == null || volunteers.Count == 0)
            {
                Console.WriteLine("No volunteers found.");
                return;
            }

            foreach (var volunteer in volunteers)
            {
                Console.WriteLine($"ID: {volunteer.Id}, Name: {volunteer.FullName}, Phone: {volunteer.PhoneNumber}, Email: {volunteer.Email}, Role: {volunteer.Role}, Active: {volunteer.IsActive}");
            }
        }
        public static void UpdateVolunteer()
        {

            Console.WriteLine("Please enter the Volunteer ID:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID. Please enter a valid integer.");
                return;
            }

            var volunteer = s_dalVolunteer?.Read(id);
            if (volunteer == null)
            {
                Console.WriteLine($"No volunteer found with ID {id}.");
                return;
            }
            Console.WriteLine("Current Volunteer details:");
            Console.WriteLine($@"
ID: {volunteer.Id}
Full Name: {volunteer.FullName}
Phone Number: {volunteer.PhoneNumber}
Email: {volunteer.Email}
Password: {volunteer.Password ?? "N/A"}
Current Address: {volunteer.CurrentAddress ?? "N/A"}
Latitude: {volunteer.Latitude?.ToString() ?? "N/A"}
Longitude: {volunteer.Longitude?.ToString() ?? "N/A"}
Role: {volunteer.Role}
Active: {volunteer.IsActive}
Max Distance: {volunteer.MaxDistance?.ToString() ?? "N/A"}
Distance Type: {volunteer.DistanceType}
");
            Console.WriteLine("Enter new details (press Enter to keep the current value):");


            Console.Write("Full Name (current: " + volunteer.FullName + "): ");
            string? fullName = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                volunteer = volunteer with { FullName = fullName };
            }


            Console.Write("Phone Number (current: " + volunteer.PhoneNumber + "): ");
            string? phoneNumber = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                volunteer = volunteer with { PhoneNumber = phoneNumber };
            }


            Console.Write("Email (current: " + volunteer.Email + "): ");
            string? email = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(email))
            {
                volunteer = volunteer with { Email = email };
            }

            Console.Write("Password (current: " + (volunteer.Password ?? "N/A") + "): ");
            string? password = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(password))
            {
                volunteer = volunteer with { Password = password };
            }

            Console.Write("Current Address (current: " + (volunteer.CurrentAddress ?? "N/A") + "): ");
            string? currentAddress = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(currentAddress))
            {
                volunteer = volunteer with { CurrentAddress = currentAddress };
            }

            Console.Write("Latitude (current: " + (volunteer.Latitude?.ToString() ?? "N/A") + "): ");
            string? latitudeInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(latitudeInput) && double.TryParse(latitudeInput, out double newLatitude))
            {
                volunteer = volunteer with { Latitude = newLatitude };
            }

            Console.Write("Longitude (current: " + (volunteer.Longitude?.ToString() ?? "N/A") + "): ");
            string? longitudeInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(longitudeInput) && double.TryParse(longitudeInput, out double newLongitude))
            {
                volunteer = volunteer with { Longitude = newLongitude };
            }

            Console.Write("Role (Manager/Volunteer) (current: " + volunteer.Role + "): ");
            string? role = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(role) && Enum.TryParse(role, out VolunteerRole newRole))
            {
                volunteer = volunteer with { Role = newRole };
            }

            Console.Write("Is Active (true/false) (current: " + volunteer.IsActive + "): ");
            string? isActive = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(isActive) && bool.TryParse(isActive, out bool newIsActive))
            {
                volunteer = volunteer with { IsActive = newIsActive };
            }

            Console.Write("Max Distance (current: " + (volunteer.MaxDistance?.ToString() ?? "N/A") + "): ");
            string? maxDistanceInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(maxDistanceInput) && double.TryParse(maxDistanceInput, out double newMaxDistance))
            {
                volunteer = volunteer with { MaxDistance = newMaxDistance };
            }

            Console.Write("Distance Type (Aerial/Ground) (current: " + volunteer.DistanceType + "): ");
            string? distanceTypeInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(distanceTypeInput) && Enum.TryParse(distanceTypeInput, out DistanceType newDistanceType))
            {
                volunteer = volunteer with { DistanceType = newDistanceType };
            }

            try
            {
                s_dalVolunteer.Update(volunteer);
                Console.WriteLine("Volunteer details updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update volunteer details: {ex.Message}");
            }
        }
        public static void DeleteVolunteer()
        {

            Console.WriteLine("Please enter the Volunteer ID to delete:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid input. Please enter a valid numeric ID.");
                return;
            }
            try
            {
                s_dalVolunteer?.Delete(id);
                Console.WriteLine($"Volunteer with ID {id} was successfully deleted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not delete volunteer: {ex.Message}");
            }
        }
        public static void DeleteAllVolunteers()
        {
            try
            {
                s_dalVolunteer?.DeleteAll();
                Console.WriteLine("All volunteers have been successfully deleted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting all volunteers: {ex.Message}");
            }
        }



        static void ManageCall()
        {
            CallMenuOptions choice;

            while (true)
            {
                ManageCallsMenu();

                Console.Write("Enter your choice: ");
                if (!Enum.TryParse(Console.ReadLine(), out choice) || !Enum.IsDefined(typeof(CallMenuOptions), choice))
                {
                    Console.WriteLine("Invalid input. Please enter a valid option.");
                    continue;
                }

                switch (choice)
                {
                    case CallMenuOptions.AddCall:
                        AddCall();
                        break;
                    case CallMenuOptions.ViewCallById:
                        ViewCallById();
                        break;
                    case CallMenuOptions.ViewAllCalls:
                        ViewAllCalls();
                        break;
                    case CallMenuOptions.UpdateCall:
                        UpdateCall();
                        break;
                    case CallMenuOptions.DeleteCall:
                        DeleteCall();
                        break;
                    case CallMenuOptions.DeleteAllCalls:
                        DeleteAllCalls();
                        break;
                    case CallMenuOptions.Exit:
                        Console.WriteLine("Returning to the main menu...");
                        return;
                    default:
                        Console.WriteLine("Invalid choice, please try again.");
                        break;
                }
            }
        }
        private static void AddCall()
        {
            Console.WriteLine("Select Call Type:");
            foreach (CallType callType in Enum.GetValues(typeof(CallType)))
            {
                Console.WriteLine($"{(int)callType}. {callType}");
            }


            int typeChoice = int.Parse(Console.ReadLine() ?? "0");
            CallType type = (CallType)typeChoice;


            Console.Write("Enter Full Address: ");
            string fullAddress = Console.ReadLine() ?? string.Empty;


            Console.Write("Enter Latitude: ");
            double latitude = double.Parse(Console.ReadLine() ?? "0");
            Console.Write("Enter Longitude: ");
            double longitude = double.Parse(Console.ReadLine() ?? "0");


            Console.Write("Enter Opening Time (yyyy-MM-dd HH:mm): ");
            string? input = Console.ReadLine();
            DateTime openingTime;
            if (!DateTime.TryParse(input, out openingTime))
            {
                openingTime = DateTime.Now; 
            }

            Console.Write("Enter Max Completion Time (yyyy-MM-dd HH:mm, press enter to skip): ");
            string? maxCompletionTimeInput = Console.ReadLine();
            DateTime? maxCompletionTime = null;
            if (!string.IsNullOrWhiteSpace(maxCompletionTimeInput) && DateTime.TryParse(maxCompletionTimeInput, out DateTime parsedTime))
            {
                maxCompletionTime = parsedTime;
            }
            else
            {
                maxCompletionTime = DateTime.Now.AddHours(2);
            }
            Console.Write("Enter Description (optional): ");
            string description = Console.ReadLine() ?? string.Empty;


            Call newCall = new Call(
                Id: 0,
                Type: type,
                FullAddress: fullAddress,
                Latitude: latitude,
                Longitude: longitude,
                OpeningTime: openingTime,
                MaxCompletionTime: maxCompletionTime,
                Description: description
            );


            s_dalCall?.Create(newCall);

            Console.WriteLine("Call added successfully!");
        }
        private static void ViewCallById()
        {
            Console.WriteLine("Please enter the Call ID:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid input. Please enter a numeric Call ID.");
            }

            try
            {
                Call? call = s_dalCall.Read(id);
                if (call != null)
                {
                    Console.WriteLine($"Call ID: {call.Id}");
                    Console.WriteLine($"Type: {call.Type}");
                    Console.WriteLine($"Address: {call.FullAddress}");
                    Console.WriteLine($"Latitude: {call.Latitude}, Longitude: {call.Longitude}");
                    Console.WriteLine($"Opening Time: {call.OpeningTime}");
                    Console.WriteLine($"Max Completion Time: {call.MaxCompletionTime}");
                    Console.WriteLine($"Description: {call.Description}");
                }
                else
                {
                    Console.WriteLine($"Call with ID {id} does not exist.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        public static void ViewAllCalls()
        {
            var calls = s_dalCall?.ReadAll();

            if (calls == null || calls.Count == 0)
            {
                Console.WriteLine("No calls found.");
                return;
            }

            foreach (var call in calls)
            {
                Console.WriteLine($"ID: {call.Id}, Type: {call.Type}, Address: {call.FullAddress}, Latitude: {call.Latitude}, Longitude: {call.Longitude}, Opening Time: {call.OpeningTime}, Max Completion Time: {(call.MaxCompletionTime.HasValue ? call.MaxCompletionTime.Value.ToString() : "N/A")}, Description: {(string.IsNullOrEmpty(call.Description) ? "N/A" : call.Description)}");
            }
        }
        public static void UpdateCall()
        {
            Console.WriteLine("Please enter the Call ID:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid input. Please enter a valid numeric ID.");
                return;
            }
            var call = s_dalCall?.Read(id);
            if (call == null)
            {
                Console.WriteLine($"Call with ID {id} not found.");
                return;
            }


            Console.WriteLine("Existing Call Details:");
            Console.WriteLine($"ID: {call.Id}\nType: {call.Type}\nAddress: {call.FullAddress}\nLatitude: {call.Latitude}\nLongitude: {call.Longitude}\nOpening Time: {call.OpeningTime}\nMax Completion Time: {(call.MaxCompletionTime.HasValue ? call.MaxCompletionTime.Value.ToString() : "N/A")}\nDescription: {(string.IsNullOrEmpty(call.Description) ? "N/A" : call.Description)}");


            Console.WriteLine("Enter updated details. Press Enter to skip any field and retain the current value.");

            Console.Write("Type (current: {0}): ", call.Type);
            string? typeInput = Console.ReadLine();
            var updatedType = Enum.TryParse(typeInput, out CallType newType) ? newType : call.Type;

            Console.Write("Full Address (current: {0}): ", call.FullAddress);
            string? updatedAddress = Console.ReadLine();
            updatedAddress = string.IsNullOrWhiteSpace(updatedAddress) ? call.FullAddress : updatedAddress;

            Console.Write("Latitude (current: {0}): ", call.Latitude);
            string? latitudeInput = Console.ReadLine();
            var updatedLatitude = double.TryParse(latitudeInput, out double newLatitude) ? newLatitude : call.Latitude;

            Console.Write("Longitude (current: {0}): ", call.Longitude);
            string? longitudeInput = Console.ReadLine();
            var updatedLongitude = double.TryParse(longitudeInput, out double newLongitude) ? newLongitude : call.Longitude;

            Console.Write("Max Completion Time (current: {0}): ", call.MaxCompletionTime.HasValue ? call.MaxCompletionTime.ToString() : "N/A");
            string? maxCompletionInput = Console.ReadLine();
            var updatedMaxCompletionTime = DateTime.TryParse(maxCompletionInput, out DateTime newMaxCompletion) ? newMaxCompletion : call.MaxCompletionTime;

            Console.Write("Opening Time (current: {0}): ", call.OpeningTime);
            string? OpeningTime = Console.ReadLine();
            var updatedOpeningTime = DateTime.TryParse(maxCompletionInput, out DateTime newOpeningTime) ? newOpeningTime : call.OpeningTime;

            Console.Write("Description (current: {0}): ", call.Description);
            string? updatedDescription = Console.ReadLine();
            updatedDescription = string.IsNullOrWhiteSpace(updatedDescription) ? call.Description : updatedDescription;


            var updatedCall = call with
            {
                Type = updatedType,
                FullAddress = updatedAddress,
                Latitude = updatedLatitude,
                Longitude = updatedLongitude,
                OpeningTime= updatedOpeningTime,
                MaxCompletionTime = updatedMaxCompletionTime,
                Description = updatedDescription
            };
            s_dalCall?.Update(updatedCall);
            Console.WriteLine("Call updated successfully.");
        }
        public static void DeleteCall()
        {

            Console.WriteLine("Please enter the Call ID to delete:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid input. Please enter a valid numeric ID.");
                return;
            }
            try
            {
                s_dalCall?.Delete(id);
                Console.WriteLine($"Call with ID {id} was successfully deleted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not delete Call: {ex.Message}");
            }
        }
        public static void DeleteAllCalls()
        {
            try
            {
                s_dalCall?.DeleteAll();
                Console.WriteLine("All Call have been successfully deleted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting all Call: {ex.Message}");
            }
        }



        static void ManageAssignment()
        {
            AssignmentMenuOption choice;

            while (true)
            {

                ManageAssignmentsMenu();
                Console.Write("Enter your choice: ");
                string? input = Console.ReadLine();

                // Attempt to parse the input to the corresponding enum value
                if (!Enum.TryParse(input, out choice) || !Enum.IsDefined(typeof(AssignmentMenuOption), choice))
                {
                    Console.WriteLine("Invalid input. Please enter a valid option from the menu.");
                    continue;
                }

                switch (choice)
                {
                    case AssignmentMenuOption.AddAssignment:
                        AddAssignment();
                        break;

                    case AssignmentMenuOption.ViewAssignmentById:
                        ViewAssignmentById();
                        break;

                    case AssignmentMenuOption.ViewAllAssignments:
                        ViewAllAssignments();
                        break;

                    case AssignmentMenuOption.UpdateAssignment:
                        UpdateAssignment();
                        break;

                    case AssignmentMenuOption.DeleteAssignment:
                        DeleteAssignment();
                        break;

                    case AssignmentMenuOption.DeleteAllAssignments:
                        DeleteAllAssignments();
                        break;

                    case AssignmentMenuOption.Exit:
                        Console.WriteLine("Returning to the main menu...");
                        return;

                    default:
                        Console.WriteLine("Invalid choice, please try again.");
                        break;
                }
            }
        }
        private static void AddAssignment()
        {

            try
            {

                Console.WriteLine("Enter Assignment Details:");


                Console.Write("Call ID: ");
                int callId = 0;
                int.TryParse(Console.ReadLine(), out callId); 

                Console.Write("Volunteer ID: ");
                int volunteerId = 0;
                int.TryParse(Console.ReadLine(), out volunteerId);

                Console.Write("Entry Time (yyyy-MM-dd HH:mm:ss): ");
                DateTime entryTime = DateTime.MinValue;
                DateTime.TryParse(Console.ReadLine(), out entryTime); 

                Console.Write("Completion Time (optional, leave empty if not completed): ");
                DateTime? completionTime = null;
                string completionTimeInput = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(completionTimeInput))
                {
                    DateTime.TryParse(completionTimeInput, out var compTime);
                    completionTime = compTime;
                }

                Console.Write("Completion Type (1 for Treated, 2 for VolunteerCancelled, 3 for AdminCancelled, 4 for Expired): ");
                AssignmentCompletionType? completionType = null;
                string completionTypeInput = Console.ReadLine();
                if (Enum.TryParse<AssignmentCompletionType>(completionTypeInput, out var parsedCompletionType))
                {
                    completionType = parsedCompletionType; 
                }


                Assignment assignment = new Assignment(
                    Id: 0,
                    CallId: callId,
                    VolunteerId: volunteerId,
                    EntryTime: entryTime,
                    CompletionTime: completionTime,
                    CompletionType: completionType
                );

                s_dalAssignment?.Create(assignment);
                Console.WriteLine("Assignment added successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        private static void ViewAssignmentById()
        {
            Console.WriteLine("Please enter the Assignment ID:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid input. Please enter a numeric Assignment ID.");
                return;
            }

            try
            {
                Assignment? assignment = s_dalAssignment?.Read(id);
                if (assignment != null)
                {
                    Console.WriteLine($"Assignment ID: {assignment.Id}");
                    Console.WriteLine($"Call ID: {assignment.CallId}");
                    Console.WriteLine($"Volunteer ID: {assignment.VolunteerId}");
                    Console.WriteLine($"Entry Time: {assignment.EntryTime}");
                    Console.WriteLine($"Completion Time: {assignment.CompletionTime}");
                    Console.WriteLine($"Completion Type: {assignment.CompletionType}");
                }
                else
                {
                    Console.WriteLine($"Assignment with ID {id} does not exist.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

        }
        public static void ViewAllAssignments()
        {
            var assignments = s_dalAssignment?.ReadAll();

            if (assignments == null || assignments.Count == 0)
            {
                Console.WriteLine("No assignments found.");
                return;
            }

            foreach (var assignment in assignments)
            {
                Console.WriteLine($"ID: {assignment.Id}, Call ID: {assignment.CallId}, Volunteer ID: {assignment.VolunteerId}, Entry Time: {assignment.EntryTime}, Completion Time: {(assignment.CompletionTime.HasValue ? assignment.CompletionTime.Value.ToString() : "N/A")}, Completion Type: {(assignment.CompletionType.HasValue ? assignment.CompletionType.Value.ToString() : "N/A")}");
            }
        }
        public static void UpdateAssignment()
        {
            Console.WriteLine("Please enter the Assignment ID:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid input. Please enter a valid numeric ID.");
                return;
            }
            var assignment = s_dalAssignment?.Read(id);
            if (assignment == null)
            {
                Console.WriteLine($"Assignment with ID {id} not found.");
                return;
            }

            Console.WriteLine("Existing Assignment Details:");
            Console.WriteLine($"ID: {assignment.Id}\nCall ID: {assignment.CallId}\nVolunteer ID: {assignment.VolunteerId}\nEntry Time: {assignment.EntryTime}\nCompletion Time: {(assignment.CompletionTime.HasValue ? assignment.CompletionTime.Value.ToString() : "N/A")}\nCompletion Type: {(assignment.CompletionType.HasValue ? assignment.CompletionType.Value.ToString() : "N/A")}");

            Console.WriteLine("Enter updated details. Press Enter to skip any field and retain the current value.");

            Console.Write("Call ID (current: {0}): ", assignment.CallId);
            string? callIdInput = Console.ReadLine();
            var updatedCallId = int.TryParse(callIdInput, out int newCallId) ? newCallId : assignment.CallId;

            Console.Write("Volunteer ID (current: {0}): ", assignment.VolunteerId);
            string? volunteerIdInput = Console.ReadLine();
            var updatedVolunteerId = int.TryParse(volunteerIdInput, out int newVolunteerId) ? newVolunteerId : assignment.VolunteerId;

            Console.Write("Entry Time (current: {0}): ", assignment.EntryTime);
            string? entryTimeInput = Console.ReadLine();
            var updatedEntryTime = DateTime.TryParse(entryTimeInput, out DateTime newEntryTime) ? newEntryTime : assignment.EntryTime;

            Console.Write("Completion Time (current: {0}): ", assignment.CompletionTime.HasValue ? assignment.CompletionTime.Value.ToString() : "N/A");
            string? completionTimeInput = Console.ReadLine();
            var updatedCompletionTime = DateTime.TryParse(completionTimeInput, out DateTime newCompletionTime) ? newCompletionTime : assignment.CompletionTime;

            Console.Write("Completion Type (current: {0}): ", assignment.CompletionType.HasValue ? assignment.CompletionType.Value.ToString() : "N/A");
            string? completionTypeInput = Console.ReadLine();
            var updatedCompletionType = Enum.TryParse(completionTypeInput, out AssignmentCompletionType newCompletionType) ? newCompletionType : assignment.CompletionType;

            var updatedAssignment = assignment with
            {
                CallId = updatedCallId,
                VolunteerId = updatedVolunteerId,
                EntryTime = updatedEntryTime,
                CompletionTime = updatedCompletionTime,
                CompletionType = updatedCompletionType
            };
            s_dalAssignment?.Update(updatedAssignment);
            Console.WriteLine("Assignment updated successfully.");
        }
        public static void DeleteAssignment()
        {
            Console.WriteLine("Please enter the Assignment ID to delete:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid input. Please enter a valid numeric ID.");
                return;
            }
            try
            {
                s_dalAssignment?.Delete(id);
                Console.WriteLine($"Assignment with ID {id} was successfully deleted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not delete Assignment: {ex.Message}");
            }
        }
        public static void DeleteAllAssignments()
        {
            try
            {
                s_dalAssignment?.DeleteAll();
                Console.WriteLine("All Assignments have been successfully deleted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting all Assignments: {ex.Message}");
            }
        }



        static void ManageConfig()
        {
            bool exit = false;

            while (!exit)
            {
                ManageConfigurationMenu();
                Console.Write("\nChoose an option: ");
                string? input = Console.ReadLine();

                if (!Enum.TryParse(input, out ConfigMenuOption choice) || !Enum.IsDefined(typeof(ConfigMenuOption), choice))
                {
                    Console.WriteLine("Invalid input. Please enter a valid option from the menu.");
                    continue;
                }

                switch (choice)
                {
                    case ConfigMenuOption.AdvanceByMinute:
                        s_dalConfig.Clock = s_dalConfig.Clock.AddMinutes(1);
                        Console.WriteLine("The system clock has been advanced by 1 minute.");
                        break;

                    case ConfigMenuOption.AdvanceByHour:
                        s_dalConfig.Clock = s_dalConfig.Clock.AddHours(1);
                        Console.WriteLine("The system clock has been advanced by 1 hour.");
                        break;

                    case ConfigMenuOption.AdvanceByDay:
                        s_dalConfig.Clock = s_dalConfig.Clock.AddDays(1);
                        Console.WriteLine("The system clock has been advanced by 1 day.");
                        break;

                    case ConfigMenuOption.AdvanceByWeek:
                        s_dalConfig.Clock = s_dalConfig.Clock.AddDays(7);
                        Console.WriteLine("The system clock has been advanced by 1 week.");
                        break;

                    case ConfigMenuOption.AdvanceByMonth:
                        s_dalConfig.Clock = s_dalConfig.Clock.AddMonths(1);
                        Console.WriteLine("The system clock has been advanced by 1 month.");
                        break;

                    case ConfigMenuOption.AdvanceByYear:
                        s_dalConfig.Clock = s_dalConfig.Clock.AddYears(1);
                        Console.WriteLine("The system clock has been advanced by 1 year.");
                        break;

                    case ConfigMenuOption.ShowCurrentClock:
                        Console.WriteLine($"Current system clock: {s_dalConfig.Clock}");
                        break;

                    case ConfigMenuOption.SetConfigValue:
                        SetConfigValue();
                        break;

                    case ConfigMenuOption.ShowConfigValue:
                        ShowConfigValue();
                        break;

                    case ConfigMenuOption.ResetConfig:
                        s_dalConfig?.Reset();
                        Console.WriteLine("All configuration values have been reset.");
                        break;

                    case ConfigMenuOption.Exit:
                        exit = true;
                        Console.WriteLine("Exiting submenu.");
                        break;

                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }
        public static void SetConfigValue()
        {
            Console.Write("Enter new date and time (yyyy-MM-dd HH:mm:ss): ");
            if (DateTime.TryParse(Console.ReadLine(), out DateTime newClock))
            {
                s_dalConfig.Clock = newClock;
            }
            else
            {
                s_dalConfig.Clock = DateTime.MinValue;
                Console.WriteLine("Default value set");
            }
            Console.WriteLine("Clock value updated.");
        }
        public static void ShowConfigValue()
        {
            Console.WriteLine("\n--- Current Configuration Values ---");
            Console.WriteLine($"1. Clock: {s_dalConfig.Clock}");
            Console.WriteLine($"2. Risk Range Lock: {s_dalConfig.RiskRangelock}");
            Console.WriteLine($"3. Next Call ID: {s_dalConfig.NextCallId}");
            Console.WriteLine($"4. Next Assignment ID: {s_dalConfig.NextAssignmentId}");
        }




        static void InitializeDatabase()
        {
            Initialization.Do(s_dalVolunteer, s_dalCall, s_dalAssignment, s_dalConfig);
            Console.WriteLine("Database initialized successfully.");
        }

        static void ResetDatabase()
        {
            s_dalVolunteer?.DeleteAll();
            s_dalCall?.DeleteAll();
            s_dalAssignment?.DeleteAll();
            s_dalConfig?.Reset();
            Console.WriteLine("Database reset successfully.");
        }

        static void DisplayAllData()
        {
            Console.WriteLine("Displaying all data...");

            Console.WriteLine("Volunteers:");
            ViewAllVolunteers();
            Console.WriteLine();

            Console.WriteLine("Calls:"); 
            ViewAllCalls();
            Console.WriteLine();

            Console.WriteLine("Assignments:");        
            ViewAllAssignments();
            Console.WriteLine();
        }


        public static void DisplayMainMenu()
        { 
            Console.WriteLine("Main Menu:");
            Console.WriteLine("1. Manage Volunteers");
            Console.WriteLine("2. Manage Calls");
            Console.WriteLine("3. Manage Assigments");
            Console.WriteLine("4. Manage Configurations");
            Console.WriteLine("5. Initialize Database");
            Console.WriteLine("6. Reset Database");
            Console.WriteLine("7. Display All Data"); // הוספת האופציה להצגת כל הנתונים
            Console.WriteLine("0. Exit");
        }
        static void ManageVolunteersMenu()
        {
            Console.WriteLine("Manage Volunteers Menu:");
            Console.WriteLine("1. Add Volunteer");
            Console.WriteLine("2. View Volunteer by ID");
            Console.WriteLine("3. View All Volunteers");
            Console.WriteLine("4. Update Volunteer");
            Console.WriteLine("5. Delete Volunteer");
            Console.WriteLine("6. Delete All Volunteers");
            Console.WriteLine("0. Back to Main Menu");

        }
        static void ManageCallsMenu()
        {
           
            Console.WriteLine("Manage Calls Menu:");
            Console.WriteLine("1. Add Call");
            Console.WriteLine("2. View Call by ID");
            Console.WriteLine("3. View All Calls");
            Console.WriteLine("4. Update Call");
            Console.WriteLine("5. Delete Call");
            Console.WriteLine("6. Delete All Calls");
            Console.WriteLine("0. Back to Main Menu");
        }
        static void ManageAssignmentsMenu()
        {
           
            Console.WriteLine("Manage Assignments Menu:");
            Console.WriteLine("1. Add Assignment");
            Console.WriteLine("2. View Assignment by ID");
            Console.WriteLine("3. View All Assignments");
            Console.WriteLine("4. Update Assignment");
            Console.WriteLine("5. Delete Assignment");
            Console.WriteLine("6. Delete All Assignments");
            Console.WriteLine("0. Back to Main Menu");
        }
        static void ManageConfigurationMenu()
        {
           
            Console.WriteLine("\n--- Configuration Menu ---");
            Console.WriteLine("1. Advance system clock by 1 minute");
            Console.WriteLine("2. Advance system clock by 1 hour");
            Console.WriteLine("3. Advance system clock by 1 day");
            Console.WriteLine("4. Advance system clock by 1 week");
            Console.WriteLine("5. Advance system clock by 1 month");
            Console.WriteLine("6. Advance system clock by 1 year");
            Console.WriteLine("7. Show current system clock value");
            Console.WriteLine("8. Set a new value for a configuration parameter");
            Console.WriteLine("9. Show the current value of a configuration parameter");
            Console.WriteLine("10. Reset all configuration values");
            Console.WriteLine("0. Exit submenu");
        }

    }  
}
    
   

  

       
        
        


       






























