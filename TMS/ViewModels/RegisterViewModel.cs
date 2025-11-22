using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Windows;
using TMS.BLL;

namespace TMS.ViewModels
{
    public class RegisterViewModel : ObservableObject
    {
        private readonly UserBL _userBL;

        public RegisterViewModel(UserBL userBL)
        {
            _userBL = userBL;
        }

        // Properties bound to UI
        private string _fullName;
        public string FullName { get => _fullName; set => SetProperty(ref _fullName, value); }

        private string _email;
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        private string _phone;
        public string Phone { get => _phone; set => SetProperty(ref _phone, value); }

        private string _role = "customer";
        public string Role { get => _role; set => SetProperty(ref _role, value); }

        // Command accepting password from PasswordBox as parameter
        public IRelayCommand<object> RegisterCommand => new RelayCommand<object>(async (param) => await Register(param));

        private async Task Register(object param)
        {
            // Get password from command parameter
            string password = param as string ?? "";

            // Validate all fields
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email)
                || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please fill all fields including password.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Call BLL to register
            bool success = await _userBL.RegisterUserAsync(FullName, Email, Phone, password, Role);
            if (success)
            {
                MessageBox.Show("Registration successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear fields (optional)
                FullName = Email = Phone = "";
                // Note: PasswordBox cannot be cleared here via MVVM; handle it in the View if needed
            }
            else
            {
                MessageBox.Show("Email already exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
