using System.Windows;
using System.Windows.Controls;
using TMS.Controls.Customer;
using TMS.DTO;

namespace TMS.Pages.Customer
{
    public partial class SeatLayoutPage : Page
    {
        private readonly ScheduleDTO _schedule;
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly string _email;

        public SeatLayoutPage(ScheduleDTO schedule, Frame mainFrame, string username, string email)
        {
            InitializeComponent();
            _schedule = schedule;
            _mainFrame = mainFrame;
            _username = username;
            _email = email;

            // Add SeatLayoutControl to the ContentControl
            var seatControl = new SeatLayoutControl(_mainFrame,_schedule,_email,_username);
            SeatLayoutHost.Content = seatControl;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Go back to tickets list
            _mainFrame.Content = new SearchTicketsPage(_mainFrame, _username, _email);
        }
    }
}
