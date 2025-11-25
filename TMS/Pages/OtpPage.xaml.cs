using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DAL;

namespace TMS.Pages
{
    public partial class OtpPage : Page
    {
        private readonly UserBL _userBL;
        private readonly Frame _mainFrame;
        private readonly string _userEmail;

        public OtpPage(Frame frame, string email)
        {
            InitializeComponent();
            _mainFrame = frame;
            _userBL = new UserBL(new UserDAL(), new OtpBL(new OtpDAL()));
            _userEmail = email;
            txtEmail.Text = email;
        }

        private async void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            string otpCode = txtOtp.Text.Trim();

            if (string.IsNullOrWhiteSpace(otpCode))
            {
                MessageBox.Show("Please enter the OTP.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool verified = await _userBL.VerifyUserOtpAsync(_userEmail, otpCode,"Register");

            if (verified)
            {
                MessageBox.Show("Email verified successfully! You can now login.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _mainFrame.Content = new LoginPage(_mainFrame);
            }
            else
            {
                MessageBox.Show("Invalid OTP. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ResendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var otpBL = new OtpBL(new OtpDAL());

                string otpCode = await new OtpBL(new OtpDAL()).GenerateAndStoreOtpAsync(_userEmail, "Register");

                _ = Task.Run(() => new OtpBL(new OtpDAL()).SendOtpEmailAsync(_userEmail, otpCode, "Register"));

                MessageBox.Show("A new OTP has been sent to your email.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("Failed to resend OTP. Try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
