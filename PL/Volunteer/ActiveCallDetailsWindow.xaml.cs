using System;
using System.Windows;

namespace PL.Volunteer
{
    public partial class ActiveCallDetailsWindow : Window
    {
        public ActiveCallDetailsWindow(BO.CallInProgress activeCall)
        {
            InitializeComponent();

            if (activeCall == null)
            {
                MessageBox.Show("Unable to load active call - data is missing.",
    "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Close(); 
            }
            else
            {
                DataContext = activeCall;
            }
        }
    }
}
