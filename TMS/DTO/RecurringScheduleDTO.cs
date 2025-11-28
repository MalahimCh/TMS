using System;

namespace TMS.DTO
{
    public class RecurringScheduleDTO
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid RouteId { get; set; }
        public Guid BusId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Frequency { get; set; } // Daily, Weekly, Mon-Fri etc.
        public TimeSpan DepartureTime { get; set; }
        public TimeSpan ArrivalTime { get; set; }
        public decimal Price { get; set; }
        public DateTime NextRunDate { get; set; } // tracks schedule generation
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
