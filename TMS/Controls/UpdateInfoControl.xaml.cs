using System.Windows;
using System.Windows.Controls;

namespace TMS.Controls
{
    public partial class UpdateInfoControl : UserControl
    {
        private readonly string _username;

        public UpdateInfoControl(string username)
        {
            InitializeComponent();
            _username = username;

            // Load user data (replace with actual data retrieval)
            txtName.Text = "John Doe";
            txtPhone.Text = "1234567890";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text.Trim();
            string phone = txtPhone.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // TODO: Save changes to database
            MessageBox.Show("Information updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}