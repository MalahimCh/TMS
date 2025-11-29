using System;
using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DTO;
using System.Collections.Generic;

namespace TMS.Controls.Admin
{
    public partial class AddRouteControl : UserControl
    {
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly RouteBL _routeBL;
        private readonly LocationBL _locationBL;  // New BL to fetch locations
        private List<LocationDTO> _locations;

        public AddRouteControl(Frame frame, string username)
        {
            InitializeComponent();
            _mainFrame = frame;
            _username = username;

            _routeBL = new RouteBL(new DAL.RouteDAL());
            _locationBL = new LocationBL(new DAL.LocationDAL()); // assuming LocationBL exists

            LoadLocations();
        }

        private async void LoadLocations()
        {
            _locations = await _locationBL.GetAllLocationsAsync();

            cmbOrigin.ItemsSource = _locations;
            cmbOrigin.DisplayMemberPath = "Name";
            cmbOrigin.SelectedValuePath = "Id";

            cmbDestination.ItemsSource = _locations;
            cmbDestination.DisplayMemberPath = "Name";
            cmbDestination.SelectedValuePath = "Id";
        }

        private async void AddRoute_Click(object sender, RoutedEventArgs e)
        {
            if (cmbOrigin.SelectedValue == null || cmbDestination.SelectedValue == null)
            {
                MessageBox.Show("Select origin and destination.");
                return;
            }

            int originId = (int)cmbOrigin.SelectedValue;
            int destinationId = (int)cmbDestination.SelectedValue;

            if (!int.TryParse(txtDistance.Text.Trim(), out int distanceKm))
            {
                MessageBox.Show("Invalid distance");
                return;
            }

            if (!int.TryParse(txtEstimatedTime.Text.Trim(), out int estimatedTime))
            {
                MessageBox.Show("Invalid estimated time");
                return;
            }

            bool added = await _routeBL.AddRouteAsync(originId, destinationId, distanceKm, estimatedTime);

            MessageBox.Show(added ? "Route added successfully!" : "Failed to add route. It might already exist.");
        }
    }
}
