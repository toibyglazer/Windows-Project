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
                MessageBox.Show("לא ניתן לטעון קריאה פעילה - הנתונים חסרים.",
                    "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                Close(); 
            }
            else
            {
                DataContext = activeCall;
            }
        }
    }
}
