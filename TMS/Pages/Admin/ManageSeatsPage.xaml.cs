using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using TMS.DTO;

namespace TMS.Pages.Admin
{
    public partial class ManageSeatsPage : Page
    {
        private readonly Frame _mainFrame;
        private readonly string _username;
        private readonly string _email;
        private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=TMS_DB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

        private List<SeatModel> _seats = new();

        public ManageSeatsPage(Frame mainFrame, string username, string email)
        {
            InitializeComponent();
            _mainFrame = mainFrame;
            _username = username;
            _email = email;

            LoadBusDropdown();
        }

        private async void LoadBusDropdown()
        {
            try
            {
                var buses = new List<BusItem>();
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = new SqlCommand("SELECT Id, BusNumber, BusType FROM Buses ORDER BY BusNumber", conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    buses.Add(new BusItem
                    {
                        Id = reader.GetInt32(0),
                        BusNumber = reader.GetString(1),
                        BusType = reader.GetString(2)
                    });
                }

                BusDropdown.ItemsSource = buses;
                BusDropdown.DisplayMemberPath = "DisplayName";
                BusDropdown.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading buses: " + ex.Message);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e) => _mainFrame.GoBack();

        private async void BusDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SeatsGrid.Children.Clear();
            SeatsGrid.RowDefinitions.Clear();
            SeatsGrid.ColumnDefinitions.Clear();

            if (BusDropdown.SelectedValue == null) return;

            int busId = (int)BusDropdown.SelectedValue;
            string busType = await GetBusType(busId);
            _seats = await LoadSeats(busId);

            if (_seats == null || _seats.Count == 0) return;

            RenderSeats(busType);
        }

        private async Task<string> GetBusType(int busId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("SELECT BusType FROM Buses WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", busId);
            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString() ?? "Economy";
        }

        private async Task<List<SeatModel>> LoadSeats(int busId)
        {
            var seats = new List<SeatModel>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand("SELECT Id, SeatNumber, Status, BunkType FROM Seats WHERE BusId=@busId ORDER BY SeatNumber", conn);
            cmd.Parameters.AddWithValue("@busId", busId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                seats.Add(new SeatModel
                {
                    Id = reader.GetInt32(0),
                    SeatNumber = reader.GetInt32(1),
                    Status = reader.GetString(2),
                    BunkType = reader.GetString(3)
                });
            }
            return seats;
        }

        private void RenderSeats(string busType)
        {
            if (_seats.Count == 0) return;

            if (busType.Equals("Sleeper", StringComparison.OrdinalIgnoreCase))
            {
                int cols = 4;
                int rows = (int)Math.Ceiling(_seats.Count / (double)cols);

                for (int c = 0; c < cols; c++)
                    SeatsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                for (int r = 0; r < rows; r++)
                    SeatsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                for (int i = 0; i < _seats.Count; i++)
                {
                    var seat = _seats[i];
                    int r = i / cols;
                    int c = i % cols;

                    var btn = CreateSeatButton(seat);
                    Grid.SetRow(btn, r);
                    Grid.SetColumn(btn, c);
                    SeatsGrid.Children.Add(btn);
                }
            }
            else
            {
                // Economy / Luxury
                var uniform = new UniformGrid
                {
                    Columns = 5,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(10)
                };

                foreach (var seat in _seats)
                    uniform.Children.Add(CreateSeatButton(seat));

                SeatsGrid.Children.Add(uniform);
            }
        }

        private Button CreateSeatButton(SeatModel seat)
        {
            var btn = new Button
            {
                Content = seat.SeatNumber.ToString(),
                Width = 35,
                Height = 35,
                Margin = new Thickness(2),
                Tag = seat,
                Background = seat.Status switch
                {
                    "Available" => Brushes.LightGray,
                    "NotAvailable" => Brushes.LightCoral,
                    "Booked" or "Reserved" => Brushes.DarkGray,
                    _ => Brushes.LightGray
                }
            };
            btn.Click += SeatButton_Click;
            return btn;
        }

        private async void SeatButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not SeatModel seat) return;

            if (seat.Status == "Available")
            {
                seat.Status = "NotAvailable";
                btn.Background = Brushes.LightCoral;
            }
            else if (seat.Status == "NotAvailable")
            {
                seat.Status = "Available";
                btn.Background = Brushes.LightGray;
            }

            await UpdateSeatInDB(seat);
        }

        private async Task UpdateSeatInDB(SeatModel seat)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();
                using var cmd = new SqlCommand("UPDATE Seats SET Status=@status WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@status", seat.Status);
                cmd.Parameters.AddWithValue("@id", seat.Id);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating seat: " + ex.Message);
            }
        }
    }

    public class BusItem
    {
        public int Id { get; set; }
        public string BusNumber { get; set; }
        public string BusType { get; set; }
        public string DisplayName => $"{BusNumber} | {BusType}";
    }

    public class SeatModel
    {
        public int Id { get; set; }
        public int SeatNumber { get; set; }
        public string Status { get; set; }
        public string BunkType { get; set; }
    }
}