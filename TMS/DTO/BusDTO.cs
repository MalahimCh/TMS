using System;

namespace TMS.DTO
{
    public class BusDTO
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string BusNumber { get; set; }
        public string BusType { get; set; }
        public int TotalSeats { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
