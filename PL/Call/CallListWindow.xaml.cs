using BO;
using PL.Volunteer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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

namespace PL.Call
{
    /// <summary>
    /// Converter for formatting remaining time based on call status
    /// </summary>

    public class RemainingTimeConverter : IMultiValueConverter
    {
       

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // First value is RemainingTime, second value is Status, third value is TotalCompletionTime
            if (values.Length < 3)
                return "--";

            TimeSpan? remainingTime = values[0] as TimeSpan?;
            CallStatus? status = values[1] as CallStatus?;
            TimeSpan? totalCompletionTime = values[2] as TimeSpan?;

            if (status == null)
                return "--";

            switch (status)
            {
                case CallStatus.Closed:
                    // Check if totalCompletionTime has a value
                    if (totalCompletionTime.HasValue && totalCompletionTime.Value > TimeSpan.Zero)
                    {
                        return "Completed";
                    }
                    return "Admin Canceled"; // שינוי מ"--" ל-"Admin Canceled"

                case CallStatus.Expired:
                    return "--";

                case CallStatus.Open:
                case CallStatus.OpenAtRisk:
                case CallStatus.InProgress:
                case CallStatus.InTreatmentAtRisk:
                    if (remainingTime.HasValue)
                    {
                        if (remainingTime.Value <= TimeSpan.Zero)
                        {
                            return "00:00:00";
                        }

                        var rt = remainingTime.Value;
                        return rt.Days > 0
                            ? $"{rt.Days}d {rt.Hours:00}:{rt.Minutes:00}:{rt.Seconds:00}"
                            : $"{rt.Hours:00}:{rt.Minutes:00}:{rt.Seconds:00}";
                    }
                    return "";

                default:
                    return "--";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for formatting completion time based on call status
    /// </summary>
    public class CompletionTimeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // First value is TotalCompletionTime, second value is Status
            if (values.Length < 2)
                return "--";

            TimeSpan? completionTime = values[0] as TimeSpan?;
            CallStatus? status = values[1] as CallStatus?;

            if (status == null)
                return "--";

            // Check call status
            switch (status)
            {
                case CallStatus.Closed:
                case CallStatus.Expired:
                    if (completionTime.HasValue && completionTime.Value > TimeSpan.Zero)
                    {
                        var time = completionTime.Value;
                        if (time.Days > 0)
                        {
                            return $"{time.Days}d {time:hh\\:mm\\:ss}";
                        }
                        return time.ToString(@"hh\:mm\:ss");
                    }
                    // הסרנו את השורה שמציגה "Admin Canceled"
                    return "--";

                case CallStatus.Open:
                case CallStatus.OpenAtRisk:
                    return ""; // Open call - leave empty

                case CallStatus.InProgress:
                case CallStatus.InTreatmentAtRisk:
                    return "--"; // Call in progress

                default:
                    return "--";
            }
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }





    /// <summary>
    /// Converter to check if a call is at risk (for background coloring)
    /// </summary>
    //public class CallAtRiskConverter : IValueConverter
    //    {
    //        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //        {
    //            if (value is CallStatus status)
    //            {
    //                // Check if the call is at risk
    //                if (status == CallStatus.OpenAtRisk || status == CallStatus.InTreatmentAtRisk)
    //                {
    //                    return Brushes.LightPink; // Light red background
    //                }
    //            }

    //            return Brushes.Transparent; // Normal background
    //        }

    //        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }

    /// <summary>
    /// Interaction logic for CallListWindow.xaml
    /// </summary>
    public partial class CallListWindow : Window, INotifyPropertyChanged
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
        private bool _isLoading = false;
        private static readonly object _dataAccessLock = new object(); // נעילה לגישה לנתונים
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isUpdating = false; // מניעת עדכונים מקבילים
        private System.Threading.Timer _debounceTimer; // טיימר לדחיית ביצוע

        // הוספת מאפיין חדש לסינון לפי שם מתנדב
        private string _volunteerFilter = string.Empty;
        public string VolunteerFilter
        {
            get { return _volunteerFilter; }
            set
            {
                if (_volunteerFilter != value)
                {
                    _volunteerFilter = value;
                    OnPropertyChanged();
                    UpdateCallList(); // עדכון הרשימה בכל שינוי בפילטר
                }
            }
        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                OnPropertyChanged();

                // עדכון ממשק המשתמש בהתאם למצב הטעינה
                if (LoadingIndicator != null)
                {
                    // תמיד להסתיר את אינדיקטור הטעינה
                    LoadingIndicator.Visibility = Visibility.Collapsed;
                }

                if (CallListView != null)
                {
                    // תמיד לאפשר את רשימת הקריאות
                    CallListView.IsEnabled = true;
                }
            }
        }

        public IEnumerable<BO.CallInList> CallList
        {
            get { return (IEnumerable<BO.CallInList>)GetValue(CallListProperty); }
            set { SetValue(CallListProperty, value); }
        }

        public static readonly DependencyProperty CallListProperty =
            DependencyProperty.Register("CallList", typeof(IEnumerable<BO.CallInList>), typeof(CallListWindow), new PropertyMetadata(null));


        public BO.CallType callType { get; set; } = BO.CallType.All;
        public BO.CallStatus callStatus { get; set; } = BO.CallStatus.All;

        public CallTypesCollection CallTypeCollection { get; set; } = new();
        public CallStatusCollection callStatusCollection { get; set; } = new();

        // טיפול באירוע שינוי טקסט בשדה סינון המתנדב
        private void VolunteerFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // שימוש בדחיית ביצוע (debounce) כדי למנוע עדכונים מרובים
            if (_debounceTimer != null)
            {
                _debounceTimer.Dispose();
            }

            _debounceTimer = new System.Threading.Timer(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    VolunteerFilter = VolunteerFilterTextBox.Text;
                });
            }, null, 300, Timeout.Infinite); // השהייה של 300 מילישניות
        }

        public async Task UpdateCallListAsync()
        {
            // אם כבר יש עדכון בתהליך, צא מהפונקציה
            if (_isUpdating) return;

            try
            {
                _isUpdating = true;
                // אין צורך להפעיל את אינדיקטור הטעינה

                // יצירת טוקן ביטול חדש
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                }
                _cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = _cancellationTokenSource.Token;

                // ביצוע פעולת הטעינה בתהליך נפרד
                IEnumerable<BO.CallInList> filteredList = await Task.Run(() =>
                {
                    // שימוש בנעילה לגישה לנתונים
                    lock (_dataAccessLock)
                    {
                        var calls = s_bl.Call.GetCalls(
                            filterField: null,
                            filterValue: null,
                            sortField: null
                        );

                        if (callType != BO.CallType.All)
                        {
                            calls = calls.Where(call => call.Type == callType);
                        }

                        if (callStatus != BO.CallStatus.All)
                        {
                            calls = calls.Where(call => call.Status == callStatus);
                        }

                        // סינון לפי שם מתנדב אחרון
                        if (!string.IsNullOrWhiteSpace(VolunteerFilter))
                        {
                            calls = calls.Where(call =>
                                call.LastVolunteer != null &&
                                call.LastVolunteer.Contains(VolunteerFilter, StringComparison.OrdinalIgnoreCase));
                        }

                        return calls.ToList();
                    }
                }, cancellationToken);

                // בדיקה אם הפעולה בוטלה
                if (cancellationToken.IsCancellationRequested)
                    return;

                CallList = filteredList;
                OnPropertyChanged(nameof(CallList));
            }
            catch (OperationCanceledException)
            {
                // הפעולה בוטלה, אין צורך בטיפול מיוחד
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading calls: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isUpdating = false;
            }
        }

        // גרסה סינכרונית לתאימות לאחור
        public void UpdateCallList()
        {
            _ = UpdateCallListAsync();
        }

        public async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await UpdateCallListAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CallListWindow window)
            {
                window.UpdateCallList();
            }
        }

        private bool _observerRegistered = false;


        // Stage 7 – PL 5A: coalescing + marshaling to UI thread
        private volatile bool _callListObserverWorking = false;

        public void CallListObserver()
        {
            if (!_callListObserverWorking)
            {
                _callListObserverWorking = true;

                _ = Dispatcher.BeginInvoke(() =>
                {
                    // כאן אנחנו כבר על ה-UI thread
                    if (!_isUpdating)
                    {
                        UpdateCallList(); // קוראת ל-UpdateCallListAsync מאחורי הקלעים
                    }

                    _callListObserverWorking = false;
                });
            }
        }

        private bool _clockObserverRegistered = false;
        public async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // הסתרת אינדיקטור הטעינה באופן מיידי
            if (LoadingIndicator != null)
                LoadingIndicator.Visibility = Visibility.Collapsed;

            // יצירת מנגנון ביטול פעולות
            _cancellationTokenSource = new CancellationTokenSource();

            // רישום לאירוע חריגה לא מטופלת
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // --- Observers ---
            // צופה לשינויים ברשימת הקריאות (כבר היה)
            if (!_observerRegistered)
            {
                s_bl.Call.AddObserver(CallListObserver);
                _observerRegistered = true;
            }

            // צופה ל"טיקים" של שעון הסימולטור (חדש) – מרענן את הרשימה כל פעם שהזמן מתקדם
            if (!_clockObserverRegistered)
            {
                s_bl.Admin.AddClockObserver(CallListObserver);   // <-- חדש
                _clockObserverRegistered = true;
            }
            // --- /Observers ---

            // טעינת הנתונים ללא הפעלת אינדיקטור
            _isUpdating = true;
            try
            {
                IEnumerable<BO.CallInList> filteredList = await Task.Run(() =>
                {
                    lock (_dataAccessLock)
                    {
                        var calls = s_bl.Call.GetCalls(
                            filterField: null,
                            filterValue: null,
                            sortField: null
                        );

                        if (callType != BO.CallType.All)
                            calls = calls.Where(call => call.Type == callType);

                        if (callStatus != BO.CallStatus.All)
                            calls = calls.Where(call => call.Status == callStatus);

                        // סינון לפי שם מתנדב אחרון
                        if (!string.IsNullOrWhiteSpace(VolunteerFilter))
                        {
                            calls = calls.Where(call =>
                                call.LastVolunteer != null &&
                                call.LastVolunteer.Contains(VolunteerFilter, StringComparison.OrdinalIgnoreCase));
                        }

                        return calls.ToList();
                    }
                });

                CallList = filteredList;
                OnPropertyChanged(nameof(CallList));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading calls: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isUpdating = false;
            }
        }


        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // הסרת הצופה במקרה של חריגה לא מטופלת
            try
            {
                if (_observerRegistered)
                {
                    s_bl.Call.RemoveObserver(CallListObserver);
                    _observerRegistered = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing observer: {ex.Message}");
            }
        }

        public void Window_Closed(object sender, EventArgs e)
        {
            // ביטול כל הפעולות האסינכרוניות שבתהליך
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            // ניקוי טיימר דחיית ביצוע אם קיים
            if (_debounceTimer != null)
            {
                _debounceTimer.Dispose();
                _debounceTimer = null;
            }

            // הסרת הצופים
            try
            {
                if (_observerRegistered)
                {
                    s_bl.Call.RemoveObserver(CallListObserver);
                    _observerRegistered = false;
                }

                if (_clockObserverRegistered)
                {
                    s_bl.Admin.RemoveClockObserver(CallListObserver); // <-- חדש
                    _clockObserverRegistered = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing observer: {ex.Message}");
            }

            // הסרת רישום לאירוע חריגה לא מטופלת
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
        }

        public BO.CallInList SelectedCall { get; set; }

        public async void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCall != null && !_isUpdating)
            {
                try
                {
                    _isUpdating = true;

                    // יצירת החלון ללא טעינת נתונים
                    var callWindow = new CallWindow();

                    // טעינת הנתונים באופן אסינכרוני
                    await callWindow.InitializeCallAsync(SelectedCall.CallId);

                    // הצגת החלון רק לאחר טעינת הנתונים
                    callWindow.Show();

                    // עדכון הרשימה
                    await UpdateCallListAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening call details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }

        public CallListWindow(int volunteerId)
        {
            InitializeComponent();

            // הסתרת אינדיקטור הטעינה באופן מיידי
            if (LoadingIndicator != null)
            {
                LoadingIndicator.Visibility = Visibility.Collapsed;
            }

            // אתחול ערך ברירת מחדל לפילטר מתנדב
            _volunteerFilter = string.Empty;

            // רישום לאירועי חלון
            Loaded += Window_Loaded;
            Closed += Window_Closed;
        }

        public CallListWindow()
        {
            InitializeComponent();

            // הסתרת אינדיקטור הטעינה באופן מיידי
            if (LoadingIndicator != null)
            {
                LoadingIndicator.Visibility = Visibility.Collapsed;
            }

            // אתחול ערך ברירת מחדל לפילטר מתנדב
            _volunteerFilter = string.Empty;

            // רישום לאירועי חלון
            Loaded += Window_Loaded;
            Closed += Window_Closed;
        }

        private async void AddCallButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isUpdating)
            {
                try
                {
                    _isUpdating = true;

                    var window = new CallWindow();
                    window.Show();

                    await UpdateCallListAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding call: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }
    }
}