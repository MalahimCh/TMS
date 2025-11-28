using System.Windows;
using System.Windows.Controls;
using TMS.Controls;

namespace TMS.Pages.Admin
{
    public partial class UpdateInfo : Page
    {
        private readonly string _username;

    public UpdateInfo(string username)
        {
            InitializeComponent();
            _username = username;

            // Load default control
            ContentArea.Content = new UpdateInfoControl(_username);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Go back to Admin Dashboard
            if (Application.Current.MainWindow is Window mainWindow)
            {
                if ((mainWindow.Content as Frame) != null)
                    ((mainWindow.Content as Frame).Content) = new AdminDashboard((mainWindow.Content as Frame), _username);
            }
        }

        private void Sidebar_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string tag = btn.Tag.ToString();

            switch (tag)
            {
                case "Info":
                    ContentArea.Content = new UpdateInfoControl(_username);
                    break;
                case "Password":
                    ContentArea.Content = new ChangePasswordControl(_username);
                    break;
            }
        }
    }
}