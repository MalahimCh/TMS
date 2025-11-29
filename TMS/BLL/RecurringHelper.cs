using TMS.DTO;
using System;
using System.Collections.Generic;

public static class RecurringHelper
{
    public static List<ScheduleDTO> GenerateSchedules(RecurringScheduleDTO rec)
    {
        var schedules = new List<ScheduleDTO>();
        DateTime current = rec.StartDate;

        while (current <= rec.EndDate)
        {
            bool shouldCreate = false;

            if (rec.Frequency.Equals("Daily", StringComparison.OrdinalIgnoreCase))
            {
                shouldCreate = true;
            }
            else if (rec.Frequency.Equals("Weekly", StringComparison.OrdinalIgnoreCase))
            {
                if (rec.SelectedDays.Contains(current.DayOfWeek))
                    shouldCreate = true;
            }

            if (shouldCreate)
            {
                schedules.Add(new ScheduleDTO
                {
                    RouteId = rec.RouteId,               // int
                    BusId = rec.BusId,                   // int
                    RecurringScheduleId = rec.Id,        // int? (nullable)
                    DepartureTime = current.Date + rec.DepartureTime,
                    ArrivalTime = current.Date + rec.ArrivalTime,
                    Price = rec.Price,
                    Completed = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            current = current.AddDays(1);
        }

        return schedules;
    }
}
