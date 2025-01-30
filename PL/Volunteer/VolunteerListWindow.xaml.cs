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
namespace PL.Volunteer
{
    /// <summary>
    /// Interaction logic for VolunteerListWindow.xaml
    /// </summary>
    public partial class VolunteerListWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public IEnumerable<BO.VolunteerInList> VolunteerList
        {
            get { return (IEnumerable<BO.VolunteerInList>)GetValue(VolunteerListProperty); }
            set { SetValue(VolunteerListProperty, value); }
        }

        public static readonly DependencyProperty VolunteerListProperty =
            DependencyProperty.Register("VolunteerList", typeof(IEnumerable<BO.VolunteerInList>), typeof(VolunteerListWindow), new PropertyMetadata(null));

        public BO.CallType callType { get; set; } = BO.CallType.All;
        public BO.Activity ActivityFilter { get; set; } = BO.Activity.All;

        public CallTypesCollection CallTypesCollection { get; set; } = new();
        public ActivityCollection ActivityCollection { get; set; } = new();


        public void UpdateVolunteerList()
        {
            IEnumerable<BO.VolunteerInList> filteredList = s_bl.Volunteer.GetVolunteersList(
       isActiveFilter: ActivityFilter == BO.Activity.All ? null : ActivityFilter == BO.Activity.Active,
       sortField: null);

            if (callType != BO.CallType.All)
            {
                filteredList = filteredList.Where(v => v.Type == callType);
            }

            if (!string.IsNullOrWhiteSpace(NameFilter))
            {
                filteredList = filteredList.Where(v => v.FullName.StartsWith(NameFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(IDFilter))
            {
                filteredList = filteredList.Where(v => v.Id.ToString().StartsWith(IDFilter));
            }

            VolunteerList = filteredList.ToList();
            OnPropertyChanged(nameof(VolunteerList));


        }


        public void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateVolunteerList();
        }

        public void VolunteerListObserver()
        {
            UpdateVolunteerList();
        }
        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
            s_bl.Volunteer.AddObserver(VolunteerListObserver);
            UpdateVolunteerList();
        }
        public void Window_Closed(object sender, EventArgs e)
        {
            s_bl.Volunteer.RemoveObserver(VolunteerListObserver);
        }




        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VolunteerListWindow window)
            {
                window.UpdateVolunteerList();
            }
        }
        public string NameFilter
        {
            get { return (string)GetValue(NameFilterProperty); }
            set { SetValue(NameFilterProperty, value); }
        }
       
        public static readonly DependencyProperty NameFilterProperty =
            DependencyProperty.Register("NameFilter", typeof(string), typeof(VolunteerListWindow), new PropertyMetadata(string.Empty, OnFilterChanged));

        public string IDFilter
        {
            get { return (string)GetValue(IDFilterProperty); }
            set { SetValue(IDFilterProperty, value); }
        }

        public static readonly DependencyProperty IDFilterProperty =
        DependencyProperty.Register("IDFilter", typeof(string), typeof(VolunteerListWindow), new PropertyMetadata(string.Empty, OnFilterChanged));


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateVolunteerList();
        }
       


        public VolunteerListWindow()
        {
            InitializeComponent();

            //var window = new VolunteerListWindow();
            //window.Show();
        }

    
    }
}
