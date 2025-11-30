using System;
using System.Threading.Tasks;
using TMS.DTO;

namespace TMS.Payments
{
    public class JazzCashPayment : IPaymentStrategy
    {
        public string MethodName => "JazzCash";

        public async Task<bool> PayAsync(BookingDTO booking)
        {
            // TODO: Replace this with real JazzCash API integration
            await Task.Delay(1500); // simulate network delay

            // Simulate successful payment response from API
            var txnId = $"JC{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";

            // Update the booking DTO
            booking.PaymentStatus = "Paid";
            booking.TransactionId = txnId;
            booking.PaymentMethod = MethodName;

            return true; // payment succeeded
        }
    }
}
