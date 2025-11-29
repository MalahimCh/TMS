using System;
using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DAL;

namespace TMS.Controls
{
    public partial class UpdateInfoControl : UserControl
    {
        public event Action<string>? OnNameUpdated;   // <-- ADD THIS

        private readonly string _email;
        private readonly UserBL _userBL;

        public UpdateInfoControl(string email)
        {
            InitializeComponent();
            _email = email;
            _userBL = new UserBL(new UserDAL(), new OtpBL(new OtpDAL()));

            LoadUserData();
        }

        private async void LoadUserData()
        {
            try
            {
                var user = await _userBL.GetUserByEmailAsync(_email);
                if (user != null)
                {
                    txtName.Text = user.FullName;
                    txtPhone.Text = user.PhoneNumber ?? "";
                }
                else
                {
                    MessageBox.Show("User not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("Failed to load user data.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();
            string phone = txtPhone.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Full name cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var updated = await _userBL.UpdateUserInfoAsync(_email, name, phone);
                if (updated)
                {
                    // notify parent page that name changed
                    OnNameUpdated?.Invoke(name);

                    MessageBox.Show("Information updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to update user information.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("An unexpected error occurred.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
