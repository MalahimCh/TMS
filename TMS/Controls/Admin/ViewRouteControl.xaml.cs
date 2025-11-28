using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DTO;

namespace TMS.Controls.Admin
{
    public partial class ViewRouteControl : UserControl
    {
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly RouteBL _routeBL;

        public ViewRouteControl(Frame frame, string username)
        {
            InitializeComponent();
            _mainFrame = frame;
            _username = username;
            _routeBL = new RouteBL(new DAL.RouteDAL());

            LoadRoutes();
        }

        private async void LoadRoutes()
        {
            List<RouteDTO> routes = await _routeBL.GetAllRoutesAsync();
            if (routes != null)
            {
                dgRoutes.ItemsSource = routes;
            }
            else
            {
                MessageBox.Show("No routes found.");
            }
        }
    }
}
