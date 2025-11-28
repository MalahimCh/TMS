using System;
using System.Collections.Generic;
using TMS.DTO;

public static class RecurringHelper
{
    public static List<ScheduleDTO> GenerateSchedules(RecurringScheduleDTO rec)
    {
        var schedules = new List<ScheduleDTO>();
        DateTime current = rec.StartDate;

        while (current <= rec.EndDate)
        {
            if (rec.Frequency == "Daily" ||
                (rec.Frequency == "Weekly" && current.DayOfWeek == rec.StartDate.DayOfWeek) ||
                (rec.Frequency == "Mon-Fri" && current.DayOfWeek >= DayOfWeek.Monday && current.DayOfWeek <= DayOfWeek.Friday))
            {
                schedules.Add(new ScheduleDTO
                {
                    RouteId = rec.RouteId,
                    BusId = rec.BusId,
                    RecurringScheduleId = rec.Id,
                    DepartureTime = current.Date + rec.DepartureTime,
                    ArrivalTime = current.Date + rec.ArrivalTime,
                    Price = rec.Price
                });
            }

            current = current.AddDays(1);
        }

        return schedules;
    }
}
