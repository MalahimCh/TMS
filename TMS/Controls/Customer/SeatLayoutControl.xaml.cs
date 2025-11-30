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

namespace TMS.Controls.Customer
{
    public partial class SeatLayoutControl : UserControl
    {
        private readonly ScheduleDTO _schedule;
        private  List<SeatModel> _seats;
        private readonly Dictionary<int, string> _selectedSeats = new(); // SeatNumber => Gender
        private readonly decimal _seatPrice;

        private readonly SeatBL _seatBL = new SeatBL(new SeatDAL()); // assumes a BL class to fetch seat data
        private readonly string _busType;

        public SeatLayoutControl(ScheduleDTO schedule)
        {
            InitializeComponent();
            _schedule = schedule;
            _seatPrice = _schedule.Price;
            _busType = schedule.BusType; // Make sure ScheduleDTO has BusType
            LoadSeatsFromDatabase();
        }

        
        private async void LoadSeatsFromDatabase()
        {
            // Get seats for this bus/schedule from DB
            _seats = await _seatBL.GetSeatsAsync(_schedule.BusId);

            RenderSeats();
        }

        private void RenderSeats()
        {
            SeatsGrid.Children.Clear();
            SeatsGrid.RowDefinitions.Clear();
            SeatsGrid.ColumnDefinitions.Clear();

            var strategy = SeatLayoutFactory.GetStrategy(_busType);
            var seatLayout = strategy.GenerateLayout(_seats); // 2D List

            if (_busType == "Sleeper")
            {
                // 2 left + 1 aisle + 1 side = 4 columns
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
                        {
                            // Empty space / aisle
                            element = new Border { Width = 35, Height = 35, Background = Brushes.Transparent };
                        }
                        else
                        {
                            // Show lower/upper side with different colors
                            var btn = new Button
                            {
                                Content = seat.SeatNumber.ToString(),
                                Width = 35,
                                Height = 35,
                                Margin = new Thickness(2),
                                Tag = seat,
                                Background = seat.Status switch
                                {
                                    "Available" => seat.BunkType == "Upper" ? Brushes.LightBlue : Brushes.LightGreen,
                                    "TemporarilyBooked" or "Booked" or "Reserved" => Brushes.DarkGray,
                                    "NotAvailable" => Brushes.Red,
                                    _ => Brushes.LightGray
                                },
                                IsEnabled = seat.Status == "Available"
                            };

                            btn.Click += SeatButton_Click;
                            element = btn;
                        }

                        Grid.SetRow(element, r);
                        Grid.SetColumn(element, c);
                        SeatsGrid.Children.Add(element);
                    }
                }
            }
            else
            {
                // Economy/Luxury layout using UniformGrid
                SeatsGrid.Children.Clear();
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
                        {
                            element = new Border { Width = 35, Height = 35, Background = Brushes.Transparent };
                        }
                        else
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
                                    "TemporarilyBooked" or "Booked" or "Reserved" => Brushes.DarkGray,
                                    "NotAvailable" => Brushes.Red,
                                    _ => Brushes.LightGray
                                },
                                IsEnabled = seat.Status == "Available"
                            };
                            btn.Click += SeatButton_Click;
                            element = btn;
                        }
                        uniformGrid.Children.Add(element);
                    }
                }

                SeatsGrid.Children.Add(uniformGrid);
            }

            UpdateSelectedSeatsOverlay();
        }

        private void SeatButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not SeatModel seat) return;

            if (_selectedSeats.ContainsKey(seat.SeatNumber))
            {
                // Deselect
                _selectedSeats.Remove(seat.SeatNumber);
                btn.Background = Brushes.LightGray;
            }
            else
            {
                // Ask gender
                var gender = AskGender();
                if (gender == null) return;

                _selectedSeats[seat.SeatNumber] = gender;
                btn.Background = gender == "Female" ? Brushes.Pink : Brushes.LightBlue;
            }

            UpdateSelectedSeatsOverlay();
        }

        private string? AskGender()
        {
            var result = MessageBox.Show("Select Yes for Female, No for Male", "Gender", MessageBoxButton.YesNoCancel);

            return result switch
            {
                MessageBoxResult.Yes => "Female",
                MessageBoxResult.No => "Male",
                _ => null
            };
        }

        private void UpdateSelectedSeatsOverlay()
        {
            if (_selectedSeats.Count == 0)
            {
                SelectedSeatsText.Text = "None";
                TotalPriceText.Text = $"Rs. 0";
            }
            else
            {
                SelectedSeatsText.Text = string.Join(", ", _selectedSeats.Keys);
                TotalPriceText.Text = $"Rs. {_selectedSeats.Count * _seatPrice:N0}";
            }
        }

        public List<(int SeatNumber, string Gender)> GetSelectedSeats()
        {
            return _selectedSeats.Select(kvp => (kvp.Key, kvp.Value)).ToList();
        }

        private void BookSeats_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSeats.Count == 0)
            {
                MessageBox.Show("Please select at least one seat.");
                return;
            }

            // Call PaymentSummary / Booking logic
            var selectedSeats = GetSelectedSeats();

            // TODO: Pass to PaymentSummaryControl or BookingBL
            MessageBox.Show($"Seats selected: {string.Join(", ", selectedSeats.Select(s => $"{s.SeatNumber}({s.Gender})"))}\nTotal: Rs. {_selectedSeats.Count * _seatPrice:N0}");
        }
    }
}
