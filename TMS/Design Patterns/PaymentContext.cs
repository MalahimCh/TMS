using TMS.DTO;
using System.Threading.Tasks;

namespace TMS.Payments
{
    public class PaymentContext
    {
        private readonly IPaymentStrategy _strategy;

        public PaymentContext(IPaymentStrategy strategy)
        {
            _strategy = strategy;
        }

        public async Task<bool> ExecutePaymentAsync(BookingDTO booking)
        {
            return await _strategy.PayAsync(booking);
        }

        public string MethodName => _strategy.MethodName;
    }
}
