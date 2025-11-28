using System;

namespace TMS.DTO
{
    public class RouteDTO
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Origin { get; set; }
        public string Destination { get; set; }
        public int DistanceKm { get; set; }
        public int EstimatedTimeMinutes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string RouteDisplay => $"{Origin} → {Destination}";

    }
}
