using System;
using DalApi;
using DalApi;
using DO;

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
        AddVolunteer = 1,
        ViewAllVolunteers = 2,
        ViewVolunteerDetails = 3,
        UpdateVolunteerDetails = 4,
        DeleteVolunteer = 5,
        Login = 6,
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
        ViewCallStatusesCount, // הוספת פונקציה חסרה
        Exit
    }
    public enum AdminMenuOptions
    {
        ViewSystemClock = 1,
        AdvanceSystemClock,
        ViewRiskTimeRange,
        SetRiskTimeRange,
        ResetDatabase,
        InitializeDatabase,
        Exit
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
                        case VolunteerMenuOption.AddVolunteer:
                            AddVolunteer();
                            break;
                        case VolunteerMenuOption.ViewAllVolunteers:
                            GetVolunteersList();
                            break;
                        case VolunteerMenuOption.ViewVolunteerDetails:
                            GetVolunteerDetails();
                            break;
                        case VolunteerMenuOption.UpdateVolunteerDetails:
                            UpdateVolunteer();
                            break;
                        case VolunteerMenuOption.DeleteVolunteer:
                            DeleteVolunteer();
                            break;
                        case VolunteerMenuOption.Login:
                            Login();
                            break;
                        default:
                            Console.WriteLine("Invalid option. Please try again.");
                            break;
                    }
                }
            }
        }
        private static VolunteerMenuOption? GetVolunteerMenuChoice()
        {
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
        public static void Login()
        {
            var volunteerRole = new BO.VolunteerRole();
        }
        public static void GetVolunteersList()
        {
            var volunteers = new List<BO.VolunteerInList>();
        }
        public static void GetVolunteerDetails()
        {
            var volunteer = new BO.Volunteer();
        }
        public static void UpdateVolunteer()
        {
        }
        public static void DeleteVolunteer()
        {
           

        }
        public static void AddVolunteer()
        {
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
                            AssignCallToVolunteer();
                            break;
                        case CallMenuOptions.ViewAllCalls:
                            GetCalls();
                            break;
                        case CallMenuOptions.ViewCallDetails:
                            GetCallDetailsById();
                            break;
                        case CallMenuOptions.AddNewCall:
                            AddCall();
                            break;
                        case CallMenuOptions.UpdateCallDetails:
                            UpdateCallDetails();
                            break;
                        case CallMenuOptions.DeleteCall:
                            DeleteCall();
                            break;
                        case CallMenuOptions.ViewClosedCallsByVolunteer:
                            GetClosedCallsByVolunteer();
                            break;
                        case CallMenuOptions.ViewOpenCallsByVolunteer:
                            GetOpenCallsByVolunteer();
                            break;
                        case CallMenuOptions.CompleteAssignment:
                            CompleteAssignment();
                            break;
                        case CallMenuOptions.CancelAssignment:
                            CancelAssignment();
                            break;
                        case CallMenuOptions.ViewCallStatusesCount: // פונקציה חדשה
                            GetCallStatusesCount();
                            break;
                        case CallMenuOptions.Exit:
                            Console.WriteLine("Exiting the Call Management Menu.");
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
        public static void GetCallStatusesCount()
        {
           
        }
        public static void GetCalls()
        {
           

        }
        public static void GetCallDetailsById()
        {

        }
        public static void UpdateCallDetails()
        {
        }
        public static void DeleteCall()
        {
        }
        public static void AddCall()
        {
        }
        public static void GetClosedCallsByVolunteer()
        {
        }
        public static void GetOpenCallsByVolunteer()
        {
        }
        public static void CompleteAssignment()
        {
        }
        public static void CancelAssignment()
        {
        }
        public static void AssignCallToVolunteer()
        {
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
                            GetSystemClock();
                            break;
                        case AdminMenuOptions.AdvanceSystemClock:
                            AdvanceSystemClock();
                            break;
                        case AdminMenuOptions.ViewRiskTimeRange:
                            GetRiskTimeRange();
                            break;
                        case AdminMenuOptions.SetRiskTimeRange:
                            SetRiskTimeRange();
                            break;
                        case AdminMenuOptions.ResetDatabase:
                            ResetDatabase();
                            break;
                        case AdminMenuOptions.InitializeDatabase:
                            InitializeDatabase();
                            break;
                        case AdminMenuOptions.Exit:
                            Console.WriteLine("Exiting the Admin Management Menu.");
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
        public static void GetSystemClock()
        {
        }
        public static void AdvanceSystemClock()
        {
        }
        public static void GetRiskTimeRange()
        {
            
        }
        public static void SetRiskTimeRange()
        {
        }
        public static void ResetDatabase()
        {
        }
        public static void InitializeDatabase()
        {
        }




        /// <summary>
        /// Displays the main menu with options for managing volunteers, calls, assignments, configurations, and more.
        /// </summary>
        /// 
        private static void DisplayMainMenu()
        {
            Console.WriteLine("\n===== Main Menu =====");
            Console.WriteLine("1. Manage Volunteers");
            Console.WriteLine("2. Manage Calls");
            Console.WriteLine("3. Manage Admin");
            Console.WriteLine("0. Exit");
        }
        private static void DisplayVolunteerMenu()
        {
            Console.WriteLine("\n===== Volunteer Management Menu =====");
            Console.WriteLine("1. Add Volunteer");
            Console.WriteLine("2. View All Volunteers");
            Console.WriteLine("3. View Volunteer Details");
            Console.WriteLine("4. Update Volunteer Details");
            Console.WriteLine("5. Delete Volunteer");
            Console.WriteLine("6. Volunteer Login");
            Console.WriteLine("7. Back to Main Menu");
        }
        private static void DisplayCallMenu()
        {
            Console.WriteLine("\n===== Call Management Menu =====");
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
            Console.WriteLine("11. View Call Statuses Count"); // שורת פונקציה חדשה
            Console.WriteLine("0. Back to Main Menu");
        }
        private static void DisplayAdminMenu()
        {
            Console.WriteLine("\n===== Admin Management Menu =====");
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
