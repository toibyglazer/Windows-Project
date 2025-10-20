using BO;
using PL.Volunteer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PL;

/// <summary>
/// Interaction logic for Window1.xaml
/// </summary>
public partial class Login : Window
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public static readonly DependencyProperty UsernameProperty =
             DependencyProperty.Register("Username", typeof(string), typeof(Login));

    public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.Register("Password", typeof(string), typeof(Login));

    public string Username
    {
        get { return (string)GetValue(UsernameProperty); }
        set { SetValue(UsernameProperty, value); }
    }

    public string Password
    {
        get { return (string)GetValue(PasswordProperty); }
        set { SetValue(PasswordProperty, value); }
    }

    public Login()
    {
        InitializeComponent();
        try
        {
            Icon = new BitmapImage(new Uri("Icons/Icon4.ico", UriKind.Relative));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading icon: {ex.Message}");
        }
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            int username;
            string password = txtPassword.Password;
            if (!int.TryParse(txtUsername.Text, out username) && string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Login details are empty please complete them", "Info", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(txtUsername.Text, out username))
            {
                MessageBox.Show("Username is required and must be a numeric ID", "Info", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Password is required", "Info", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // השינוי העיקרי: הרצת הפעולה הסינכרונית על thread אחר כדי למנוע חסימה של ה-UI thread
            var loginResult = await Task.Run(() => s_bl.Volunteer.Login(username, password));
            string userName = loginResult.Name;
            BO.VolunteerRole userRole = loginResult.Role;

            string roleText = userRole == BO.VolunteerRole.Manager ? "Manager" : "Volunteer";
            SuccessText.Text = $"Hello {userName},\nYou have been identified as a {roleText}";

            // הצגת הטקסט והאנימציה
            LoadingPanel.Visibility = Visibility.Visible;
            Storyboard sb = (Storyboard)FindResource("LoadingAnimation");
            sb.Begin();

            // מחכים 4 שניות
            await Task.Delay(4000);

            // יצירת החלון החדש לפני סגירת החלון הנוכחי
            Window newWindow = null;

            if (userRole == BO.VolunteerRole.Manager)
            {
                newWindow = new MainManagementScreen();
            }
            else if (userRole == BO.VolunteerRole.Volunteer)
            {
                newWindow = new MainVolunteerScreen(username);
            }

            // הצגת החלון החדש
            if (newWindow != null)
            {
                newWindow.Show();
            }

            // סגירת מסך ההתחברות
            this.Close();
        }
        // הסרנו את הטיפול בחריגת BlLockoutException כי היא לא תיזרק יותר
        catch (BO.BlDoesNotExistException)
        {
            MessageBox.Show("User does not exist. Please check your ID and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (DO.DalDoesNotExistException)
        {
            MessageBox.Show("User does not exist. Please check your ID and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (BO.BlInvalidPasswordException)
        {
            MessageBox.Show("Incorrect password. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

