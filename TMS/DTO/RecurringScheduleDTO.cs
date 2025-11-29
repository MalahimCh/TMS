using System;
using System.Collections.Generic;

namespace TMS.DTO
{
    public class RecurringScheduleDTO
    {
        public int Id { get; set; }   // INT IDENTITY
        public int RouteId { get; set; }
        public int BusId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string Frequency { get; set; } = "Daily";

        public List<DayOfWeek> SelectedDays { get; set; } = new List<DayOfWeek>();

        public TimeSpan DepartureTime { get; set; }
        public TimeSpan ArrivalTime { get; set; }

        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
