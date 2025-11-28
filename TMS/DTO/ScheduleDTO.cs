using System;

namespace TMS.DTO
{
    public class ScheduleDTO
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid RouteId { get; set; }
        public Guid BusId { get; set; }
        public Guid? RecurringScheduleId { get; set; } // nullable for normal schedules
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal Price { get; set; }

        public bool Completed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string DisplayName => $"{DepartureTime:yyyy-MM-dd HH:mm} | Bus: {BusId} | Route: {RouteId}";

    }


}
