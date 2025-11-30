using System.Windows;
using System.Windows.Controls;
using TMS.Pages.Admin;

namespace TMS.Pages.Customer
{
    public partial class SupportPage : Page
    {
        private Frame _mainFrame;
        private string _username;
        private string _email;

        public SupportPage(Frame mainFrame, string username, string email)
        {
            InitializeComponent();

            _mainFrame = mainFrame;
            _username = username;
            _email = email;

            // Load AddRequestControl by default
            LoadViewRequestsControl();
        }

        private void LoadAddRequestControl()
        {
            var addControl = new AddRequestControl(_username, _email);
            MainContentArea.Content = addControl;
        }

        private void LoadViewRequestsControl()
        {
            var viewControl = new ViewRequestControl(_username, _email);
            MainContentArea.Content = viewControl;
        }

        private void BtnAddNewRequest_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LoadAddRequestControl();
        }

        private void BtnViewRequests_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LoadViewRequestsControl();
        }

  
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to AdminDashboard or whatever page hosts the main frame
            if (_mainFrame != null)
            {
                _mainFrame.Content = new CustomerDashboard(_mainFrame, _username, _email);
            }
        }

    }
}
