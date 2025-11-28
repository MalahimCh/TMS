using System;
using System.Linq;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DTO;

namespace TMS.Controls.Admin
{
    public partial class ViewScheduleControl : UserControl
    {
        private readonly ScheduleBL _scheduleBL;
        private readonly BusBL _busBL;
        private readonly RouteBL _routeBL;

        public ViewScheduleControl(Frame frame, string username)
        {
            InitializeComponent();

            _scheduleBL = new ScheduleBL(new DAL.ScheduleDAL());
            _busBL = new BusBL(new DAL.BusDAL());
            _routeBL = new RouteBL(new DAL.RouteDAL());

            LoadSchedulesAsync();
        }

        private async void LoadSchedulesAsync()
        {
            var schedules = await _scheduleBL.GetAllSchedulesAsync();
            var recurringSchedules = (await _scheduleBL.GetAllRecurringSchedulesAsync()) ?? new System.Collections.Generic.List<RecurringScheduleDTO>();
            var buses = await _busBL.GetAllBusesAsync();
            var routes = await _routeBL.GetAllRoutesAsync();

            // ---------------- Recurring Schedules ----------------
            var recurringDisplay = recurringSchedules.Select(r =>
            {
                var bus = buses.FirstOrDefault(b => b.Id == r.BusId);
                var route = routes.FirstOrDefault(ro => ro.Id == r.RouteId);

                // Abbreviate days to 3 letters each
                var daysAbbr = string.Join(",", r.SelectedDays.Select(d => d.ToString().Substring(0, 3)));

                return new
                {
                    BusName = bus?.BusNumber ?? "N/A",
                    RouteName = route?.RouteDisplay ?? "N/A",
                    DateRange = $"{r.StartDate:yyyy-MM-dd} → {r.EndDate:yyyy-MM-dd}",
                    Days = daysAbbr,
                    Frequency = r.Frequency,
                    DepartureTime = r.DepartureTime.ToString(@"hh\:mm"),
                    ArrivalTime = r.ArrivalTime.ToString(@"hh\:mm"),
                    Price = r.Price
                };
            }).ToList();


            dgRecurringSchedules.ItemsSource = recurringDisplay;

            // ---------------- One-Time Schedules ----------------
            var oneTimeDisplay = schedules
                .Where(s => s.RecurringScheduleId == null)
                .Select(s =>
                {
                    var bus = buses.FirstOrDefault(b => b.Id == s.BusId);
                    var route = routes.FirstOrDefault(r => r.Id == s.RouteId);

                    return new
                    {
                        BusName = bus?.BusNumber ?? "N/A",
                        RouteName = route?.RouteDisplay ?? "N/A",
                        DepartureTime = s.DepartureTime.ToString("yyyy-MM-dd HH:mm"),
                        ArrivalTime = s.ArrivalTime.ToString("yyyy-MM-dd HH:mm"),
                        Price = s.Price,
                        Completed = s.Completed ? "Yes" : "No"
                    };
                }).ToList();

            dgOneTimeSchedules.ItemsSource = oneTimeDisplay;
        }
    }
}
