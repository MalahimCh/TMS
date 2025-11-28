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
            var buses = await _busBL.GetAllBusesAsync();
            var routes = await _routeBL.GetAllRoutesAsync();

            var displayList = schedules.Select(s =>
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
                    Completed = s.Completed ? "Yes" : "No",
                    IsRecurring = s.RecurringScheduleId != null ? "Yes" : "No"
                };
            }).ToList();

            dgSchedules.ItemsSource = displayList;
        }
    }
}
