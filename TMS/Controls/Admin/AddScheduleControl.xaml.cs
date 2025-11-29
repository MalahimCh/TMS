using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DTO;

namespace TMS.Controls.Admin
{
    public partial class AddScheduleControl : UserControl
    {
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly ScheduleBL _scheduleBL;
        private TimeSpan _routeEstimatedTime;   // For auto-calculation of arrival

        public AddScheduleControl(Frame frame, string username)
        {
            InitializeComponent();
            _mainFrame = frame;
            _username = username;
            _scheduleBL = new ScheduleBL(new DAL.ScheduleDAL());

            LoadBuses();
            LoadRoutes();

            Loaded += AddScheduleControl_Loaded;
        }

        private void AddScheduleControl_Loaded(object sender, RoutedEventArgs e)
        {
            ShowPanels();
            DaysSelectionPanel.Visibility = Visibility.Collapsed; // hide days checkboxes initially
        }

        #region Load Combo Data
        private async void LoadBuses()
        {
            var buses = await new BusBL(new DAL.BusDAL()).GetAllBusesAsync();
            cmbBusAdd.ItemsSource = buses;
            cmbBusAdd.DisplayMemberPath = "BusNumber";
            cmbBusAdd.SelectedValuePath = "Id";
        }

        private async void LoadRoutes()
        {
            var routes = await new RouteBL(new DAL.RouteDAL()).GetAllRoutesAsync();
            cmbRouteAdd.ItemsSource = routes;
            cmbRouteAdd.DisplayMemberPath = "RouteDisplay";
            cmbRouteAdd.SelectedValuePath = "Id";
        }
        #endregion

        #region Route ETA & Arrival Calculation
        private async void cmbRouteAdd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbRouteAdd.SelectedValue == null) return;

            var routeId = (int)cmbRouteAdd.SelectedValue; // <-- int now
            var routeBL = new RouteBL(new DAL.RouteDAL());
            var route = await routeBL.GetRouteByIdAsync(routeId);

            _routeEstimatedTime = TimeSpan.FromMinutes(route?.EstimatedTimeMinutes ?? 0);

            UpdateArrivalTimeOneTime();
            UpdateArrivalTimeRecurring();
        }

  
        private void DepartureDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateArrivalTimeOneTime();
        }

        private void DepartureTimeChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UpdateArrivalTimeOneTime();
            UpdateArrivalTimeRecurring();
        }

        private void UpdateArrivalTimeOneTime()
        {
            if (_routeEstimatedTime == TimeSpan.Zero) return;
            if (dpDepartureDateAdd.SelectedDate.HasValue && tpDepartureTimeAdd.Value.HasValue)
            {
                var dep = dpDepartureDateAdd.SelectedDate.Value + tpDepartureTimeAdd.Value.Value.TimeOfDay;
                var arr = dep + _routeEstimatedTime;
                dpArrivalDateAdd.SelectedDate = arr.Date;
                tpArrivalTimeAdd.Value = arr;
            }
        }

        private void UpdateArrivalTimeRecurring()
        {
            if (_routeEstimatedTime == TimeSpan.Zero) return;
            if (tpDepartureTimeRecurring.Value.HasValue)
            {
                tpArrivalTimeRecurring.Value = tpDepartureTimeRecurring.Value.Value + _routeEstimatedTime;
            }
        }
        #endregion

        #region Panel Switch
        private void ScheduleType_Checked(object sender, RoutedEventArgs e) => ShowPanels();

        private void ShowPanels()
        {
            if (OneTimePanel == null) return;
            OneTimePanel.Visibility = rbOneTime.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            RecurringPanelAdd.Visibility = rbOneTime.IsChecked == true ? Visibility.Collapsed : Visibility.Visible;
        }
        #endregion

        #region Recurring Days Handling
        private void FrequencyChanged(object sender, SelectionChangedEventArgs e)
        {
            var freq = ((ComboBoxItem)cmbFrequencyAdd.SelectedItem)?.Content.ToString();

            if (freq == "Daily")
            {
                DaysSelectionPanel.Visibility = Visibility.Collapsed;
            }
            else if (freq == "Weekly")
            {
                DaysSelectionPanel.Visibility = Visibility.Visible;
            }
        }

        private List<DayOfWeek> GetSelectedDays()
        {
            var freq = ((ComboBoxItem)cmbFrequencyAdd.SelectedItem)?.Content.ToString();

            if (freq == "Daily")
            {
                // All days for daily
                return new List<DayOfWeek>
                {
                    DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                    DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday,
                    DayOfWeek.Sunday
                };
            }

            // Weekly -> checkboxes
            var days = new List<DayOfWeek>();
            if (chkMon.IsChecked == true) days.Add(DayOfWeek.Monday);
            if (chkTue.IsChecked == true) days.Add(DayOfWeek.Tuesday);
            if (chkWed.IsChecked == true) days.Add(DayOfWeek.Wednesday);
            if (chkThu.IsChecked == true) days.Add(DayOfWeek.Thursday);
            if (chkFri.IsChecked == true) days.Add(DayOfWeek.Friday);
            if (chkSat.IsChecked == true) days.Add(DayOfWeek.Saturday);
            if (chkSun.IsChecked == true) days.Add(DayOfWeek.Sunday);
            return days;
        }
        #endregion

        #region Save Button


        private async void AddScheduleAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cmbBusAdd.SelectedValue == null || cmbRouteAdd.SelectedValue == null)
            {
                MessageBox.Show("Select Bus and Route.");
                return;
            }

            if (!decimal.TryParse(txtPriceAdd.Text.Trim(), out decimal price))
            {
                MessageBox.Show("Invalid price");
                return;
            }

            // ONE-TIME
            if (rbOneTime.IsChecked == true)
            {
                if (!dpDepartureDateAdd.SelectedDate.HasValue || tpDepartureTimeAdd.Value == null)
                {
                    MessageBox.Show("Fill all departure fields.");
                    return;
                }

                var schedule = new ScheduleDTO
                {
                    BusId = (int)cmbBusAdd.SelectedValue,    // <-- changed to int
                    RouteId = (int)cmbRouteAdd.SelectedValue,// <-- changed to int
                    DepartureTime = dpDepartureDateAdd.SelectedDate.Value + tpDepartureTimeAdd.Value.Value.TimeOfDay,
                    ArrivalTime = dpArrivalDateAdd.SelectedDate.Value + tpArrivalTimeAdd.Value.Value.TimeOfDay,
                    Price = price,
                    Completed = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _scheduleBL.AddScheduleAsync(schedule);
                MessageBox.Show("One-Time schedule added.");
            }
            // RECURRING
            else
            {
                if (!dpStartDateAdd.SelectedDate.HasValue || !dpEndDateAdd.SelectedDate.HasValue || tpDepartureTimeRecurring.Value == null)
                {
                    MessageBox.Show("Fill all recurring fields.");
                    return;
                }

                var rec = new RecurringScheduleDTO
                {
                    BusId = (int)cmbBusAdd.SelectedValue,      // <-- changed to int
                    RouteId = (int)cmbRouteAdd.SelectedValue,  // <-- changed to int
                    DepartureTime = tpDepartureTimeRecurring.Value.Value.TimeOfDay,
                    ArrivalTime = tpArrivalTimeRecurring.Value.Value.TimeOfDay,
                    Price = price,
                    StartDate = dpStartDateAdd.SelectedDate.Value,
                    EndDate = dpEndDateAdd.SelectedDate.Value,
                    Frequency = ((ComboBoxItem)cmbFrequencyAdd.SelectedItem)?.Content.ToString(),
                    SelectedDays = GetSelectedDays()
                };

                await _scheduleBL.AddRecurringScheduleAsync(rec);

                MessageBox.Show("Recurring schedule added.");
            }

            ResetForm();
        }
        #endregion

        #region Reset Form
        private void ResetForm()
        {
            cmbBusAdd.SelectedIndex = -1;
            cmbRouteAdd.SelectedIndex = -1;
            txtPriceAdd.Text = "";

            dpDepartureDateAdd.SelectedDate = null;
            tpDepartureTimeAdd.Value = null;
            dpArrivalDateAdd.SelectedDate = null;
            tpArrivalTimeAdd.Value = null;

            dpStartDateAdd.SelectedDate = null;
            dpEndDateAdd.SelectedDate = null;
            tpDepartureTimeRecurring.Value = null;
            tpArrivalTimeRecurring.Value = null;

            cmbFrequencyAdd.SelectedIndex = -1;

            chkMon.IsChecked = chkTue.IsChecked = chkWed.IsChecked =
            chkThu.IsChecked = chkFri.IsChecked = chkSat.IsChecked = chkSun.IsChecked = false;

            rbOneTime.IsChecked = true;
            ShowPanels();
        }
        #endregion
    }
}
