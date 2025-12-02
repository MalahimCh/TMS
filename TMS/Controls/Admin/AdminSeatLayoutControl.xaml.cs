using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using TMS.BLL;
using TMS.DAL;
using TMS.Design_Patterns;
using TMS.DTO;

namespace TMS.Controls.Admin
{
    public partial class AdminSeatLayoutControl : UserControl
    {
        private List<SeatModel> _seats;
        private readonly SeatBL _seatBL = new SeatBL(new SeatDAL());
        private string _busType;

        // Keep original statuses to detect changes
        private Dictionary<int, string> _originalStatuses = new();

        public AdminSeatLayoutControl()
        {
            InitializeComponent();
        }

        public async void LoadSeats(int busId, string busType)
        {
            _busType = busType;
            _seats = await _seatBL.GetSeatsAsync(busId);

            // Save original statuses
            _originalStatuses = _seats.ToDictionary(s => s.Id, s => s.Status);

            RenderSeats();
        }

        private void RenderSeats()
        {
            SeatsGrid.Children.Clear();
            SeatsGrid.RowDefinitions.Clear();
            SeatsGrid.ColumnDefinitions.Clear();

            if (_seats == null || !_seats.Any()) return;

            var strategy = SeatLayoutFactory.GetStrategy(_busType);
            var seatLayout = strategy.GenerateLayout(_seats); // 2D list

            if (_busType == "Sleeper")
            {
                int cols = 4;
                for (int c = 0; c < cols; c++)
                    SeatsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                for (int r = 0; r < seatLayout.Count; r++)
                {
                    SeatsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    var row = seatLayout[r];

                    for (int c = 0; c < row.Count; c++)
                    {
                        var seat = row[c];
                        UIElement element;

                        if (seat == null)
                            element = new Border { Width = 35, Height = 35, Background = Brushes.Transparent };
                        else
                            element = CreateSeatButton(seat);

                        Grid.SetRow(element, r);
                        Grid.SetColumn(element, c);
                        SeatsGrid.Children.Add(element);
                    }
                }
            }
            else
            {
                var uniformGrid = new UniformGrid
                {
                    Rows = seatLayout.Count,
                    Columns = seatLayout.Max(r => r.Count),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(10)
                };

                foreach (var row in seatLayout)
                {
                    foreach (var seat in row)
                    {
                        UIElement element;
                        if (seat == null)
                            element = new Border { Width = 35, Height = 35, Background = Brushes.Transparent };
                        else
                            element = CreateSeatButton(seat);

                        uniformGrid.Children.Add(element);
                    }
                }

                SeatsGrid.Children.Add(uniformGrid);
            }
        }

        private Button CreateSeatButton(SeatModel seat)
        {
            var stack = new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Center };
            stack.Children.Add(new TextBlock
            {
                Text = seat.SeatNumber.ToString(),
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            if (_busType == "Sleeper")
            {
                stack.Children.Add(new TextBlock
                {
                    Text = seat.BunkType == "Upper" ? "U" : "L",
                    FontSize = 10,
                    HorizontalAlignment = HorizontalAlignment.Center
                });
            }

            var btn = new Button
            {
                Width = 35,
                Height = 35,
                Margin = new Thickness(2),
                Content = stack,
                Tag = seat,
                Background = seat.Status == "Available" ? Brushes.LightGray : Brushes.Red,
                BorderThickness = _originalStatuses[seat.Id] != seat.Status ? new Thickness(3) : new Thickness(1),
                BorderBrush = _originalStatuses[seat.Id] != seat.Status ? Brushes.Gold : Brushes.Black
            };

            btn.Click += Seat_Click;

            return btn;
        }

        private async void Seat_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not SeatModel seat) return;

            // Only allow toggling if status is Available or NotAvailable
            if (seat.Status != "Available" && seat.Status != "NotAvailable")
                return;

            // Toggle status
            seat.Status = seat.Status == "Available" ? "NotAvailable" : "Available";

            // Update color
            btn.Background = seat.Status == "Available" ? Brushes.LightGray : Brushes.Red;

            // Highlight change
            if (_originalStatuses[seat.Id] != seat.Status)
            {
                btn.BorderThickness = new Thickness(3);
                btn.BorderBrush = Brushes.Gold;
            }
            else
            {
                btn.BorderThickness = new Thickness(1);
                btn.BorderBrush = Brushes.Black;
            }

            // Optionally prompt admin to save changes
            var result = MessageBox.Show("Do you want to save this change?", "Confirm Seat Change",
                                         MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                await _seatBL.UpdateSeatStatusAsync(seat.Id, seat.Status);
                _originalStatuses[seat.Id] = seat.Status; // update original status
                btn.BorderThickness = new Thickness(1);
                btn.BorderBrush = Brushes.Black;
            }
        }
    }
}
