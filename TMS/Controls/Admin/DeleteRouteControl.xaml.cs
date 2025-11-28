using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DTO;

namespace TMS.Controls.Admin
{
    public partial class DeleteRouteControl : UserControl
    {
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly RouteBL _routeBL;
        private RouteDTO _route;

        public DeleteRouteControl(Frame frame, string username)
        {
            InitializeComponent();
            _mainFrame = frame;
            _username = username;
            _routeBL = new RouteBL(new DAL.RouteDAL());

            LoadRoutes();
        }

        private async void LoadRoutes()
        {
            var routes = await _routeBL.GetAllRoutesAsync();
            cmbRoutes.ItemsSource = routes;
            cmbRoutes.DisplayMemberPath = "RouteDisplay";
        }

        private void SelectRoute_Click(object sender, RoutedEventArgs e)
        {
            _route = cmbRoutes.SelectedItem as RouteDTO;
            if (_route == null)
            {
                MessageBox.Show("Please select a route first.");
                return;
            }

            txtOrigin.Text = _route.Origin;
            txtDestination.Text = _route.Destination;
            txtDistance.Text = _route.DistanceKm.ToString();
            txtEstimatedTime.Text = _route.EstimatedTimeMinutes.ToString();
        }

        private async void DeleteRoute_Click(object sender, RoutedEventArgs e)
        {
            if (_route == null)
            {
                MessageBox.Show("No route selected.");
                return;
            }

            bool deleted = await _routeBL.DeleteRouteAsync(_route.Id);
            MessageBox.Show(deleted ? "Route deleted successfully!" : "Failed to delete route.");
        }
    }
}
