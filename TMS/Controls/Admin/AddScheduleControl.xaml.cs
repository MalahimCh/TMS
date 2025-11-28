using System;
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

        public AddScheduleControl(Frame frame, string username)
        {
            InitializeComponent();
            _mainFrame = frame;
            _username = username;
            _scheduleBL = new ScheduleBL(new DAL.ScheduleDAL());

            LoadBuses();
            LoadRoutes();
        }

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

        private void ChkRecurringAdd_Checked(object sender, RoutedEventArgs e) => RecurringPanelAdd.Visibility = Visibility.Visible;
        private void ChkRecurringAdd_Unchecked(object sender, RoutedEventArgs e) => RecurringPanelAdd.Visibility = Visibility.Collapsed;

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

            if (!TimeSpan.TryParse(txtDepartureTimeAdd.Text.Trim(), out TimeSpan depTime) ||
                !TimeSpan.TryParse(txtArrivalTimeAdd.Text.Trim(), out TimeSpan arrTime))
            {
                MessageBox.Show("Invalid time format. Use HH:mm.");
                return;
            }

            if (chkRecurringAdd.IsChecked == true)
            {
                if (!dpStartDateAdd.SelectedDate.HasValue || !dpEndDateAdd.SelectedDate.HasValue)
                {
                    MessageBox.Show("Select start and end date for recurring schedule");
                    return;
                }

                var rec = new RecurringScheduleDTO
                {
                    BusId = (Guid)cmbBusAdd.SelectedValue,
                    RouteId = (Guid)cmbRouteAdd.SelectedValue,
                    DepartureTime = depTime,
                    ArrivalTime = arrTime,
                    Price = price,
                    StartDate = dpStartDateAdd.SelectedDate.Value,
                    EndDate = dpEndDateAdd.SelectedDate.Value,
                    Frequency = ((ComboBoxItem)cmbFrequencyAdd.SelectedItem).Content.ToString(),
                    NextRunDate = dpStartDateAdd.SelectedDate.Value
                };

                await _scheduleBL.AddRecurringScheduleAsync(rec);
                MessageBox.Show("Recurring schedule added successfully!");
            }
            else
            {
                var schedule = new ScheduleDTO
                {
                    BusId = (Guid)cmbBusAdd.SelectedValue,
                    RouteId = (Guid)cmbRouteAdd.SelectedValue,
                    DepartureTime = dpDepartureDateAdd.SelectedDate.Value + depTime,
                    ArrivalTime = dpArrivalDateAdd.SelectedDate.Value + arrTime,
                    Price = price,
                    Completed = false
                };

                await _scheduleBL.AddScheduleAsync(schedule);
                MessageBox.Show("Schedule added successfully!");
            }

            // Reset
            cmbBusAdd.SelectedIndex = -1;
            cmbRouteAdd.SelectedIndex = -1;
            dpDepartureDateAdd.SelectedDate = null;
            txtDepartureTimeAdd.Text = "";
            dpArrivalDateAdd.SelectedDate = null;
            txtArrivalTimeAdd.Text = "";
            txtPriceAdd.Text = "";
            chkRecurringAdd.IsChecked = false;
        }
    }
}
