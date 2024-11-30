using Dal;
using DalApi;
using DO;


namespace DalTest
{
    // Enumeration for main menu options in the application.
    public enum MainMenuOptions
    {
        ManageVolunteer = 1, // Option to manage volunteers.
        ManageCall,          // Option to manage calls.
        ManageAssignment,    // Option to manage assignments.
        ManageConfig,        // Option to manage configurations.
        InitializeDatabase,  // Option to initialize the database.
        ResetDatabase,       // Option to reset the database.
        DisplayAllData,      // Option to display all data.
        Exit = 0             // Option to exit the application.
    }

    // Enumeration for the volunteer management menu options.
    public enum VolunteerMenuOption
    {
        AddVolunteer = 1,         // Add a new volunteer.
        ViewVolunteerById = 2,    // View a specific volunteer by ID.
        ViewAllVolunteers = 3,    // View all volunteers.
        UpdateVolunteer = 4,      // Update volunteer details.
        DeleteVolunteer = 5,      // Delete a specific volunteer.
        DeleteAllVolunteers = 6,  // Delete all volunteers.
        Exit = 0                  // Exit to the main menu.
    }

    // Enumeration for the call management menu options.
    public enum CallMenuOptions
    {
        AddCall = 1,         // Add a new call.
        ViewCallById = 2,    // View a specific call by ID.
        ViewAllCalls = 3,    // View all calls.
        UpdateCall = 4,      // Update call details.
        DeleteCall = 5,      // Delete a specific call.
        DeleteAllCalls = 6,  // Delete all calls.
        Exit = 0             // Exit to the main menu.
    }

    // Enumeration for assignment management menu options.
    public enum AssignmentMenuOption
    {
        Exit = 0,                // Exit to the main menu.
        AddAssignment = 1,       // Add a new assignment.
        ViewAssignmentById = 2,  // View a specific assignment by ID.
        ViewAllAssignments = 3,  // View all assignments.
        UpdateAssignment = 4,    // Update assignment details.
        DeleteAssignment = 5,    // Delete a specific assignment.
        DeleteAllAssignments = 6 // Delete all assignments.
    }

    // Enumeration for configuration menu options.
    public enum ConfigMenuOption
    {
        Exit = 0,              // Exit to the main menu.
        AdvanceByMinute = 1,   // Advance the clock by one minute.
        AdvanceByHour = 2,     // Advance the clock by one hour.
        AdvanceByDay = 3,      // Advance the clock by one day.
        AdvanceByWeek = 4,     // Advance the clock by one week.
        AdvanceByMonth = 5,    // Advance the clock by one month.
        AdvanceByYear = 6,     // Advance the clock by one year.
        ShowCurrentClock = 7,  // Display the current clock time.
        SetConfigValue = 8,    // Set a specific configuration value.
        ShowConfigValue = 9,   // Display a specific configuration value.
        ResetConfig = 10       // Reset all configuration settings.
    }


    internal class Program
    {
        // Static instances for managing application data and configuration.
        static readonly IDal s_dal = new DalList(); //stage 2

        /// <summary>
        /// Entry point of the application.
        /// Manages the main loop and handles global exceptions.
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                bool exit = false;
                // Main application loop that continues until the user chooses to exit.
                while (!exit)
                {
                    DisplayMainMenu(); // Show the main menu to the user.
                    exit = HandleUserInput(); // Process user input and determine if the application should exit.
                }
            }
            catch (Exception ex)
            {
                // Handle any unexpected exceptions that occur during execution.
                Console.WriteLine($"An error occurred: {ex}");
            }
        }
        /// <summary>
        /// Processes the user's main menu selection.
        /// Executes the appropriate action based on the user's choice.
        /// </summary>
        static bool HandleUserInput()
        {
            try
            {
                MainMenuOptions? selectedOption = GetUserChoice(); // Get the user's menu choice.

                if (selectedOption.HasValue) // Ensure the user provided a valid choice.
                {
                    switch (selectedOption)
                    {
                        case MainMenuOptions.ManageVolunteer:
                            ManageVolunteer(); // Handle volunteer management menu.
                            break;

                        case MainMenuOptions.ManageCall:
                            ManageCall(); // Handle call management menu.
                            break;

                        case MainMenuOptions.ManageAssignment:
                            ManageAssignment(); // Handle assignment management menu.
                            break;

                        case MainMenuOptions.ManageConfig:
                            ManageConfig(); // Handle configuration management menu.
                            break;

                        case MainMenuOptions.InitializeDatabase:
                            InitializeDatabase(); // Initialize the database.
                            break;

                        case MainMenuOptions.ResetDatabase:
                            ResetDatabase(); // Reset the database to its default state.
                            break;

                        case MainMenuOptions.DisplayAllData:
                            DisplayAllData(); // Display all stored data.
                            break;

                        case MainMenuOptions.Exit:
                            return true; // Indicate the user wants to exit the application.

                        default:
                            Console.WriteLine("Invalid choice, please try again."); // Handle unexpected menu options.
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors that occur while processing the user's input.
                Console.WriteLine($"Error handling input: {ex.Message}");
            }
            return false; // Continue the main loop if no exit condition was met.
        }
        /// <summary>
        /// Prompts the user for a menu option.
        /// Validates the input and returns the selected menu option, or null if invalid.
        /// </summary>
        static MainMenuOptions? GetUserChoice()
        {
            Console.Write("Enter your choice: "); // Prompt the user to enter a menu choice.
            string? input = Console.ReadLine(); // Read the user's input.

            if (Enum.TryParse(input, out MainMenuOptions selectedOption)) // Attempt to parse the input into a valid menu option.
            {
                return selectedOption; // Return the parsed option if successful.
            }
            else
            {
                // Notify the user if the input was invalid.
                Console.WriteLine("Invalid input, please enter a valid number.");
                return null; // Return null to indicate invalid input.
            }
        }



        /// <summary>
        /// Handles the volunteer management menu.
        /// Allows the user to perform various actions such as adding, viewing, updating, and deleting volunteers.
        /// Loops until the user chooses to exit to the main menu.
        /// </summary>
        static void ManageVolunteer()
        {
            VolunteerMenuOption choice;

            while (true)
            {
                ManageVolunteersMenu(); // Display the volunteer management menu.
                Console.Write("Enter your choice: ");
                string? input = Console.ReadLine();

                // Attempt to parse the input to the corresponding enum value.
                if (!Enum.TryParse(input, out choice) || !Enum.IsDefined(typeof(VolunteerMenuOption), choice))
                {
                    Console.WriteLine("Invalid input. Please enter a valid option from the menu.");
                    continue; // Prompt the user again for a valid input.
                }

                switch (choice)
                {
                    case VolunteerMenuOption.AddVolunteer:
                        AddVolunteer(); // Adds a new volunteer.
                        break;

                    case VolunteerMenuOption.ViewVolunteerById:
                        ViewVolunteerById(); // Views details of a specific volunteer by ID.
                        break;

                    case VolunteerMenuOption.ViewAllVolunteers:
                        ViewAllVolunteers(); // Displays a list of all volunteers.
                        break;

                    case VolunteerMenuOption.UpdateVolunteer:
                        UpdateVolunteer(); // Updates details of an existing volunteer.
                        break;

                    case VolunteerMenuOption.DeleteVolunteer:
                        DeleteVolunteer(); // Deletes a specific volunteer by ID.
                        break;

                    case VolunteerMenuOption.DeleteAllVolunteers:
                        DeleteAllVolunteers(); // Deletes all volunteers from the system.
                        break;

                    case VolunteerMenuOption.Exit:
                        Console.WriteLine("Returning to the main menu...");
                        return; // Exit the volunteer management menu.

                    default:
                        Console.WriteLine("Invalid choice, please try again."); // Fallback for unexpected cases.
                        break;
                }
            }
        }
        /// <summary>
        /// Collects input from the user to create a new volunteer.
        /// Validates and parses user input for all required and optional fields.
        /// Creates the volunteer and adds it to the database.
        /// </summary>
        private static void AddVolunteer()
        {
            try
            {
                // Prompt for volunteer details.
                Console.WriteLine("Enter Volunteer Details:");

                // Read and validate the volunteer's ID.
                Console.Write("ID: ");
                int id = int.Parse(Console.ReadLine() ?? throw new InvalidOperationException("ID is required."));

                // Read the volunteer's full name.
                Console.Write("Full Name: ");
                string fullName = Console.ReadLine() ?? string.Empty;

                // Read the volunteer's phone number.
                Console.Write("Phone Number: ");
                string phoneNumber = Console.ReadLine() ?? string.Empty;

                // Read the volunteer's email address.
                Console.Write("Email: ");
                string email = Console.ReadLine() ?? string.Empty;

                // Read the optional password for the volunteer.
                Console.Write("Password (optional): ");
                string? password = Console.ReadLine();

                // Read the optional current address of the volunteer.
                Console.Write("Current Address (optional): ");
                string? currentAddress = Console.ReadLine();

                // Read and parse the optional latitude.
                Console.Write("Latitude (optional): ");
                double? latitude = double.TryParse(Console.ReadLine(), out var lat) ? lat : null;

                // Read and parse the optional longitude.
                Console.Write("Longitude (optional): ");
                double? longitude = double.TryParse(Console.ReadLine(), out var lon) ? lon : null;

                // Read and parse the volunteer's role (default to Volunteer if invalid).
                Console.Write("Role (1 for Manager, 2 for Volunteer): ");
                VolunteerRole role = Enum.TryParse(Console.ReadLine(), out VolunteerRole parsedRole) ? parsedRole : VolunteerRole.Volunteer;

                // Read and parse whether the volunteer is active.
                Console.Write("Is Active (true/false): ");
                bool isActive = bool.TryParse(Console.ReadLine(), out var activeStatus) && activeStatus;

                // Read and parse the optional maximum distance.
                Console.Write("Max Distance (optional): ");
                double? maxDistance = double.TryParse(Console.ReadLine(), out var dist) ? dist : null;

                // Read and parse the distance type (default to Aerial if invalid).
                Console.Write("Distance Type (1 for Aerial, 2 for Walking, 3 for Driving): ");
                DistanceType distanceType = Enum.TryParse(Console.ReadLine(), out DistanceType parsedDistance) ? parsedDistance : DistanceType.Aerial;

                // Create a new Volunteer object using the provided details.
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

                // Add the volunteer to the database.
                s_dal.Volunteer?.Create(volunteer);
                Console.WriteLine("Volunteer added successfully!");
            }
            catch (Exception ex)
            {
                // Handle and display any errors that occur during the process.
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        /// <summary>
        /// Retrieves and displays the details of a volunteer by their ID.
        /// </summary>
        public static void ViewVolunteerById()
        {
            Console.WriteLine("Please enter the Volunteer ID:");

            // Validate the user input for a numeric ID.
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid input. Please enter a valid numeric ID.");
                return;
            }

            try
            {
                // Retrieve the volunteer by ID.
                var volunteer = s_dal.Volunteer?.Read(id);

                // Check if the volunteer exists and display their details.
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
                else
                {
                    Console.WriteLine("Volunteer not found.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving the volunteer: {ex.Message}");
            }
        }

        /// <summary>
        /// Displays a summary of all volunteers in the system.
        /// </summary>
        public static void ViewAllVolunteers()
        {
            // Retrieve all volunteers from the database.
            var volunteers = s_dal.Volunteer?.ReadAll();

            // Check if there are any volunteers to display.
            if (volunteers == null || volunteers.Count == 0)
            {
                Console.WriteLine("No volunteers found.");
                return;
            }

            // Loop through and display a summary of each volunteer.
            foreach (var volunteer in volunteers)
            {
                Console.WriteLine($"ID: {volunteer.Id}, Name: {volunteer.FullName}, Phone: {volunteer.PhoneNumber}, Email: {volunteer.Email}, Role: {volunteer.Role}, Active: {volunteer.IsActive}");
            }
        }

        /// <summary>
        /// Updates the details of an existing volunteer based on user input.
        /// </summary>
        public static void UpdateVolunteer()
        {
            // Prompt the user for the volunteer ID and validate the input.
            Console.WriteLine("Please enter the Volunteer ID:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID. Please enter a valid integer.");
                return;
            }

            // Attempt to retrieve the volunteer by ID.
            var volunteer = s_dal.Volunteer?.Read(id);
            if (volunteer == null)
            {
                Console.WriteLine($"No volunteer found with ID {id}.");
                return;
            }

            // Display the current details of the volunteer.
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

            // Allow the user to update fields selectively.
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

            // Attempt to save the updated volunteer details.
            try
            {
                s_dal.Volunteer?.Update(volunteer);
                Console.WriteLine("Volunteer details updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update volunteer details: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a specific volunteer based on their ID.
        /// </summary>
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
                // Attempt to delete the volunteer by ID.
                s_dal.Volunteer?.Delete(id);
                Console.WriteLine($"Volunteer with ID {id} was successfully deleted.");
            }
            catch (Exception ex)
            {
                // Handle errors during deletion.
                Console.WriteLine($"Could not delete volunteer: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes all volunteers from the system.
        /// </summary>
        public static void DeleteAllVolunteers()
        {
            try
            {
                // Attempt to delete all volunteers.
                s_dal.Volunteer?.DeleteAll();
                Console.WriteLine("All volunteers have been successfully deleted.");
            }
            catch (Exception ex)
            {
                // Handle errors during bulk deletion.
                Console.WriteLine($"An error occurred while deleting all volunteers: {ex.Message}");
            }
        }




        /// <summary>
        /// Manages the call-related actions in the application, presenting the user with a menu and handling their choice.
        /// </summary>
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
                        AddCall(); // Calls the method to add a new call.
                        break;
                    case CallMenuOptions.ViewCallById:
                        ViewCallById(); // Calls the method to view a call by its ID.
                        break;
                    case CallMenuOptions.ViewAllCalls:
                        ViewAllCalls(); // Calls the method to view all calls.
                        break;
                    case CallMenuOptions.UpdateCall:
                        UpdateCall(); // Calls the method to update an existing call.
                        break;
                    case CallMenuOptions.DeleteCall:
                        DeleteCall(); // Calls the method to delete a specific call.
                        break;
                    case CallMenuOptions.DeleteAllCalls:
                        DeleteAllCalls(); // Calls the method to delete all calls.
                        break;
                    case CallMenuOptions.Exit:
                        Console.WriteLine("Returning to the main menu..."); // Exits the loop and returns to the main menu.
                        return;
                    default:
                        Console.WriteLine("Invalid choice, please try again."); // Informs the user of invalid input.
                        break;
                }
            }
        }
        /// <summary>
        ///  // Adds a new call to the system, collecting all relevant details from the user.
        /// </summary>
        private static void AddCall()
        {
            // Prompt user to select a call type.
            Console.WriteLine("Select Call Type:");
            foreach (CallType callType in Enum.GetValues(typeof(CallType)))
            {
                // Display call type options.
                Console.WriteLine($"{(int)callType}. {callType}");
            }

            // Read and parse the user's selection of call type.
            int typeChoice = int.Parse(Console.ReadLine() ?? "0");
            CallType type = (CallType)typeChoice;

            // Collect the full address from the user.
            Console.Write("Enter Full Address: ");
            string fullAddress = Console.ReadLine() ?? string.Empty;

            // Collect geographical coordinates from the user.
            Console.Write("Enter Latitude: ");
            double latitude = double.Parse(Console.ReadLine() ?? "0");
            Console.Write("Enter Longitude: ");
            double longitude = double.Parse(Console.ReadLine() ?? "0");

            // Collect the opening time, defaulting to the current time if invalid.
            Console.Write("Enter Opening Time (yyyy-MM-dd HH:mm): ");
            string? input = Console.ReadLine();
            DateTime openingTime;
            if (!DateTime.TryParse(input, out openingTime))
            {
                openingTime = DateTime.Now; // Default opening time.
            }

            // Collect the maximum completion time, with an optional default value.
            Console.Write("Enter Max Completion Time (yyyy-MM-dd HH:mm, press enter to skip): ");
            string? maxCompletionTimeInput = Console.ReadLine();
            DateTime? maxCompletionTime = null;
            if (!string.IsNullOrWhiteSpace(maxCompletionTimeInput) && DateTime.TryParse(maxCompletionTimeInput, out DateTime parsedTime))
            {
                maxCompletionTime = parsedTime;
            }
            else
            {
                maxCompletionTime = DateTime.Now.AddHours(2); // Default to 2 hours from now.
            }

            // Collect an optional description from the user.
            Console.Write("Enter Description (optional): ");
            string description = Console.ReadLine() ?? string.Empty;

            // Create a new Call object with the provided details.
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

            // Add the call to the data access layer (DAL).
            s_dal.Call?.Create(newCall);

            // Confirm success to the user.
            Console.WriteLine("Call added successfully!");
        }
        /// <summary>
        /// // Displays details of a specific call by its ID.
        /// </summary>
        private static void ViewCallById()
        {
            // Prompt user to enter a Call ID and validate input.
            Console.WriteLine("Please enter the Call ID:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid input. Please enter a numeric Call ID.");
            }

            try
            {
                // Retrieve the call details from the data access layer (DAL).
                Call? call = s_dal.Call.Read(id);
                if (call != null)
                {
                    // Display call details if found.
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
                    // Notify if the call ID does not exist.
                    Console.WriteLine($"Call with ID {id} does not exist.");
                }
            }
            catch (Exception ex)
            {
                // Handle and display any errors that occur.
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        /// <summary>
        /// // Displays all calls currently in the system.
        /// </summary>
        public static void ViewAllCalls()
        {
            // Retrieve all calls from the data access layer (DAL).
            var calls = s_dal.Call?.ReadAll();

            // Check if any calls exist in the system.
            if (calls == null || calls.Count == 0)
            {
                Console.WriteLine("No calls found.");
                return;
            }

            // Iterate through and display details for each call.
            foreach (var call in calls)
            {
                Console.WriteLine($"ID: {call.Id}, Type: {call.Type}, Address: {call.FullAddress}, Latitude: {call.Latitude}, Longitude: {call.Longitude}, Opening Time: {call.OpeningTime}, Max Completion Time: {(call.MaxCompletionTime.HasValue ? call.MaxCompletionTime.Value.ToString() : "N/A")}, Description: {(string.IsNullOrEmpty(call.Description) ? "N/A" : call.Description)}");
            }
        }
        /// <summary>
        ///  // Updates the details of an existing call by its ID.
        /// </summary>
        public static void UpdateCall()
        {
            // Prompt user to enter a Call ID and validate input.
            Console.WriteLine("Please enter the Call ID:");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid input. Please enter a valid numeric ID.");
                return;
            }

            // Retrieve the call details from the data access layer (DAL).
            var call = s_dal.Call?.Read(id);
            if (call == null)
            {
                Console.WriteLine($"Call with ID {id} not found.");
                return;
            }

            // Display existing details of the call for reference.
            Console.WriteLine("Existing Call Details:");
            Console.WriteLine($"ID: {call.Id}\nType: {call.Type}\nAddress: {call.FullAddress}\nLatitude: {call.Latitude}\nLongitude: {call.Longitude}\nOpening Time: {call.OpeningTime}\nMax Completion Time: {(call.MaxCompletionTime.HasValue ? call.MaxCompletionTime.Value.ToString() : "N/A")}\nDescription: {(string.IsNullOrEmpty(call.Description) ? "N/A" : call.Description)}");

            Console.WriteLine("Enter updated details. Press Enter to skip any field and retain the current value.");

            // Collect updated call type, if provided.
            Console.Write("Type (current: {0}): ", call.Type);
            string? typeInput = Console.ReadLine();
            var updatedType = Enum.TryParse(typeInput, out CallType newType) ? newType : call.Type;

            // Collect updated address, if provided.
            Console.Write("Full Address (current: {0}): ", call.FullAddress);
            string? updatedAddress = Console.ReadLine();
            updatedAddress = string.IsNullOrWhiteSpace(updatedAddress) ? call.FullAddress : updatedAddress;

            // Collect updated latitude, if provided.
            Console.Write("Latitude (current: {0}): ", call.Latitude);
            string? latitudeInput = Console.ReadLine();
            var updatedLatitude = double.TryParse(latitudeInput, out double newLatitude) ? newLatitude : call.Latitude;

            // Collect updated longitude, if provided.
            Console.Write("Longitude (current: {0}): ", call.Longitude);
            string? longitudeInput = Console.ReadLine();
            var updatedLongitude = double.TryParse(longitudeInput, out double newLongitude) ? newLongitude : call.Longitude;

            // Collect updated max completion time, if provided.
            Console.Write("Max Completion Time (current: {0}): ", call.MaxCompletionTime.HasValue ? call.MaxCompletionTime.ToString() : "N/A");
            string? maxCompletionInput = Console.ReadLine();
            var updatedMaxCompletionTime = DateTime.TryParse(maxCompletionInput, out DateTime newMaxCompletion) ? newMaxCompletion : call.MaxCompletionTime;

            // Collect updated opening time, if provided.
            Console.Write("Opening Time (current: {0}): ", call.OpeningTime);
            string? OpeningTime = Console.ReadLine();
            var updatedOpeningTime = DateTime.TryParse(OpeningTime, out DateTime newOpeningTime) ? newOpeningTime : call.OpeningTime;

            // Collect updated description, if provided.
            Console.Write("Description (current: {0}): ", call.Description);
            string? updatedDescription = Console.ReadLine();
            updatedDescription = string.IsNullOrWhiteSpace(updatedDescription) ? call.Description : updatedDescription;

            // Create a new updated Call object with modified details.
            var updatedCall = call with
            {
                Type = updatedType,
                FullAddress = updatedAddress,
                Latitude = updatedLatitude,
                Longitude = updatedLongitude,
                OpeningTime = updatedOpeningTime,
                MaxCompletionTime = updatedMaxCompletionTime,
                Description = updatedDescription
            };

            // Save the updated call details to the data access layer (DAL).
            s_dal.Call?.Update(updatedCall);
            Console.WriteLine("Call updated successfully.");
        }
        /// <summary>
        ///  // Deletes a specific call from the system based on its ID.
        /// </summary>
        public static void DeleteCall()
        {
            Console.WriteLine("Please enter the Call ID to delete:");

            // Validates user input to ensure it is a numeric ID.
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid input. Please enter a valid numeric ID.");
                return;
            }

            try
            {
                // Attempts to delete the call with the specified ID.
                s_dal.Call?.Delete(id);
                Console.WriteLine($"Call with ID {id} was successfully deleted.");
            }
            catch (Exception ex)
            {
                // Handles any errors during the deletion process.
                Console.WriteLine($"Could not delete Call: {ex.Message}");
            }
        }
        /// <summary>
        /// Deletes all calls from the system.
        /// </summary>
        public static void DeleteAllCalls()
        {
            try
            {
                // Attempts to delete all calls.
                s_dal.Call?.DeleteAll();
                Console.WriteLine("All Calls have been successfully deleted.");
            }
            catch (Exception ex)
            {
                // Handles any errors during the deletion process.
                Console.WriteLine($"An error occurred while deleting all Calls: {ex.Message}");
            }
        }




        /// <summary>
        /// Manages assignments by presenting a menu to the user and handling their choices.
        /// </summary>
        static void ManageAssignment()
        {
            AssignmentMenuOption choice;

            while (true)
            {
                // Displays the assignments management menu.
                ManageAssignmentsMenu();
                Console.Write("Enter your choice: ");
                string? input = Console.ReadLine();

                // Validates user input against the available menu options.
                if (!Enum.TryParse(input, out choice) || !Enum.IsDefined(typeof(AssignmentMenuOption), choice))
                {
                    Console.WriteLine("Invalid input. Please enter a valid option from the menu.");
                    continue;
                }

                // Handles the user's menu choice.
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

        /// <summary>
        /// Adds a new assignment to the system.
        /// </summary>
        private static void AddAssignment()
        {
            try
            {
                Console.WriteLine("Enter Assignment Details:");

                // Reads the call ID associated with the assignment.
                Console.Write("Call ID: ");
                int callId = 0;
                int.TryParse(Console.ReadLine(), out callId);

                // Reads the volunteer ID assigned to the call.
                Console.Write("Volunteer ID: ");
                int volunteerId = 0;
                int.TryParse(Console.ReadLine(), out volunteerId);

                // Reads the entry time when the assignment began.
                Console.Write("Entry Time (yyyy-MM-dd HH:mm:ss): ");
                DateTime entryTime = DateTime.MinValue;
                DateTime.TryParse(Console.ReadLine(), out entryTime);

                // Reads the optional completion time if the assignment has been completed.
                Console.Write("Completion Time (optional, leave empty if not completed): ");
                DateTime? completionTime = null;
                string completionTimeInput = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(completionTimeInput))
                {
                    DateTime.TryParse(completionTimeInput, out var compTime);
                    completionTime = compTime;
                }

                // Reads the completion type for the assignment.
                Console.Write("Completion Type (1 for Treated, 2 for VolunteerCancelled, 3 for AdminCancelled, 4 for Expired): ");
                AssignmentCompletionType? completionType = null;
                string completionTypeInput = Console.ReadLine();
                if (Enum.TryParse<AssignmentCompletionType>(completionTypeInput, out var parsedCompletionType))
                {
                    completionType = parsedCompletionType;
                }

                // Creates a new assignment object with the provided details.
                Assignment assignment = new Assignment(
                    Id: 0,
                    CallId: callId,
                    VolunteerId: volunteerId,
                    EntryTime: entryTime,
                    CompletionTime: completionTime,
                    CompletionType: completionType
                );

                // Saves the new assignment using the data access layer.
                s_dal.Assignment?.Create(assignment);
                Console.WriteLine("Assignment added successfully!");
            }
            catch (Exception ex)
            {
                // Handles any errors that occur during the assignment creation.
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Displays details of a specific Assignment based on the provided ID.
        /// Prompts the user for an Assignment ID, retrieves the Assignment from the system, 
        /// and displays its details or an appropriate message if not found.
        /// </summary>
        private static void ViewAssignmentById()
        {
            Console.WriteLine("Please enter the Assignment ID:");
            if (!int.TryParse(Console.ReadLine(), out int id)) // Validate input to ensure it's numeric
            {
                Console.WriteLine("Invalid input. Please enter a numeric Assignment ID.");
                return;
            }

            try
            {
                // Attempt to read the assignment from the system by ID
                Assignment? assignment = s_dal.Assignment?.Read(id);
                if (assignment != null) // Check if the assignment exists
                {
                    // Display assignment details
                    Console.WriteLine($"Assignment ID: {assignment.Id}");
                    Console.WriteLine($"Call ID: {assignment.CallId}");
                    Console.WriteLine($"Volunteer ID: {assignment.VolunteerId}");
                    Console.WriteLine($"Entry Time: {assignment.EntryTime}");
                    Console.WriteLine($"Completion Time: {assignment.CompletionTime}");
                    Console.WriteLine($"Completion Type: {assignment.CompletionType}");
                }
                else
                {
                    // Inform the user if the assignment doesn't exist
                    Console.WriteLine($"Assignment with ID {id} does not exist.");
                }
            }
            catch (Exception ex) // Catch and display any unexpected errors
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves and displays all Assignments stored in the system.
        /// If no Assignments exist, informs the user.
        /// </summary>
        public static void ViewAllAssignments()
        {
            // Fetch all assignments from the system
            var assignments = s_dal.Assignment?.ReadAll();

            if (assignments == null || assignments.Count == 0) // Check if the list is empty
            {
                Console.WriteLine("No assignments found.");
                return;
            }

            // Iterate through and display each assignment's details
            foreach (var assignment in assignments)
            {
                Console.WriteLine($"ID: {assignment.Id}, Call ID: {assignment.CallId}, Volunteer ID: {assignment.VolunteerId}, Entry Time: {assignment.EntryTime}, Completion Time: {(assignment.CompletionTime.HasValue ? assignment.CompletionTime.Value.ToString() : "N/A")}, Completion Type: {(assignment.CompletionType.HasValue ? assignment.CompletionType.Value.ToString() : "N/A")}");
            }
        }

        /// <summary>
        /// Updates an existing Assignment based on user input.
        /// Prompts the user for the Assignment ID and allows them to modify any field, retaining current values if left empty.
        /// </summary>
        public static void UpdateAssignment()
        {
            Console.WriteLine("Please enter the Assignment ID:");
            if (!int.TryParse(Console.ReadLine(), out int id)) // Validate ID input
            {
                Console.WriteLine("Invalid input. Please enter a valid numeric ID.");
                return;
            }

            // Attempt to retrieve the existing assignment
            var assignment = s_dal.Assignment?.Read(id);
            if (assignment == null) // If no assignment is found
            {
                Console.WriteLine($"Assignment with ID {id} not found.");
                return;
            }

            // Display the current assignment details
            Console.WriteLine("Existing Assignment Details:");
            Console.WriteLine($"ID: {assignment.Id}\nCall ID: {assignment.CallId}\nVolunteer ID: {assignment.VolunteerId}\nEntry Time: {assignment.EntryTime}\nCompletion Time: {(assignment.CompletionTime.HasValue ? assignment.CompletionTime.Value.ToString() : "N/A")}\nCompletion Type: {(assignment.CompletionType.HasValue ? assignment.CompletionType.Value.ToString() : "N/A")}");

            Console.WriteLine("Enter updated details. Press Enter to skip any field and retain the current value.");

            // Prompt for and parse updated values, retaining current values if inputs are empty
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

            // Create a new Assignment object with updated values
            var updatedAssignment = assignment with
            {
                CallId = updatedCallId,
                VolunteerId = updatedVolunteerId,
                EntryTime = updatedEntryTime,
                CompletionTime = updatedCompletionTime,
                CompletionType = updatedCompletionType
            };

            // Update the assignment in the system
            s_dal.Assignment?.Update(updatedAssignment);
            Console.WriteLine("Assignment updated successfully.");
        }

        /// <summary>
        /// Deletes a specific Assignment based on its ID.
        /// Prompts the user for an Assignment ID and removes it from the system.
        /// </summary>
        public static void DeleteAssignment()
        {
            Console.WriteLine("Please enter the Assignment ID to delete:");
            if (!int.TryParse(Console.ReadLine(), out int id)) // Validate input to ensure it's numeric
            {
                Console.WriteLine("Invalid input. Please enter a valid numeric ID.");
                return;
            }
            try
            {
                // Attempt to delete the assignment
                s_dal.Assignment?.Delete(id);
                Console.WriteLine($"Assignment with ID {id} was successfully deleted.");
            }
            catch (Exception ex) // Catch and handle any exceptions
            {
                Console.WriteLine($"Could not delete Assignment: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes all Assignments in the system.
        /// Provides a confirmation message upon successful deletion or an error message if an exception occurs.
        /// </summary>
        public static void DeleteAllAssignments()
        {
            try
            {
                // Attempt to delete all assignments
                s_dal.Assignment?.DeleteAll();
                Console.WriteLine("All Assignments have been successfully deleted.");
            }
            catch (Exception ex) // Handle errors that may occur during deletion
            {
                Console.WriteLine($"An error occurred while deleting all Assignments: {ex.Message}");
            }
        }



        /// <summary>
        /// Manages configuration settings through a menu-based interface.
        /// Allows advancing the system clock, viewing and updating configuration values, and resetting configurations.
        /// </summary>
        static void ManageConfig()
        {
            bool exit = false;

            while (!exit)
            {
                // Display the configuration management menu
                ManageConfigurationMenu();
                Console.Write("\nChoose an option: ");
                string? input = Console.ReadLine();

                // Validate user input and parse it into a menu option
                if (!Enum.TryParse(input, out ConfigMenuOption choice) || !Enum.IsDefined(typeof(ConfigMenuOption), choice))
                {
                    Console.WriteLine("Invalid input. Please enter a valid option from the menu.");
                    continue;
                }

                // Perform actions based on the selected menu option
                switch (choice)
                {
                    case ConfigMenuOption.AdvanceByMinute:
                        // Advance the clock by 1 minute
                        s_dal.Config.Clock = s_dal.Config.Clock.AddMinutes(1);
                        Console.WriteLine("The system clock has been advanced by 1 minute.");
                        break;

                    case ConfigMenuOption.AdvanceByHour:
                        // Advance the clock by 1 hour
                        s_dal.Config.Clock = s_dal.Config.Clock.AddHours(1);
                        Console.WriteLine("The system clock has been advanced by 1 hour.");
                        break;

                    case ConfigMenuOption.AdvanceByDay:
                        // Advance the clock by 1 day
                        s_dal.Config.Clock = s_dal.Config.Clock.AddDays(1);
                        Console.WriteLine("The system clock has been advanced by 1 day.");
                        break;

                    case ConfigMenuOption.AdvanceByWeek:
                        // Advance the clock by 1 week
                        s_dal.Config.Clock = s_dal.Config.Clock.AddDays(7);
                        Console.WriteLine("The system clock has been advanced by 1 week.");
                        break;

                    case ConfigMenuOption.AdvanceByMonth:
                        // Advance the clock by 1 month
                        s_dal.Config.Clock = s_dal.Config.Clock.AddMonths(1);
                        Console.WriteLine("The system clock has been advanced by 1 month.");
                        break;

                    case ConfigMenuOption.AdvanceByYear:
                        // Advance the clock by 1 year
                        s_dal.Config.Clock = s_dal.Config.Clock.AddYears(1);
                        Console.WriteLine("The system clock has been advanced by 1 year.");
                        break;

                    case ConfigMenuOption.ShowCurrentClock:
                        // Display the current system clock
                        Console.WriteLine($"Current system clock: {s_dal.Config.Clock}");
                        break;

                    case ConfigMenuOption.SetConfigValue:
                        // Set a new configuration value
                        SetConfigValue();
                        break;

                    case ConfigMenuOption.ShowConfigValue:
                        // Show current configuration values
                        ShowConfigValue();
                        break;

                    case ConfigMenuOption.ResetConfig:
                        // Reset all configuration values to their defaults
                        s_dal.Config?.Reset();
                        Console.WriteLine("All configuration values have been reset.");
                        break;

                    case ConfigMenuOption.Exit:
                        // Exit the configuration management submenu
                        exit = true;
                        Console.WriteLine("Exiting submenu.");
                        break;

                    default:
                        // Handle invalid menu choices
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }
        public static void SetConfigValue()
        {
            /// <summary>
            /// Sets a new clock value for the configuration.
            /// Prompts the user for a date and time input, validates it, and updates the clock.
            /// </summary>
            
            Console.Write("Enter new date and time (yyyy-MM-dd HH:mm:ss): ");

            // Parse the user input into a DateTime value
            if (DateTime.TryParse(Console.ReadLine(), out DateTime newClock))
            {
                s_dal.Config.Clock = newClock; // Update the clock if input is valid
            }
            else
            {
                s_dal.Config.Clock = DateTime.MinValue; // Set default value if input is invalid
                Console.WriteLine("Default value set");
            }
            Console.WriteLine("Clock value updated.");
        }
        public static void ShowConfigValue()
        {
            /// <summary>
            /// Displays the current configuration values.
            /// Outputs the system clock, risk range lock, and next ID values for calls and assignments.
            /// </summary>
            Console.WriteLine("\n--- Current Configuration Values ---");
            Console.WriteLine($"1. Clock: {s_dal.Config.Clock}"); // Display current clock value
            Console.WriteLine($"2. Risk Range Lock: {s_dal.Config.RiskRangelock}"); // Display risk range lock value
            Console.WriteLine($"3. Next Call ID: {s_dal.Config.NextCallId}"); // Display the next call ID
            Console.WriteLine($"4. Next Assignment ID: {s_dal.Config.NextAssignmentId}"); // Display the next assignment ID
        }




        /// <summary>
        /// Initializes the database by calling the `Initialization.Do` method with the data access layers (DALs).
        /// Ensures all necessary components are properly set up for further operations.
        /// </summary>
        static void InitializeDatabase()
        {
            Initialization.Do(s_dal); // Perform the initialization process
            Console.WriteLine("Database initialized successfully."); // Notify user of successful initialization
        }

        /// <summary>
        /// Resets the database by clearing all data from volunteers, calls, and assignments,
        /// and resetting the configuration to its default state.
        /// </summary>
        static void ResetDatabase()
        {
            s_dal.Volunteer?.DeleteAll(); // Delete all volunteer data
            s_dal.Call?.DeleteAll();      // Delete all call data
            s_dal.Assignment?.DeleteAll(); // Delete all assignment data
            s_dal.Config?.Reset();        // Reset configuration values
            Console.WriteLine("Database reset successfully."); // Notify user of successful reset
        }

        /// <summary>
        /// Displays all data currently stored in the database.
        /// Retrieves and outputs the data for volunteers, calls, and assignments in a formatted manner.
        /// </summary>
        static void DisplayAllData()
        {
            Console.WriteLine("Displaying all data...");

            Console.WriteLine("Volunteers:");
            ViewAllVolunteers(); // Display all volunteer data
            Console.WriteLine();

            Console.WriteLine("Calls:");
            ViewAllCalls(); // Display all call data
            Console.WriteLine();

            Console.WriteLine("Assignments:");
            ViewAllAssignments(); // Display all assignment data
            Console.WriteLine();
        }


        /// <summary>
        /// Displays the main menu with options for managing volunteers, calls, assignments, configurations, and more.
        /// Displays the menu for managing volunteers with options to add, view, update, or delete volunteers.
        /// Displays the menu for managing calls with options to add, view, update, or delete calls.
        /// Displays the menu for managing assignments with options to add, view, update, or delete assignments.
        /// Displays the configuration menu with options to modify system time and configuration values.
        /// </summary>
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
    
   

  

       
        
        


       






























