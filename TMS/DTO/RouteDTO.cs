using System;

namespace TMS.DTO
{
    public class RouteDTO
    {
        public int Id { get; set; }   // INT IDENTITY
        public string Origin { get; set; }
        public string Destination { get; set; }
        public int DistanceKm { get; set; }
        public int EstimatedTimeMinutes { get; set; }
        public DateTime CreatedAt { get; set; }

        public string RouteDisplay => $"{Origin} → {Destination}";
    }
}
