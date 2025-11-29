using System;

namespace TMS.DTO
{
    public class ScheduleDTO
    {
        public int Id { get; set; }   // INT IDENTITY
        public int RouteId { get; set; }
        public int BusId { get; set; }
        public int? RecurringScheduleId { get; set; }  // nullable
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal Price { get; set; }
        public bool Completed { get; set; }
        public DateTime CreatedAt { get; set; }

        // Display fields
        public string BusNumber { get; set; }
        public string BusType { get; set; }
        public string RouteDisplay { get; set; }

        // New fields for UI
        public double DistanceKm { get; set; }     // e.g., 350.5 km
        public string Duration { get; set; }       // e.g., "5h 30m"

        public string DisplayName =>
            $"{DepartureTime:yyyy-MM-dd HH:mm} | Bus: {BusNumber} | Route: {RouteDisplay}";
    }
}
