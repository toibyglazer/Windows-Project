using BO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using PL.Volunteer;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PL.Call
{
    /// <summary>
    /// Converter for handling null values in the UI
    /// </summary>
    public class NullToTextConverter : IValueConverter
    {
        public string DefaultText { get; set; } = "Not defined";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? DefaultText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == DefaultText ? null : value;
        }
    }

    /// <summary>
    /// Converter for formatting dates consistently
    /// </summary>
    /// <summary>
    /// Converter for formatting dates consistently
    /// </summary>
    public class DateFormatConverter : IValueConverter
    {
        public string Format { get; set; } = "dd/MM/yyyy HH:mm:ss";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                return dateTime.ToString(Format);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
            {
                if (DateTime.TryParseExact(stringValue, Format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }

                // Try standard parsing if exact format fails
                if (DateTime.TryParse(stringValue, out result))
                {
                    return result;
                }
            }
            return value;
        }
    }

    /// <summary>
    /// Interaction logic for CallWindow.xaml
    /// </summary>
    public partial class CallWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private bool _isInitialized = false;

        public BO.CallType callType { get; set; } = BO.CallType.All;
        public BO.CallStatus callStatus { get; set; } = BO.CallStatus.All;

        public CallTypesCollection CallTypeCollection { get; set; } = new();
        public CallStatusCollection CallStatusCollection { get; set; } = new();

        public BO.Call? CurrentCall
        {
            get { return (BO.Call?)GetValue(CurrentCallProperty); }
            set { SetValue(CurrentCallProperty, value); OnPropertyChanged(); }
        }

        public static readonly DependencyProperty CurrentCallProperty =
            DependencyProperty.Register("CurrentCall", typeof(BO.Call), typeof(CallWindow), new PropertyMetadata(null));

        public string ButtonText
        {
            get => (string)GetValue(ButtonTextProperty);
            set => SetValue(ButtonTextProperty, value);
        }
        public static readonly DependencyProperty ButtonTextProperty =
        DependencyProperty.Register(nameof(ButtonText), typeof(string), typeof(CallWindow), new PropertyMetadata(string.Empty));

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // פונקציה להוספת רווחים לפני אותיות גדולות
        private string FormatCallType(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // הוספת רווח לפני כל אות גדולה (למעט האות הראשונה)
            string result = Regex.Replace(input, @"(?<=[a-z])([A-Z])", " $1");

            return result;
        }

        public int id;
        public int Id
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                    ButtonText = id == 0 ? "Add" : "Update";
                }
            }
        }

        public CallWindow() : this(0)
        {
        }

        public CallWindow(int id = 0)
        {
            try
            {
                ButtonText = id == 0 ? "Add" : "Update";
                Id = id;

                if (id != 0)
                {
                    var call = s_bl.Call.GetCallDetailsById(id);
                    if (call == null)
                    {
                        MessageBox.Show("No call data available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    CurrentCall = call;
                }
                else
                {
                    CurrentCall = new BO.Call()
                    {
                        Id = 0, // מזהה ברירת מחדל
                        Type = CallType.None, // בהנחה ש-General הוא ערך מתאים
                        Description = string.Empty, // תיאור ריק כברירת מחדל
                        FullAddress = null, // כתובת ברירת מחדל
                        Latitude = 0.0, // קו רוחב ברירת מחדל
                        Longitude = 0.0, // קו אורך ברירת מחדל
                        OpeningTime = DateTime.Now, // זמן פתיחה הוא הזמן הנוכחי
                        MaxCompletionTime = null, // ללא זמן סיום ברירת מחדל
                        Status = CallStatus.Open, // בהנחה ש-Pending הוא ערך מתאים
                        callAssignInList = null, // רשימה ריקה
                    };
                }

                InitializeComponent();

                Loaded += CallWindow_Loaded;
                Closing += CallWindow_Closing;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading call data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Modified to use synchronous calls but maintain the Task return type for compatibility
        public Task InitializeCallAsync(int callId)
        {
            if (callId == 0 || _isInitialized)
                return Task.CompletedTask;

            try
            {
                Id = callId;

                // Load call data synchronously
                var call = s_bl.Call.GetCallDetailsById(callId);

                if (call == null)
                {
                    MessageBox.Show("No call data available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return Task.CompletedTask;
                }

                // Update UI directly
                CurrentCall = call;
                _isInitialized = true;

                // If the window is already loaded, update the display
                if (IsLoaded)
                {
                    LoadCallTypeImage(CurrentCall.Type);

                    if (FormattedCallType != null)
                    {
                        FormattedCallType.Text = FormatCallType(CurrentCall.Type.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading call data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return Task.CompletedTask;
        }

        public void CallWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCallTypeImage(CurrentCall.Type);

            // Set formatted call type text
            if (FormattedCallType != null && CurrentCall != null)
            {
                FormattedCallType.Text = FormatCallType(CurrentCall.Type.ToString());
            }

            // אם מדובר בקריאה קיימת, נוסיף צופה
            if (CurrentCall?.Id != 0)
            {
                s_bl.Call.AddObserver(CurrentCall.Id, UpdateCallData);
            }
        }

        public void CallWindow_Closing(object sender, CancelEventArgs e)
        {
            if (CurrentCall?.Id != 0)
            {
                s_bl.Call.RemoveObserver(CurrentCall.Id, UpdateCallData);
            }
        }

        public volatile DispatcherOperation _updateCallDataOperation = null;
        public void UpdateCallData()
        {
            if (_updateCallDataOperation == null || _updateCallDataOperation.Status == DispatcherOperationStatus.Completed)
            {
                _updateCallDataOperation = Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (CurrentCall?.Id != null)
                    {
                        var updatedCall = s_bl.Call.GetCallDetailsById(CurrentCall.Id);
                        if (updatedCall != null)
                        {
                            CurrentCall = updatedCall;

                            // Update formatted call type text
                            if (FormattedCallType != null)
                            {
                                FormattedCallType.Text = FormatCallType(CurrentCall.Type.ToString());
                            }

                            // Update image
                            LoadCallTypeImage(CurrentCall.Type);
                        }
                    }
                }));
            }
        }

        public async void AddOrUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ButtonText == "Add")
                {
                    // Wait for the operation to complete
                    await s_bl.Call.AddCallAsync(CurrentCall);
                    OnPropertyChanged(nameof(CurrentCall));
                    MessageBox.Show($"Call added successfully! The system sent an email to all volunteers in the call area.",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (ButtonText == "Update")
                {
                    // Wait for the operation to complete
                    await s_bl.Call.UpdateCallDetailsAsync(CurrentCall);
                    OnPropertyChanged(nameof(CurrentCall));

                    // הצגת הודעה על עדכון מוצלח
                    MessageBox.Show("Call updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Operation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // See all the assignments for this call
        private void BtnViewAssignments_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open the AssignmentCall window and pass the CurrentCall object
                var assignmentWindow = new AssignmentCall(CurrentCall);
                assignmentWindow.ShowDialog(); // Open as a modal window
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public const string ImageFolder = "Icons"; // תיקיית התמונות

        /// <summary>
        /// טוען את תמונת האייקון על פי סוג הקריאה
        /// </summary>
        /// <param name="type">סוג הקריאה</param>
        private void LoadCallTypeImage(BO.CallType type)
        {
            // קבלת נתיב התמונה מהתקייה
            string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ImageFolder, $"{type}.png");

            if (File.Exists(imagePath))
            {
                CallTypeImage.Source = new BitmapImage(new Uri(imagePath));
            }
            else
            {
                string defaultImagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ImageFolder, "default.png");
                CallTypeImage.Source = new BitmapImage(new Uri(defaultImagePath));
            }
        }

        public void DeleteCallButton_Click(object sender, RoutedEventArgs e)
        {
            // בדיקה אם במצב הוספה
            if (CurrentCall.Id == 0)
            {
                MessageBox.Show("You are in Add mode. You must save the call before deleting.",
                                "Action Not Allowed",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }
            if (CurrentCall.callAssignInList != null)
            {
                MessageBox.Show("Cannot be deleted. The call is or has ever been assigned in the system.",
                                "Action Not Allowed",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            try
            {
                var result = MessageBox.Show("Are you sure you want to delete this call?",
                                              "Delete Confirmation",
                                              MessageBoxButton.YesNo,
                                              MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // ניסיון למחוק את המתנדב
                    s_bl.Call.DeleteCall(CurrentCall.Id);
                    MessageBox.Show("Call deleted successfully.",
                                    "Success",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    Close();
                }
                else
                {
                    MessageBox.Show("Deletion cancelled.",
                                    "Cancelled",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                // הצגת הודעה עבור חריגה משכבת BL או כל חריגה אחרת
                MessageBox.Show($"An error occurred: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void CallTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // בדיקה אם יש ערך נבחר בקומבובוקס
            if (CallTypeComboBox.SelectedItem is BO.CallType selectedCallType)
            {
                LoadCallTypeImage(selectedCallType);

                // Update formatted call type text
                if (FormattedCallType != null)
                {
                    FormattedCallType.Text = FormatCallType(selectedCallType.ToString());
                }
            }
        }
    }
}