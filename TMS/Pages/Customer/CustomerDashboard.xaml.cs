using System.Windows;
using System.Windows.Controls;
using TMS.Controls;
using TMS.Pages.Admin;

namespace TMS.Pages.Customer
{
    public partial class CustomerDashboard : Page
    {
        private readonly Frame _mainFrame;
        private readonly string _username;

        public string UsernameDisplay => $"{_username} ▼";

        public CustomerDashboard(Frame frame, string username)
        {
            InitializeComponent();
            _mainFrame = frame;
            _username = username;

            DataContext = this;
            txtCustomerName.Text = $"Welcome, {username}";
        }

        // ---------------------- DASHBOARD BUTTONS ----------------------
        private void SearchTickets_Click(object sender, RoutedEventArgs e)
        {
            //_mainFrame.Content = new SearchTicketsPage(_mainFrame, _username);
        }

        private void MyBookings_Click(object sender, RoutedEventArgs e)
        {
            //_mainFrame.Content = new MyBookingsPage(_mainFrame, _username);
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            //_mainFrame.Content = new UpdateInfo(_username);
        }

        private void Payment_Click(object sender, RoutedEventArgs e)
        {
            //_mainFrame.Content = new PaymentPage(_mainFrame, _username);
        }

        private void ETicket_Click(object sender, RoutedEventArgs e)
        {
            //_mainFrame.Content = new ETicketHistoryPage(_mainFrame, _username);
        }

        private void Support_Click(object sender, RoutedEventArgs e)
        {
            //_mainFrame.Content = new SupportPage(_mainFrame, _username);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new LoginPage(_mainFrame);
        }

        // ---------------------- PROFILE MENU HANDLERS ----------------------
        private void UpdateProfile_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new UpdateInfo(_username);
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var updatePage = new UpdateInfo(_username);
            updatePage.ContentArea.Content = new ChangePasswordControl(_username);
            _mainFrame.Content = updatePage;
        }

        private void ProfileMenu_Click(object sender, RoutedEventArgs e)
        {
            ProfilePopup.IsOpen = !ProfilePopup.IsOpen;
        }
    }
}
