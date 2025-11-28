using System;
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

        private ScheduleDTO selectedSchedule;
        private RecurringScheduleDTO selectedRecurring;

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

            // Clear selection
            cmbScheduleDelete.SelectedIndex = -1;
            RecurringWarningPanel.Visibility = Visibility.Collapsed;
        }

        private async void CmbScheduleDelete_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedSchedule = cmbScheduleDelete.SelectedItem as ScheduleDTO;
            selectedRecurring = null;

            if (selectedSchedule != null && selectedSchedule.RecurringScheduleId != null)
            {
                selectedRecurring = await _scheduleBL.GetRecurringScheduleByIdAsync(selectedSchedule.RecurringScheduleId.Value);
                RecurringWarningPanel.Visibility = Visibility.Visible;
            }
            else
            {
                RecurringWarningPanel.Visibility = Visibility.Collapsed;
            }
        }

        private async void DeleteScheduleDelete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSchedule == null)
            {
                MessageBox.Show("Select a schedule first.");
                return;
            }

            bool confirmed = true;

            if (selectedRecurring != null)
            {
                var result = MessageBox.Show(
                    "This schedule is part of a recurring schedule. Deleting it will remove all future occurrences. Are you sure?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;

                confirmed = await _scheduleBL.DeleteRecurringScheduleAsync(selectedRecurring.Id);
            }
            else
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete this schedule?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;

                confirmed = await _scheduleBL.DeleteScheduleAsync(selectedSchedule.Id);
            }

            MessageBox.Show(confirmed ? "Schedule deleted successfully!" : "Failed to delete schedule.");
            LoadSchedules();
        }
    }
}
