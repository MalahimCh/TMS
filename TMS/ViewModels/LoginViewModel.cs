using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Windows;
using TMS.BLL;

namespace TMS.ViewModels
{
    public class LoginViewModel : ObservableObject
    {
        private readonly UserBL _userBL;

        public LoginViewModel(UserBL userBL)
        {
            _userBL = userBL;
        }

        // Properties bound to UI
        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        // LOGIN COMMAND
        public IRelayCommand LoginCommand => new RelayCommand<object>(async (param) => await Login(param));

        private async Task Login(object param)
        {
            string password = param as string ?? "";

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both email and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = await _userBL.LoginAsync(Email, password);

            if (user != null)
            {
                MessageBox.Show($"Welcome {user.FullName}! Role: {user.Role}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                // TODO: Navigate to main dashboard or next window based on role
            }
            else
            {
                MessageBox.Show("Invalid email or password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
