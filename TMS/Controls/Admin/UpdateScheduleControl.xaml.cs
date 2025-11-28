using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DTO;
using System.Collections.Generic;

namespace TMS.Controls.Admin
{
    public partial class UpdateScheduleControl : UserControl
    {
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly ScheduleBL _scheduleBL;
        private readonly BusBL _busBL;
        private readonly RouteBL _routeBL;

        private ScheduleDTO selectedSchedule;
        private RecurringScheduleDTO selectedRecurring;
        private TimeSpan _routeEstimatedTime;

        public UpdateScheduleControl(Frame frame, string username)
        {
            InitializeComponent();
            _mainFrame = frame;
            _username = username;

            _scheduleBL = new ScheduleBL(new DAL.ScheduleDAL());
            _busBL = new BusBL(new DAL.BusDAL());
            _routeBL = new RouteBL(new DAL.RouteDAL());

            LoadBuses();
            LoadRoutes();
            LoadSchedules();
        }

        private void UpdateOption_Checked(object sender, RoutedEventArgs e)
        {
            if (rbUpdateAllFuture.IsChecked == true)
            {
                // Disable departure date for all future updates
                dpDepartureDateUpdate.IsEnabled = false;
            }
            else
            {
                // Enable departure date for single instance
                dpDepartureDateUpdate.IsEnabled = true;
            }
        }

        private async void LoadBuses()
        {
            var buses = await _busBL.GetAllBusesAsync();
            cmbBusUpdate.ItemsSource = buses;
            cmbBusUpdate.DisplayMemberPath = "BusNumber";
            cmbBusUpdate.SelectedValuePath = "Id";
        }

        private async void LoadRoutes()
        {
            var routes = await _routeBL.GetAllRoutesAsync();
            cmbRouteUpdate.ItemsSource = routes;
            cmbRouteUpdate.DisplayMemberPath = "RouteDisplay";
            cmbRouteUpdate.SelectedValuePath = "Id";
        }

        private async void LoadSchedules()
        {
            var schedules = await _scheduleBL.GetAllSchedulesAsync();
            cmbScheduleUpdate.ItemsSource = schedules;
            cmbScheduleUpdate.DisplayMemberPath = "DisplayName";
            cmbScheduleUpdate.SelectedValuePath = "Id";
        }

        private async void CmbScheduleUpdate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedSchedule = cmbScheduleUpdate.SelectedItem as ScheduleDTO;
            if (selectedSchedule == null)
            {
                SchedulePanelUpdate.Visibility = Visibility.Collapsed;
                RecurringUpdateModePanel.Visibility = Visibility.Collapsed;
                return;
            }

            cmbBusUpdate.SelectedValue = selectedSchedule.BusId;
            cmbRouteUpdate.SelectedValue = selectedSchedule.RouteId;
            txtPriceUpdate.Text = selectedSchedule.Price.ToString();

            var route = await _routeBL.GetRouteByIdAsync(selectedSchedule.RouteId);
            _routeEstimatedTime = TimeSpan.FromMinutes(route?.EstimatedTimeMinutes ?? 0);

            // Show common schedule panel
            SchedulePanelUpdate.Visibility = Visibility.Visible;

            // Fill departure
            dpDepartureDateUpdate.SelectedDate = selectedSchedule.DepartureTime.Date;
            tpDepartureTimeUpdate.Value = selectedSchedule.DepartureTime;

            // Auto-fill arrival
            UpdateArrivalTime();

            if (selectedSchedule.RecurringScheduleId != null)
            {
                // Recurring schedule
                selectedRecurring = await _scheduleBL.GetRecurringScheduleByIdAsync(selectedSchedule.RecurringScheduleId.Value);
                RecurringUpdateModePanel.Visibility = Visibility.Visible;

                rbUpdateThisInstance.IsChecked = true;
                rbUpdateAllFuture.IsChecked = false;
            }
            else
            {
                // One-time schedule
                selectedRecurring = null;
                RecurringUpdateModePanel.Visibility = Visibility.Collapsed;
            }
        }

        private async void CmbRouteUpdate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbRouteUpdate.SelectedValue == null) return;
            var route = await _routeBL.GetRouteByIdAsync((Guid)cmbRouteUpdate.SelectedValue);
            _routeEstimatedTime = TimeSpan.FromMinutes(route?.EstimatedTimeMinutes ?? 0);
            UpdateArrivalTime();
        }

        private void DepartureDateUpdate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateArrivalTime();
        }

        private void DepartureTimeUpdate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UpdateArrivalTime();
        }

        private void UpdateArrivalTime()
        {
            if (_routeEstimatedTime == TimeSpan.Zero) return;
            if (dpDepartureDateUpdate.SelectedDate.HasValue && tpDepartureTimeUpdate.Value.HasValue)
            {
                var dep = dpDepartureDateUpdate.SelectedDate.Value + tpDepartureTimeUpdate.Value.Value.TimeOfDay;
                var arr = dep + _routeEstimatedTime;
                dpArrivalDateUpdate.SelectedDate = arr.Date;
                tpArrivalTimeUpdate.Value = arr;
            }
        }

        private async void UpdateScheduleUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSchedule == null && selectedRecurring == null)
            {
                MessageBox.Show("Select a schedule first.");
                return;
            }

            if (!decimal.TryParse(txtPriceUpdate.Text.Trim(), out decimal price))
            {
                MessageBox.Show("Invalid price.");
                return;
            }

            // Compute departure & arrival
            var newDepartureDateTime = dpDepartureDateUpdate.SelectedDate.Value + tpDepartureTimeUpdate.Value.Value.TimeOfDay;
            var newArrivalDateTime = newDepartureDateTime + _routeEstimatedTime;

            if (selectedRecurring != null)
            {
                bool updateThisInstance = rbUpdateThisInstance.IsChecked == true;

                if (updateThisInstance)
                {
                    // Update only this instance
                    var scheduleDto = new ScheduleDTO
                    {
                        Id = selectedSchedule.Id,
                        BusId = (Guid)cmbBusUpdate.SelectedValue,
                        RouteId = (Guid)cmbRouteUpdate.SelectedValue,
                        DepartureTime = newDepartureDateTime,
                        ArrivalTime = newArrivalDateTime,
                        Price = price,
                        Completed = selectedSchedule.Completed
                    };

                    bool success = await _scheduleBL.UpdateScheduleAsync(scheduleDto);
                    MessageBox.Show(success ? "Schedule instance updated!" : "Failed to update schedule instance.");
                }
                else
                {
                    // Update all future instances by updating the recurring template
                    selectedRecurring.BusId = (Guid)cmbBusUpdate.SelectedValue;
                    selectedRecurring.RouteId = (Guid)cmbRouteUpdate.SelectedValue;
                    selectedRecurring.Price = price;

                    // Update DepartureTime & ArrivalTime (TimeSpan of day)
                    selectedRecurring.DepartureTime = newDepartureDateTime.TimeOfDay;
                    selectedRecurring.ArrivalTime = newArrivalDateTime.TimeOfDay;

                    bool success = await _scheduleBL.UpdateRecurringScheduleAsync(selectedRecurring);
                    MessageBox.Show(success ? "All future schedules updated!" : "Failed to update recurring schedules.");
                }
            }
            else if (selectedSchedule != null)
            {
                // One-time schedule update
                selectedSchedule.BusId = (Guid)cmbBusUpdate.SelectedValue;
                selectedSchedule.RouteId = (Guid)cmbRouteUpdate.SelectedValue;
                selectedSchedule.DepartureTime = newDepartureDateTime;
                selectedSchedule.ArrivalTime = newArrivalDateTime;
                selectedSchedule.Price = price;

                bool success = await _scheduleBL.UpdateScheduleAsync(selectedSchedule);
                MessageBox.Show(success ? "Schedule updated!" : "Failed to update schedule.");
            }

            LoadSchedules();
        }


    }
}
