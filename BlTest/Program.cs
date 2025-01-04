using System;
using BO;
using DalApi;
using DO;
using BlImplementation;
using BlApi;
using Helpers;
using System.Numerics;
using System.Security.Cryptography;
namespace BlTest
{
    public enum MainMenuOptions
    {
        ManageVolunteer = 1, // Option to manage volunteers.
        ManageCall,          // Option to manage calls.
        ManageAdmin,    // Option to manage assignments.
        Exit = 0             // Option to exit the application.
    }
    public enum VolunteerMenuOption
    {
        Login = 1,
        ViewAllVolunteers = 2,
        ViewVolunteerDetails = 3,
        UpdateVolunteerDetails = 4,
        UpdatePassword = 5,
        AddVolunteer = 6,
        DeleteVolunteer = 7,
        Exit = 0
    }
    public enum CallMenuOptions
    {
        AssignCallToVolunteer = 1,
        ViewAllCalls,
        ViewCallDetails,
        AddNewCall,
        UpdateCallDetails,
        DeleteCall,
        ViewClosedCallsByVolunteer,
        ViewOpenCallsByVolunteer,
        CompleteAssignment,
        CancelAssignment,
        ViewCallStatusesCount,
        Exit = 0
    }
    public enum AdminMenuOptions
    {
        ViewSystemClock = 1,
        AdvanceSystemClock,
        ViewRiskTimeRange,
        SetRiskTimeRange,
        ResetDatabase,
        InitializeDatabase,
        Exit = 0
    }


    internal class Program
    {
        // Static field for accessing logical interfaces through the BL layer
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

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
            catch (BO.BlAlreadyExistsException ex)
            {
                Console.WriteLine($"An error occurred: {ex}");
            }
            catch (BO.BlDoesNotExistException ex)
            {
                Console.WriteLine($"An error occurred: {ex}");
            }
        }
        static bool HandleUserInput()
        {
            try
            {
                MainMenuOptions? selectedOption = GetUserMenuChoice();

                if (selectedOption.HasValue)
                {
                    switch (selectedOption)
                    {
                        case MainMenuOptions.ManageVolunteer:
                            HandleVolunteerMenu();
                            break;
                        case MainMenuOptions.ManageCall:
                            HandleCallMenu();
                            break;
                        case MainMenuOptions.ManageAdmin:
                            HandleAdminMenu();
                            break;
                        case MainMenuOptions.Exit:
                            return true;
                        default:
                            Console.WriteLine("Invalid option. Please try again.");
                            break;
                    }
                }

            }

            catch (BO.BlAlreadyExistsException ex)
            {
                Console.WriteLine($"Error handling input: {ex.Message}");
            }
            catch (BO.BlDoesNotExistException ex)
            {
                Console.WriteLine($"Error handling input: {ex.Message}");
            }
            return false;
        }
        static MainMenuOptions? GetUserMenuChoice()
        {
            Console.Write("Enter your choice: ");
            string? input = Console.ReadLine();

            if (Enum.TryParse(input, out MainMenuOptions selectedOption))
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


        private static void HandleVolunteerMenu()
        {

            while (true)
            {
                DisplayVolunteerMenu();
                VolunteerMenuOption? selectedOption = GetVolunteerMenuChoice();

                if (selectedOption.HasValue)
                {
                    switch (selectedOption)
                    {
                        case VolunteerMenuOption.Login:
                            TestLogin();
                            break;
                        case VolunteerMenuOption.ViewAllVolunteers:
                            TestGetVolunteersList();
                            break;
                        case VolunteerMenuOption.ViewVolunteerDetails:
                            TestGetVolunteerDetails();
                            break;
                        case VolunteerMenuOption.UpdateVolunteerDetails:
                            TestUpdateVolunteer();
                            break;
                        case VolunteerMenuOption.UpdatePassword:
                            TestUpdatePassword();
                            break;
                        case VolunteerMenuOption.AddVolunteer:
                            TestAddVolunteer();
                            break;
                        case VolunteerMenuOption.DeleteVolunteer:
                            TestDeleteVolunteer();
                            break;
                        case VolunteerMenuOption.Exit:
                            Console.WriteLine("Exiting the Volunteer Management Menu.");
                            return;
                        default:
                            Console.WriteLine("Invalid option. Please try again.");
                            break;
                    }
                }
            }

        }
        private static VolunteerMenuOption? GetVolunteerMenuChoice()
        {
            Console.Write("Enter your choice: ");
            string? input = Console.ReadLine();

            if (Enum.TryParse(input, out VolunteerMenuOption selectedOption) && Enum.IsDefined(typeof(VolunteerMenuOption), selectedOption))
            {
                return selectedOption;
            }
            else
            {
                Console.WriteLine("Invalid input, please enter a valid option.");
                return null;
            }
        }
        public static void TestLogin()
        {
            Console.WriteLine("\n---Login to Volunteer Management System---");
            Console.Write("Please enter your ID number (username): ");
            string? usernameInput = Console.ReadLine()?.Trim();

            
            if (!int.TryParse(usernameInput, out int username))
            {
                Console.WriteLine("Invalid ID number. Please enter a valid number.");
                return;
            }

            int maxAttempts = 5; // עכשיו 5 ניסיונות
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                Console.Write("Please enter your password: ");
                string? password = Console.ReadLine();

            
                if (string.IsNullOrWhiteSpace(password))
                {
                    Console.WriteLine("Password cannot be empty.");
                    continue; 
                }

                try
                {
                    // ניסיון כניסה למערכת
                    var userRole = s_bl.Volunteer.Login(username, password);
                    Console.WriteLine($"Login successful. User role: {userRole}");
                    return; // אם ההתחברות הצליחה, סיום הפונקציה
                }
                catch (DalDoesNotExistException ex)
                {
                    Console.WriteLine($"volunteer with ID={usernameInput} does Not exist");
                    return;
                }
                catch (BO.BlInvalidPasswordException ex)
                {
                    attempts++; 
                    Console.WriteLine($"Login failed: {ex.Message}");
                    if (attempts == maxAttempts - 1)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine();
                        Console.WriteLine("Warning:\r\nThis is your last attempt. If you fail, your account will be locked for 3 minutes.");
                        Console.WriteLine();
                        Console.ResetColor();
                    }

                }
                catch (BO.BlDoesNotExistException ex)
                {
                    Console.WriteLine($"Login failed: {ex.Message}");
                    return;
                }
                catch (BO.BlLockoutException ex)
                {
                  
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red; 
                    Console.WriteLine($"Account locked: {ex.Message}");
                    Console.ResetColor();
                    return;
                }

            }
            Console.WriteLine("Too many failed attempts. Your account is now locked.");
        }
      
        public static void TestGetVolunteersList()
        {
            Console.WriteLine("\n---Retrieve Volunteers List---");

            // Filter by active status
            Console.WriteLine("Would you like to filter the list by active status?");
            Console.WriteLine("Options: 'Active', 'Inactive', or 'Skip'");
            Console.Write("Your choice: ");
            string? activeInput = Console.ReadLine()?.Trim();
            bool? isActive = activeInput switch
            {
                "Active" => true,
                "Inactive" => false,
                "Skip" => null,
                _ => null
            };

            if (isActive == null && activeInput != "Skip")
            {
                Console.WriteLine("Invalid input. Please choose 'Active', 'Inactive', or 'Skip'.");
                return;
            }

            // Sort the list by a specific field
            Console.WriteLine("\nWould you like to sort the list by a specific field?");
            Console.WriteLine("Options: Choose a field from the list below or type 'Skip':");
            foreach (var field in Enum.GetNames(typeof(VolunteerSortField)))
            {
                Console.WriteLine($"- {field}");
            }
            Console.Write("Your choice: ");
            string? sortInput = Console.ReadLine()?.Trim();
            BO.VolunteerSortField? sortField = Enum.TryParse<BO.VolunteerSortField>(sortInput, true, out var parsedField)
                ? parsedField
                : null;

            if (sortField == null && sortInput != "Skip")
            {
                Console.WriteLine("Invalid input. Please choose a valid field or type 'Skip'.");
                return;
            }

            // Fetch and display the volunteers list
            try
            {
                var volunteers = s_bl.Volunteer.GetVolunteersList(isActive, sortField);
                Console.WriteLine("\n--- Volunteers List ---");
                foreach (var volunteer in volunteers)
                {
                    Console.WriteLine(volunteer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.GetType().Name} - {ex.Message}");
            }
        }
        private static void TestGetVolunteerDetails()
        {
            Console.WriteLine("\n---Volunteer Details---");

            try
            {
                Console.Write("Enter volunteer ID: ");
                if (!int.TryParse(Console.ReadLine(), out int volunteerId))
                {
                    Console.WriteLine("Invalid input. Please enter a valid numerical ID.");
                    return;
                }


                BO.Volunteer volunteerDetails = s_bl.Volunteer.GetVolunteerDetails(volunteerId);

                var maxDistance = volunteerDetails.MaxDistance.HasValue ? volunteerDetails.MaxDistance.Value.ToString() : "Not provided";
                Console.WriteLine("Volunteer Details:");
                Console.WriteLine($"ID: {volunteerDetails.Id}");
                Console.WriteLine($"Full Name: {volunteerDetails.FullName}");
                Console.WriteLine($"Phone Number: {volunteerDetails.PhoneNumber}");
                Console.WriteLine($"Email: {volunteerDetails.Email}");
                Console.WriteLine($"Current Address: {volunteerDetails.CurrentAddress ?? "Not provided"}");
                Console.WriteLine($"Active: {(volunteerDetails.IsActive ? "Yes" : "No")}");
                Console.WriteLine($"Role: {volunteerDetails.Role}");
                Console.WriteLine($"Max Distance: {maxDistance}");
                Console.WriteLine($"Distance Type: {volunteerDetails.DistanceType}");
                Console.WriteLine($"Total Calls Handled: {volunteerDetails.TotalHandledCalls}");
                Console.WriteLine($"Total Calls Canceled: {volunteerDetails.TotalCancelledCalls}");
                Console.WriteLine($"Total Calls Expired: {volunteerDetails.TotalExpiredCalls}");

                if (volunteerDetails.ActiveCall != null)
                {
                    Console.WriteLine("Call in Progress:");
                    Console.WriteLine($" ID: {volunteerDetails.ActiveCall.Id}");
                    Console.WriteLine($" Call ID: {volunteerDetails.ActiveCall.CallId}");
                    Console.WriteLine($" Type: {volunteerDetails.ActiveCall.Type}");
                    Console.WriteLine($" Description: {volunteerDetails.ActiveCall.Description ?? "Not provided"}");
                    Console.WriteLine($" Full Address: {volunteerDetails.ActiveCall.FullAddress}");
                    Console.WriteLine($" Opening Time: {volunteerDetails.ActiveCall.OpeningTime}");
                    Console.WriteLine($" Max Completion Time: {volunteerDetails.ActiveCall.MaxCompletionTime?.ToString() ?? "Not provided"}");
                    Console.WriteLine($" Entry Time: {volunteerDetails.ActiveCall.EntryTime}");
                    Console.WriteLine($" Distance To Active Call: {volunteerDetails.ActiveCall.DistanceToActiveCall}");
                    Console.WriteLine($" Active Call Status: {volunteerDetails.ActiveCall.activeCallStatus}");
                }

                else
                {
                    Console.WriteLine("No call in progress.");
                }
            }
            catch (BO.BlDoesNotExistException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        public static void TestUpdateVolunteer()
        {
            Console.WriteLine("\n---Update Volunteer---");
            Console.WriteLine();
            Console.WriteLine("──────────────────────────────────────────────────────────────────────────────");
            Console.WriteLine("  NOTE: We added a password request to the update function for a volunteer   ");
            Console.WriteLine("  or administrator after entering their ID card to prevent phishing. This   ");
            Console.WriteLine("  ensures the identity of the person requesting the update is verified       ");
            Console.WriteLine("  not only by their ID card but also by a password.                          ");
            Console.WriteLine("──────────────────────────────────────────────────────────────────────────────");
            Console.WriteLine();


            Console.Write("Volunteer ID (national ID): ");
            int volunteerId;
            while (!int.TryParse(Console.ReadLine(), out volunteerId) || volunteerId <= 0)
            {
                Console.WriteLine("Invalid ID. Please enter a positive number.");
            }
            BO.Volunteer RequestVolunteer;
            try
            {
                RequestVolunteer = s_bl.Volunteer.GetVolunteerDetails(volunteerId)
                    ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {volunteerId} does not exist.");
                Console.WriteLine($"Volunteer request found: {RequestVolunteer.FullName}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving volunteer: {ex.Message}");
                return;
            }


            string newPassword = " ";
            if (RequestVolunteer.Role != BO.VolunteerRole.Manager)
            {
                Console.Write($"{RequestVolunteer.FullName} We just want to make sure it's you. Please enter Your password (example Orl!Y1tz#h@k79786): ");
                newPassword = Console.ReadLine()?.Trim();
            }

            string managerPassword = " ";
            if (RequestVolunteer.Role == BO.VolunteerRole.Manager)
            {
                Console.Write($"{RequestVolunteer.FullName} We just want to make sure it's you. Please enter Your password (example Orl!Y1tz#h@k79786): ");
                managerPassword = Console.ReadLine()?.Trim();
                
            }


            Console.Write("Please enter the volunteer's ID that you would like to update: ");
            int id;
            while (!int.TryParse(Console.ReadLine(), out id) || id <= 0)
            {
                Console.WriteLine("Invalid ID. Please enter a positive number.");
            }
            BO.Volunteer originalVolunteer;
            try
            {
                originalVolunteer = s_bl.Volunteer.GetVolunteerDetails(id)
                    ?? throw new BO.BlDoesNotExistException($"Volunteer with ID {id} does not exist.");
                Console.WriteLine($"Volunteer found: {originalVolunteer.FullName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving volunteer: {ex.Message}");
                return;
            }




           

            var updatedVolunteer = new BO.Volunteer
            {
                Id = originalVolunteer.Id,
                FullName = originalVolunteer.FullName,
                PhoneNumber = originalVolunteer.PhoneNumber,
                Email = originalVolunteer.Email,
                Password = RequestVolunteer.Role == BO.VolunteerRole.Manager ? originalVolunteer.Password : newPassword,
                CurrentAddress = originalVolunteer.CurrentAddress,
                Role = originalVolunteer.Role,
                IsActive = originalVolunteer.IsActive,
                MaxDistance = originalVolunteer.MaxDistance,
                DistanceType = originalVolunteer.DistanceType
            };

            var fieldsUpdated = new Dictionary<string, bool>();

            Console.Write($"Full Name (current: {updatedVolunteer.FullName}): ");
            string? fullName = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                updatedVolunteer.FullName = fullName;
                fieldsUpdated["Full Name"] = true;
            }

            Console.Write($"Phone Number (current: {updatedVolunteer.PhoneNumber}): ");
            string? phoneNumber = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                updatedVolunteer.PhoneNumber = phoneNumber;
                fieldsUpdated["Phone Number"] = true;
            }


            Console.Write($"Email (current: {updatedVolunteer.Email}): ");
            string? email = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(email))
            {
                updatedVolunteer.Email = email;
                fieldsUpdated["Email"] = true;
            }


           
            Console.Write($"Current Address (current: {(updatedVolunteer.CurrentAddress ?? "No Value")}): ");
            string? currentAddress = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(currentAddress))
            {
                updatedVolunteer.CurrentAddress = currentAddress;
                fieldsUpdated["Current Address"] = true;
            }

            Console.Write($"Role (current: {updatedVolunteer.Role}): ");
            string? roleInput = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(roleInput) && Enum.TryParse(roleInput, out BO.VolunteerRole role))
            {
                updatedVolunteer.Role = role;
                fieldsUpdated["Role"] = true;
            }

            Console.Write($"Is Active (current: {updatedVolunteer.IsActive}): ");
            string? isActiveInput = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(isActiveInput) && bool.TryParse(isActiveInput, out bool isActive))
            {
                updatedVolunteer.IsActive = isActive;
                fieldsUpdated["Is Active"] = true;
            }

            Console.Write($"Max Distance (current: {(updatedVolunteer.MaxDistance?.ToString() ?? "No Value")}): ");
            string? maxDistanceInput = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(maxDistanceInput) && double.TryParse(maxDistanceInput, out double maxDistance))
            {
                updatedVolunteer.MaxDistance = maxDistance;
                fieldsUpdated["Max Distance"] = true;
            }

            Console.Write($"Distance Type (current: {updatedVolunteer.DistanceType}): ");
            string? distanceTypeInput = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(distanceTypeInput) && Enum.TryParse(distanceTypeInput, out BO.DistanceType distanceType))
            {
                updatedVolunteer.DistanceType = distanceType;
                fieldsUpdated["Distance Type"] = true;
            }


           
            Console.WriteLine("\nFields updated:");
            foreach (var field in fieldsUpdated.Where(f => f.Value))
            {
                Console.WriteLine($"- {field.Key}");
            }
            Console.WriteLine();

          
            try
            {
                s_bl.Volunteer.UpdateVolunteer(volunteerId, updatedVolunteer, managerPassword);
                Console.WriteLine("Volunteer updated successfully.");
            }
            catch (BO.BlDoesNotExistException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public static void TestDeleteVolunteer()
        {
            Console.WriteLine("\n---Delete Volunteer---");
            Console.Write("Enter the ID of the volunteer to delete: ");

            if (!int.TryParse(Console.ReadLine(), out int volunteerId))
            {
                Console.WriteLine("Invalid input. Please enter a valid numerical ID.");
                return;
            }

            try
            {
                s_bl.Volunteer.DeleteVolunteer(volunteerId);
                Console.WriteLine($"Volunteer with ID {volunteerId} was successfully deleted.");
            }
            catch (BO.BlDoesNotExistException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.GetType().Name} - {ex.Message}");
            }
        }
        public static void TestAddVolunteer()
        {
            Console.WriteLine("\n---Add New Volunteer---");
            // Volunteer ID
            Console.Write("Volunteer ID (Required field Luhn algorithm |example 123456782): ");
            int id;
            while (!int.TryParse(Console.ReadLine(), out id) || id <= 0)
            {
                Console.WriteLine("Invalid ID. Please enter a positive number.");
            }

            // Full Name
            Console.Write("Full Name (required field |example toiby): ");
            string? fullName = Console.ReadLine()?.Trim();

            while (string.IsNullOrWhiteSpace(fullName))
            {
                Console.WriteLine("Invalid name. Please enter a valid full name.");
                fullName = Console.ReadLine();
            }

            // Phone Number
            Console.Write("Phone Number (required field |example 0545377986): ");
            string? phoneNumber = Console.ReadLine()?.Trim();

            while (string.IsNullOrWhiteSpace(phoneNumber) || !phoneNumber.All(char.IsDigit))
            {
                Console.WriteLine("Invalid phone number. Please enter only digits.");
                phoneNumber = Console.ReadLine();
            }

            // Email
            Console.Write("Email (required field |example user123@example.com): ");
            string? email = Console.ReadLine()?.Trim();
            while (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                Console.WriteLine("Invalid email. Please enter a valid email address.");
                email = Console.ReadLine();
            }

            // Password
            Console.Write("Would you like to set a password now? (y/n): ");
            string? answer = Console.ReadLine()?.ToLower();
            string? password = null;
            if (answer == "y")
            {
                Console.Write("Password (example| #Examp%lePassword1!): ");
                password = Console.ReadLine();
                while (string.IsNullOrWhiteSpace(password))
                {
                    Console.WriteLine("Password cannot be empty. Please enter a valid password.");
                    password = Console.ReadLine();
                }
            }
            else
            {
                // Displaying important message in a clear, highlighted format
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("A temporary password will be generated for you.");
                string tempPassword = GenerateTemporaryPassword();
                Console.WriteLine("Temporary Password: " + tempPassword); // Display the generated password
                Console.WriteLine("----------------------------------------");

                // Warning message within the lines, highlighting the importance
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("----------------------------------------");
                Console.WriteLine("IMPORTANT: This is the only time you can view the password.");
                Console.WriteLine("The system is encrypted, and the password cannot be retrieved.");
                Console.WriteLine("You can update it later if needed.");
                Console.WriteLine("----------------------------------------");
                Console.ResetColor();
                password = tempPassword;
            }
           
            // Current Address
            Console.Write("Current Address (Optional |example 1 Rothschild Blvd, Tel Aviv-Yafo, Israel): ");
            string? currentAddress = Console.ReadLine()?.Trim();

            // Role
            Console.Write("Role (0 for Volunteer, 1 for Manager) (optional): ");
            if (!Enum.TryParse<BO.VolunteerRole>(Console.ReadLine(), out BO.VolunteerRole role))
            {
                role = BO.VolunteerRole.Volunteer;
            }

            // Is Active
            Console.Write("Is Active (true/false) (optional): ");
            if (!bool.TryParse(Console.ReadLine(), out bool isActive))
            {
                isActive = true;
            }

            // Max Distance
            Console.Write("Max Distance (optional): ");
            string? maxDistanceInput = Console.ReadLine();
            double? maxDistance = null;
            if (!string.IsNullOrWhiteSpace(maxDistanceInput) && double.TryParse(maxDistanceInput, out double distanceValue))
            {
                maxDistance = distanceValue;
            }
          

            // Distance Type
            Console.Write("Distance Type (0 for Aerial, 1 for Walking, 2 for Driving) (optional): ");
            if (!Enum.TryParse<BO.DistanceType>(Console.ReadLine(), out BO.DistanceType distanceType))
            {
                
                distanceType = BO.DistanceType.Aerial;
            }

            // Create Volunteer Object
            var volunteer = new BO.Volunteer
            {
                Id = id,
                FullName = fullName,
                PhoneNumber = phoneNumber,
                Email = email,
                Password = password,
                CurrentAddress = currentAddress,
                Role = role,
                IsActive = isActive,
                MaxDistance = maxDistance,
                DistanceType = distanceType
            };

            // Add Volunteer
            try
            {
                s_bl.Volunteer.AddVolunteer(volunteer);
                Console.WriteLine($"Volunteer '{fullName}' was successfully added.");
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public static void TestUpdatePassword()
        {
            Console.WriteLine("\n---Test Update Password---");

            // Get inputs from the user
            Console.Write("Enter volunteer ID:");
            int volunteerId = int.Parse(Console.ReadLine());

            Console.Write("Enter current password:");
            string currentPassword = Console.ReadLine()?.Trim();

            Console.Write("Enter new password (example| Orl!Y1tz#h@k79786): ");
            string newPassword = Console.ReadLine()?.Trim(); 

            
            try
            {
                // Attempt to change the password
               s_bl.Volunteer.UpdatePassword(volunteerId, currentPassword, newPassword);
                Console.WriteLine();
                Console.WriteLine("The system has successfully identified you");
                Console.WriteLine("     Password changed successfully!");
            }
            catch (PasswordValidationException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            catch (IncorrectPasswordException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (BlDoesNotExistException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

        }
        public static string GenerateTemporaryPassword(int length = 12)
        {
            if (length < 8)
                throw new PasswordValidationException("Password length must be at least 8 characters.");

            const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specialChars = "!@#$%^&*(),.?\":{}|<>";
            const string allValidChars = upperCase + lowerCase + digits + specialChars;

            var random = new Random();
            var password = new List<char>();

            // Ensure each required character group is represented at least once
            password.Add(upperCase[random.Next(upperCase.Length)]);
            password.Add(lowerCase[random.Next(lowerCase.Length)]);
            password.Add(digits[random.Next(digits.Length)]);
            password.Add(specialChars[random.Next(specialChars.Length)]);

            // Fill the rest of the password with random characters from all valid characters
            while (password.Count < length)
            {
                password.Add(allValidChars[random.Next(allValidChars.Length)]);
            }

            // Shuffle the password to avoid predictable patterns
            return new string(password.OrderBy(_ => random.Next()).ToArray());
        }
        private static void HandleException(Exception ex)
        {
            Console.WriteLine($"Error: {ex.GetType().Name}, Message: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.GetType().Name}, Message: {ex.InnerException.Message}");
            }

            switch (ex)
            {
                case BO.BlAlreadyExistsException:
                    Console.WriteLine($"Specific Error: {ex.Message}");
                    break;
                case InvalidPhoneNumberException:
                    Console.WriteLine($"Phone Number Validation Error: {ex.Message}");
                    break;
                case InvalidEmailException:
                    Console.WriteLine($"Email Validation Error: {ex.Message}");
                    break;
                case PasswordValidationException:
                    Console.WriteLine($"Password Validation Error: {ex.Message}");
                    break;
                case AddressValidationException:
                    Console.WriteLine($"Address Validation Error: {ex.Message}");
                    break;
                case InvalidCoordinatesException:
                    Console.WriteLine($"Coordinates Validation Error: {ex.Message}");
                    break;
                case InvalidRoleException:
                    Console.WriteLine($"Role Validation Error: {ex.Message}");
                    break;
                case InvalidMaxDistanceException:
                    Console.WriteLine($"Max Distance Validation Error: {ex.Message}");
                    break;
                case InvalidDistanceTypeException:
                    Console.WriteLine($"Distance Type Validation Error: {ex.Message}");
                    break;
                default:
                    Console.WriteLine($"Unexpected Error: {ex.Message}");
                    break;
            }
        }
        public static bool VerifyPassword(string password, string storedHash)
        {

            string[] parts = storedHash.Split(':');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] storedHashBytes = Convert.FromBase64String(parts[1]);
            int iterations = 10000;
            using (var rfc2898 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                byte[] computedHash = rfc2898.GetBytes(32);
                return CryptographicOperations.FixedTimeEquals(computedHash, storedHashBytes);
            }
        }





        private static void HandleCallMenu()
        {
            while (true)
            {
                DisplayCallMenu();
                CallMenuOptions? selectedOption = GetCallMenuChoice();

                if (selectedOption.HasValue)
                {
                    switch (selectedOption.Value)
                    {
                        case CallMenuOptions.AssignCallToVolunteer:
                            TestAssignCallToVolunteer();
                            break;
                        case CallMenuOptions.ViewAllCalls:
                            TestGetCalls();
                            break;
                        case CallMenuOptions.ViewCallDetails:
                            TestGetCallDetailsById();
                            break;
                        case CallMenuOptions.AddNewCall:
                            TestAddCall();
                            break;
                        case CallMenuOptions.UpdateCallDetails:
                            TestUpdateCallDetails();
                            break;
                        case CallMenuOptions.DeleteCall:
                            TestDeleteCall();
                            break;
                        case CallMenuOptions.ViewClosedCallsByVolunteer:
                            TestGetClosedCallsByVolunteer();
                            break;
                        case CallMenuOptions.ViewOpenCallsByVolunteer:
                            TestGetOpenCallsByVolunteer();
                            break;
                        case CallMenuOptions.CompleteAssignment:
                            TestCompleteAssignment();
                            break;
                        case CallMenuOptions.CancelAssignment:
                            TestCancelAssignment();
                            break;
                        case CallMenuOptions.ViewCallStatusesCount:
                            TestGetCallStatusesCount();
                            break;
                        case CallMenuOptions.Exit:
                            return;
                        default:
                            Console.WriteLine("Invalid option. Please try again.");
                            break;
                    }
                }
            }
        }
        static CallMenuOptions? GetCallMenuChoice()
        {
            Console.Write("Enter your choice: ");
            string? input = Console.ReadLine();

            if (Enum.TryParse(input, out CallMenuOptions selectedOption) && Enum.IsDefined(typeof(CallMenuOptions), selectedOption))
            {
                return selectedOption;
            }
            else
            {
                Console.WriteLine("Invalid input, please enter a valid option.");
                return null;
            }
        }
        public static void TestGetCallStatusesCount()
        {
            Console.WriteLine("\n---Testing GetCallStatusesCount Method---");

            try
            {
                int[] callCounts = s_bl.Call.GetCallStatusesCount();

                // Step 3: Validate the results
                Console.WriteLine("\nCall Statuses Report:");
                Console.WriteLine("-------------------------------");
                Console.WriteLine(" Status                 | Count");
                Console.WriteLine("-------------------------------");

                var callStatuses = Enum.GetValues(typeof(CallStatus)).Cast<CallStatus>().ToArray();
                int totalCalculatedCalls = 0;

                for (int i = 0; i < callCounts.Length; i++)
                {
                    Console.WriteLine($" {callStatuses[i],-21} | {callCounts[i],5}");
                    totalCalculatedCalls += callCounts[i];
                }

                Console.WriteLine("-------------------------------");
                Console.WriteLine("\nTest Passed: Total call count matches expected value.");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"\nTest Failed: {ex.Message}");
            }
        }
        public static void TestGetCalls()
        {
            Console.WriteLine("\n--- Test GetCalls Functionality ---");
            Console.WriteLine("Enter filter field (or press Enter to skip):");
            PrintEnumOptions<CallInListField>();
            string? filterFieldInput = Console.ReadLine();

            CallInListField? filterField = Enum.TryParse(filterFieldInput, out CallInListField tempFilterField) ? tempFilterField : (CallInListField?)null;
            object? filterValue = null;

            if (filterField.HasValue)
            {
                if (filterField.Value == CallInListField.CallType || filterField.Value == CallInListField.Status)
                {
                    Type enumType = filterField.Value switch
                    {
                        CallInListField.CallType => typeof(BO.CallType),
                        CallInListField.Status => typeof(BO.CallStatus),
                        _ => throw new InvalidOperationException("Unexpected field.")
                    };

                    Console.WriteLine($"Enter filter value for {filterField.Value} (or press Enter to skip):");
                    PrintEnumOptions(enumType);
                    string? filterValueInput = Console.ReadLine();

                    if (!string.IsNullOrWhiteSpace(filterValueInput) && Enum.TryParse(enumType, filterValueInput, out object? parsedValue))
                    {
                        filterValue = parsedValue;
                    }
                }
                else
                {
                    Console.WriteLine($"Enter value for {filterField.Value}:");
                    string? genericInput = Console.ReadLine();
                    filterValue = !string.IsNullOrWhiteSpace(genericInput) ? genericInput : null;
                }
            }

            Console.WriteLine("Enter sort field (or press Enter to skip):");
            PrintEnumOptions<CallInListField>();
            string? sortFieldInput = Console.ReadLine();
            CallInListField? sortField = Enum.TryParse(sortFieldInput, out CallInListField tempSortField) ? tempSortField : (CallInListField?)null;

            var calls = s_bl.Call.GetCalls(filterField, filterValue, sortField);

            Console.WriteLine("\nFiltered and Sorted Calls:");
            PrintCalls(calls);
        }
        public static void TestGetCallDetailsById()
        {
            Console.WriteLine("\n--- Test Get Call Details by ID ---");
            try
            {
                Console.WriteLine("Enter the Call ID:");
                string input = Console.ReadLine();
                if (int.TryParse(input, out int callId))
                {
                    BO.Call callDetails = s_bl.Call.GetCallDetailsById(callId);

                    if (callDetails != null)
                    {
                        Console.WriteLine(Tools.ToStringProperty(callDetails));
                    }
                    else
                    {
                        Console.WriteLine("Call not found.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid Call ID.");
                }
            }
            catch (BO.BlDoesNotExistException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        public static void TestUpdateCallDetails()
        {
            Console.WriteLine("\n--- Test Update Call Details ---");
            try
            {
                Console.Write("Enter the Call ID to update:");

          
                string input = Console.ReadLine();
                if (int.TryParse(input, out int callId))
                {
                   
                    BO.Call existingCall = s_bl.Call.GetCallDetailsById(callId);
             
                    if (existingCall == null)
                    {
                        Console.WriteLine($"No call found with ID {callId}.");
                        return;
                    }

                    Console.WriteLine("\nCurrent Call Details:");
                    Console.WriteLine($"Call ID: {existingCall.Id}");
                    Console.WriteLine($"Type: {existingCall.Type}");
                    Console.WriteLine($"Description: {existingCall.Description}");
                    Console.WriteLine($"Full Address: {existingCall.FullAddress}");
                    Console.WriteLine($"Opening Time: {existingCall.OpeningTime:yyyy-MM-dd HH:mm}");
                    Console.WriteLine($"Max Completion Time: {existingCall.MaxCompletionTime?.ToString("yyyy-MM-dd HH:mm") ?? "Not set"}");
                    

                    BO.Call callToUpdate = new BO.Call
                    {
                        Id = existingCall.Id,
                        Type= existingCall.Type,
                        Description = existingCall.Description,
                        FullAddress = existingCall.FullAddress,
                        Latitude = existingCall.Latitude,
                        Longitude = existingCall.Longitude,
                        OpeningTime = existingCall.OpeningTime,
                        MaxCompletionTime = existingCall.MaxCompletionTime,
                        Status = existingCall.Status
                    };

                    var fieldsUpdated = new Dictionary<string, bool>();

                    Console.WriteLine();
                    PrintCallTypes();
                    Console.Write("Enter the Call Type:");
                    string typeInput = Console.ReadLine()?.Trim();

                    if (!string.IsNullOrWhiteSpace(typeInput))
                    {
                        if (Enum.TryParse(typeInput, out BO.CallType newTypeInput))
                        {
                            callToUpdate.Type = newTypeInput;
                            fieldsUpdated["Call Type"] = true;
                        }
                        else
                        {
                            Console.WriteLine("Invalid input. The system will not update the call type.\n");
                        }
                    }

                    Console.Write($"Description (current: {callToUpdate.Description}): ");
                    string? description = Console.ReadLine()?.Trim();
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        callToUpdate.Description = description;
                        fieldsUpdated["Description"] = true;
                    }

                    Console.Write($"Full Address (current: {callToUpdate.FullAddress} |example: 17 Rothschild Boulevard, Tel Aviv, Israel): ");
                    string? fullAddress = Console.ReadLine()?.Trim();
                    if (!string.IsNullOrWhiteSpace(fullAddress))
                    {
                        callToUpdate.FullAddress = fullAddress;
                        fieldsUpdated["Full Address"] = true;
                    }
                    
                    
                    Console.Write($"Max Completion Time (current: {callToUpdate.MaxCompletionTime?.ToString("yyyy-MM-dd HH:mm")}|example: 2025-11-01 14:30:15): ");
                    string? maxCompletionTimeInput = Console.ReadLine()?.Trim();
                    if (!string.IsNullOrWhiteSpace(maxCompletionTimeInput))
                    {
                        if (DateTime.TryParse(maxCompletionTimeInput, out DateTime maxCompletionTime))
                        {
                            callToUpdate.MaxCompletionTime = maxCompletionTime;
                            fieldsUpdated["Max Completion Time"] = true;
                        }
                        else
                        {
                            Console.WriteLine("Invalid date format. The system will not update the Max Completion end date of the call");
                        }
                    }


                    Console.WriteLine("\nFields updated:");
                    foreach (var field in fieldsUpdated.Where(f => f.Value))
                    {
                        Console.WriteLine($"- {field.Key}");
                    }

                    s_bl.Call.UpdateCallDetails(callToUpdate);
                    Console.WriteLine("Call details updated successfully!");
                }
                else
                {
                    Console.WriteLine("Invalid Call ID. Please enter a valid number.");
                }
            }
            catch (BO.BlDoesNotExistException ex)
            {
                Console.WriteLine($"Login failed: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.GetType().Name}, Message: {ex.InnerException.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        public static void TestDeleteCall()
        {
            Console.WriteLine("\n--- Test Delete Call ---");
            try
            {
                Console.Write("Enter the Call ID to delete:");
                if (int.TryParse(Console.ReadLine(), out int callId))
                {
                    try
                    {
                        s_bl.Call.DeleteCall(callId);
                        Console.WriteLine("Call deleted successfully.");
                    }
                    catch (BO.BlDoesNotExistException ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}"); // קריאה לא קיימת
                    }
                    catch (BO.BlCallAssignedException ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}"); // קריאה שהוקצתה למתנדב
                    }
                    catch (BO.BlInvalidStatusException ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}"); // קריאה לא בסטטוס "Open"
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Unexpected error: {ex.Message}"); // שגיאה כלשהי שלא ציפינו לה
                    }
                }
                else
                {
                    Console.WriteLine("Invalid Call ID entered.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during the test: {ex.Message}");
            }
        }
        public static void TestAddCall()
        {
            Console.WriteLine("\n--- Add New Call ---");
            try
            {


                BO.Call newCall = new BO.Call
                {
                    Id = 0
                };

                    
                    PrintCallTypes();
                    Console.Write("Enter the Call Type (Mandatory):");
                    string typeInput = Console.ReadLine()?.Trim();
                    if (string.IsNullOrEmpty(typeInput))
                    {
                        Console.WriteLine("Call Type Required field. Please enter a valid value.");
                        return;
                    }
                    else if (Enum.TryParse(typeInput, out BO.CallType type))
                    {
                        newCall.Type = type;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input for Call Type. ");
                    }

                    Console.WriteLine("Enter the Full Address (Required field |example: 17 Rothschild Boulevard, Tel Aviv, Israel): ");
                    newCall.FullAddress = Console.ReadLine()?.Trim();
                    if (string.IsNullOrEmpty(newCall.FullAddress))
                    {
                        Console.WriteLine("Full Address is mandatory.");
                        return;
                    }

                   
                    Console.WriteLine("Enter the Max Completion Time (Optional, Format: yyyy-MM-dd HH:mm|example: 2025-11-01 14:30:15) or press Enter to skip:");
                    string maxCompletionTimeInput = Console.ReadLine()?.Trim();
                    if (string.IsNullOrEmpty(maxCompletionTimeInput))
                    {
                        newCall.MaxCompletionTime = null;
                    }
                    else if (DateTime.TryParse(maxCompletionTimeInput, out DateTime maxCompletionTime))
                    {
                        newCall.MaxCompletionTime = maxCompletionTime;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input for Max Completion Time. Skipping...");
                        newCall.MaxCompletionTime = null;
                    }
                    Console.Write("Enter the Description (Optional) or press Enter to skip:");
                    newCall.Description = Console.ReadLine()?.Trim();
                    s_bl.Call.AddCall(newCall);
                    Console.WriteLine("Call added successfully.");
               
            }

            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during the test: {ex.Message}");
            }
        }
        public static void TestGetClosedCallsByVolunteer()
        {
            Console.WriteLine("\n--- Retrieve Closed Calls by Volunteer with Optional Filtering and Sorting ---");
            try
            {

                Console.Write("Enter Volunteer ID:");
                string volunteerIdInput = Console.ReadLine();
                if (!int.TryParse(volunteerIdInput, out int volunteerId))
                {
                    Console.WriteLine("Invalid Volunteer ID.");
                    return;
                }

                Console.WriteLine("Available Call Types:");
                foreach (var callType in Enum.GetValues(typeof(BO.CallType)))
                {
                    Console.WriteLine($"{(int)callType}. {callType}");
                }
                Console.Write("Enter Call Type (Enter number or press Enter for all):");
                string callTypeInput = Console.ReadLine();
                BO.CallType? callTypeFilter = null;

                if (!string.IsNullOrEmpty(callTypeInput))
                {
                    if (int.TryParse(callTypeInput, out int callTypeValue)
                        && Enum.IsDefined(typeof(BO.CallType), callTypeValue))
                    {
                        callTypeFilter = (BO.CallType)callTypeValue;
                    }
                    else if (Enum.TryParse(callTypeInput, true, out BO.CallType callTypeEnumValue))
                    {
                        callTypeFilter = callTypeEnumValue;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input for Call Type. ");
                    }
                }


               
                Console.WriteLine("--Available Sort Fields--");
                foreach (var sortField in Enum.GetValues(typeof(ClosedCallSortField)))
                {
                    Console.WriteLine($"{(int)sortField}. {sortField}");
                }
                Console.Write("Enter Sort Field:");
                string sortByFieldInput = Console.ReadLine();
                ClosedCallSortField? sortByField = null;

                if (!string.IsNullOrEmpty(sortByFieldInput))
                {
                    if (Enum.TryParse<BO.ClosedCallSortField>(sortByFieldInput, out ClosedCallSortField parsedSortField))
                    {
                        sortByField = parsedSortField;
                    }
                    else
                    {
                        Console.WriteLine("Invalid Sort Field. Defaulting to sorting by ID.");
                    }
                }

                Console.WriteLine();
                var closedCalls = s_bl.Call.GetClosedCallsByVolunteer(volunteerId, callTypeFilter, sortByField);
                if (closedCalls.Count()!=0)
                {
                    foreach (var call in closedCalls)
                    {
                        Console.WriteLine(Tools.ToStringProperty(call));
                    }
                }
                else
                {
                    Console.WriteLine("No closed calls found for the specified filters.");
                }
            }
            catch (BO.BlDoesNotExistException ex)
            {
                Console.WriteLine($"Login failed: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.GetType().Name}, Message: {ex.InnerException.Message}");
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        public static void TestGetOpenCallsByVolunteer()
        {
            Console.WriteLine("\n--- Retrieve Open Calls by Volunteer with Optional Filtering and Sorting ---");
            try
            {

                Console.Write("Enter Volunteer ID:");
                string volunteerIdInput = Console.ReadLine();
                if (!int.TryParse(volunteerIdInput, out int volunteerId))
                {
                    Console.WriteLine("Invalid Volunteer ID.");
                    return;
                }

                Console.WriteLine("Available Call Types:");
                foreach (var callType in Enum.GetValues(typeof(BO.CallType)))
                {
                    Console.WriteLine($"{(int)callType}. {callType}");
                }
                Console.Write("Enter Call Type (Enter number or press Enter for all): ");
                string callTypeInput = Console.ReadLine();
                BO.CallType? callTypeFilter = null;

                if (!string.IsNullOrEmpty(callTypeInput))
                {
                    if (int.TryParse(callTypeInput, out int callTypeValue) &&
                        Enum.IsDefined(typeof(BO.CallType), callTypeValue))
                    {
                        callTypeFilter = (BO.CallType)callTypeValue;
                    }
                    else if (Enum.TryParse(typeof(BO.CallType), callTypeInput, true, out var parsedEnumValue))
                    {
                        callTypeFilter = (BO.CallType)parsedEnumValue;
                    }
                    else
                    {
                        Console.WriteLine("Invalid Call Type. The system will return all types of calls.");
                    }
                }



                
                Console.WriteLine("--Available Sort Fields--");
                foreach (var sortField in Enum.GetValues(typeof(OpenCallSortField)))
                {
                    Console.WriteLine($"{(int)sortField}. {sortField}");
                }
                Console.Write("Enter Sort Field: ");
                string sortByFieldInput = Console.ReadLine();
                OpenCallSortField? sortByField = null;

                if (!string.IsNullOrEmpty(sortByFieldInput))
                {
                    if (Enum.TryParse<OpenCallSortField>(sortByFieldInput, true, out OpenCallSortField parsedSortField))
                    {
                        sortByField = parsedSortField;
                    }
                    else if (int.TryParse(sortByFieldInput, out int numericValue) &&
                             Enum.IsDefined(typeof(OpenCallSortField), numericValue))
                    {
                        sortByField = (OpenCallSortField)numericValue;
                    }
                    else
                    {
                        Console.WriteLine("Invalid Sort Field. Defaulting to sorting by ID.");
                      
                    }
                }



                var openCalls = s_bl.Call.GetOpenCallsByVolunteer(volunteerId, callTypeFilter, sortByField);
                if (openCalls.Any())
                {
                    foreach (var call in openCalls)
                    {
                        Console.WriteLine(Tools.ToStringProperty(call)); 
                    }
                }
                else
                {
                    Console.WriteLine("No closed calls found for the specified filters.");
                }
            }
            catch (BO.BlDoesNotExistException ex)
            {
                Console.WriteLine($"Login failed: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.GetType().Name}, Message: {ex.InnerException.Message}");
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }



        }
        public static void TestCompleteAssignment()
        {
            Console.WriteLine("\n--- Complete Assignment for Volunteer ---");

            Console.Write("Enter Volunteer ID:");
            string volunteerInput = Console.ReadLine();
            if (!int.TryParse(volunteerInput, out int volunteerId))
            {
                Console.WriteLine("Invalid Volunteer ID. Please enter a valid number.");
                return;
            }

            Console.Write("Enter Assignment ID:");
            string assignmentInput = Console.ReadLine();
            if (!int.TryParse(assignmentInput, out int assignmentId))
            {
                Console.WriteLine("Invalid Assignment ID. Please enter a valid number.");
                return;
            }

            try
            {

                s_bl.Call.CompleteAssignment(volunteerId, assignmentId);
                Console.WriteLine($"Assignment {assignmentId} has been successfully completed by Volunteer {volunteerId}.");
            }
            catch (BO.BlDoesNotExistException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (BO.BlInvalidRoleException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }
        public static void TestCancelAssignment()
        {
            Console.WriteLine("\n--- Cancel Assignment for Volunteer ---");
            Console.Write("Enter Volunteer ID:");
            string volunteerInput = Console.ReadLine();
            if (!int.TryParse(volunteerInput, out int volunteerId))
            {
                Console.WriteLine("Invalid Volunteer ID. Please enter a valid number.");
                return;
            }

            Console.Write("Enter Assignment ID:");
            string assignmentInput = Console.ReadLine();
            if (!int.TryParse(assignmentInput, out int assignmentId))
            {
                Console.WriteLine("Invalid Assignment ID. Please enter a valid number.");
                return;
            }

            try
            {
                // קריאה לפונקציה לביטול ההקצאה
                s_bl.Call.CancelAssignment(volunteerId, assignmentId);
                Console.WriteLine($"Assignment {assignmentId} has been successfully canceled by Volunteer {volunteerId}.");
            }
            catch (BO.BlDoesNotExistException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (BO.BlInvalidRoleException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }
        public static void TestAssignCallToVolunteer()
        {
            Console.WriteLine("\n--- Assign Call to Volunteer ---");
            Console.WriteLine("Enter Volunteer ID:");
            string volunteerInput = Console.ReadLine();
            if (!int.TryParse(volunteerInput, out int volunteerId))
            {
                Console.WriteLine("Invalid Volunteer ID. Please enter a valid number.");
                return;
            }

            Console.WriteLine("Enter Call ID:");
            string callInput = Console.ReadLine();
            if (!int.TryParse(callInput, out int callId))
            {
                Console.WriteLine("Invalid Call ID. Please enter a valid number.");
                return;
            }

            try
            {
                
                s_bl.Call.AssignCallToVolunteer(volunteerId, callId);
                Console.WriteLine($"Call {callId} has been successfully assigned to Volunteer {volunteerId}.");
            }
            catch (BO.BlDoesNotExistException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (BO.BlInvalidValueException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (BO.BlAlreadyExistsException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }
        private static void PrintEnumOptions<T>() where T : Enum
        {
            foreach (var value in Enum.GetValues(typeof(T)))
            {
                Console.WriteLine($" - {value}");
            }
        }
        private static void PrintEnumOptions(Type enumType)
        {
            foreach (var value in Enum.GetValues(enumType))
            {
                Console.WriteLine($" - {value}");
            }
        }
        private static void PrintCalls(IEnumerable<BO.CallInList> calls)
        {
            foreach (var call in calls)
            {
                Console.WriteLine($"CallId: {call.CallId}, " +
                                  $"Type: {call.Type}, " +
                                  $"OpeningTime: {call.OpeningTime}, " +
                                  $"RemainingTime: {call.RemainingTime?.ToString() ?? "N/A"}, " +
                                  $"LastVolunteer: {call.LastVolunteer ?? "N/A"}, " +
                                  $"TotalCompletionTime: {call.TotalCompletionTime?.ToString() ?? "N/A"}, " +
                                  $"TotalAssignments: {call.TotalAssignments}, " +
                                  $"Status: {call.Status}");
            }
        }
        private static void PrintCallTypes()
        {
            Console.WriteLine("Please choose a Call Type from the following options:");

            foreach (BO.CallType type in Enum.GetValues(typeof(BO.CallType)))
            {
                Console.WriteLine($"{(int)type}: {type}");
            }
        }
        private static void UpdateCallField(string fieldName, Action<string> updateAction, Dictionary<string, bool> fieldsUpdated,
            Func<string, bool> validate = null, string errorMessage = null)
        {
            Console.WriteLine($"Enter new value for {fieldName} (press Enter to skip):");
            string input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input))
            {
                if (validate == null || validate(input))
                {
                    updateAction(input);
                    fieldsUpdated[fieldName] = true;
                }
                else
                {
                    Console.WriteLine(errorMessage ?? "Invalid input.");
                }
            }
            else
            {
                fieldsUpdated[fieldName] = false;
            }
        }



        private static void HandleAdminMenu()
        {
            while (true)
            {
                DisplayAdminMenu();
                AdminMenuOptions? selectedOption = GetAdminMenuChoice();

                if (selectedOption.HasValue)
                {
                    switch (selectedOption.Value)
                    {
                        case AdminMenuOptions.ViewSystemClock:
                            TestGetSystemClock();
                            break;
                        case AdminMenuOptions.AdvanceSystemClock:
                            TestAdvanceSystemClock();
                            break;
                        case AdminMenuOptions.ViewRiskTimeRange:
                            TestGetRiskTimeRange();
                            break;
                        case AdminMenuOptions.SetRiskTimeRange:
                            TestSetRiskTimeRange();
                            break;
                        case AdminMenuOptions.ResetDatabase:
                            TestResetDatabase();
                            break;
                        case AdminMenuOptions.InitializeDatabase:
                            TestInitializeDatabase();
                            break;
                        case AdminMenuOptions.Exit:
                            
                            return;
                        default:
                            Console.WriteLine("Invalid option. Please try again.");
                            break;
                    }
                }
            }
        }
        private static AdminMenuOptions? GetAdminMenuChoice()
        {
            Console.Write("Enter your choice: ");
            string? input = Console.ReadLine();

            if (Enum.TryParse(input, out AdminMenuOptions selectedOption) && Enum.IsDefined(typeof(AdminMenuOptions), selectedOption))
            {
                return selectedOption;
            }
            else
            {
                Console.WriteLine("Invalid input, please enter a valid option.");
                return null;
            }
        }
        public static void TestGetSystemClock()
        {
            Console.WriteLine("--- Retrieve Current System Clock ---");
            try
            {
                AdminImplementation admin = new AdminImplementation();
                DateTime systemClock = admin.GetSystemClock();
                Console.WriteLine($"Current System Clock: {systemClock}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        public static void TestAdvanceSystemClock()
        {
            Console.WriteLine("--- Advance System Clock Based on User Input ---");
            AdminImplementation admin = new AdminImplementation();
            DateTime initialClock = ClockManager.Now;

            while (true)
            {
                // מציג את אפשרויות המשתמש
                Console.WriteLine("Now advancing the system clock. By how much would you like to advance?");
                Console.WriteLine("1. Minute");
                Console.WriteLine("2. Hour");
                Console.WriteLine("3. Day");
                Console.WriteLine("4. Month");
                Console.WriteLine("5. Year");
                Console.WriteLine("Enter a number between 1 and 5, or press Enter to exit:");

                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Exiting...");
                    break;
                }

                if (!int.TryParse(input, out int choice) || choice < 1 || choice > 5)
                {
                    Console.WriteLine("Invalid input. Please enter a number between 1 and 5.");
                    continue;
                }

                BO.TimeUnit timeUnit;
                switch (choice)
                {
                    case 1:
                        timeUnit = BO.TimeUnit.MINUTE;
                        break;
                    case 2:
                        timeUnit = BO.TimeUnit.HOUR;
                        break;
                    case 3:
                        timeUnit = BO.TimeUnit.DAY;
                        break;
                    case 4:
                        timeUnit = BO.TimeUnit.MONTH;
                        break;
                    case 5:
                        timeUnit = BO.TimeUnit.YEAR;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please select a number between 1 and 5.");
                        continue;
                }

                Console.WriteLine($"Advancing the system clock by {timeUnit}...");
                DateTime oldClock = ClockManager.Now; // שמירת השעון הנוכחי לפני הקידום
                s_bl.Admin.AdvanceSystemClock(timeUnit); // קידום השעון
                DateTime newClock = ClockManager.Now; // שמירת השעון לאחר הקידום

                // הצגת הפלט
                Console.WriteLine($"Old Clock: {oldClock}");
                Console.WriteLine($"New Clock: {newClock}");

                initialClock = newClock; // עדכון השעון ההתחלתי לבדיקה הבאה
            }
        }
        public static void TestGetRiskTimeRange()
        {
            Console.WriteLine("--- Retrieve Current Risk Time Range ---");
            try
            {
                var admin = new AdminImplementation();
                TimeSpan result = admin.GetRiskTimeRange();
                Console.WriteLine($"Risk Time Range: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

        }
        public static void TestSetRiskTimeRange()
        {
            Console.WriteLine("--- Set and Verify New Risk Time Range ---");
            // Create an instance of AdminImplementation
            var admin = new AdminImplementation();

            // Prompt the user to enter the risk time range
            Console.WriteLine("Please enter the Risk Time Range:");
            Console.WriteLine("Example format: 10.00:00:00 (10 days), 02:00:00 (2 hours), 00:30:00 (30 minutes)");

            // Read user input
            string userInput = Console.ReadLine();

            // Try to parse the input as TimeSpan
            if (TimeSpan.TryParse(userInput, out TimeSpan result))
            {
                // Call the SetRiskTimeRange method with the parsed value
                admin.SetRiskTimeRange(result);

                // Call the GetRiskTimeRange method to verify if the value was updated
                TimeSpan updatedRiskTimeRange = admin.GetRiskTimeRange();

                // Display the updated value to verify the change
                Console.WriteLine($"Updated Risk Time Range: {updatedRiskTimeRange}");
            }
            else
            {
                // If input is invalid, notify the user
                Console.WriteLine("Invalid input. Please enter a valid TimeSpan format (e.g., 10.00:00:00 for 10 days).");
            }
        }
        public static void TestResetDatabase()
        {
            Console.WriteLine("--- Reset Database to Default State ---");
            try
            {
                // Create an instance of AdminImplementation to access the ResetDatabase method
                var admin = new AdminImplementation();

                // Inform the user that the reset operation is about to start
                Console.WriteLine("Starting database reset...");

                // Call the ResetDatabase method to reset the database
                admin.ResetDatabase();

                // If we reach this point without an exception, the reset was successful
                Console.WriteLine("Database reset successfully!");
            }
            catch (Exception ex)
            {
                // In case of an error, display the exception message
                Console.WriteLine($"Error resetting database: {ex.Message}");
            }
        }
        public static void TestInitializeDatabase()
        {
            Console.WriteLine("--- Initialize Database with Default Configuration ---");
            try
            {
                // Create an instance of AdminImplementation to access the InitializeDatabase method
                var admin = new AdminImplementation();

                // Inform the user that the initialization is starting
                Console.WriteLine("Starting database initialization...");

                // Call the InitializeDatabase method to initialize the database
                admin.InitializeDatabase();

                // If we reach this point without an exception, the initialization was successful
                Console.WriteLine("Database initialized successfully!");
            }
            catch (BlAlreadyExistsException ex)
            {
                // Specific handling for the case when the database already exists
                Console.WriteLine($"Database already exists: {ex.Message}");
            }
            catch (DatabaseInitializationException ex)
            {
                // Handling other errors related to database initialization
                Console.WriteLine($"Error initializing database: {ex.Message}");
            }
            catch (Exception ex)
            {
                // General error handling
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }


        /// <summary>
        /// Displays the main menu with options for managing volunteers, calls, assignments, configurations, and more.
        /// </summary>
        /// 
        private static void DisplayMainMenu()
        {
            Console.WriteLine("╔═══════════════════════════╗");
            Console.WriteLine("║         Main Menu         ║");
            Console.WriteLine("╠═══════════════════════════╣");
            Console.WriteLine("║ 1. Manage Volunteers      ║");
            Console.WriteLine("║ 2. Manage Calls           ║");
            Console.WriteLine("║ 3. Manage Admin           ║");
            Console.WriteLine("║ 0. Exit                   ║");
            Console.WriteLine("╚═══════════════════════════╝");
        }

        private static void DisplayVolunteerMenu()
        {
            Console.WriteLine("\n----- Volunteer Management Menu -----");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. View All Volunteers");
            Console.WriteLine("3. View Volunteer Details");
            Console.WriteLine("4. Update Volunteer Details");
            Console.WriteLine("5. Change Your Password");
            Console.WriteLine("6. Add Volunteer");
            Console.WriteLine("7. Delete Volunteer");
            Console.WriteLine("0. Back to Main Menu");
        }
        private static void DisplayCallMenu()
        {
            Console.WriteLine("\n----- Call Management Menu -----");
            Console.WriteLine("1. Assign Call to Volunteer");
            Console.WriteLine("2. View All Calls");
            Console.WriteLine("3. View Call Details");
            Console.WriteLine("4. Add New Call");
            Console.WriteLine("5. Update Call Details");
            Console.WriteLine("6. Delete Call");
            Console.WriteLine("7. View Closed Calls by Volunteer");
            Console.WriteLine("8. View Open Calls by Volunteer");
            Console.WriteLine("9. Complete Assignment");
            Console.WriteLine("10. Cancel Assignment");
            Console.WriteLine("11. View Call Statuses Count");
            Console.WriteLine("0. Back to Main Menu");
        }
        private static void DisplayAdminMenu()
        {
            Console.WriteLine("\n----- Admin Management Menu -----");
            Console.WriteLine("1. View System Clock");
            Console.WriteLine("2. Advance System Clock");
            Console.WriteLine("3. View Risk Time Range");
            Console.WriteLine("4. Set Risk Time Range");
            Console.WriteLine("5. Reset Database");
            Console.WriteLine("6. Initialize Database");
            Console.WriteLine("0. Back to Main Menu");
        }
    }
}
