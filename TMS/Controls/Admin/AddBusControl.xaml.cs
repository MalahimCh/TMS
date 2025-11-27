using System;
using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DTO;

namespace TMS.Controls.Admin
{
    public partial class AddBusControl : UserControl
    {
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly BusBL _busBL;

        public AddBusControl(Frame frame, string username)
        {
            InitializeComponent();
            _mainFrame = frame;
            _username = username;
            _busBL = new BusBL(new DAL.BusDAL()); // Create BusBL instance
        }

        private async void AddBus_Click(object sender, RoutedEventArgs e)
        {
            string busNumber = txtBusNumber.Text;
            string busType = ((ComboBoxItem)cmbBusType.SelectedItem)?.Content.ToString();
            if (!int.TryParse(txtTotalSeats.Text, out int totalSeats))
            {
                MessageBox.Show("Invalid total seats");
                return;
            }

            bool added = await _busBL.AddBusAsync(busNumber,busType,totalSeats);
            if (added)
                MessageBox.Show("Bus added successfully!");
            else
                MessageBox.Show("Failed to add bus. Bus number might already exist.");
        }
    }
}
