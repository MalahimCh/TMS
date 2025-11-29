using System.Windows;
using System.Windows.Controls;
using TMS.Controls.Admin;

namespace TMS.Pages.Admin
{
    public partial class ManageRoutesPage : Page
    {
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly string _email;
        public ManageRoutesPage(Frame frame, string username, string email)
        {
            InitializeComponent();
            _mainFrame = frame;
            _username = username;

            ContentArea.Content = new ViewRouteControl(_mainFrame, _username);
            _email = email;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new AdminDashboard(_mainFrame, _username,_email);
        }

        private void Sidebar_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string action = btn.Tag.ToString();

            switch (action)
            {
                case "Add":
                    ContentArea.Content = new AddRouteControl(_mainFrame, _username);
                    break;

                case "Update":
                    ContentArea.Content = new UpdateRouteControl(_mainFrame, _username);
                    break;

                case "Delete":
                    ContentArea.Content = new DeleteRouteControl(_mainFrame, _username);
                    break;

                case "View":
                    ContentArea.Content = new ViewRouteControl(_mainFrame, _username);
                    break;
            }
        }
    }
}
