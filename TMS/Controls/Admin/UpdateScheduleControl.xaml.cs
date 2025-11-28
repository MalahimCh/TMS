using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DTO;
using TMS.DAL;

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

        private void CmbScheduleUpdate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedSchedule = cmbScheduleUpdate.SelectedItem as ScheduleDTO;
            if (selectedSchedule == null) return;

            cmbBusUpdate.SelectedValue = selectedSchedule.BusId;
            cmbRouteUpdate.SelectedValue = selectedSchedule.RouteId;
            dpDepartureDateUpdate.SelectedDate = selectedSchedule.DepartureTime.Date;
            txtDepartureTimeUpdate.Text = selectedSchedule.DepartureTime.ToString("HH:mm");
            dpArrivalDateUpdate.SelectedDate = selectedSchedule.ArrivalTime.Date;
            txtArrivalTimeUpdate.Text = selectedSchedule.ArrivalTime.ToString("HH:mm");
            txtPriceUpdate.Text = selectedSchedule.Price.ToString();

            if (selectedSchedule.RecurringScheduleId != null)
            {
                chkRecurringUpdate.IsChecked = true;
                RecurringPanelUpdate.Visibility = Visibility.Visible;

                var rec = new ScheduleBL(new ScheduleDAL()).GetRecurringScheduleByIdAsync(selectedSchedule.RecurringScheduleId.Value).Result;
                dpStartDateUpdate.SelectedDate = rec.StartDate;
                dpEndDateUpdate.SelectedDate = rec.EndDate;
                cmbFrequencyUpdate.SelectedItem = cmbFrequencyUpdate.Items.OfType<ComboBoxItem>()
                    .FirstOrDefault(x => x.Content.ToString() == rec.Frequency);
            }
            else
            {
                chkRecurringUpdate.IsChecked = false;
                RecurringPanelUpdate.Visibility = Visibility.Collapsed;
            }
        }

        private void ChkRecurringUpdate_Checked(object sender, RoutedEventArgs e) => RecurringPanelUpdate.Visibility = Visibility.Visible;
        private void ChkRecurringUpdate_Unchecked(object sender, RoutedEventArgs e) => RecurringPanelUpdate.Visibility = Visibility.Collapsed;

        private async void UpdateScheduleUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSchedule == null) { MessageBox.Show("Select a schedule."); return; }

            if (!decimal.TryParse(txtPriceUpdate.Text.Trim(), out decimal price))
            {
                MessageBox.Show("Invalid price"); return;
            }

            if (!TimeSpan.TryParse(txtDepartureTimeUpdate.Text.Trim(), out TimeSpan depTime) ||
                !TimeSpan.TryParse(txtArrivalTimeUpdate.Text.Trim(), out TimeSpan arrTime))
            {
                MessageBox.Show("Invalid time format. Use HH:mm."); return;
            }

            if (chkRecurringUpdate.IsChecked == true)
            {
                var rec = new RecurringScheduleDTO
                {
                    Id = selectedSchedule.RecurringScheduleId.Value,
                    BusId = (Guid)cmbBusUpdate.SelectedValue,
                    RouteId = (Guid)cmbRouteUpdate.SelectedValue,
                    DepartureTime = depTime,
                    ArrivalTime = arrTime,
                    Price = price,
                    StartDate = dpStartDateUpdate.SelectedDate.Value,
                    EndDate = dpEndDateUpdate.SelectedDate.Value,
                    Frequency = ((ComboBoxItem)cmbFrequencyUpdate.SelectedItem).Content.ToString(),
                    NextRunDate = dpStartDateUpdate.SelectedDate.Value
                };
                await _scheduleBL.UpdateRecurringScheduleAsync(rec);
                MessageBox.Show("Recurring schedule updated!");
            }
            else
            {
                selectedSchedule.BusId = (Guid)cmbBusUpdate.SelectedValue;
                selectedSchedule.RouteId = (Guid)cmbRouteUpdate.SelectedValue;
                selectedSchedule.DepartureTime = dpDepartureDateUpdate.SelectedDate.Value + depTime;
                selectedSchedule.ArrivalTime = dpArrivalDateUpdate.SelectedDate.Value + arrTime;
                selectedSchedule.Price = price;

                await _scheduleBL.UpdateScheduleAsync(selectedSchedule);
                MessageBox.Show("Schedule updated!");
            }

            LoadSchedules();
        }
    }
}
