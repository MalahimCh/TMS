using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.DTO;


    namespace TMS.Payments
    {
        public interface IPaymentStrategy
        {
            Task<bool> PayAsync(BookingDTO booking);
            string MethodName { get; }
        }
    }


