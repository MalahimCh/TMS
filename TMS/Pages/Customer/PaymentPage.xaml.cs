using System;
using System.Windows;
using System.Windows.Controls;
using TMS.DTO;
using TMS.BLL;
using TMS.Payments;

namespace TMS.Pages.Customer
{
    public partial class PaymentPage : Page
    {
        private readonly BookingDTO _booking;
        private readonly BookingBL _bookingBL;
        private readonly string _username;
        private readonly string _email;
        private readonly Frame _mainFrame;

        public PaymentPage(Frame frame,BookingDTO booking, string username, string email)
        {
            InitializeComponent();
            _booking = booking;
            _username = username;
            _email = email;
            _mainFrame = frame;

            _bookingBL = new BookingBL();
            TotalAmountText.Text = $"Rs. {_booking.TotalAmount:N0}";
        }

        private async void PayNow_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentMethodCombo.SelectedItem is not ComboBoxItem selected)
            {
                MessageBox.Show("Please select a payment method.");
                return;
            }

            IPaymentStrategy strategy = selected.Content.ToString() switch
            {
                "JazzCash" => new JazzCashPayment(),
                "EasyPaisa" => new EasyPaisaPayment(),
                _ => null
            };

            if (strategy == null)
                return;

            var context = new PaymentContext(strategy);

            try
            {
                bool success = await context.ExecutePaymentAsync(_booking);

                if (success)
                {
                    // Update booking payment status to Paid
                    await _bookingBL.UpdatePaymentAsync(_booking.Id, "Paid", Guid.NewGuid().ToString(), context.MethodName);
                    MessageBox.Show("Payment successful!");
                    _mainFrame.Content = new CustomerDashboard(_mainFrame, _username, _email);

                }
                else
                {
                    MessageBox.Show("Payment failed. Please try again.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Payment error: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Your booking is pending. It will be valid for 2 hours.");
            _mainFrame.Content = new CustomerDashboard(_mainFrame, _username, _email);

        }
    }
}
