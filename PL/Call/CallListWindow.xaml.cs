using BO;
using PL.Volunteer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for CallListWindow.xaml
    /// </summary>
    public partial class CallListWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

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

       


        public void UpdateCallList()
        {
            IEnumerable<BO.CallInList> filteredList = s_bl.Call.GetCalls(
                filterField: null,
                filterValue: null,
                sortField: null
            );

            if (callType != BO.CallType.All)
            {
                filteredList = filteredList.Where(call => call.Type == callType);
            }

            if (callStatus != BO.CallStatus.All)
            {
                filteredList = filteredList.Where(call => call.Status == callStatus);
            }



            CallList = filteredList.ToList();
            OnPropertyChanged(nameof(CallList));
        }


        public void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCallList();
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
        public void CallListObserver()
        {
            UpdateCallList(); 
        }

        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl.Call.AddObserver(CallListObserver);
            UpdateCallList();
        }

        public void Window_Closed(object sender, EventArgs e)
        {
            s_bl.Call.RemoveObserver(CallListObserver);
        }

        public BO.CallInList SelectedCall { get; set; }
        public void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
           
            if (SelectedCall != null)
            {
                new CallWindow(SelectedCall.CallId).Show();
                UpdateCallList(); 
            }
        }

        public CallListWindow(int volunteerId)
        {
            UpdateCallList();
            InitializeComponent();

        }
        public CallListWindow()
        {
            UpdateCallList();
            InitializeComponent();
        }
        private void AddCallButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new CallWindow(); 
            window.Show(); 
            UpdateCallList(); 
        }

    }

}








