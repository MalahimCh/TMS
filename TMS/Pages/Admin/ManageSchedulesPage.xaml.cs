using System.Windows;
using System.Windows.Controls;
using TMS.Controls.Admin;

namespace TMS.Pages.Admin
{
    public partial class ManageSchedulesPage : Page
    {
        private readonly Frame _mainFrame;
        private readonly string _username;

        public ManageSchedulesPage(Frame frame, string username)
        {
            InitializeComponent();
            _mainFrame = frame;
            _username = username;

            ContentArea.Content = new ViewScheduleControl(_mainFrame, _username);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new AdminDashboard(_mainFrame, _username);
        }

        private void Sidebar_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string action = btn.Tag.ToString();

            switch (action)
            {
                case "Add":
                    ContentArea.Content = new AddScheduleControl(_mainFrame, _username);
                    break;
                case "Update":
                    ContentArea.Content = new UpdateScheduleControl(_mainFrame, _username);
                    break;
                case "Delete":
                    ContentArea.Content = new DeleteScheduleControl(_mainFrame, _username);
                    break;
                case "View":
                    ContentArea.Content = new ViewScheduleControl(_mainFrame, _username);
                    break;
            }
        }
    }
}
