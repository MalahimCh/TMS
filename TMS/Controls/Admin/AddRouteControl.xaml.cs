using System;
using System.Windows;
using System.Windows.Controls;
using TMS.BLL;

namespace TMS.Controls.Admin
{
    public partial class AddRouteControl : UserControl
    {
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly RouteBL _routeBL;

        public AddRouteControl(Frame frame, string username)
        {
            InitializeComponent();
            _mainFrame = frame;
            _username = username;
            _routeBL = new RouteBL(new DAL.RouteDAL());
        }

        private async void AddRoute_Click(object sender, RoutedEventArgs e)
        {
            string origin = txtOrigin.Text.Trim();
            string destination = txtDestination.Text.Trim();

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

            bool added = await _routeBL.AddRouteAsync(origin, destination, distanceKm, estimatedTime);

            MessageBox.Show(added ? "Route added successfully!" : "Failed to add route. It might already exist.");
        }
    }
}
