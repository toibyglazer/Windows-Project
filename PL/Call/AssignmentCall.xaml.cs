using System.Collections.ObjectModel;
using System.Windows;

namespace PL.Call
{
    public partial class AssignmentCall : Window
    {
        public ObservableCollection<BO.CallAssignInList> Assignments { get; set; }

        public AssignmentCall(BO.Call call)
        {
            Assignments = new ObservableCollection<BO.CallAssignInList>(
                (call.callAssignInList ?? new List<BO.CallAssignInList>())
                .OrderBy(a => a.EntryTime)
            );

            InitializeComponent();
            DataContext = this;
        }
    }
}
