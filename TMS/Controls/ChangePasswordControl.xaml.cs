using System.Windows;
using System.Windows.Controls;
using TMS.BLL; // Make sure you have UserBL namespace
using TMS.DAL;

namespace TMS.Controls
{
    public partial class ChangePasswordControl : UserControl
    {
        private readonly string _email;
        private readonly UserBL _userBL;

        public ChangePasswordControl(string email)
        {
            InitializeComponent();
            _email = email;
            _userBL = new UserBL(new UserDAL(),new OtpBL(new OtpDAL())); // Instantiate your BL here
        }

        private async void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string current = txtCurrent.Password.Trim();
            string newPass = txtNew.Password.Trim();
            string confirm = txtConfirm.Password.Trim();

            if (string.IsNullOrEmpty(current) || string.IsNullOrEmpty(newPass) || string.IsNullOrEmpty(confirm))
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (newPass != confirm)
            {
                MessageBox.Show("New password and confirmation do not match.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Fetch the user by email
                var user = await _userBL.GetUserByEmailAsync(_email);

                if (user == null)
                {
                    MessageBox.Show("User not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Verify current password
                bool valid = BCrypt.Net.BCrypt.Verify(current, user.PasswordHash);

                if (!valid)
                {
                    MessageBox.Show("Current password is incorrect.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Update in DB
                bool updated = await _userBL.ResetPasswordAsync(_email, newPass);

                if (updated)
                {
                    MessageBox.Show("Password changed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtCurrent.Clear();
                    txtNew.Clear();
                    txtConfirm.Clear();
                }
                else
                {
                    MessageBox.Show("Failed to update password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("An unexpected error occurred.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

            private void ToggleCurrentPasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (txtCurrentVisible.Visibility == Visibility.Collapsed)
            {
                txtCurrentVisible.Text = txtCurrent.Password;
                txtCurrent.Visibility = Visibility.Collapsed;
                txtCurrentVisible.Visibility = Visibility.Visible;
                IconCurrent.Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOff;
            }
            else
            {
                txtCurrent.Password = txtCurrentVisible.Text;
                txtCurrentVisible.Visibility = Visibility.Collapsed;
                txtCurrent.Visibility = Visibility.Visible;
                IconCurrent.Kind = MaterialDesignThemes.Wpf.PackIconKind.Eye;
            }
        }

        private void ToggleNewPasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (txtNewVisible.Visibility == Visibility.Collapsed)
            {
                txtNewVisible.Text = txtNew.Password;
                txtNew.Visibility = Visibility.Collapsed;
                txtNewVisible.Visibility = Visibility.Visible;
                IconNew.Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOff;
            }
            else
            {
                txtNew.Password = txtNewVisible.Text;
                txtNewVisible.Visibility = Visibility.Collapsed;
                txtNew.Visibility = Visibility.Visible;
                IconNew.Kind = MaterialDesignThemes.Wpf.PackIconKind.Eye;
            }
        }

        private void ToggleConfirmPasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (txtConfirmVisible.Visibility == Visibility.Collapsed)
            {
                txtConfirmVisible.Text = txtConfirm.Password;
                txtConfirm.Visibility = Visibility.Collapsed;
                txtConfirmVisible.Visibility = Visibility.Visible;
                IconConfirm.Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOff;
            }
            else
            {
                txtConfirm.Password = txtConfirmVisible.Text;
                txtConfirmVisible.Visibility = Visibility.Collapsed;
                txtConfirm.Visibility = Visibility.Visible;
                IconConfirm.Kind = MaterialDesignThemes.Wpf.PackIconKind.Eye;
            }
        }

    }
}

