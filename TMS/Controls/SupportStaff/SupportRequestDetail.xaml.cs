using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TMS.BLL;
using TMS.DAL;
using TMS.DTO;

namespace TMS.Pages.SupportStaff
{
    public partial class SupportRequestDetail : UserControl
    {
        private readonly Frame _mainFrame;
        private readonly string _staffEmail;
        private readonly SupportBL _supportBL;

        private UserBL _userBL;

        public SupportRequestDTO Request { get; set; }

        public SupportRequestDetail(Frame frame, SupportRequestDTO request, string staffEmail)
        {
            InitializeComponent();
            _mainFrame = frame;
            Request = request;
            _staffEmail = staffEmail;

            _supportBL = new SupportBL();

            _userBL = new UserBL(new UserDAL(), new OtpBL(new OtpDAL()));
            DataContext = this;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadRepliesAsync();
        }
        private async Task LoadRepliesAsync()
        {
            var responses = await _supportBL.GetResponsesByRequest(Request.RequestId);
            var replyItems = new List<ReplyItem>();

            foreach (var r in responses)
            {
                bool isStaff = r.UserId != Request.CustomerId; // staff messages
                string name;

                if (isStaff)
                {
                    // Fetch staff name from their user ID
                    var staff = await _userBL.GetUserByEmailAsync(_staffEmail);
                    name = staff?.FullName ?? "Support Staff"; // fallback
                }
                else
                {
                    name = Request.CustomerName;
                }

                replyItems.Add(new ReplyItem
                {
                    UserName = name,
                    Message = r.Message,
                    CreatedAt = r.CreatedAt,
                    ReplyBackground = isStaff
                         ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4DA6FF")) // strong blue
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB347")) // warm orange
                });
            }

            RepliesList.ItemsSource = replyItems;
        }

  
        private async void SendReply_Click(object sender, RoutedEventArgs e)
        {
            string message = ReplyTextBox.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            var staffId = await _userBL.GetUserIDByEmailAsync(_staffEmail);
            var response = new SupportRequestResponseDTO
            {
                RequestId = Request.RequestId,
                UserId = staffId,
                Message = message
            };

            try
            {
                await _supportBL.AddResponse(response);

                // Update status logic for staff
                // Staff just replied → now waiting for customer
                if (Request.Status != "Resolved")
                {
                    await _supportBL.UpdateRequestStatus(Request.RequestId, "WaitingCustomer");
                    Request.Status = "WaitingCustomer";

                    // Force UI refresh
                    DataContext = null;
                    DataContext = this;
                }

               

                ReplyTextBox.Clear();
                await LoadRepliesAsync(); // refresh thread
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending reply: {ex.Message}");
            }
        }


        private async void MarkResolved_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _supportBL.UpdateRequestStatus(Request.RequestId, "Resolved");
                Request.Status = "Resolved";

                DataContext = null;
                DataContext = this;
                await LoadRepliesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error marking resolved: {ex.Message}");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new SupportStaffDashboard(_mainFrame, _staffEmail.Split('@')[0], _staffEmail);
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
