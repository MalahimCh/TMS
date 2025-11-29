using System.Windows;
using System.Windows.Controls;
using TMS.Controls;
using TMS.Pages.Customer;

namespace TMS.Pages.Admin
{
    public partial class UpdateInfo : Page
    {
        private readonly string _email;
        private  string _username;

        private readonly string _role;

        public UpdateInfo(string email, string username, string role)
        {
            InitializeComponent();
            _email = email;
            _username = username;
            _role = role;

            var control = new UpdateInfoControl(_email);
            control.OnNameUpdated += newName => _username = newName;  // <-- UPDATE USERNAME

            ContentArea.Content = control;
        }


        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is Window mainWindow && (mainWindow.Content as Frame) != null)
            {
                switch (_role.ToLower())
                {
                    case "admin":
                        ((mainWindow.Content as Frame).Content) = new AdminDashboard((mainWindow.Content as Frame), _username, _email);
                        break;
                    case "customer":
                        ((mainWindow.Content as Frame).Content) = new CustomerDashboard((mainWindow.Content as Frame), _username, _email);
                        break;
                }
            }
        }

        private void Sidebar_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string tag = btn.Tag.ToString();

            switch (tag)
            {
                case "Info":
                    var info = new UpdateInfoControl(_email);
                    info.OnNameUpdated += newName => _username = newName;
                    ContentArea.Content = info;
                    break;

                case "Password":
                    ContentArea.Content = new ChangePasswordControl(_email);
                    break;
            }
        }

    }
}