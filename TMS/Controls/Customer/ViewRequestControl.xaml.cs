using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TMS.BLL;
using TMS.DTO;
using TMS.DAL;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TMS.Pages.Customer
{
    public partial class ViewRequestControl : UserControl
    {
        private string _username;
        private string _email;
        private SupportBL _supportBL;
        private UserBL _userBL;

        public ViewRequestControl(string username, string email)
        {
            InitializeComponent();
            _username = username;
            _email = email;
            _supportBL = new SupportBL();
            _userBL = new UserBL(new UserDAL(), new OtpBL(new OtpDAL()));
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRequestsAsync();
        }
        private async Task LoadRequestsAsync()
        {
            try
            {
                int customerId = await _userBL.GetUserIDByEmailAsync(_email);
                List<SupportRequestDTO> requests = await _supportBL.GetRequestsByCustomer(customerId);

                // Order requests by status
                requests.Sort((a, b) =>
                {
                    int GetOrder(string status) => status switch
                    {
                        "WaitingCustomer" => 0,
                        "WaitingSupport" => 1,
                        "Assigned" => 2,
                        "Resolved" => 3,
                        _ => 4
                    };

                    return GetOrder(a.Status).CompareTo(GetOrder(b.Status));
                });

                RequestsList.ItemsSource = requests;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading requests: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void RequestsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (RequestsList.SelectedItem is SupportRequestDTO request)
            {
                //Open the request details control
                var detailsControl = new RequestDetailsControl(request.RequestId, _username, _email);

                // Replace parent content (assuming it's a ContentControl)
                if (this.Parent is ContentControl parentContent)
                {
                    parentContent.Content = detailsControl;
                }
            }
        }
    }
}
