using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TMS.BLL;
using TMS.DTO;
using TMS.DAL;

namespace TMS.Pages.Customer
{
    public partial class RequestDetailsControl : UserControl
    {
        private int _requestId;
        private string _username;
        private string _email;
        private SupportBL _supportBL;
        private UserBL _userBL;

        public SupportRequestDTO Request { get; set; }
        private int _customerId;

        public RequestDetailsControl(int requestId, string username, string email)
        {
            InitializeComponent();
            _requestId = requestId;
            _username = username;
            _email = email;

            _supportBL = new SupportBL();
            _userBL = new UserBL(new UserDAL(), new OtpBL(new OtpDAL()));

            DataContext = this;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRequestDetailsAsync();
        }

        private async Task LoadRequestDetailsAsync()
        {
            try
            {
                _customerId = await _userBL.GetUserIDByEmailAsync(_email);

                // Load request
                Request = await _supportBL.GetRequestById(_requestId);

                // Load replies
                await LoadRepliesAsync();

                DataContext = null;
                DataContext = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading request details: {ex.Message}");
            }
        }

        private async Task LoadRepliesAsync()
        {
            var responses = await _supportBL.GetResponsesByRequest(_requestId);
            var replyItems = new List<ReplyItem>();

            foreach (var r in responses)
            {
                bool isStaff = r.UserId != _customerId;
                string name = isStaff ? "Support Staff" : _username;

                replyItems.Add(new ReplyItem
                {
                    UserName = name,
                    Message = r.Message,
                    CreatedAt = r.CreatedAt,
                    ReplyBackground = isStaff ?   new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4DA6FF")) // strong blue
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB347")) // warm orange
                });
            }

            RepliesList.ItemsSource = replyItems;
        }

        private async void SendReply_Click(object sender, RoutedEventArgs e)
        {
            string message = ReplyTextBox.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            var response = new SupportRequestResponseDTO
            {
                RequestId = _requestId,
                UserId = _customerId,
                Message = message
            };

            try
            {
                await _supportBL.AddResponse(response);

                // Update request status automatically
                if (Request.Status == "Assigned" || Request.Status == "WaitingCustomer")
                {
                    await _supportBL.UpdateRequestStatus(_requestId, "WaitingSupport");
                    Request.Status = "WaitingSupport";
                }
                DataContext = null;
                DataContext = this;
                ReplyTextBox.Clear();
                await LoadRepliesAsync(); // refresh thread
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending reply: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl parentContent)
            {
                var viewRequests = new ViewRequestControl(_username, _email);
                parentContent.Content = viewRequests;
            }
        }

        private class ReplyItem
        {
            public string UserName { get; set; }
            public string Message { get; set; }
            public DateTime CreatedAt { get; set; }
            public Brush ReplyBackground { get; set; }
        }
    }
}
