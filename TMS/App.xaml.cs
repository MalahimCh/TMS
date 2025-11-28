using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;
using TMS.BLL;
using TMS.DAL;

namespace TMS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private DispatcherTimer _scheduleTimer;
        private ScheduleBL _scheduleBL;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize BL (inject DAL)
            _scheduleBL = new ScheduleBL(new ScheduleDAL());

            // Setup timer
            _scheduleTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(5) // run every 5 minutes
            };
            _scheduleTimer.Tick += ScheduleTimer_Tick;
            _scheduleTimer.Start();
        }

        private async void ScheduleTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                await _scheduleBL.UpdateCompletedSchedulesAsync();
            }
            catch (Exception ex)
            {
                // Optionally log error
                Console.WriteLine("Error updating schedules: " + ex.Message);
            }
        }
    }

    


}
