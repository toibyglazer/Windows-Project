using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BO;
using PL.Volunteer;

namespace PL
{
    /// <summary>
    /// Interaction logic for CallHistoryWindow.xaml
    /// </summary>
    public partial class CallHistoryWindow : Window, INotifyPropertyChanged
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private readonly int _volunteerId;

        // Constants
        public const string ImageFolder = "ProfileImage"; // Image folder

        // Volunteer Info Properties
        public string VolunteerName { get; private set; }

        // Regular properties for filters
        public CallType SelectedCallType { get; set; } = CallType.All;

        // Use a string for the sort field key to avoid null key issues
        public string SelectedSortKey { get; set; } = "default";

        // Collections for ComboBoxes
        public CallTypesCollection CallTypes { get; } = new CallTypesCollection();

        // Use dependency property for the list
        public IEnumerable<ClosedCallInList> ClosedCalls
        {
            get { return (IEnumerable<ClosedCallInList>)GetValue(ClosedCallsProperty); }
            set { SetValue(ClosedCallsProperty, value); }
        }

        public static readonly DependencyProperty ClosedCallsProperty =
            DependencyProperty.Register("ClosedCalls", typeof(IEnumerable<ClosedCallInList>),
                typeof(CallHistoryWindow), new PropertyMetadata(null));

        public event PropertyChangedEventHandler PropertyChanged;

        public CallHistoryWindow(int volunteerId)
        {
            InitializeComponent();
            _volunteerId = volunteerId;
            DataContext = this;

            // Register for window events
            Loaded += Window_Loaded;
            Closed += Window_Closed;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load volunteer data - now using the async version
                await LoadVolunteerDataAsync();

                // Load volunteer profile image
                LoadProfileImage(_volunteerId);

                // Initialize filter and sort comboboxes
                InitializeComboBoxes();

                // Add observer for call updates
                s_bl.Call.AddObserver(CallHistoryObserver);

                // Load initial data
                UpdateCallHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing window: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                // Remove observer when window is closed
                s_bl.Call.RemoveObserver(CallHistoryObserver);
            }
            catch (Exception ex)
            {
                // Just log the error, don't show a message box since the window is closing
                System.Diagnostics.Debug.WriteLine($"Error removing observer: {ex.Message}");
            }
        }

        private async Task LoadVolunteerDataAsync()
        {
            try
            {
                // Get volunteer data
                var volunteer = await s_bl.Volunteer.GetVolunteerDetailsAsync(_volunteerId);
                VolunteerName = volunteer.FullName;

                // Notify UI
                OnPropertyChanged(nameof(VolunteerName));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading volunteer data: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void LoadProfileImage(int volunteerId)
        {
            try
            {
                // Get image path from folder
                string imagePath = System.IO.Path.Combine(AppContext.BaseDirectory, ImageFolder, $"{volunteerId}.jpg");

                if (File.Exists(imagePath))
                {
                    // Display image in WPF
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    // Set the image source
                    VolunteerProfileImageBrush.ImageSource = bitmap;
                }
                else
                {
                    // Display default image if volunteer image doesn't exist
                    string defaultPath = System.IO.Path.Combine(AppContext.BaseDirectory, ImageFolder, "default.jpg");

                    if (File.Exists(defaultPath))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(defaultPath);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();

                        // Set the image source
                        VolunteerProfileImageBrush.ImageSource = bitmap;
                    }
                    else
                    {
                        // If default image doesn't exist, use a solid color
                        VolunteerProfileBorder.Background = new SolidColorBrush(Color.FromRgb(229, 231, 235));
                    }
                }
            }
            catch (Exception ex)
            {
                // If there's an error loading the image, just log it and continue
                System.Diagnostics.Debug.WriteLine($"Error loading profile image: {ex.Message}");

                // Use a solid color as fallback
                VolunteerProfileBorder.Background = new SolidColorBrush(Color.FromRgb(229, 231, 235));
            }
        }

        private void InitializeComboBoxes()
        {
            try
            {
                // For CallTypeFilter, use CallTypesCollection
                CallTypeFilter.ItemsSource = CallTypes;
                CallTypeFilter.SelectedValue = CallType.All;

                // Setup Sort By Field ComboBox using strings as keys
                var sortFields = new Dictionary<string, string>
                {
                    { "default", "Default (ID)" },
                    { "id", "ID" },
                    { "type", "Call Type" },
                    { "address", "Address" },
                    { "openingTime", "Opening Time" },
                    { "entryTime", "Entry Time" },
                    { "completionTime", "Completion Time" },
                    { "status", "Status" }
                };

                SortByField.ItemsSource = sortFields;
                SortByField.DisplayMemberPath = "Value";
                SortByField.SelectedValuePath = "Key";
                SortByField.SelectedValue = "default";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error initializing combo boxes: {ex.Message}", ex);
            }
        }

        public void UpdateCallHistory()
        {
            try
            {
                // Convert string key to enum value
                ClosedCallSortField? sortField = GetSortFieldFromKey(SelectedSortKey);

                // Get closed calls from BL layer
                var closedCalls = s_bl.Call.GetClosedCallsByVolunteer(
                    _volunteerId,
                    SelectedCallType == CallType.All ? null : SelectedCallType,
                    sortField);

                // Update the collection
                ClosedCalls = closedCalls.ToList();
                OnPropertyChanged(nameof(ClosedCalls));
                AdjustColumnWidths();
                // If no calls found, show a message

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading call history: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Helper method to convert string key to enum value
        private ClosedCallSortField? GetSortFieldFromKey(string key)
        {
            switch (key)
            {
                case "id": return ClosedCallSortField.Id;
                case "type": return ClosedCallSortField.Type;
                case "address": return ClosedCallSortField.FullAddress;
                case "openingTime": return ClosedCallSortField.OpeningTime;
                case "entryTime": return ClosedCallSortField.EntryTime;
                case "completionTime": return ClosedCallSortField.CompletionTime;
                case "status": return ClosedCallSortField.CompletionType;
                default: return null;
            }
        }

        // Stage 7 – PL 5A: coalescing + marshaling to UI thread
        private volatile bool _callHistoryObserverWorking = false;

        public void CallHistoryObserver()
        {
            if (!_callHistoryObserverWorking)
            {
                _callHistoryObserverWorking = true;

                _ = Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        // כל עדכון ה-UI חייב לרוץ על ה-UI thread
                        UpdateCallHistory();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error updating call history: {ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        _callHistoryObserverWorking = false;
                    }
                });
            }
        }

        private void CallTypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (CallTypeFilter.SelectedItem is CallType callType)
                {
                    SelectedCallType = callType;
                    UpdateCallHistory();

                }
                AdjustColumnWidths();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error changing call type filter: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SortByField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (SortByField.SelectedValue is string key)
                {
                    SelectedSortKey = key;
                    UpdateCallHistory();
                }
                AdjustColumnWidths();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error changing sort field: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ViewStatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var statisticsWindow = new VolunteerStatisticsWindow(_volunteerId);
                statisticsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening statistics: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CallHistoryDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // התאמת רוחב העמודות לתוכן
            AdjustColumnWidths();
        }

        // פונקציה חדשה להתאמת רוחב העמודות ורוחב החלון
        private void AdjustColumnWidths()
        {
            try
            {
                // התאמת רוחב העמודות לתוכן
                foreach (var column in CallHistoryDataGrid.Columns)
                {
                    column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                }

                // מתן זמן לעדכון הממשק
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    // התאמת רוחב החלון לרוחב הטבלה
                    double totalWidth = CallHistoryDataGrid.Columns.Sum(c => c.ActualWidth) + 80; // תוספת שוליים

                    // הגבלת הרוחב המקסימלי לרוחב המסך
                    double maxWidth = SystemParameters.WorkArea.Width * 0.9;
                    double newWidth = Math.Min(totalWidth, maxWidth);

                    // עדכון רוחב החלון
                    Width = newWidth;

                    // מרכוז החלון מחדש
                    Left = (SystemParameters.WorkArea.Width - Width) / 2;
                }), DispatcherPriority.Render);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adjusting column widths: {ex.Message}");
            }
        }
    }

}

