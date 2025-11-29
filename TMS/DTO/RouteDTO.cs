using System;

namespace TMS.DTO
{
    public class RouteDTO
    {
        public int Id { get; set; }   // INT IDENTITY
        public int OriginId { get; set; }
        public int DestinationId { get; set; }
        public string OriginName { get; set; }        // optional, for UI display
        public string DestinationName { get; set; }   // optional, for UI display
        public int DistanceKm { get; set; }
        public int EstimatedTimeMinutes { get; set; }
        public DateTime CreatedAt { get; set; }

        public string RouteDisplay => $"{OriginName} → {DestinationName}";
    }
}
