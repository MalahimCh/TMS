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
        private List<SeatModel> _seats;
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
                int cols = 4; // 2 left + 1 aisle + 1 side
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
                            element = new Border { Width = 35, Height = 35, Background = Brushes.Transparent };
                        }
                        else
                        {
                            var btn = new Button
                            {
                                Width = 35,
                                Height = 35,
                                Margin = new Thickness(2),
                                Tag = seat,
                                IsEnabled = seat.Status == "Available",
                                Background = seat.Status == "Available" ? Brushes.LightGray :
                                             seat.Status switch
                                             {
                                                 "TemporarilyBooked" or "Booked" or "Reserved" => Brushes.DarkGray,
                                                 "NotAvailable" => Brushes.Red,
                                                 _ => Brushes.LightGray
                                             }
                            };

                            // StackPanel inside button: seat number + U/L
                            var stack = new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Center };
                            stack.Children.Add(new TextBlock
                            {
                                Text = seat.SeatNumber.ToString(),
                                FontSize = 12,
                                FontWeight = FontWeights.Bold,
                                HorizontalAlignment = HorizontalAlignment.Center
                            });
                            stack.Children.Add(new TextBlock
                            {
                                Text = seat.BunkType == "Upper" ? "U" : "L",
                                FontSize = 10,
                                HorizontalAlignment = HorizontalAlignment.Center
                            });
                            btn.Content = stack;

                            btn.Click += SeatButton_Click;
                            element = btn;
                        }

                        Grid.SetRow(element, r);
                        Grid.SetColumn(element, c);
                        element.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center); // Center horizontally
                        SeatsGrid.Children.Add(element);
                    }

                    // Optional: separator line between upper and lower bunks
                    if (r > 0 && seatLayout[r].Any(s => s != null) && seatLayout[r][0]?.BunkType == "Lower" &&
                        seatLayout[r - 1].Any(s => s != null) && seatLayout[r - 1][0]?.BunkType == "Upper")
                    {
                        var line = new Border
                        {
                            BorderBrush = Brushes.Gray,
                            BorderThickness = new Thickness(0, 1, 0, 0),
                            Margin = new Thickness(0, 2, 0, 2)
                        };
                        Grid.SetRow(line, r);
                        Grid.SetColumnSpan(line, cols);
                        SeatsGrid.Children.Add(line);
                    }
                }
            }
            else
            {
                // Economy/Luxury using UniformGrid
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
                // Show nice popup for gender selection
                var genderDialog = new GenderSelectionWindow
                {
                    Owner = Window.GetWindow(this) // set parent window
                };

                if (genderDialog.ShowDialog() == true && genderDialog.SelectedGender != null)
                {
                    _selectedSeats[seat.SeatNumber] = genderDialog.SelectedGender;
                    btn.Background = genderDialog.SelectedGender == "Female" ? Brushes.Pink : Brushes.LightBlue;
                }
            }

            UpdateSelectedSeatsOverlay();
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