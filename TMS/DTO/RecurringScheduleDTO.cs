using System;
using System.Collections.Generic;

namespace TMS.DTO
{
    public class RecurringScheduleDTO
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid RouteId { get; set; }
        public Guid BusId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string Frequency { get; set; } = "Daily"; // or Daily

        public List<DayOfWeek> SelectedDays { get; set; } = new List<DayOfWeek>();

        public TimeSpan DepartureTime { get; set; }
        public TimeSpan ArrivalTime { get; set; }

        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }


}
