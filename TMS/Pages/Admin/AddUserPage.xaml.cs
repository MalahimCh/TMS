using BCrypt.Net;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace TMS.Pages.Admin
{
    public partial class AddUserPage : Page
    {
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly string _email;
        private bool _isPasswordVisible = false;

    public AddUserPage(Frame frame, string username, string email)
        {
            InitializeComponent();
            _mainFrame = frame;
            _username = username;
            _email = email;
        }

        private async void AddUser_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string password = txtPassword.Password;
            string role = (cmbRole.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (string.IsNullOrEmpty(fullName)) { MessageBox.Show("Please enter full name."); return; }
            if (string.IsNullOrEmpty(email)) { MessageBox.Show("Please enter email."); return; }
            if (string.IsNullOrEmpty(phone)) { MessageBox.Show("Please enter phone number."); return; }
            if (string.IsNullOrEmpty(password)) { MessageBox.Show("Please enter password."); return; }
            if (string.IsNullOrEmpty(role)) { MessageBox.Show("Please select user role."); return; }

            role = role.ToLower().Replace(" ", "");

            try
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                using (SqlConnection conn = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TMS_DB;Integrated Security=True"))
                {
                    await conn.OpenAsync();

                    string query = @"INSERT INTO Users (FullName, Email, PhoneNumber, PasswordHash, Role, IsEmailVerified)  
                                 VALUES (@FullName, @Email, @PhoneNumber, @PasswordHash, @Role, @IsEmailVerified)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.Add("@FullName", System.Data.SqlDbType.NVarChar, 100).Value = fullName;
                        cmd.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 100).Value = email;
                        cmd.Parameters.Add("@PhoneNumber", System.Data.SqlDbType.NVarChar, 20).Value = phone;
                        cmd.Parameters.Add("@PasswordHash", System.Data.SqlDbType.NVarChar, 255).Value = hashedPassword;
                        cmd.Parameters.Add("@Role", System.Data.SqlDbType.NVarChar, 50).Value = role;
                        cmd.Parameters.Add("@IsEmailVerified", System.Data.SqlDbType.Bit).Value = 1;

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                MessageBox.Show("User added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                txtFullName.Clear();
                txtEmail.Clear();
                txtPhone.Clear();
                txtPassword.Clear();
                cmbRole.SelectedIndex = -1;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                    MessageBox.Show("Email already exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content = new AdminDashboard(_mainFrame, _username, _email);
        }

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                txtPasswordVisible.Text = txtPassword.Password;
                PasswordBorder.Visibility = Visibility.Collapsed;
                PasswordTextBorder.Visibility = Visibility.Visible;
                EyePack.Kind = PackIconKind.EyeOff;
            }
            else
            {
                txtPassword.Password = txtPasswordVisible.Text;
                PasswordBorder.Visibility = Visibility.Visible;
                PasswordTextBorder.Visibility = Visibility.Collapsed;
                EyePack.Kind = PackIconKind.Eye;
            }
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isPasswordVisible)
                txtPasswordVisible.Text = txtPassword.Password;
        }

        private void txtPasswordVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isPasswordVisible)
                txtPassword.Password = txtPasswordVisible.Text;
        }
    }
}