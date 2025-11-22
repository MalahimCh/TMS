using System.Windows;
using TMS.Pages;

namespace TMS
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Load LoginPage by default
            MainFrame.Content = new LoginPage(MainFrame);
        }
    }
}
