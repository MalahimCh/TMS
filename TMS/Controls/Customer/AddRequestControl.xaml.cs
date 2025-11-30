using System;
using System.Windows;
using System.Windows.Controls;
using TMS.BLL; // Your business logic layer
using TMS.DTO; // Your data transfer objects
using TMS.DAL;

namespace TMS.Pages.Customer
{
    public partial class AddRequestControl : UserControl
    {
        private string _username;
        private string _email;
        private SupportBL _supportBL; // Assuming you have a BL for support requests
        private UserBL _userBL;
        public AddRequestControl(string username, string email)
        {
            InitializeComponent();
            _username = username;
            _email = email;
            _supportBL = new SupportBL(); // Initialize your business logic
            _userBL = new UserBL(new UserDAL(),new OtpBL(new OtpDAL()));
        }

        private async void SubmitRequest_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a category.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(SubjectTextBox.Text))
            {
                MessageBox.Show("Please enter a subject.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Please enter a description.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var userId = await _userBL.GetUserIDByEmailAsync(_email);

                var request = new SupportRequestDTO
                {
                    CustomerId = userId,
                    Category = (CategoryComboBox.SelectedItem as ComboBoxItem).Content.ToString(),
                    Subject = SubjectTextBox.Text.Trim(),
                    Description = DescriptionTextBox.Text.Trim(),
                    Status = "Assigned", // BL will update if using round-robin
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _supportBL.AddRequest(request); // async call

                MessageBox.Show("Support request submitted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear form
                CategoryComboBox.SelectedItem = null;
                SubjectTextBox.Clear();
                DescriptionTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting request: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
