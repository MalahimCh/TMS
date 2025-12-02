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
using TMS.Pages.Customer;
using System.Diagnostics;




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
        private readonly BookingBL _bookingBL = new BookingBL();
        private readonly string _email;
        private readonly UserBL _userBL;
        private readonly Frame _mainFrame;
        private string _username;

        public SeatLayoutControl(Frame frame, ScheduleDTO schedule, string email, string username)
        {
            InitializeComponent();
            _schedule = schedule;
            _seatPrice = _schedule.Price;
            _busType = schedule.BusType; // Make sure ScheduleDTO has BusType
            _username = username;
            LoadSeatsFromDatabase();
            _email = email;
            _userBL = new UserBL(new UserDAL(), new OtpBL(new OtpDAL()));
            _mainFrame = frame;
        }

        private async void LoadSeatsFromDatabase()
        {
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
                            element = new Border { Width = 35, Height = 35, Background = Brushes.Transparent };
                        }
                        else
                        {
                            if (seat.Status == "Available")
                            {
                                var btn = new Button
                                {
                                    Width = 35,
                                    Height = 35,
                                    Margin = new Thickness(2),
                                    Tag = seat,
                                    Background = GetSeatBackground(seat)
                                };

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
                            else
                            {
                                // Booked / TemporarilyBooked seat displayed as Border to preserve color
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

                                element = new Border
                                {
                                    Width = 35,
                                    Height = 35,
                                    Margin = new Thickness(2),
                                    Background = GetSeatBackground(seat),
                                    BorderBrush = Brushes.Black,
                                    BorderThickness = new Thickness(1),
                                    Child = stack
                                };
                            }
                        }

                        Grid.SetRow(element, r);
                        Grid.SetColumn(element, c);
                        element.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
                        SeatsGrid.Children.Add(element);
                    }
                }
            }
            else
            {
                // Non-sleeper layout
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
                        else if (seat.Status == "Available")
                        {
                            var btn = new Button
                            {
                                Width = 35,
                                Height = 35,
                                Margin = new Thickness(2),
                                Tag = seat,
                                Content = seat.SeatNumber.ToString(),
                                Background = GetSeatBackground(seat)
                            };
                            btn.Click += SeatButton_Click;
                            element = btn;
                        }
                        else
                        {
                            // Booked / TemporarilyBooked
                            element = new Border
                            {
                                Width = 35,
                                Height = 35,
                                Margin = new Thickness(2),
                                Background = GetSeatBackground(seat),
                                BorderBrush = Brushes.Black,
                                BorderThickness = new Thickness(1),
                                Child = new TextBlock
                                {
                                    Text = seat.SeatNumber.ToString(),
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Center,
                                    FontSize = 12,
                                    FontWeight = FontWeights.Bold
                                }
                            };
                        }

                        uniformGrid.Children.Add(element);
                    }
                }

                SeatsGrid.Children.Add(uniformGrid);
            }

            UpdateSelectedSeatsOverlay();
        }


        // Determine correct background color
        private Brush GetSeatBackground(SeatModel seat)
        {
            // Selected by current user
            if (_selectedSeats.TryGetValue(seat.SeatNumber, out var gender))
            {
                return gender switch
                {
                    "Female" => Brushes.DeepPink,
                    "Male" => Brushes.DodgerBlue,
                    _ => Brushes.LightGreen
                };
            }

            // Already booked seats
            if (seat.Status == "Booked" || seat.Status == "TemporarilyBooked")
            {
                return seat.Gender switch
                {
                    "Female" => Brushes.Pink,
                    "Male" => Brushes.LightBlue,
                    _ => Brushes.Gray
                };
            }

            // Available / other statuses
            return seat.Status switch
            {
                "Available" => Brushes.LightGray,
                "Reserved" => Brushes.DarkGray,
                "NotAvailable" => Brushes.Red,
                _ => Brushes.LightGray
            };
        }


        private void SeatButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not SeatModel seat) return;

            if (_selectedSeats.ContainsKey(seat.SeatNumber))
            {
                // Deselect
                _selectedSeats.Remove(seat.SeatNumber);
                btn.Background = Brushes.LightGray;
                btn.BorderThickness = new Thickness(1);   // Reset border thickness
                btn.BorderBrush = Brushes.Black;
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
                    btn.Background = genderDialog.SelectedGender switch
                    {
                        "Female" => Brushes.DeepPink,
                        "Male" => Brushes.DodgerBlue,
                        _ => Brushes.LightGreen
                    };
                    btn.BorderThickness = _selectedSeats.ContainsKey(seat.SeatNumber) ? new Thickness(2) : new Thickness(1);
                    btn.BorderBrush = _selectedSeats.ContainsKey(seat.SeatNumber) ? Brushes.Gold : Brushes.Black;


                }
            }

            UpdateSelectedSeatsOverlay();
        }




        public List<(int SeatNumber, string Gender)> GetSelectedSeats()
        {
            return _selectedSeats.Select(kvp => (kvp.Key, kvp.Value)).ToList();
        }


        private async Task<BookingDTO> BuildBookingDTOAsync()
        {
            if (_selectedSeats.Count == 0)
                throw new Exception("No seats selected.");

            decimal total = 0;
            var seatsDto = new List<BookingSeatDTO>();

            foreach (var kvp in _selectedSeats)
            {
                var seat = _seats.FirstOrDefault(s => s.SeatNumber == kvp.Key);
                if (seat == null) continue;

                decimal price = seat.IsSide ? _seatPrice * 1.1m : _seatPrice;
                total += price;

                seatsDto.Add(new BookingSeatDTO
                {
                    SeatId = seat.Id,
                    SeatPrice = price,
                    Gender = _selectedSeats[seat.SeatNumber] // <- use selected gender
                });

            }

            var bookingRef = $"BK{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
            var user = await _userBL.GetUserByEmailAsync(_email); // **await instead of .Result**

            if (user == null)
                throw new Exception("User not found.");

            var booking = new BookingDTO
            {
                UserId = user.Id,
                ScheduleId = _schedule.Id,
                TotalAmount = total,
                DiscountAmount = 0,
                PromotionCode = null,
                BookingReference = bookingRef,
                Seats = seatsDto
            };

            return booking;
        }


        private void UpdateSelectedSeatsOverlay()
        {
            // Prepare list for ItemsControl
            var seatList = new List<dynamic>();
            decimal total = 0;

            foreach (var kvp in _selectedSeats)
            {
                var seat = _seats.FirstOrDefault(s => s.SeatNumber == kvp.Key);
                if (seat == null) continue;

                // Apply 1.1x for side seats
                decimal price = seat.IsSide ? _seatPrice * 1.1m : _seatPrice;
                total += price;

                seatList.Add(new { SeatNumber = seat.SeatNumber, Gender = kvp.Value, Price = price });
            }

            SelectedSeatsList.ItemsSource = seatList;
            TotalPriceTextSidebar.Text = $"Rs. {total:N0}";
        }


        private async void PayLater_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var booking = await BuildBookingDTOAsync();
                booking = await _bookingBL.CreateBookingAsync(booking);

                MessageBox.Show(
                    $"Booking reserved! You have 2 hours to pay.\nBooking Ref: {booking.BookingReference}",
                    "Booking Reserved",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                _mainFrame.Content = new CustomerDashboard(_mainFrame, _username, _email);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating booking: {ex.Message}");
            }
        }


        private async void PayNow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var booking = await BuildBookingDTOAsync();
                booking = await _bookingBL.CreateBookingAsync(booking);

                // Navigate to PaymentPage with booking
                _mainFrame.Content = new PaymentPage(_mainFrame, booking, _username, _email);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating booking: {ex.Message}");
            }
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