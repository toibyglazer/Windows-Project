using System;
using System.Windows;

namespace PL
{
    /// <summary>
    /// Interaction logic for MainActiveCallDetailsWindow.xaml
    /// </summary>
    public partial class MainActiveCallDetailsWindow : Window
    {
        private readonly BO.CallInProgress _activeCall;
        private readonly int _volunteerId; // Store the volunteer ID as a field
        private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        private readonly MainVolunteerScreen _parentWindow;

        public MainActiveCallDetailsWindow(BO.CallInProgress activeCall, int volunteerId, MainVolunteerScreen parentWindow)
        {
            InitializeComponent();
            _parentWindow = parentWindow;

            if (activeCall == null)
            {
                MessageBox.Show("Unable to load active call - data is missing.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Close();
            }
            else
            {
                _activeCall = activeCall;
                _volunteerId = volunteerId;
                DataContext = activeCall;
            }
        }
        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show confirmation dialog
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to mark this assignment as completed?",
                    "Confirm Completion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Call the backend method to complete the assignment
                    s_bl.Call.CompleteAssignment(_volunteerId, _activeCall.Id);

                    // Force update of the parent window
                    if (_parentWindow != null)
                    {
                        _parentWindow.UpdateVolunteerData();
                    }

                    MessageBox.Show(
                        "Assignment has been successfully completed.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Close the window after successful completion
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error completing assignment: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show confirmation dialog
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to cancel this assignment?",
                    "Confirm Cancellation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // Call the backend method to cancel the assignment
                    await s_bl.Call.CancelAssignmentAsync(_volunteerId, _activeCall.Id);

                    // Force update of the parent window
                    if (_parentWindow != null)
                    {
                       _parentWindow.UpdateVolunteerData();
                    }

                    MessageBox.Show(
                        "Assignment has been cancelled.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Close the window after successful cancellation
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error cancelling assignment: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}