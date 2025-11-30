using System;
using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DTO;

namespace TMS.Controls.Admin
{
    public partial class DeleteBusControl : UserControl
    {
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly BusBL _busBL;
        private BusDTO _bus;

        public DeleteBusControl(Frame frame, string username)
        {
            InitializeComponent();
            _mainFrame = frame;
            _username = username;
            _busBL = new BusBL(new DAL.BusDAL());

            LoadBuses(); // Load list of buses for selection
        }

        private async void LoadBuses()
        {
            var buses = await _busBL.GetAllBusesAsync();
            cmbBuses.ItemsSource = buses;
            cmbBuses.DisplayMemberPath = "BusNumber"; // display bus number
        }

        // Automatically fill data when bus is selected
        private void CmbBuses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _bus = cmbBuses.SelectedItem as BusDTO;
            if (_bus == null) return;

            txtBusNumber.Text = _bus.BusNumber;
            txtBusType.Text = _bus.BusType;
            txtTotalSeats.Text = _bus.TotalSeats.ToString();
        }

        private async void DeleteBus_Click(object sender, RoutedEventArgs e)
        {
            if (_bus == null)
            {
                MessageBox.Show("No bus selected.");
                return;
            }

            bool deleted = await _busBL.DeleteBusAsync(_bus.Id);
            MessageBox.Show(deleted ? "Bus deleted successfully!" : "Failed to delete bus.");
        }
    }
}