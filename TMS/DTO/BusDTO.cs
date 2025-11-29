using System;

namespace TMS.DTO
{
    public class BusDTO
    {
        public int Id { get; set; }   // INT IDENTITY
        public string BusNumber { get; set; }
        public string BusType { get; set; }
        public int TotalSeats { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
