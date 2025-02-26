using BO;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using PL.Volunteer;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Data;

namespace PL.Call
{
    /// <summary>
    /// Interaction logic for CallWindow.xaml
    /// </summary>
    public partial class CallWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

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
                MessageBox.Show($"Error loading volunteer data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        public void CallWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCallTypeImage(CurrentCall.Type);

            // אם מדובר במתנדב קיים, נוסיף צופה
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
                        }
                    }
                }));
            }
        }


        public void AddOrUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                if (ButtonText == "Add")
                {

                    s_bl.Call.AddCall(CurrentCall);
                    OnPropertyChanged(nameof(CurrentCall));
                    MessageBox.Show($"Call added successfully! The system sent an email to all volunteers in the call area.",
         "Success", MessageBoxButton.OK, MessageBoxImage.Information);


                }
                else if (ButtonText == "Update")
                {

                    s_bl.Call.UpdateCallDetails(CurrentCall);
                    OnPropertyChanged(nameof(CurrentCall));

                    // הצגת הודעה על עדכון מוצלח
                    MessageBox.Show("Volunteer updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }


                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Operation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //public void ListOfPastAssignmentsClick(object sender, RoutedEventArgs e)
        //{
        //    if (CurrentCall.callAssignInList == null)
        //    {
        //        MessageBox.Show("There is no allocation for this call in the system. This call has never been taken by any volunteer.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);


        //    }
        //    else
        //    {
        //        var callAssigPastList = new CallAssignPastListDetails(CurrentCall.callAssignInList);
        //        callAssigPastList.ShowDialog();
        //    }
        //}


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
        //private void OnOpenActiveCallClick(object sender, RoutedEventArgs e)
        //{
        //    if (CurrentCall.callAssignInList == null || CurrentCall.Id == 0)
        //    {
        //        MessageBox.Show("No volunteers were ever assigned to this call, and there is no past assignment list.", "No Assignments", MessageBoxButton.OK, MessageBoxImage.Information);
        //        return;
        //    }

        //    try
        //    {
        //        var activeCallWindow = new PastAssignmentsListWindow(CurrentCall.callAssignInList);
        //        activeCallWindow.ShowDialog();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"An error occurred while loading past assignments: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}
        private void CallTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // בדיקה אם יש ערך נבחר בקומבובוקס
            if (CallTypeComboBox.SelectedItem is BO.CallType selectedCallType)
            {
                LoadCallTypeImage(selectedCallType);
            }
        }

    }



}
