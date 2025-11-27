using System.Windows;
using System.Windows.Controls;
using TMS.Controls.Admin;

namespace TMS.Pages.Admin
{
    public partial class ManageBusesPage : Page
    {
        private readonly Frame _mainFrame;
        private readonly string _username;

        public ManageBusesPage(Frame frame, string username)
        {
            InitializeComponent();
            _mainFrame = frame;
            _username = username;
            ContentArea.Content = new ViewBusControl(_mainFrame, _username);
        }

        // ---------------------- BACK ----------------------
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to AdminDashboard
            _mainFrame.Content = new AdminDashboard(_mainFrame, _username);
        }

        // ---------------------- SIDEBAR NAVIGATION ----------------------
        private void Sidebar_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string action = btn.Tag.ToString();

            switch (action)
            {
                case "Add":
                    ContentArea.Content = new AddBusControl(_mainFrame, _username);
                    break;

                case "Update":
                    ContentArea.Content = new UpdateBusControl(_mainFrame, _username);
                    break;

                case "Delete":
                    ContentArea.Content = new DeleteBusControl(_mainFrame, _username);
                    break;

                case "View":
                    ContentArea.Content = new ViewBusControl(_mainFrame, _username);
                    break;
            }
        }
    }
}
