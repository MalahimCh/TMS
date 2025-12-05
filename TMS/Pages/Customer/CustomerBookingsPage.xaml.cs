using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DAL;
using TMS.DTO;

namespace TMS.Pages.Customer
{
    public partial class CustomerBookingsPage : Page
    {
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly string _email;
          private readonly UserBL _userBL;
        private readonly BookingBL _bookingBL;

   
        public CustomerBookingsPage(Frame frame, string username, string email)
        {
            InitializeComponent();

            _mainFrame = frame;
            _username = username;
            _email = email;
            _bookingBL = new BookingBL();
            _userBL = new UserBL(new UserDAL(), new OtpBL(new OtpDAL()));

            this.Loaded += CustomerBookingsPage_Loaded;
        }

        private async void CustomerBookingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadBookingHistoryAsync();
        }

        private async Task LoadBookingHistoryAsync()
        {
            try
            {

                MessageBox.Show("Starting LoadBookingHistoryAsync");

                var userId = await _userBL.GetUserIDByEmailAsync(_email);
                MessageBox.Show("UserID: " + userId);
                List<BookingDTO> bookings = null;
                try
                {
                    bookings = await _bookingBL.GetBookingsByUserIdAsync(userId);
                    MessageBox.Show("Inside try: bookings call returned");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error inside GetBookingsByUserIdAsync:\n" + ex.Message);
                }
                MessageBox.Show("Bookings: " + (bookings?.Count ?? 0));


                if (bookings == null || bookings.Count == 0)
                {
                    ConfirmedBookingsDataGrid.ItemsSource = null;
                    PendingBookingsDataGrid.ItemsSource = null;
                    return;
                }


                // Split into confirmed and pending
                var confirmed = bookings.Where(b => b.BookingStatus.Equals("Confirmed", StringComparison.OrdinalIgnoreCase)).ToList();
                var pending = bookings.Where(b => b.BookingStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase)).ToList();

                ConfirmedBookingsDataGrid.ItemsSource = confirmed;
                PendingBookingsDataGrid.ItemsSource = pending;

                MessageBox.Show($"Confirmed: {confirmed.Count}, Pending: {pending.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR LOADING BOOKINGS:\n" + ex.Message);
            }
        }




        // Button click handlers (empty for now)
        private void PayNow_Click(object sender, RoutedEventArgs e)
        {
            // TODO: implement payment logic
        }

        private void CancelBooking_Click(object sender, RoutedEventArgs e)
        {
            // TODO: implement cancel logic
        }


        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new CustomerDashboard(_mainFrame, _username, _email);
        }
    }
}