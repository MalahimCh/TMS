using System;
using System.Threading.Tasks;
using TMS.DTO;

namespace TMS.Payments
{
    public class EasyPaisaPayment : IPaymentStrategy
    {
        public string MethodName => "EasyPaisa";

        public async Task<bool> PayAsync(BookingDTO booking)
        {
            // TODO: Replace this with real EasyPaisa API integration
            await Task.Delay(1500); // simulate network delay

            var txnId = $"EP{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";

            booking.PaymentStatus = "Paid";
            booking.TransactionId = txnId;
            booking.PaymentMethod = MethodName;

            return true;
        }
    }
}
