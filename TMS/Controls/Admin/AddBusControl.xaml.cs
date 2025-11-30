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

            // Event to auto-set seats based on selected bus type  
            cmbBusType.SelectionChanged += CmbBusType_SelectionChanged;
        }

        // Event handler to automatically set total seats based on bus type  
        private void CmbBusType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedBusType = ((ComboBoxItem)cmbBusType.SelectedItem)?.Content.ToString();

            switch (selectedBusType)
            {
                case "Economy":
                    txtTotalSeats.Text = "49";
                    break;
                case "Luxury":
                    txtTotalSeats.Text = "33";
                    break;
                case "Sleeper":
                    txtTotalSeats.Text = "20";
                    break;
                default:
                    txtTotalSeats.Text = "";
                    break;
            }
        }

        // Add Bus button click handler  
        private async void AddBus_Click(object sender, RoutedEventArgs e)
        {
            // 1️⃣ Validate bus type selection  
            string busType = ((ComboBoxItem)cmbBusType.SelectedItem)?.Content.ToString();
            if (string.IsNullOrEmpty(busType))
            {
                MessageBox.Show("Please select a bus type.");
                return;
            }

            // 2️⃣ Validate bus number  
            string busNumber = txtBusNumber.Text.Trim();
            if (string.IsNullOrEmpty(busNumber))
            {
                MessageBox.Show("Please enter a bus number.");
                return;
            }

            // 3️⃣ Validate total seats (read-only, but still parse)  
            if (!int.TryParse(txtTotalSeats.Text, out int totalSeats))
            {
                MessageBox.Show("Invalid total seats");
                return;
            }

            try
            {
                // 4️⃣ Add bus via BusBL  
                bool added = await _busBL.AddBusAsync(busNumber, busType, totalSeats);
                if (added)
                {
                    MessageBox.Show("Bus added successfully!");

                    // Optional: Clear fields for new entry  
                    txtBusNumber.Text = "";
                    cmbBusType.SelectedIndex = -1;
                    txtTotalSeats.Text = "";
                }
                else
                {
                    MessageBox.Show("Failed to add bus. Bus number might already exist.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
    }
}