using System.Windows;
using System.Windows.Controls;
using TMS.Controls.Admin;

namespace TMS.Pages.Admin
{
    public partial class PoliciesPage : Page
    {
        private Frame _mainFrame;
        private string _email;
        private string _username;

        public PoliciesPage(Frame mainFrame, string email, string username)
        {
            InitializeComponent();
            _mainFrame = mainFrame;
            _email = email;
            _username = username;

            // Load the default view policies control
            MainContentControl.Content = new ViewPoliciesControl();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Content=new AdminDashboard(_mainFrame,_username,_email);
        }

        private void Sidebar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                switch (btn.Tag.ToString())
                {
                    case "View":
                        MainContentControl.Content = new ViewPoliciesControl();
                        break;
                    case "Add":
                        MainContentControl.Content = new AddPolicyControl();
                        break;
                    case "Update":
                        MainContentControl.Content = new UpdatePolicyControl();
                        break;
                    case "Delete":
                        MainContentControl.Content = new DeletePolicyControl();
                        break;
                }
            }
        }
    }
}
