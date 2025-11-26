using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DAL;

namespace TMS.Pages
{
    public partial class LoginPage : Page
    {
        private readonly UserBL _userBL;
        private readonly Frame _mainFrame;

        public LoginPage(Frame frame)
        {
            InitializeComponent();
            _mainFrame = frame;
            _userBL = new UserBL(new UserDAL(), new OtpBL(new OtpDAL()));
        }
   

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text;
            string password = pwdBox.Password;

            try
            {

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Please enter both email and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var user = await _userBL.LoginAsync(email, password);

                if (user != null)
                {
                    if (user.Role == "admin")
                    {
                        _mainFrame.Content = new AdminDashboard(_mainFrame, user.FullName);
                    }
                    else
                    {
                        MessageBox.Show($"Welcome {user.FullName}! Role: {user.Role}",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    
                }
                else
                {
                    MessageBox.Show("Invalid email or password.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "NOT_VERIFIED")
                {
                    MessageBox.Show("Your email is not verified. Please verify your account first.",
                        "Email Not Verified", MessageBoxButton.OK, MessageBoxImage.Warning);

                    // Navigate to OTP page automatically
                    _mainFrame.Content = new OtpPage(_mainFrame,email);
                }
                else
                {
                    MessageBox.Show("An unexpected error occurred.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new ForgotPasswordPage(_mainFrame);
        }


        private void NavigateToRegister_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new RegisterPage(_mainFrame);
        }
    }
}
