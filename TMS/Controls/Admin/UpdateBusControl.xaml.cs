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
            cmbBuses.DisplayMemberPath = "BusNumber"; // or any property
        }

        private void SelectBus_Click(object sender, RoutedEventArgs e)
        {
            _bus = cmbBuses.SelectedItem as BusDTO;
            if (_bus == null)
            {
                MessageBox.Show("Please select a bus first.");
                return;
            }

            txtBusNumber.Text = _bus.BusNumber;
            txtTotalSeats.Text = _bus.TotalSeats.ToString();

            foreach (ComboBoxItem item in cmbBusType.Items)
            {
                if ((string)item.Content == _bus.BusType)
                {
                    cmbBusType.SelectedItem = item;
                    break;
                }
            }
        }

        private async void UpdateBus_Click(object sender, RoutedEventArgs e)
        {
            if (_bus == null)
            {
                MessageBox.Show("No bus selected.");
                return;
            }

            _bus.BusType = ((ComboBoxItem)cmbBusType.SelectedItem)?.Content.ToString();
            if (!int.TryParse(txtTotalSeats.Text, out int seats))
            {
                MessageBox.Show("Invalid total seats");
                return;
            }
            _bus.TotalSeats = seats;

            bool updated = await _busBL.UpdateBusAsync(_bus);
            MessageBox.Show(updated ? "Bus updated successfully!" : "Failed to update bus.");
        }
    }
}
