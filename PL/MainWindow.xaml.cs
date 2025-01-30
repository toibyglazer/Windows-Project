using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using BlApi;

namespace PL;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    public DateTime CurrentTime
    {
        get { return (DateTime)GetValue(CurrentTimeProperty); }
        set { SetValue(CurrentTimeProperty, value); }
    }
    public static readonly DependencyProperty CurrentTimeProperty =
        DependencyProperty.Register("CurrentTime", typeof(DateTime), typeof(MainWindow));

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

    private void clockObserver()
    {
        CurrentTime = s_bl.Admin.GetSystemClock();
    }
    private void configObserver()
    {
        TimeSpan riskTimeSpan = s_bl.Admin.GetRiskTimeRange();
        MaxTimeSpanValue = riskTimeSpan.ToString();
        
    }
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
          
            CurrentTime = s_bl.Admin.GetSystemClock();
            TimeSpan riskTime = s_bl.Admin.GetRiskTimeRange();
            MaxTimeSpanValue = riskTime.ToString(); 
            s_bl.Admin.AddClockObserver(clockObserver);
            s_bl.Admin.AddConfigObserver(configObserver);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        {
            e.Cancel = true;
        }
    }


    public string MaxTimeSpanValue
    {
        get
        {
            return (string)GetValue(MaxTimeSpanValueProperty);
        }
        set
        {
            SetValue(MaxTimeSpanValueProperty, value);
        }
    }

    public static readonly DependencyProperty MaxTimeSpanValueProperty =
        DependencyProperty.Register(
            "MaxTimeSpanValue",
            typeof(string),     
            typeof(MainWindow),  
            new PropertyMetadata(string.Empty)); 

    private void btnUpdateMaxYearRange_Click(object sender, RoutedEventArgs e)
    {
        try
        {
          
            string parsedTimeSpan = MaxTimeSpanValue;

            if (IsValidTimeSpanFormat(parsedTimeSpan))
            {
                string StringAfterRepair = CorrectTimeSpanRange(parsedTimeSpan);
                TimeSpan currentTimeSpan = s_bl.Admin.GetRiskTimeRange();


                if (StringAfterRepair == currentTimeSpan.ToString())
                {
                    MessageBox.Show("The value remains unchanged in the configuration. The value will remain as is and no update will be made.",
                                    "Warning",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                    TimeSpan riskTimeSpan = s_bl.Admin.GetRiskTimeRange();
                    MaxTimeSpanValue = riskTimeSpan.ToString();
                }

                else
                {
                    TimeSpan timeSpan = TimeSpan.Parse(StringAfterRepair);
                    s_bl.Admin.SetRiskTimeRange(timeSpan);
                    MessageBox.Show("The value has been successfully updated! 🎉",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Asterisk);

                }

            }
            else
            {
                MessageBox.Show("Invalid TimeSpan format. Please enter a valid value in the format hh:mm:ss.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TimeSpan riskTimeSpan = s_bl.Admin.GetRiskTimeRange();
                MaxTimeSpanValue = riskTimeSpan.ToString();
            }
        }
        
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating value: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            TimeSpan riskTimeSpan = s_bl.Admin.GetRiskTimeRange();
            MaxTimeSpanValue = riskTimeSpan.ToString();
        }
    }
    public bool IsValidTimeSpanFormat(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        string[] parts = value.Split(':');
        if (parts.Length != 3)
            return false;

        foreach (var part in parts)
        {
            if (!part.All(char.IsDigit) || part.Length > 2)
                return false;
        }

        int hours = int.Parse(parts[0]);
        int minutes = int.Parse(parts[1]);
        int seconds = int.Parse(parts[2]);

        if (seconds < 0  || minutes < 0 || hours < 0)
            return false;

        return true;
    }
    public string CorrectTimeSpanRange(string value)
    {
        string[] parts = value.Split(':');

        int hours = int.Parse(parts[0]);
        int minutes = int.Parse(parts[1]);
        int seconds = int.Parse(parts[2]);

        if (seconds > 59)
        {
            int overflowMinutes = seconds / 60; 
            seconds = seconds % 60;  
            minutes += overflowMinutes; 

           
            if (minutes > 59)
            {
                int overflowHours = minutes / 60;
                minutes = minutes % 60;
                hours += overflowHours;
            }
        }

        if (minutes > 59)
        {
            int overflowHours = minutes / 60;
            minutes = minutes % 60;
            hours += overflowHours;
            if(hours>23)
            {
                throw new Exception("The number of hours cannot be more than 23.");
            } 
        }

        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";  
    }






    private void InitializeDB_Click(object sender, RoutedEventArgs e)
    {
        
        var result = MessageBox.Show("Are you sure you want to initialize the database? This action will reset all data.",
                                     "Initialize Database",
                                     MessageBoxButton.YesNo,
                                     MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            CloseAllWindowsExceptMain();
            s_bl.Admin.InitializeDatabase();
            MessageBox.Show("The database has been successfully initialized.",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred during initialization: {ex.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    // מתודה לאיפוס בסיס הנתונים
    private void ResetDB_Click(object sender, RoutedEventArgs e)
    {
        
        var result = MessageBox.Show("Are you sure you want to reset the database? This action will clear all data.",
                                     "Reset Database",
                                     MessageBoxButton.YesNo,
                                     MessageBoxImage.Warning);

       
        if (result != MessageBoxResult.Yes) return;

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;

            CloseAllWindowsExceptMain();

            s_bl.Admin.ResetDatabase();

            MessageBox.Show("The database has been successfully reset.",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred during reset: {ex.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
        }
        finally
        {
          
            Mouse.OverrideCursor = null;
        }
    }

    private void CloseAllWindowsExceptMain()
    {
        foreach (Window window in Application.Current.Windows)
        {
            if (window != this)
            {
                window.Close();
            }
        }
    }

public MainWindow()
    {
        InitializeComponent();
    }

}














   