using System;
using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DAL;

namespace TMS.Pages
{
    public partial class ForgotPasswordPage : Page
    {
        private readonly UserBL _userBL;
        private readonly Frame _mainFrame;
        private int _currentStep = 1; // 1 = send OTP, 2 = reset password

        public ForgotPasswordPage(Frame frame)
        {
            InitializeComponent();
            _mainFrame = frame;
            _userBL = new UserBL(new UserDAL(), new OtpBL(new OtpDAL()));
        }
        private async void ResendButton_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            if (string.IsNullOrWhiteSpace(email)) return;

            try
            {
                await new OtpBL(new OtpDAL()).SendOtpEmailAsync(email, "ResetPassword");
                MessageBox.Show("A new OTP has been sent to your email.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("Failed to resend OTP. Try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Please enter your email.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentStep == 1)
            {
                // Step 1: Verify email exists
                var user = await _userBL.GetUserByEmailAsync(email);
                if (user == null)
                {
                    MessageBox.Show("Email not found. Please check your email or register first.",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Send OTP
                try
                {
                    await new OtpBL(new OtpDAL()).SendOtpEmailAsync(email, "ResetPassword");
                   

                    // Show OTP + New Password fields
                    lblOtp.Visibility = brdOtp.Visibility = Visibility.Visible;
                    lblNewPassword.Visibility = brdNewPassword.Visibility = Visibility.Visible;
                    lblConfirmPassword.Visibility = brdConfirmPassword.Visibility = Visibility.Visible;

                    // Show Resend OTP button
                    btnResend.Visibility = Visibility.Visible;

                    btnAction.Content = "Reset Password";
                    _currentStep = 2;

                }
                catch
                {
                    MessageBox.Show("Failed to send OTP. Try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            else if (_currentStep == 2)
            {
                // Step 2: Reset Password
                string otp = txtOtp.Text.Trim();
                string newPwd = pwdNew.Password.Trim();
                string confirmPwd = pwdConfirm.Password.Trim();

                if (string.IsNullOrWhiteSpace(otp) || string.IsNullOrWhiteSpace(newPwd) || string.IsNullOrWhiteSpace(confirmPwd))
                {
                    MessageBox.Show("All fields are required.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (newPwd != confirmPwd)
                {
                    MessageBox.Show("Passwords do not match.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool validOtp = await new OtpBL(new OtpDAL()).ValidateOtpAsync(email, otp, "ResetPassword");
                if (!validOtp)
                {
                    MessageBox.Show("Invalid OTP.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bool reset = await _userBL.ResetPasswordAsync(email, newPwd);
                if (reset)
                {
                    MessageBox.Show("Password reset successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    _mainFrame.Content = new LoginPage(_mainFrame);
                }
                else
                {
                    MessageBox.Show("Failed to reset password. Try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
