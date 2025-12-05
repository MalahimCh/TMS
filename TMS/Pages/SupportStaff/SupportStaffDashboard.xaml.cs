using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TMS.BLL;
using TMS.Controls;
using TMS.DTO;
using TMS.Pages.Admin;

namespace TMS.Pages.SupportStaff
{
    public partial class SupportStaffDashboard : Page
    {
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly string _email;

        private List<SupportRequestDTO> _allRequests;

        public string UsernameDisplay => $"{_username} ▼";

        public SupportStaffDashboard(Frame frame, string username, string email)
        {
            InitializeComponent();
            _mainFrame = frame;
            _username = username;
            _email = email;

            DataContext = this;
            txtWelcomeUser.Text = $"Welcome, {_username}";

            // Attach Loaded event
            Loaded += SupportStaffDashboard_Loaded;
        }

        private async void SupportStaffDashboard_Loaded(object sender, RoutedEventArgs e)
        {
            // Use dispatcher to ensure UI elements are fully initialized
            await Dispatcher.BeginInvoke(new Action(async () =>
            {
                await LoadRequestsAsync();
            }));
        }

        private async System.Threading.Tasks.Task LoadRequestsAsync()
        {
            var bl = new SupportBL();
            _allRequests = await bl.GetAssignedSupportRequests(_email);

            // Set status color and unread flag
            foreach (var r in _allRequests)
            {
                r.StatusColor = r.Status switch
                {
                    "WaitingSupport" => "#DC3545",
                    "Assigned" => "#0D6EFD",
                    "WaitingCustomer" => "#FFC107",
                    "Resolved" => "#198754",
                    _ => "#6C757D"
                };

                r.ShowUnreadDot = r.Status == "WaitingSupport";
            }

            // Populate categories in ComboBox
            cmbCategoryFilter.Items.Clear();
            cmbCategoryFilter.Items.Add(new ComboBoxItem { Content = "All", IsSelected = true });

            var categories = _allRequests
                .Select(r => r.Category)
                .Distinct()
                .OrderBy(c => c);

            foreach (var cat in categories)
            {
                cmbCategoryFilter.Items.Add(new ComboBoxItem { Content = cat });
            }

            // Apply initial filter
            ApplyFilter("All");
        }

        private void CategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_allRequests == null) return;

            var selected = (cmbCategoryFilter.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "All";
            ApplyFilter(selected);
        }


        private void ApplyFilter(string category)
        {
            if (_allRequests == null || RequestList == null) return;

            IEnumerable<SupportRequestDTO> filtered = category == "All"
                ? _allRequests
                : _allRequests.Where(r => r.Category == category);

            // Sort by status priority
            filtered = filtered.OrderBy(r => GetStatusPriority(r.Status))
                               .ThenByDescending(r => r.CreatedAt); // recent requests first

            RequestList.ItemsSource = filtered.ToList();
        }

        // Helper to assign priority (lower = higher in the list)
        private int GetStatusPriority(string status)
        {
            return status switch
            {
                "WaitingSupport" => 0,   // most urgent
                "Assigned" => 1,
                "WaitingCustomer" => 2,
                "Resolved" => 3,
                _ => 4
            };
        }


        private void Request_Click(object sender, MouseButtonEventArgs e)
        {
            var border = (Border)sender;
            var req = (SupportRequestDTO)border.DataContext;
            _mainFrame.Content = new SupportRequestDetail(_mainFrame, req, _email);
        }

        private void Assigned_Click(object sender, RoutedEventArgs e) => ApplyFilter("All");

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new UpdateInfo(_mainFrame, _email, _username, "supportstaff");
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new LoginPage(_mainFrame);
        }

        private void ProfileMenu_Click(object sender, RoutedEventArgs e)
        {
            ProfilePopup.IsOpen = !ProfilePopup.IsOpen;
        }

        private void UpdateProfile_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new UpdateInfo(_mainFrame,_email, _username, "supportstaff");
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var page = new UpdateInfo(_mainFrame,_email, _username, "supportstaff");
            page.ContentArea.Content = new ChangePasswordControl(_email);
            _mainFrame.Content = page;
        }
    }
}
