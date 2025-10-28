using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;            // IValueConverter
using System.Globalization;          // CultureInfo
using BO;                            // VolunteerRole
using BlApi;
using PL.Call;
using PL.Volunteer;

namespace PL;

public partial class MainManagementScreen : Window
{
    // Stage 7 – PL 5A: coalescing + marshaling to UI thread
    private volatile bool _clockObserverWorking = false;
    private volatile bool _configObserverWorking = false;

    static readonly IBl s_bl = Factory.Get();

    // מזהה המשתמש שמחובר כרגע (מוזרם מה-Login)
    private readonly int _currentUserId;

    #region NEW: Simulator Dependency Properties

    public int Interval
    {
        get => (int)GetValue(IntervalProperty);
        set => SetValue(IntervalProperty, value);
    }
    public static readonly DependencyProperty IntervalProperty =
        DependencyProperty.Register(nameof(Interval), typeof(int), typeof(MainManagementScreen),
            new PropertyMetadata(1)); // ברירת מחדל: 1 דקה

    public bool IsSimulatorRunning
    {
        get => (bool)GetValue(IsSimulatorRunningProperty);
        set => SetValue(IsSimulatorRunningProperty, value);
    }
    public static readonly DependencyProperty IsSimulatorRunningProperty =
        DependencyProperty.Register(nameof(IsSimulatorRunning), typeof(bool), typeof(MainManagementScreen),
            new PropertyMetadata(false));

    #endregion

    #region DP: CurrentTime & MaxTimeSpanValue

    public DateTime CurrentTime
    {
        get => (DateTime)GetValue(CurrentTimeProperty);
        set => SetValue(CurrentTimeProperty, value);
    }
    public static readonly DependencyProperty CurrentTimeProperty =
        DependencyProperty.Register(nameof(CurrentTime), typeof(DateTime), typeof(MainManagementScreen));

    public string MaxTimeSpanValue
    {
        get => (string)GetValue(MaxTimeSpanValueProperty);
        set => SetValue(MaxTimeSpanValueProperty, value);
    }
    public static readonly DependencyProperty MaxTimeSpanValueProperty =
        DependencyProperty.Register(nameof(MaxTimeSpanValue), typeof(string), typeof(MainManagementScreen),
            new PropertyMetadata(string.Empty));

    #endregion

    #region Ctors

    // בנאי ברירת-מחדל (למקרה שנקרא ממקום שלא מעביר מזהה)
    public MainManagementScreen()
    {
        InitializeComponent();
        // ערך התחלתי דיפולטי לקצב הסימולטור (אם לא מגיע מבחוץ)
        if (Interval <= 0) Interval = 1;
    }

    // בנאי שמעביר את מזהה המשתמש הנוכחי (מנהל שהוא גם מתנדב)
    public MainManagementScreen(int currentUserId) : this()
    {
        _currentUserId = currentUserId;
    }

    #endregion

    #region Window Events

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            CurrentTime = s_bl.Admin.GetSystemClock();
            var riskTime = s_bl.Admin.GetRiskTimeRange();
            MaxTimeSpanValue = riskTime.ToString();

            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);

            // הצגת כפתור מצב מתנדב רק אם המשתמש הנוכחי הוא מנהל
            if (_currentUserId > 0)
            {
                try
                {
                    var vol = await s_bl.Volunteer.GetVolunteerDetailsAsync(_currentUserId);
                    VolunteerModeButton.Visibility =
                        (vol != null && vol.Role == VolunteerRole.Manager)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
                catch
                {
                    VolunteerModeButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                VolunteerModeButton.Visibility = Visibility.Collapsed;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        s_bl.Admin.RemoveClockObserver(clockObserver);
        s_bl.Admin.RemoveConfigObserver(configObserver);
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        var result = MessageBox.Show("Are you sure you want to close this window?",
                                     "Confirm Close",
                                     MessageBoxButton.YesNo,
                                     MessageBoxImage.Question);
        if (result == MessageBoxResult.No)
            e.Cancel = true;
    }

    #endregion

    #region Observers

    private void clockObserver()
    {
        if (!_clockObserverWorking)
        {
            _clockObserverWorking = true;
            _ = Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    CurrentTime = s_bl.Admin.GetSystemClock();
                }
                finally
                {
                    _clockObserverWorking = false;
                }
            });
        }
    }

    private void configObserver()
    {
        if (!_configObserverWorking)
        {
            _configObserverWorking = true;
            _ = Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    var riskTimeSpan = s_bl.Admin.GetRiskTimeRange();
                    MaxTimeSpanValue = riskTimeSpan.ToString();
                }
                finally
                {
                    _configObserverWorking = false;
                }
            });
        }
    }

    #endregion

    #region Buttons: Clock Advance

    private void btnAddOneMinute_Click(object sender, RoutedEventArgs e)
    {
        s_bl.Admin.AdvanceSystemClock(BO.TimeUnit.MINUTE);
    }

    private void btnAddOneHour_Click(object sender, RoutedEventArgs e)
    {
        s_bl.Admin.AdvanceSystemClock(BO.TimeUnit.HOUR);
    }

    private void btnAddOneDay_Click(object sender, RoutedEventArgs e)
    {
        s_bl.Admin.AdvanceSystemClock(BO.TimeUnit.DAY);
    }

    private void btnAddOneMonth_Click(object sender, RoutedEventArgs e)
    {
        s_bl.Admin.AdvanceSystemClock(BO.TimeUnit.MONTH);
    }

    private void btnAddOneYear_Click(object sender, RoutedEventArgs e)
    {
        s_bl.Admin.AdvanceSystemClock(BO.TimeUnit.YEAR);
    }

    #endregion

    #region Update Max Risk Time

    private void btnUpdateMaxYearRange_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string parsed = MaxTimeSpanValue;

            if (IsValidTimeSpanFormat(parsed))
            {
                string fixedValue = CorrectTimeSpanRange(parsed);
                var current = s_bl.Admin.GetRiskTimeRange().ToString();

                if (fixedValue == current)
                {
                    MessageBox.Show("The value remains unchanged in the configuration.",
                                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    MaxTimeSpanValue = current;
                }
                else
                {
                    var ts = TimeSpan.Parse(fixedValue);
                    s_bl.Admin.SetRiskTimeRange(ts);
                    MessageBox.Show("The value has been successfully updated! 🎉",
                                    "Success", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }
            }
            else
            {
                MessageBox.Show("Invalid TimeSpan format. Please use hh:mm:ss.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                MaxTimeSpanValue = s_bl.Admin.GetRiskTimeRange().ToString();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating value: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
            MaxTimeSpanValue = s_bl.Admin.GetRiskTimeRange().ToString();
        }
    }

    public bool IsValidTimeSpanFormat(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;

        var parts = value.Split(':');
        if (parts.Length != 3) return false;
        if (parts.Any(p => !p.All(char.IsDigit) || p.Length > 2)) return false;

        int h = int.Parse(parts[0]);
        int m = int.Parse(parts[1]);
        int s = int.Parse(parts[2]);

        return !(s < 0 || m < 0 || h < 0);
    }

    public string CorrectTimeSpanRange(string value)
    {
        var parts = value.Split(':');
        int h = int.Parse(parts[0]);
        int m = int.Parse(parts[1]);
        int s = int.Parse(parts[2]);

        if (s > 59)
        {
            m += s / 60;
            s = s % 60;
        }
        if (m > 59)
        {
            h += m / 60;
            m = m % 60;
            if (h > 23) throw new Exception("The number of hours cannot be more than 23.");
        }
        return $"{h:D2}:{m:D2}:{s:D2}";
    }

    #endregion

    #region DB Init/Reset

    private void InitializeDB_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("Are you sure you want to initialize the database? This will reset all data.",
                                     "Initialize Database", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            CloseAllWindowsExceptMain();
            s_bl.Admin.InitializeDatabase();
            MessageBox.Show("The database has been successfully initialized.",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred during initialization: {ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private void ResetDB_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("Are you sure you want to reset the database? This will clear all data.",
                                     "Reset Database", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            CloseAllWindowsExceptMain();
            s_bl.Admin.ResetDatabase();
            MessageBox.Show("The database has been successfully reset.",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred during reset: {ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private void CloseAllWindowsExceptMain()
    {
        foreach (Window w in Application.Current.Windows)
            if (w != this) w.Close();
    }

    #endregion

    #region Navigation Buttons

    private void VolunteerManagementButton_Click(object sender, RoutedEventArgs e)
    {
        var win = new VolunteerListWindow
        {
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        win.Show();
        win.Activate();
    }

    private void CallManagementButton_Click(object sender, RoutedEventArgs e)
    {
        var win = new CallListWindow
        {
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        win.Show();
        win.Activate();
    }

    #endregion

    #region Volunteer Mode

    private async void VolunteerModeButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_currentUserId <= 0)
            {
                MessageBox.Show("Volunteer identity not available.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // אימות שיש פרופיל מתנדב
            BO.Volunteer vol = null;
            try
            {
                vol = await s_bl.Volunteer.GetVolunteerDetailsAsync(_currentUserId);
            }
            catch
            {
                MessageBox.Show("Volunteer profile not found for this user.",
                                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (vol == null)
            {
                MessageBox.Show("Volunteer profile not found for this user.",
                                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // לא לפתוח כפילויות – אם חלון כזה כבר פתוח, נעיר אותו
            foreach (Window w in Application.Current.Windows)
            {
                if (w is MainVolunteerScreen existing /* && existing.VolunteerId == _currentUserId */)
                {
                    existing.Activate();
                    return;
                }
            }

            // פתיחת חלון מתנדב למשתמש הנוכחי
            var vmWin = new MainVolunteerScreen(_currentUserId)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            vmWin.Show();
            vmWin.Activate();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening Volunteer Mode: {ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region NEW: Start/Stop Simulator button handler

    private void ToggleSimulatorButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // וידוא שה-Interval חוקי
            if (Interval <= 0)
            {
                MessageBox.Show("Interval must be a positive integer (minutes).",
                                "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                Interval = 1;
                return;
            }

            if (!IsSimulatorRunning)
            {
                // START
                s_bl.Admin.StartSimulator(Interval);   // stage 7
                IsSimulatorRunning = true;
            }
            else
            {
                // STOP
                s_bl.Admin.StopSimulator();    // stage 7 (לפי הנוסח שסיפקת)
                IsSimulatorRunning = false;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Simulator error: {ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion
}

