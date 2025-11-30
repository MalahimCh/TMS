using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.Controls;
using TMS.DAL;
using TMS.DTO;
using TMS.Pages.Admin;

namespace TMS.Pages.Customer
{
    public partial class CustomerDashboard : Page
    {
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly string _email;
        private SupportBL _supportBL;
        private UserBL _userBL;
        public string UsernameDisplay => $"{_username} ▼";
        public CustomerDashboard(Frame frame, string username, string email)
        {
            InitializeComponent();
            _mainFrame = frame;
            _username = username;
            _email = email;

            _supportBL = new SupportBL();
            _userBL = new UserBL(new UserDAL(), new OtpBL(new OtpDAL()));

            DataContext = this;
            txtCustomerName.Text = $"Welcome, {username}";

            Loaded += CustomerDashboard_Loaded;
        }
        private async void CustomerDashboard_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await UpdateSupportBadgeAsync();
        }

        private async Task UpdateSupportBadgeAsync()
        {
            try
            {
                int customerId = await _userBL.GetUserIDByEmailAsync(_email);
                List<SupportRequestDTO> requests = await _supportBL.GetRequestsByCustomer(customerId);

                int waitingCount = requests.Count(r => r.Status == "WaitingCustomer");

                if (waitingCount > 0)
                {
                    SupportBadgeCard.Visibility = Visibility.Visible;
                    SupportBadgeCardText.Text = waitingCount.ToString();
                }
                else
                {
                    SupportBadgeCard.Visibility = Visibility.Collapsed;
                }
            }
            catch
            {
                SupportBadgeCard.Visibility = Visibility.Collapsed;
            }
        }

        // ---------------------- DASHBOARD BUTTONS ----------------------
        private void SearchTickets_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new SearchTicketsPage(_mainFrame, _username,_email);
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
            _mainFrame.Content = new SupportPage(_mainFrame, _username,_email);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new LoginPage(_mainFrame);
        }

        // ---------------------- PROFILE MENU HANDLERS ----------------------
        private void UpdateProfile_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new UpdateInfo(_email,_username,"customer");
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var updatePage = new UpdateInfo(_email,_username,"customer");
            updatePage.ContentArea.Content = new ChangePasswordControl(_email);
            _mainFrame.Content = updatePage;
        }

        private void ProfileMenu_Click(object sender, RoutedEventArgs e)
        {
            ProfilePopup.IsOpen = !ProfilePopup.IsOpen;
        }
    }
}


