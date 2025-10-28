using PL.Volunteer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PL
{
    /// <summary>
    /// Interaction logic for MainVolunteerScreen.xaml
    /// </summary>
    public partial class MainVolunteerScreen : Window, INotifyPropertyChanged
    {
        // Stage 7 – PL 5A (Option 1 - flag)
        private volatile bool _updateVolunteerDataWorking = false;

        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        public event PropertyChangedEventHandler? PropertyChanged;

        public BO.DistanceType distanceType { get; set; } = BO.DistanceType.Aerial;
        public DistanceTypeCollection DistanceTypesCollection { get; set; } = new();

        public BO.Activity ActivityFilter { get; set; } = BO.Activity.All;
        public ActivityCollection ActivityCollection { get; set; } = new();

        public BO.VolunteerRole VolunteerRole { get; set; } = BO.VolunteerRole.Volunteer;
        public VolunteerRoleCollection volunteerRoleCollection { get; set; } = new();

        // מאפיין חדש לציון מצב טעינה
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                    UpdateLoadingIndicator();
                }
            }
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public BO.Volunteer? CurrentVolunteer
        {
            get { return (BO.Volunteer?)GetValue(CurrentVolunteerProperty); }
            set { SetValue(CurrentVolunteerProperty, value); OnPropertyChanged(); }
        }

        public static readonly DependencyProperty CurrentVolunteerProperty =
            DependencyProperty.Register("CurrentVolunteer", typeof(BO.Volunteer), typeof(MainVolunteerScreen), new PropertyMetadata(null));

        private string _welcomeMessage;
        public string WelcomeMessage
        {
            get { return _welcomeMessage; }
            set
            {
                _welcomeMessage = value;
                OnPropertyChanged();
            }
        }

        private int volunteerID;
        
        public MainVolunteerScreen(int id)
        {
            try
            {
                volunteerID = id;

                // אתחול הממשק ללא נתונים תחילה
                InitializeComponent();

                // הוספת אירועים
                Loaded += MainVolunteerScreen_Loaded;
                Closing += MainVolunteerScreen_Closing;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add a public getter so other windows can check which volunteer id this window represents
        public int VolunteerId => volunteerID;

        // טעינת הנתונים באירוע Loaded
        private async void MainVolunteerScreen_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // טעינת נתוני המתנדב באופן אסינכרוני
                var volunteer = await s_bl.Volunteer.GetVolunteerDetailsAsync(volunteerID);
                if (volunteer == null)
                {
                    MessageBox.Show("No volunteer data available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                CurrentVolunteer = volunteer;
                string firstName = volunteer.FullName.Contains(" ")
                    ? volunteer.FullName.Substring(0, volunteer.FullName.IndexOf(" "))
                    : volunteer.FullName;

                WelcomeMessage = $"Welcome {firstName}";

                // טעינת תמונת הפרופיל
                LoadProfileImage(CurrentVolunteer.Id);

                // הוספת צופה לעדכונים בזמן אמת
                s_bl.Volunteer.AddObserver(CurrentVolunteer.Id, UpdateVolunteerData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading volunteer data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MainVolunteerScreen_Closing(object sender, CancelEventArgs e)
        {
            // Remove observer when closing
            s_bl.Volunteer.RemoveObserver(CurrentVolunteer.Id, UpdateVolunteerData);
        }

        // פונקציה חדשה לעדכון מחוון הטעינה
        private void UpdateLoadingIndicator()
        {
            if (LoadingIndicator != null)
            {
                if (IsLoading)
                {
                    LoadingIndicator.Visibility = Visibility.Visible;
                }
                else
                {
                    LoadingIndicator.Visibility = Visibility.Collapsed;
                }
            }
        }


        public void UpdateVolunteerData()
        {
            if (!_updateVolunteerDataWorking)
            {
                _updateVolunteerDataWorking = true;

                _ = Dispatcher.BeginInvoke(new Action(async () =>
                {
                    try
                    {
                        if (CurrentVolunteer != null && CurrentVolunteer.Id != 0)
                        {
                            var updatedVolunteer = await s_bl.Volunteer.GetVolunteerDetailsAsync(CurrentVolunteer.Id);
                            if (updatedVolunteer != null)
                            {
                                CurrentVolunteer = updatedVolunteer;

                                if (!string.IsNullOrEmpty(updatedVolunteer.FullName))
                                {
                                    string firstName = updatedVolunteer.FullName.Contains(" ")
                                        ? updatedVolunteer.FullName.Substring(0, updatedVolunteer.FullName.IndexOf(" "))
                                        : updatedVolunteer.FullName;

                                    WelcomeMessage = $"Welcome {firstName}";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error updating volunteer data: {ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        _updateVolunteerDataWorking = false;
                    }
                }));
            }
        }

        

        // שינוי UpdateButton_Click להיות אסינכרוני ולהציג את מחוון הטעינה
        public async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // הפעלת מחוון הטעינה
                IsLoading = true;

                // שמירת מזהה המתנדב הנוכחי למקרה של כישלון
                int volunteerId = CurrentVolunteer.Id;

                // עדכון פרטי המתנדב
                CurrentVolunteer.Password = CurrentVolunteer.Password;
                await s_bl.Volunteer.UpdateVolunteerAsync(CurrentVolunteer.Id, CurrentVolunteer, null);
                OnPropertyChanged(nameof(CurrentVolunteer));

                // כיבוי מחוון הטעינה
                IsLoading = false;

                // הצגת הודעת הצלחה
                MessageBox.Show("Your information has been updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Operation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // טעינה מחדש של נתוני המתנדב מהמסד נתונים כדי לבטל שינויים
                try
                {
                    // קבלת הנתונים העדכניים מהמסד נתונים
                    CurrentVolunteer = await s_bl.Volunteer.GetVolunteerDetailsAsync(CurrentVolunteer.Id);
                    OnPropertyChanged(nameof(CurrentVolunteer));
                }
                catch (Exception reloadEx)
                {
                    // אם הטעינה מחדש נכשלת, יידע את המשתמש
                    MessageBox.Show($"Failed to reload original data: {reloadEx.Message}",
                                   "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                finally
                {
                    // כיבוי מחוון הטעינה גם במקרה של שגיאה
                    IsLoading = false;
                }
            }
        }

        public void OnOpenActiveCallClick(object sender, RoutedEventArgs e)
        {
            if (CurrentVolunteer.ActiveCall == null)
            {
                MessageBox.Show("You don't have any active calls at the moment.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                var MainActiveCallDetailsWindow = new MainActiveCallDetailsWindow(CurrentVolunteer.ActiveCall, CurrentVolunteer.Id, this);
                MainActiveCallDetailsWindow.ShowDialog();
            }
        }

        public MainVolunteerScreen() : this(0)
        {
        }

        public const string ImageFolder = "ProfileImage"; // Image folder

        /// <summary>
        /// Loads the profile image based on volunteer ID
        /// </summary>
        /// <param name="volunteerId">Volunteer ID</param>
        public void LoadProfileImage(int volunteerId)
        {
            // Get image path from folder
            string imagePath = System.IO.Path.Combine(AppContext.BaseDirectory, ImageFolder, $"{volunteerId}.jpg");

            if (File.Exists(imagePath))
            {
                // Display image in WPF
                ProfileImageBrush.ImageSource = new BitmapImage(new Uri(imagePath));
            }
            else
            {
                // Display default image if volunteer image doesn't exist
                ProfileImageBrush.ImageSource = new BitmapImage(new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, ImageFolder, "default.jpg")));
            }
        }

        private void OnViewCallHistoryClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a new instance of the CallHistoryWindow
                // Pass the current volunteer's ID to the window
                var callHistoryWindow = new CallHistoryWindow(CurrentVolunteer.Id);
                callHistoryWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening call history: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add this method to your MainVolunteerScreen class
        public void OnSelectCallClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if the volunteer already has an active call
                if (CurrentVolunteer.ActiveCall != null)
                {
                    MessageBox.Show("You already have an active call. Please complete or cancel it before selecting a new one.",
                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Check if the volunteer has a valid address
                if (string.IsNullOrWhiteSpace(CurrentVolunteer.CurrentAddress))
                {
                    MessageBox.Show("Can't display the open calls list because a valid address hasn't been set. Please enter a valid address first, and then you'll be able to view the open calls list",
                        "Missing Address", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
              
                // Open the call selection window
                var openCallsWindow = new OpenCallsWindow(
                    CurrentVolunteer.Id,
                    this,
                    CurrentVolunteer.Latitude.GetValueOrDefault(31.895049),
                    CurrentVolunteer.Longitude.GetValueOrDefault(34.810181)
                );
              
                openCallsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening call selection window: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add this method to refresh the data after a call is selected
        public async Task RefreshVolunteerDataAsync()
        {
            try
            {
                // Reload volunteer data from the database
                CurrentVolunteer = await s_bl.Volunteer.GetVolunteerDetailsAsync(CurrentVolunteer.Id);

                // Update UI bindings
                OnPropertyChanged(nameof(CurrentVolunteer));
                OnPropertyChanged(nameof(WelcomeMessage));

                // Update UI state based on active call status
                UpdateUIForActiveCall();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing volunteer data: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateUIForActiveCall()
        {
            // Enable/disable buttons based on whether the volunteer has an active call
            bool hasActiveCall = CurrentVolunteer.ActiveCall != null;

            // You may need to adjust these control names to match your actual UI
            if (this.FindName("SelectCallButton") is Button selectCallButton)
                selectCallButton.IsEnabled = !hasActiveCall;

            if (this.FindName("ActiveCallButton") is Button activeCallButton)
                activeCallButton.IsEnabled = hasActiveCall;
        }
    }
    //public class DistanceTypeCollection : List<string>
    //{
    //    public DistanceTypeCollection()
    //    {
    //        Add("Aerial");
    //        Add("Walking");
    //        Add("Driving");

    //    }
       
    //}

    //public class ActivityCollection : List<string>
    //{
    //    public ActivityCollection()
    //    {
    //        Add("All");
    //        Add("Babysitting");
    //        Add("Shopping");
    //        Add("DogWalking");
    //        Add("Other");
    //    }
    //}

    //public class VolunteerRoleCollection : List<string>
    //{
    //    public VolunteerRoleCollection()
    //    {
    //        Add("Volunteer");
    //        Add("Manager");
         
    //    }
       
    //}
}

