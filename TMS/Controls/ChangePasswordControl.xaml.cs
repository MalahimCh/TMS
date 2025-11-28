using System.Windows;
using System.Windows.Controls;

namespace TMS.Controls
{
    public partial class ChangePasswordControl : UserControl
    {
        private readonly string _username;

        public ChangePasswordControl(string username)
        {
            InitializeComponent();
            _username = username;
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
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

            // TODO: Verify current password from database and update to newPass
            // For now, we just show success
            MessageBox.Show("Password changed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            txtCurrent.Clear();
            txtNew.Clear();
            txtConfirm.Clear();
        }
    }
}