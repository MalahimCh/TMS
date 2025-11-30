using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.DTO
{
    
    public class BookingSeatDTO
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int SeatId { get; set; }
        public decimal SeatPrice { get; set; }

        public string? Gender { get; set; }
    }
}
