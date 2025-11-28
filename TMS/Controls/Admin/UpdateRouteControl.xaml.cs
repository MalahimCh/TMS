using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DTO;

namespace TMS.Controls.Admin
{
    public partial class UpdateRouteControl : UserControl
    {
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly RouteBL _routeBL;
        private RouteDTO _route;

        public UpdateRouteControl(Frame frame, string username)
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
            cmbRoutes.DisplayMemberPath = "RouteDisplay"; // Custom property to show Origin -> Destination
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

        private async void UpdateRoute_Click(object sender, RoutedEventArgs e)
        {
            if (_route == null)
            {
                MessageBox.Show("No route selected.");
                return;
            }

            _route.Origin = txtOrigin.Text.Trim();
            _route.Destination = txtDestination.Text.Trim();

            if (!int.TryParse(txtDistance.Text, out int distanceKm))
            {
                MessageBox.Show("Invalid distance");
                return;
            }
            if (!int.TryParse(txtEstimatedTime.Text, out int estimatedTime))
            {
                MessageBox.Show("Invalid estimated time");
                return;
            }

            _route.DistanceKm = distanceKm;
            _route.EstimatedTimeMinutes = estimatedTime;

            bool updated = await _routeBL.UpdateRouteAsync(_route);
            MessageBox.Show(updated ? "Route updated successfully!" : "Failed to update route.");
        }
    }
}
