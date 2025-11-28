using System.Windows;
using System.Windows.Controls;
using TMS.Controls;

namespace TMS.Pages.Admin
{
    public partial class AdminDashboard : Page
    {
        private readonly Frame _mainFrame;
        private readonly string _username;

        public string UsernameDisplay => $"{_username} ▼";

        public AdminDashboard(Frame frame, string username)
        {
            InitializeComponent();
            _mainFrame = frame;
            _username = username;

            DataContext = this; // for profile menu binding
            txtAdminName.Text = $"Welcome, {username}";
        }

        // ---------------------- BUSES ----------------------
        private void ManageBuses_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new ManageBusesPage(_mainFrame, _username);
        }

        // ---------------------- ROUTES ----------------------
        private void ManageRoutes_Click(object sender, RoutedEventArgs e)
        {
            //_mainFrame.Content = new ManageRoutesPage(_mainFrame, _username);
        }

        // ---------------------- SCHEDULES ----------------------
        private void ManageSchedules_Click(object sender, RoutedEventArgs e)
        {
            //_mainFrame.Content = new ManageSchedulesPage(_mainFrame, _username);
        }

        // ---------------------- PRICES ----------------------
        private void ManagePrices_Click(object sender, RoutedEventArgs e)
        {
            //_mainFrame.Content = new ManagePricesPage(_mainFrame, _username);
        }

        // ---------------------- SEATS ----------------------
        private void ManageSeats_Click(object sender, RoutedEventArgs e)
        {
            //_mainFrame.Content = new ManageSeatsPage(_mainFrame, _username);
        }

        // ---------------------- REPORTS ----------------------
        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            //_mainFrame.Content = new ReportsPage(_mainFrame, _username);
        }

        // ---------------------- POLICIES ----------------------
        private void Policies_Click(object sender, RoutedEventArgs e)
        {
            //_mainFrame.Content = new PoliciesPage(_mainFrame, _username);
        }

        // ---------------------- PROMO ----------------------
        private void Promo_Click(object sender, RoutedEventArgs e)
        {
            //_mainFrame.Content = new PromoPage(_mainFrame, _username);
        }

        // ---------------------- LOGOUT ----------------------
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new LoginPage(_mainFrame);
        }

        // ---------------------- PROFILE MENU HANDLERS ----------------------
        private void UpdateProfile_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to UpdateInfo page (formerly AdminSettingsPage)
            _mainFrame.Content = new UpdateInfo(_username);
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to UpdateInfo page but open ChangePasswordControl by default
            var updatePage = new UpdateInfo(_username);
            updatePage.ContentArea.Content = new ChangePasswordControl(_username);
            _mainFrame.Content = updatePage;
        }

        private void ProfileMenu_Click(object sender, RoutedEventArgs e)
        {
            // Toggle the popup open/close
            ProfilePopup.IsOpen = !ProfilePopup.IsOpen;
        }
    }
}