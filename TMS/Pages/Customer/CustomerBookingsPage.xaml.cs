using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace TMS.Pages.Customer
{
    public partial class CustomerBookingsPage : Page
    {
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly string _email;
        private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TMS_DB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

        private int _userId;

        public CustomerBookingsPage(Frame frame, string username, string email)
        {
            InitializeComponent();

            _mainFrame = frame;
            _username = username;
            _email = email;

            LoadUserId();
            LoadBookingHistory();
        }

        private void LoadUserId()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT Id FROM Users WHERE Email = @Email";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", _email);
                    object result = cmd.ExecuteScalar();
                    _userId = result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        private void LoadBookingHistory()
        {
            if (_userId == 0) return;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT BookingReference, BookingDate, ScheduleId, TotalAmount, DiscountAmount, FinalAmount, BookingStatus, PaymentStatus
                    FROM Bookings
                    WHERE UserId = @UserId
                    ORDER BY BookingDate DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", _userId);

                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        BookingsDataGrid.ItemsSource = dt.DefaultView;
                    }
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new CustomerDashboard(_mainFrame, _username, _email);
        }
    }
}