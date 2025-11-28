using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DTO;

namespace TMS.Controls.Admin
{
    public partial class DeleteScheduleControl : UserControl
    {
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly ScheduleBL _scheduleBL;

        public DeleteScheduleControl(Frame frame, string username)
        {
            InitializeComponent();
            _mainFrame = frame;
            _username = username;
            _scheduleBL = new ScheduleBL(new DAL.ScheduleDAL());

            LoadSchedules();
        }

        private async void LoadSchedules()
        {
            var schedules = await _scheduleBL.GetAllSchedulesAsync();
            cmbScheduleDelete.ItemsSource = schedules;
            cmbScheduleDelete.DisplayMemberPath = "DisplayName";
            cmbScheduleDelete.SelectedValuePath = "Id";
        }

        private async void DeleteScheduleDelete_Click(object sender, RoutedEventArgs e)
        {
            if (cmbScheduleDelete.SelectedItem == null)
            {
                MessageBox.Show("Select a schedule first.");
                return;
            }

            var schedule = cmbScheduleDelete.SelectedItem as ScheduleDTO;
            bool deleted = await _scheduleBL.DeleteScheduleAsync(schedule.Id);
            MessageBox.Show(deleted ? "Schedule deleted successfully!" : "Failed to delete schedule.");
            LoadSchedules();
        }
    }
}
