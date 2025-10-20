using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PL.Call
{
    public partial class AssignmentCall : Window
    {
        public ObservableCollection<BO.CallAssignInList> Assignments { get; set; }

        // Call details properties
        public int CallId { get; set; }
        public string CallType { get; set; }
        public string FormattedCallType { get; set; }

        // Statistics properties
        public int TotalAssignments { get; set; }

        // שמירת אובייקט הקריאה
        private BO.Call _call;

        public AssignmentCall(BO.Call call)
        {
            _call = call;

            // Initialize assignments collection
            var assignmentsList = call.callAssignInList ?? new List<BO.CallAssignInList>();
            Assignments = new ObservableCollection<BO.CallAssignInList>(
                assignmentsList.OrderBy(a => a.EntryTime)
            );

            // Set call details
            CallId = call.Id;
            CallType = call.Type.ToString();

            // Format call type with spaces before capital letters
            FormattedCallType = FormatCallType(CallType);

            // Set statistics
            TotalAssignments = assignmentsList.Count;

            InitializeComponent();
            DataContext = this;

            // Set window title to include call information
            this.Title = $"Assignment History - Call #{call.Id}";
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // טעינת אייקון אחרי שהחלון נטען
            LoadCallTypeImage(_call.Type);
        }

        private void LoadCallTypeImage(BO.CallType type)
        {
            try
            {
                // נתיב לקובץ תמונה המתאים לסוג הקריאה
                string imagePath = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    CallWindow.ImageFolder,
                    $"{type}.png");

                if (File.Exists(imagePath))
                {
                    ImageBrush brush = new ImageBrush();
                    brush.ImageSource = new BitmapImage(new Uri(imagePath));
                    CallIconBrush.ImageSource = brush.ImageSource;
                }
                else
                {
                    // טעינת אייקון ברירת מחדל
                    string defaultImagePath = System.IO.Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        CallWindow.ImageFolder,
                        "default.png");
                    ImageBrush brush = new ImageBrush();
                    brush.ImageSource = new BitmapImage(new Uri(defaultImagePath));
                    CallIconBrush.ImageSource = brush.ImageSource;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading icon: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}