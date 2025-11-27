using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DAL;

namespace TMS.Pages
{
    public partial class RegisterPage : Page
    {
        private readonly UserBL _userBL;
        private readonly Frame _mainFrame;

        public RegisterPage(Frame frame)
        {
            InitializeComponent();
            _mainFrame = frame;
            _userBL = new UserBL(new UserDAL(), new OtpBL(new OtpDAL()));
        }
        private bool _isPasswordVisible = false;

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                txtPasswordVisible.Text = pwdBox.Password;
                PasswordBorder.Visibility = Visibility.Collapsed;
                PasswordTextBorder.Visibility = Visibility.Visible;

                EyePack.Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOff;
            }
            else
            {
                pwdBox.Password = txtPasswordVisible.Text;
                PasswordBorder.Visibility = Visibility.Visible;
                PasswordTextBorder.Visibility = Visibility.Collapsed;

                EyePack.Kind = MaterialDesignThemes.Wpf.PackIconKind.Eye;
            }
        }

        private void pwdBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isPasswordVisible)
                txtPasswordVisible.Text = pwdBox.Password;
        }

        private void txtPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isPasswordVisible)
                pwdBox.Password = txtPasswordVisible.Text;
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text;
            string email = txtEmail.Text;
            string phone = txtPhone.Text;
            string password = pwdBox.Password;

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please fill all fields including password.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = await _userBL.RegisterUserAsync(fullName, email, phone, password);

            if (success)
            {
                _mainFrame.Content = new OtpPage(_mainFrame, email);
                txtFullName.Clear();
                txtEmail.Clear();
                txtPhone.Clear();
                pwdBox.Clear();
            }
            else
            {
                MessageBox.Show("Email already exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void NavigateToLogin_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new LoginPage(_mainFrame);
        }

    }
}
