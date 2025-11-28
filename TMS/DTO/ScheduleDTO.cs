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

    // These are populated when fetching schedules for display
    public string BusNumber { get; set; } = "";
    public string RouteDisplay { get; set; } = "";

    public string DisplayName => $"{DepartureTime:yyyy-MM-dd HH:mm} | Bus: {BusNumber} | Route: {RouteDisplay}";
}
