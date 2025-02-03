using BlApi;
using BO;
using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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
namespace PL.Volunteer;

/// <summary>
/// Interaction logic for VolunteerWindow.xaml
/// </summary>
public partial class VolunteerWindow : Window, INotifyPropertyChanged
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public BO.DistanceType distanceType { get; set; } = BO.DistanceType.Aerial;
    public DistanceTypeCollection DistanceTypesCollection { get; set; } = new();

    public BO.Activity ActivityFilter { get; set; } = BO.Activity.All;
    public ActivityCollection ActivityCollection { get; set; } = new();

    public BO.VolunteerRole VolunteerRole { get; set; } = BO.VolunteerRole.Volunteer;
    public VolunteerRoleCollection volunteerRoleCollection { get; set; } = new();



    public BO.Volunteer? CurrentVolunteer
    {
        get { return (BO.Volunteer?)GetValue(CurrentVolunteerProperty); }
        set { SetValue(CurrentVolunteerProperty, value); OnPropertyChanged(); }
    }

    public static readonly DependencyProperty CurrentVolunteerProperty =
        DependencyProperty.Register("CurrentVolunteer", typeof(BO.Volunteer), typeof(VolunteerWindow), new PropertyMetadata(null));

    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    public static readonly DependencyProperty ButtonTextProperty =
        DependencyProperty.Register(nameof(ButtonText), typeof(string), typeof(VolunteerWindow), new PropertyMetadata(string.Empty));

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
    public VolunteerWindow() : this(0)
    {
    }

   
    public VolunteerWindow(int id = 0)
    {

       
      
        try
        {
            ButtonText = id == 0 ? "Add" : "Update";

            if (id != 0)
            {
                var volunteer = s_bl.Volunteer.GetVolunteerDetails(id);
                if (volunteer == null)
                {
                    MessageBox.Show("No volunteer data available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                CurrentVolunteer = volunteer;
            }
            else
            {
                CurrentVolunteer = new BO.Volunteer()
                {
                    Id = 0,
                    FullName = string.Empty,
                    PhoneNumber = string.Empty,
                    Email = string.Empty,
                    IsActive = false,
                    MaxDistance = 0.0,
                    DistanceType = BO.DistanceType.Aerial,
                    TotalHandledCalls = 0,
                    TotalCancelledCalls = 0,
                    TotalExpiredCalls = 0
                    //ImageProfilePath = System.IO.Path.GetFullPath("Profile Volunteer Image/Default_pfp.svg.jpg")
                };
            }
           
            InitializeComponent();
          
            // הוספת אירועים לאחר אתחול החלון
            Loaded += VolunteerWindow_Loaded;
            Closing += VolunteerWindow_Closing;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading volunteer data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void VolunteerWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // טוען את התמונה
        LoadProfileImage(CurrentVolunteer.Id);

        // אם מדובר במתנדב קיים, נוסיף צופה
        if (CurrentVolunteer?.Id != 0)
        {
            s_bl.Volunteer.AddObserver(CurrentVolunteer.Id, UpdateVolunteerData);
        }
    }


    public void VolunteerWindow_Closing(object sender, CancelEventArgs e)
    {
        if (CurrentVolunteer?.Id != 0)
        {
            s_bl.Volunteer.RemoveObserver(CurrentVolunteer.Id, UpdateVolunteerData);
        }
    }

    public volatile DispatcherOperation _updateVolunteerDataOperation = null;
    public void UpdateVolunteerData()
    {
        if (_updateVolunteerDataOperation == null || _updateVolunteerDataOperation.Status == DispatcherOperationStatus.Completed)
        {
            _updateVolunteerDataOperation = Dispatcher.BeginInvoke(new Action(() =>
            {
                if (CurrentVolunteer?.Id != null)
                {
                    var updatedVolunteer = s_bl.Volunteer.GetVolunteerDetails(CurrentVolunteer.Id);
                    if (updatedVolunteer != null)
                    {
                        CurrentVolunteer = updatedVolunteer;
                    }
                }
            }));
        }
    }

    private string GeneratePassword()
    {
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string specialChars = "!@#$%^&*()_+<>?";
        const int passwordLength = 12;

        var allChars = upperChars + lowerChars + digits + specialChars;
        var random = new Random();

        // מוודאים שהסיסמה כוללת לפחות תו אחד מכל סוג
        string password = new string(new[]
        {
        upperChars[random.Next(upperChars.Length)],
        lowerChars[random.Next(lowerChars.Length)],
        digits[random.Next(digits.Length)],
        specialChars[random.Next(specialChars.Length)]
    });

        // משלימים את שאר התווים באורך הרצוי
        password += new string(Enumerable.Range(0, passwordLength - 4)
            .Select(_ => allChars[random.Next(allChars.Length)]).ToArray());

        // ערבוב הסיסמה למניעת תבנית קבועה
        return new string(password.OrderBy(_ => random.Next()).ToArray());
    }

    public void AddOrUpdateButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string generatedPassword = GeneratePassword();

            if (ButtonText == "Add")
            {
                CurrentVolunteer.Password = "Default password";
                s_bl.Volunteer.AddVolunteer(CurrentVolunteer);
                OnPropertyChanged(nameof(CurrentVolunteer));
                MessageBox.Show($"Volunteer added successfully! A new password has been sent to the volunteer's email.\nPlease ask them to check their inbox.",
     "Success", MessageBoxButton.OK, MessageBoxImage.Information);
           

            }
            else if (ButtonText == "Update")
            {
                CurrentVolunteer.Password = CurrentVolunteer.Password;
                s_bl.Volunteer.UpdateVolunteer(CurrentVolunteer.Id, CurrentVolunteer, null);
                
                OnPropertyChanged(nameof(CurrentVolunteer));

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
    public void OnOpenActiveCallClick(object sender, RoutedEventArgs e)
    {
        if (CurrentVolunteer.ActiveCall == null)
        {
            MessageBox.Show("There is no active call for the current volunteer.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

        }
        else
        {
            var activeCallWindow = new ActiveCallDetailsWindow(CurrentVolunteer.ActiveCall);
            activeCallWindow.ShowDialog();
        }
    }
   
    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

   
   public const string ImageFolder = "ProfileImage"; // תיקיית התמונות 

    /// <summary>
    /// טוען את תמונת הפרופיל על פי תעודת הזהות של המתנדב
    /// </summary>
    /// <param name="volunteerId">תעודת זהות של המתנדב</param>
    public void LoadProfileImage(int volunteerId)
    {
        // קבלת נתיב התמונה מהתקייה
        string imagePath = System.IO.Path.Combine(AppContext.BaseDirectory, ImageFolder, $"{volunteerId}.jpg");

        if (File.Exists(imagePath))
        {
            // הצגת התמונה ב-WPF
            ProfileImageBrush.ImageSource = new BitmapImage(new Uri(imagePath)); // כאן משתנים את ה-ImageSource של ה-ImageBrush
        }
        else
        {
            // תצוגת תמונה ברירת מחדל אם התמונה לא קיימת
            ProfileImageBrush.ImageSource = new BitmapImage(new Uri(System.IO.Path.Combine(AppContext.BaseDirectory, ImageFolder, "default.jpg")));
        }
    }
    

    
    public void DeleteVolunteerButton_Click(object sender, RoutedEventArgs e)
    {
        // בדיקה אם במצב הוספה
        if (CurrentVolunteer.Id==0)
        {
            MessageBox.Show("You are in Add mode. You must save the volunteer before deleting.",
                            "Action Not Allowed",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
            return;
        }

        try
        {
            var result = MessageBox.Show("Are you sure you want to delete this volunteer?",
                                          "Delete Confirmation",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // ניסיון למחוק את המתנדב
                s_bl.Volunteer.DeleteVolunteer(CurrentVolunteer.Id);
                MessageBox.Show("Volunteer deleted successfully.",
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








}
