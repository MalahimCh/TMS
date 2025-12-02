using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.Controls.Admin; // updated to AdminSeatLayoutControl
using TMS.DTO;

namespace TMS.Pages.Admin
{
    public partial class ManageSeatsPage : Page
    {
        private readonly BusBL _busBL;
        private AdminSeatLayoutControl _seatLayoutControl;
        private readonly string _email;
        private readonly string _username;
        private readonly Frame _mainframe;

        public ManageSeatsPage(Frame frame, string email, string username)
        {
            InitializeComponent();
            _busBL = new BusBL(new DAL.BusDAL());
            _mainframe = frame;
            _email = email;
            _username = username;


            LoadBuses();

            // Initialize seat layout control
            _seatLayoutControl = new AdminSeatLayoutControl();
            SeatLayout.Content = _seatLayoutControl; // put it inside the ScrollViewer
        }

        private async void LoadBuses()
        {
            var buses = await _busBL.GetAllBusesAsync();
            BusDropdown.ItemsSource = buses;
            BusDropdown.DisplayMemberPath = "BusNumber";
            BusDropdown.SelectedValuePath = "Id";
        }

        private void BusDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BusDropdown.SelectedValue == null) return;

            int busId = (int)BusDropdown.SelectedValue;

            // Assume bus object has BusType property
            var bus = BusDropdown.SelectedItem as DTO.BusDTO;
            string busType = bus?.BusType ?? "Standard"; // fallback

            // Load seats for the selected bus
            _seatLayoutControl.LoadSeats(busId, busType);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _mainframe.Content = new AdminDashboard(_mainframe, _username, _email);

        }
    }
}
