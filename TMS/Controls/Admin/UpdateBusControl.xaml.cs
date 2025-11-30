using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DTO;

namespace TMS.Controls.Admin
{
    public partial class UpdateBusControl : UserControl
    {
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly BusBL _busBL;
        private BusDTO _bus;

        public UpdateBusControl(Frame frame, string username)
        {
            InitializeComponent();
            _mainFrame = frame;
            _username = username;
            _busBL = new BusBL(new DAL.BusDAL());

            LoadBuses();
        }

        private async void LoadBuses()
        {
            var buses = await _busBL.GetAllBusesAsync();
            cmbBuses.ItemsSource = buses;
            cmbBuses.DisplayMemberPath = "BusNumber";
        }

        // Auto-load bus data when selected
        private void CmbBuses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _bus = cmbBuses.SelectedItem as BusDTO;
            if (_bus == null) return;

            txtBusNumber.Text = _bus.BusNumber;

            foreach (ComboBoxItem item in cmbBusType.Items)
            {
                if ((string)item.Content == _bus.BusType)
                {
                    cmbBusType.SelectedItem = item;
                    break;
                }
            }

            // Show total seats for selected bus
            txtTotalSeats.Text = _bus.TotalSeats.ToString();
        }

        private void CmbBusType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_bus == null) return;

            var selectedType = ((ComboBoxItem)cmbBusType.SelectedItem)?.Content.ToString();
            switch (selectedType)
            {
                case "Economy":
                    _bus.TotalSeats = 49;
                    break;
                case "Luxury":
                    _bus.TotalSeats = 33;
                    break;
                case "Sleeper":
                    _bus.TotalSeats = 20;
                    break;
            }
            txtTotalSeats.Text = _bus.TotalSeats.ToString();
        }

        private async void UpdateBus_Click(object sender, RoutedEventArgs e)
        {
            if (_bus == null)
            {
                MessageBox.Show("No bus selected.");
                return;
            }

            _bus.BusNumber = txtBusNumber.Text.Trim();
            _bus.BusType = ((ComboBoxItem)cmbBusType.SelectedItem)?.Content.ToString();

            bool updated = await _busBL.UpdateBusAsync(_bus);
            MessageBox.Show(updated ? "Bus updated successfully!" : "Failed to update bus.");
        }
    }
}