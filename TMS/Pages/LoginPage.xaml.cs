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
            _userBL = new UserBL(new UserDAL());
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text;
            string password = pwdBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both email and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = await _userBL.LoginAsync(email, password);

            if (user != null)
                MessageBox.Show($"Welcome {user.FullName}! Role: {user.Role}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("Invalid email or password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void NavigateToRegister_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new RegisterPage(_mainFrame);
        }
    }
}
