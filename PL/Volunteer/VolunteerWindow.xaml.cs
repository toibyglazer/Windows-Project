using BlApi;
using BO;
using DO;
using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
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
using static PL.Volunteer.SendEmailNewVolunteer;
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


    //public VolunteerWindow(int id = 0)
    //{
    //    try
    //    {
    //        ButtonText = id == 0 ? "Add" : "Update";

    //        if (id != 0)
    //        {
    //            // קריאה סינכרונית למתודה אסינכרונית
    //            var volunteer = s_bl.Volunteer.GetVolunteerDetailsAsync(id).GetAwaiter().GetResult();
    //            if (volunteer == null)
    //            {
    //                MessageBox.Show("No volunteer data available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    //                return;
    //            }
    //            CurrentVolunteer = volunteer;
    //        }
    //        else
    //        {
    //            CurrentVolunteer = new BO.Volunteer()
    //            {
    //                Id = 0,
    //                FullName = string.Empty,
    //                PhoneNumber = string.Empty,
    //                Email = string.Empty,
    //                IsActive = false,
    //                MaxDistance = 0.0,
    //                DistanceType = BO.DistanceType.Aerial,
    //                TotalHandledCalls = 0,
    //                TotalCancelledCalls = 0,
    //                TotalExpiredCalls = 0
    //                //ImageProfilePath = System.IO.Path.GetFullPath("Profile Volunteer Image/Default_pfp.svg.jpg")
    //            };
    //        }

    //        InitializeComponent();

    //        // הוספת אירועים לאחר אתחול החלון
    //        Loaded += VolunteerWindow_Loaded;
    //        Closing += VolunteerWindow_Closing;
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show($"Error loading volunteer data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    //    }
    //}
    //public void VolunteerWindow_Loaded(object sender, RoutedEventArgs e)
    //{
    //    // טוען את התמונה
    //    LoadProfileImage(CurrentVolunteer.Id);

    //    // אם מדובר במתנדב קיים, נוסיף צופה
    //    if (CurrentVolunteer?.Id != 0)
    //    {
    //        s_bl.Volunteer.AddObserver(CurrentVolunteer.Id, UpdateVolunteerData);
    //    }
    //}

    private int _volunteerId;
    private async void VolunteerWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_volunteerId != 0)
            {
                var volunteer = await s_bl.Volunteer.GetVolunteerDetailsAsync(_volunteerId);
                if (volunteer == null)
                {
                    MessageBox.Show("No volunteer data available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }
                CurrentVolunteer = volunteer;
            }
            else
            {
                CurrentVolunteer = new BO.Volunteer
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
                };
            }

            // טוען את התמונה
            LoadProfileImage(CurrentVolunteer.Id);

            // אם מדובר במתנדב קיים, נוסיף צופה
            if (CurrentVolunteer?.Id != 0)
            {
                s_bl.Volunteer.AddObserver(CurrentVolunteer.Id, UpdateVolunteerData);
            }

            // לעבודה עם Binding אם צריך
            DataContext = this;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading volunteer data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
    }

    public VolunteerWindow(int id = 0)
    {
        InitializeComponent(); // שימי לב שהוא עובר לפה למעלה
        _nominatimService = new NominatimService(); // יצירת אובייקט של שירות Nominatim
        _volunteerId = id;
        ButtonText = id == 0 ? "Add" : "Update";

        // מוסיפים את האירועים אחרי אתחול הרכיבים
        Loaded += VolunteerWindow_Loaded;
        Closing += VolunteerWindow_Closing;
    }

    public void VolunteerWindow_Closing(object sender, CancelEventArgs e)
    {
        if (CurrentVolunteer?.Id != 0)
        {
            s_bl.Volunteer.RemoveObserver(CurrentVolunteer.Id, UpdateVolunteerData);
        }
    }

    // Stage 7 – PL 5A (Option 1 - flag)
    private volatile bool _updateVolunteerDataWorking = false;

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

    //public async void AddOrUpdateButton_Click(object sender, RoutedEventArgs e)
    //{
    //    try
    //    {
    //        string generatedPassword = GeneratePassword();

    //        if (ButtonText == "Add")
    //        {
    //            //CurrentVolunteer.Password = "Default password";
    //            CurrentVolunteer.Password = GeneratePassword();
    //            await s_bl.Volunteer.AddVolunteerAsync(CurrentVolunteer);
    //            OnPropertyChanged(nameof(CurrentVolunteer));
    //            MessageBox.Show($"Volunteer added successfully! A new password has been sent to the volunteer's email.\nPlease ask them to check their inbox.",
    //                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    //        }
    //        else if (ButtonText == "Update")
    //        {
    //            CurrentVolunteer.Password = CurrentVolunteer.Password;
    //            await s_bl.Volunteer.UpdateVolunteerAsync(CurrentVolunteer.Id, CurrentVolunteer, null);

    //            OnPropertyChanged(nameof(CurrentVolunteer));

    //            // הצגת הודעה על עדכון מוצלח
    //            MessageBox.Show("Volunteer updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    //        }

    //        Close();
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show($"Operation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    //    }
    //}

   
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

  
    // The NominatimService instance
    private readonly NominatimService _nominatimService;

    
    // Timer for debouncing address input
    private System.Threading.Timer _debounceTimer;
    private int _requestCounter = 0;

    /// <summary>
    /// Handles key up events in the address text box to show address suggestions
    /// </summary>
    private void AddressTextBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AddressTextBox.Text))
        {
            SuggestionsPopup.IsOpen = false;
            return;
        }

        int currentRequest = ++_requestCounter;

        // Cancel previous timer if exists
        _debounceTimer?.Dispose();

        // Create new timer with delay
        _debounceTimer = new System.Threading.Timer(async _ =>
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    // Send request to Nominatim for address suggestions
                    var addresses = await _nominatimService.GetAddressSuggestionsAsync(AddressTextBox.Text, currentRequest);

                    // Show results in ListBox only if there are results
                    if (addresses.Count > 0)
                    {
                        SuggestionsList.ItemsSource = addresses;
                        SuggestionsPopup.IsOpen = true;
                    }
                    else
                    {
                        SuggestionsPopup.IsOpen = false;
                    }
                }
                catch (Exception)
                {
                    // Hide suggestions on error
                    SuggestionsPopup.IsOpen = false;
                }
            });
        }, null, 500, Timeout.Infinite); // 500ms delay for better performance
    }

    /// <summary>
    /// Handles selection of an address from the suggestions list
    /// </summary>
    private void SuggestionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SuggestionsList.SelectedItem != null)
        {
            // Set the selected address to the text box
            AddressTextBox.Text = SuggestionsList.SelectedItem.ToString();

            // Update the address in the current volunteer object
            if (CurrentVolunteer != null)
            {
                CurrentVolunteer.CurrentAddress = AddressTextBox.Text;
                OnPropertyChanged(nameof(CurrentVolunteer));
            }

            // Hide the suggestions popup
            SuggestionsPopup.IsOpen = false;
        }
    }

    /// <summary>
    /// Closes the suggestions list when clicking outside
    /// </summary>
    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (SuggestionsPopup.IsOpen &&
            !SuggestionsList.IsMouseOver &&
            !AddressTextBox.IsMouseOver)
        {
            SuggestionsPopup.IsOpen = false;
        }
    }
    public async void AddOrUpdateButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ButtonText == "Add")
            {
                // Ask for confirmation before adding the volunteer
                var confirmResult = MessageBox.Show($"Are you sure you want to add {CurrentVolunteer.FullName} as a volunteer?",
                    "Confirm Addition", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirmResult == MessageBoxResult.Yes)
                {
                    // Generate password
                    string generatedPassword = GeneratePassword();
                    CurrentVolunteer.Password = generatedPassword;
                    // Add volunteer to database without sending email from BL
                    // Comment out the email sending line in the BL method
                    await s_bl.Volunteer.AddVolunteerAsync(CurrentVolunteer);


                    // Create a progress dialog
                    var progressDialog = new ProgressDialog("Sending email to volunteer...");
                    progressDialog.Show();

                    try
                    {
                        await SimpleEmailSender.SendStaticEmailAsync(CurrentVolunteer.Email, CurrentVolunteer.FullName, generatedPassword);


                        // Close progress dialog
                        progressDialog.Close();

                        // Show success message
                        MessageBox.Show("Email sent successfully to the volunteer!",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        OnPropertyChanged(nameof(CurrentVolunteer));
                        Close();
                    }
                    catch (Exception ex)
                    {
                        progressDialog.Close();
                        MessageBox.Show($"Failed to send email: {ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else if (ButtonText == "Update")
            {
                CurrentVolunteer.Password = CurrentVolunteer.Password;
                await s_bl.Volunteer.UpdateVolunteerAsync(CurrentVolunteer.Id, CurrentVolunteer, null);

                OnPropertyChanged(nameof(CurrentVolunteer));

                // Show success message
                MessageBox.Show("Volunteer updated successfully!",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Operation failed: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Simple progress dialog class
    public class ProgressDialog : Window
    {
        public ProgressDialog(string message)
        {
            Width = 300;
            Height = 150;
            WindowStyle = WindowStyle.ToolWindow;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            Title = "Processing";

            var grid = new Grid();

            var stackPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var textBlock = new TextBlock
            {
                Text = message,
                Margin = new Thickness(0, 0, 0, 20),
                TextAlignment = TextAlignment.Center
            };

            var progressBar = new ProgressBar
            {
                IsIndeterminate = true,
                Width = 250,
                Height = 20
            };

            stackPanel.Children.Add(textBlock);
            stackPanel.Children.Add(progressBar);
            grid.Children.Add(stackPanel);

            Content = grid;
        }
    }
}

public class SendEmailNewVolunteer
{
    private const string SmtpHost = "smtp.gmail.com";
    private const int SmtpPort = 587;
    private const string SenderEmail = "toibymichalminiproject@gmail.com";
    private const string SenderPassword = "hwxs xylf bfmy iwqg";

    public class SimpleEmailSender
    {
        private const string SmtpHost = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const string SenderEmail = "toibymichalminiproject@gmail.com";
        private const string SenderPassword = "hwxs xylf bfmy iwqg";

        public static async Task SendStaticEmailAsync(string recipientEmail, string volunteerFullName, string generatedPassword)
        {
            if (string.IsNullOrEmpty(recipientEmail))
                throw new InvalidOperationException("Recipient email address is missing.");

            try
            {
                using (var smtpClient = new SmtpClient(SmtpHost, SmtpPort))
                using (var mailMessage = new MailMessage())
                {
                    smtpClient.Credentials = new NetworkCredential(SenderEmail, SenderPassword);
                    smtpClient.EnableSsl = true;

                    mailMessage.From = new MailAddress(SenderEmail, "Volunteer Support Team");
                    mailMessage.Subject = "Welcome to Our Volunteer Family!";
                    mailMessage.IsBodyHtml = true;

                    mailMessage.Body = $@"
<html>
<head>
  <style>
    body {{
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      background-color: #f9f9f9;
      margin: 0;
      padding: 0;
    }}
    .email-container {{
      max-width: 480px;
      margin: 30px auto;
      padding: 20px;
      background-color: #ffffff;
      border: 1px solid #ddd;
      border-radius: 10px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.03);
    }}
    h2 {{
      text-align: center;
      color: #2c3e50;
      margin-top: 0;
    }}
    p {{
      font-size: 15px;
      line-height: 1.6;
      color: #34495e;
    }}
    .highlight {{
      background-color: #ecf0f1;
      padding: 6px 10px;
      border-radius: 4px;
      font-weight: bold;
      display: inline-block;
      margin: 10px 0;
      color: #2c3e50;
    }}
    .footer {{
      margin-top: 30px;
      font-size: 13px;
      color: #7f8c8d;
      border-top: 1px solid #eee;
      padding-top: 10px;
      text-align: center;
    }}
    .globe {{
      font-size: 30px;
      text-align: center;
      margin-bottom: 10px;
    }}
  </style>
</head>
<body>
  <div class='email-container'>
   

    <h2>Welcome to our volunteer family, {volunteerFullName}!</h2>

    <p>We’re so excited to have you on board! Your willingness to help and make an impact means a lot.</p>

    <p>Here are your login credentials:</p>
    <p class='highlight'>Password: {generatedPassword}</p>

    <p>Please keep this password in a safe place.</p>

    <p>If you have any questions or need help, feel free to reach out. We're here for you!🌍</p>

    <div class='footer'>
      Volunteer Management Team<br/>
      Email: toibymichalminiproject@gmail.com<br/>
      Phone: +972-3-1234567<br/>
      Location: Tel Aviv, Israel
    </div>
  </div>
</body>
</html>";

                    mailMessage.To.Add(recipientEmail);

                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send email: {ex.Message}");
            }
        }

    }
}