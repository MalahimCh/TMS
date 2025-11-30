using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.DTO
{
    public class SeatModel
    {
        public int Id { get; set; }             // DB primary key
        public int BusId { get; set; }          // foreign key to Bus
        public int SeatNumber { get; set; }     // sequential number
        public bool IsSide { get; set; }        // true for single seat (premium)
        public string? BunkType { get; set; }   // "Lower", "Upper" for sleeper, null otherwise
        public string Status { get; set; } = "Available"; // Available, TemporarilyBooked, Booked, Reserved, NotAvailable
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

