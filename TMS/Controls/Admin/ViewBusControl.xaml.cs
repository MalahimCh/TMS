using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TMS.BLL;
using TMS.DTO;

namespace TMS.Controls.Admin
{
    public partial class ViewBusControl : UserControl
    {
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly BusBL _busBL;

        public ViewBusControl(Frame frame, string username)
        {
            InitializeComponent();
            _mainFrame = frame;
            _username = username;
            _busBL = new BusBL(new DAL.BusDAL());

            LoadBuses();
        }

        private async void LoadBuses()
        {
            List<BusDTO> buses = await _busBL.GetAllBusesAsync();
            if (buses != null)
            {
                dgBuses.ItemsSource = buses;
            }
            else
            {
                MessageBox.Show("No buses found.");
            }
        }
    }
}
