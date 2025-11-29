using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TMS.DTO;
using TMS.BLL;

namespace TMS.Pages.Customer
{
    public partial class SearchTicketsPage : Page
    {
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly string _email;
        private readonly LocationBL _locationBL;
        private readonly BusBL _busBL;
        private readonly ScheduleBL _scheduleBL;

        public SearchTicketsPage(Frame mainFrame, string username,string email)
        {
            InitializeComponent();

            _mainFrame = mainFrame;
            _username = username;

            _locationBL = new LocationBL(new DAL.LocationDAL());
            _busBL = new BusBL(new DAL.BusDAL());
            _scheduleBL = new ScheduleBL(new DAL.ScheduleDAL());

            LoadLocations();
            LoadBusTypes();
        }

        private async void LoadLocations()
        {
            var locations = await _locationBL.GetAllLocationsAsync();
            cmbOrigin.ItemsSource = locations;
            cmbOrigin.DisplayMemberPath = "Name";
            cmbOrigin.SelectedValuePath = "Id";

            cmbDestination.ItemsSource = locations;
            cmbDestination.DisplayMemberPath = "Name";
            cmbDestination.SelectedValuePath = "Id";
        }

        private async void LoadBusTypes()
        {
            var busTypes = await _busBL.GetAllBusTypesAsync();
            cmbBusType.ItemsSource = busTypes;
        }

        private async void ViewSchedules_Click(object sender, RoutedEventArgs e)
        {
            if (cmbOrigin.SelectedValue == null || cmbDestination.SelectedValue == null)
            {
                MessageBox.Show("Please select origin and destination.");
                return;
            }

            int originId = (int)cmbOrigin.SelectedValue;
            int destinationId = (int)cmbDestination.SelectedValue;
            string busType = cmbBusType.SelectedItem?.ToString();
            DateTime? date = dpDate.SelectedDate;

            // Fetch schedules from BL
            List<ScheduleDTO> schedules = await _scheduleBL.GetSchedulesAsync(originId, destinationId, date, busType);

            ScheduleCardsPanel.ItemsSource = schedules;

            if (schedules.Count == 0)
            {
                MessageBox.Show("No schedules found for the selected criteria.");
            }
        }

        private void SelectSeats_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ScheduleDTO schedule)
            {
                // Navigate to seat layout control/page
                //_mainFrame.Content = new SeatLayoutPage(_mainFrame, _username, schedule);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new CustomerDashboard(_mainFrame, _username, _email);
        }
    }
}
