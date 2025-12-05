using System.Windows;
using System.Windows.Controls;
using TMS.Controls;
using TMS.Pages.Customer;
using TMS.Pages.SupportStaff;

namespace TMS.Pages.Admin
{
    public partial class UpdateInfo : Page
    {
        private readonly string _email;
        private  string _username;

        private readonly string _role;
        private readonly Frame _mainFrame;
        public UpdateInfo(Frame frame,string email, string username, string role)
        {
            InitializeComponent();
            _email = email;
            _username = username;
            _role = role;
            _mainFrame = frame;

            var control = new UpdateInfoControl(_email);
            control.OnNameUpdated += newName => _username = newName;  // <-- UPDATE USERNAME

            ContentArea.Content = control;
        }


        private void Back_Click(object sender, RoutedEventArgs e)
        {
            switch (_role.ToLower())
            {
                case "admin":
                    _mainFrame.Content = new AdminDashboard(_mainFrame, _username, _email);
                    break;
                case "customer":
                    _mainFrame.Content = new CustomerDashboard(_mainFrame, _username, _email);
                    break;
                case "supportstaff":
                    _mainFrame.Content = new SupportStaffDashboard(_mainFrame, _username, _email);
                    break;


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