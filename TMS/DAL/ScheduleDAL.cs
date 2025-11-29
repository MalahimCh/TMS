using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMS.BLL;
using TMS.DTO;

namespace TMS.DAL
{
    public class ScheduleDAL
    {
        private readonly DBConnection _db = new DBConnection();

        // ---------------- ADD NORMAL SCHEDULE ----------------
        public async Task<int> AddScheduleAsync(ScheduleDTO s)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                INSERT INTO Schedules
                (RouteId, BusId, RecurringScheduleId, DepartureTime, ArrivalTime, Price, Completed)
                OUTPUT INSERTED.Id
                VALUES (@RouteId,@BusId,@RecurringScheduleId,@DepartureTime,@ArrivalTime,@Price,@Completed)",
                conn);

            cmd.Parameters.AddWithValue("@RouteId", s.RouteId);
            cmd.Parameters.AddWithValue("@BusId", s.BusId);
            cmd.Parameters.AddWithValue("@RecurringScheduleId", (object)s.RecurringScheduleId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DepartureTime", s.DepartureTime);
            cmd.Parameters.AddWithValue("@ArrivalTime", s.ArrivalTime);
            cmd.Parameters.AddWithValue("@Price", s.Price);
            cmd.Parameters.AddWithValue("@Completed", s.Completed);

            // return the generated int Id
            return (int)await cmd.ExecuteScalarAsync();
        }

        // ---------------- UPDATE NORMAL SCHEDULE ----------------
        public async Task<bool> UpdateScheduleAsync(ScheduleDTO s)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                UPDATE Schedules
                SET RouteId = @RouteId,
                    BusId = @BusId,
                    DepartureTime = @DepartureTime,
                    ArrivalTime = @ArrivalTime,
                    Price = @Price,
                    Completed = @Completed
                WHERE Id = @Id", conn);

            cmd.Parameters.AddWithValue("@Id", s.Id);
            cmd.Parameters.AddWithValue("@RouteId", s.RouteId);
            cmd.Parameters.AddWithValue("@BusId", s.BusId);
            cmd.Parameters.AddWithValue("@DepartureTime", s.DepartureTime);
            cmd.Parameters.AddWithValue("@ArrivalTime", s.ArrivalTime);
            cmd.Parameters.AddWithValue("@Price", s.Price);
            cmd.Parameters.AddWithValue("@Completed", s.Completed);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        // ---------------- DELETE NORMAL SCHEDULE ----------------
        public async Task<bool> DeleteScheduleAsync(int id)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand("DELETE FROM Schedules WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        // ---------------- ADD RECURRING SCHEDULE ----------------
        public async Task<int> AddRecurringScheduleAsync(RecurringScheduleDTO rec)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();

            try
            {
                string daysString = string.Join(",", rec.SelectedDays.Select(d => (int)d));

                var cmd = new SqlCommand(@"
                INSERT INTO RecurringSchedules
                (RouteId, BusId, StartDate, EndDate, Frequency, SelectedDays, DepartureTime, ArrivalTime, Price)
                OUTPUT INSERTED.Id
                VALUES (@RouteId,@BusId,@StartDate,@EndDate,@Frequency,@SelectedDays,@DepartureTime,@ArrivalTime,@Price)",
                                    conn, trans);

                cmd.Parameters.AddWithValue("@RouteId", rec.RouteId);
                cmd.Parameters.AddWithValue("@BusId", rec.BusId);
                cmd.Parameters.AddWithValue("@StartDate", rec.StartDate);
                cmd.Parameters.AddWithValue("@EndDate", rec.EndDate);
                cmd.Parameters.AddWithValue("@Frequency", rec.Frequency);
                cmd.Parameters.AddWithValue("@SelectedDays", daysString);
                cmd.Parameters.AddWithValue("@DepartureTime", rec.DepartureTime);
                cmd.Parameters.AddWithValue("@ArrivalTime", rec.ArrivalTime);
                cmd.Parameters.AddWithValue("@Price", rec.Price);

                int recId = (int)await cmd.ExecuteScalarAsync();
                rec.Id = recId;

                // Generate schedules based on DTO
                var schedules = RecurringHelper.GenerateSchedules(rec);
                foreach (var s in schedules)
                {
                    s.RecurringScheduleId = rec.Id;

                    var scheduleCmd = new SqlCommand(@"
INSERT INTO Schedules
(RouteId, BusId, RecurringScheduleId, DepartureTime, ArrivalTime, Price, Completed)
OUTPUT INSERTED.Id
VALUES (@RouteId,@BusId,@RecurringScheduleId,@DepartureTime,@ArrivalTime,@Price,@Completed)",
                        conn, trans);

                    scheduleCmd.Parameters.AddWithValue("@RouteId", s.RouteId);
                    scheduleCmd.Parameters.AddWithValue("@BusId", s.BusId);
                    scheduleCmd.Parameters.AddWithValue("@RecurringScheduleId", s.RecurringScheduleId ?? (object)DBNull.Value);
                    scheduleCmd.Parameters.AddWithValue("@DepartureTime", s.DepartureTime);
                    scheduleCmd.Parameters.AddWithValue("@ArrivalTime", s.ArrivalTime);
                    scheduleCmd.Parameters.AddWithValue("@Price", s.Price);
                    scheduleCmd.Parameters.AddWithValue("@Completed", s.Completed);

                    s.Id = (int)await scheduleCmd.ExecuteScalarAsync();
                }

                trans.Commit();
                return recId;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        // ---------------- UPDATE RECURRING SCHEDULE ----------------
        public async Task<bool> UpdateRecurringScheduleAsync(RecurringScheduleDTO rec)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();

            try
            {
                string daysString = string.Join(",", rec.SelectedDays.Select(d => (int)d));

                var cmd = new SqlCommand(@"
                    UPDATE RecurringSchedules
                    SET RouteId=@RouteId, BusId=@BusId, StartDate=@StartDate, EndDate=@EndDate,
                        Frequency=@Frequency, SelectedDays=@SelectedDays,
                        DepartureTime=@DepartureTime, ArrivalTime=@ArrivalTime, Price=@Price
                    WHERE Id=@Id", conn, trans);

                cmd.Parameters.AddWithValue("@Id", rec.Id);
                cmd.Parameters.AddWithValue("@RouteId", rec.RouteId);
                cmd.Parameters.AddWithValue("@BusId", rec.BusId);
                cmd.Parameters.AddWithValue("@StartDate", rec.StartDate);
                cmd.Parameters.AddWithValue("@EndDate", rec.EndDate);
                cmd.Parameters.AddWithValue("@Frequency", rec.Frequency);
                cmd.Parameters.AddWithValue("@SelectedDays", daysString);
                cmd.Parameters.AddWithValue("@DepartureTime", rec.DepartureTime);
                cmd.Parameters.AddWithValue("@ArrivalTime", rec.ArrivalTime);
                cmd.Parameters.AddWithValue("@Price", rec.Price);

                await cmd.ExecuteNonQueryAsync();

                // Delete future child schedules
                var deleteCmd = new SqlCommand(@"
DELETE FROM Schedules 
WHERE RecurringScheduleId = @Id AND DepartureTime >= @Now", conn, trans);

                deleteCmd.Parameters.AddWithValue("@Id", rec.Id);
                deleteCmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);
                await deleteCmd.ExecuteNonQueryAsync();

                // Regenerate schedules
                var schedules = RecurringHelper.GenerateSchedules(rec);
                foreach (var s in schedules)
                {
                    s.RecurringScheduleId = rec.Id;

                    var scheduleCmd = new SqlCommand(@"
INSERT INTO Schedules
(RouteId, BusId, RecurringScheduleId, DepartureTime, ArrivalTime, Price, Completed)
OUTPUT INSERTED.Id
VALUES (@RouteId,@BusId,@RecurringScheduleId,@DepartureTime,@ArrivalTime,@Price,@Completed)",
                        conn, trans);

                    scheduleCmd.Parameters.AddWithValue("@RouteId", s.RouteId);
                    scheduleCmd.Parameters.AddWithValue("@BusId", s.BusId);
                    scheduleCmd.Parameters.AddWithValue("@RecurringScheduleId", s.RecurringScheduleId ?? (object)DBNull.Value);
                    scheduleCmd.Parameters.AddWithValue("@DepartureTime", s.DepartureTime);
                    scheduleCmd.Parameters.AddWithValue("@ArrivalTime", s.ArrivalTime);
                    scheduleCmd.Parameters.AddWithValue("@Price", s.Price);
                    scheduleCmd.Parameters.AddWithValue("@Completed", s.Completed);

                    s.Id = (int)await scheduleCmd.ExecuteScalarAsync();
                }

                trans.Commit();
                return true;
            }
            catch
            {
                trans.Rollback();
                return false;
            }
        }

        // ---------------- DELETE RECURRING SCHEDULE ----------------
        public async Task<bool> DeleteRecurringScheduleAsync(int id)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();

            try
            {
                var delSchedules = new SqlCommand(@"
                    DELETE FROM Schedules
                    WHERE RecurringScheduleId=@Id AND DepartureTime >= @Now", conn, trans);

                delSchedules.Parameters.AddWithValue("@Id", id);
                delSchedules.Parameters.AddWithValue("@Now", DateTime.UtcNow);
                await delSchedules.ExecuteNonQueryAsync();

                var delRecurring = new SqlCommand("DELETE FROM RecurringSchedules WHERE Id=@Id", conn, trans);
                delRecurring.Parameters.AddWithValue("@Id", id);
                int rows = await delRecurring.ExecuteNonQueryAsync();

                trans.Commit();
                return rows > 0;
            }
            catch
            {
                trans.Rollback();
                return false;
            }
        }

        // ---------------- UPDATE COMPLETED STATUS ----------------
        public async Task<int> UpdateCompletedSchedulesAsync()
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                UPDATE Schedules
                SET Completed = 1
                WHERE DepartureTime <= SYSUTCDATETIME() AND Completed = 0",
                                conn);

            return await cmd.ExecuteNonQueryAsync();
        }

        // ---------------- GET ALL SCHEDULES ----------------
        public async Task<List<ScheduleDTO>> GetAllSchedulesAsync()
        {
            var list = new List<ScheduleDTO>();
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                SELECT s.Id, s.RouteId, s.BusId, s.RecurringScheduleId, s.DepartureTime, s.ArrivalTime, s.Price, s.Completed, s.CreatedAt,
                       b.BusNumber, r.Origin, r.Destination
                FROM Schedules s
                JOIN Buses b ON s.BusId = b.Id
                JOIN Routes r ON s.RouteId = r.Id
                ORDER BY s.DepartureTime", conn);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new ScheduleDTO
                {
                    Id = reader.GetInt32(0),
                    RouteId = reader.GetInt32(1),
                    BusId = reader.GetInt32(2),
                    RecurringScheduleId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    DepartureTime = reader.GetDateTime(4),
                    ArrivalTime = reader.GetDateTime(5),
                    Price = reader.GetDecimal(6),
                    Completed = reader.GetBoolean(7),
                    CreatedAt = reader.GetDateTime(8),
                    BusNumber = reader.GetString(9),
                    RouteDisplay = $"{reader.GetString(10)} → {reader.GetString(11)}"
                });
            }

            return list;
        }

        // ---------------- GET SCHEDULE BY ID ----------------
        public async Task<ScheduleDTO?> GetScheduleByIdAsync(int id)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                SELECT s.Id, s.RouteId, s.BusId, s.RecurringScheduleId, s.DepartureTime, s.ArrivalTime, s.Price, s.Completed, s.CreatedAt,
                       b.BusNumber, r.Origin, r.Destination
                FROM Schedules s
                JOIN Buses b ON s.BusId = b.Id
                JOIN Routes r ON s.RouteId = r.Id
                WHERE s.Id = @Id", conn);

            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new ScheduleDTO
                {
                    Id = reader.GetInt32(0),
                    RouteId = reader.GetInt32(1),
                    BusId = reader.GetInt32(2),
                    RecurringScheduleId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    DepartureTime = reader.GetDateTime(4),
                    ArrivalTime = reader.GetDateTime(5),
                    Price = reader.GetDecimal(6),
                    Completed = reader.GetBoolean(7),
                    CreatedAt = reader.GetDateTime(8),
                    BusNumber = reader.GetString(9),
                    RouteDisplay = $"{reader.GetString(10)} → {reader.GetString(11)}"
                };
            }

            return null;
        }

        // ---------------- GET RECURRING SCHEDULE BY ID ----------------
        public async Task<RecurringScheduleDTO?> GetRecurringScheduleByIdAsync(int id)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                SELECT Id, RouteId, BusId, StartDate, EndDate, Frequency, SelectedDays, DepartureTime, ArrivalTime, Price, CreatedAt
                FROM RecurringSchedules 
                WHERE Id=@Id", conn);

            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var selectedDaysString = reader.IsDBNull(6) ? "" : reader.GetString(6);
                var selectedDays = selectedDaysString
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .Select(d => (DayOfWeek)d)
                    .ToList();

                return new RecurringScheduleDTO
                {
                    Id = reader.GetInt32(0),
                    RouteId = reader.GetInt32(1),
                    BusId = reader.GetInt32(2),
                    StartDate = reader.GetDateTime(3),
                    EndDate = reader.GetDateTime(4),
                    Frequency = reader.GetString(5),
                    SelectedDays = selectedDays,
                    DepartureTime = reader.GetTimeSpan(7),
                    ArrivalTime = reader.GetTimeSpan(8),
                    Price = reader.GetDecimal(9),
                    CreatedAt = reader.GetDateTime(10)
                };
            }

            return null;
        }

        // ---------------- GET ALL RECURRING SCHEDULES ----------------
        public async Task<List<RecurringScheduleDTO>> GetAllRecurringSchedulesAsync()
        {
            var list = new List<RecurringScheduleDTO>();
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                SELECT Id, RouteId, BusId, StartDate, EndDate, Frequency, SelectedDays, DepartureTime, ArrivalTime, Price, CreatedAt
                FROM RecurringSchedules
                ORDER BY StartDate", conn);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var selectedDaysString = reader.IsDBNull(6) ? "" : reader.GetString(6);
                var selectedDays = selectedDaysString
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .Select(d => (DayOfWeek)d)
                    .ToList();

                list.Add(new RecurringScheduleDTO
                {
                    Id = reader.GetInt32(0),
                    RouteId = reader.GetInt32(1),
                    BusId = reader.GetInt32(2),
                    StartDate = reader.GetDateTime(3),
                    EndDate = reader.GetDateTime(4),
                    Frequency = reader.GetString(5),
                    SelectedDays = selectedDays,
                    DepartureTime = reader.GetTimeSpan(7),
                    ArrivalTime = reader.GetTimeSpan(8),
                    Price = reader.GetDecimal(9),
                    CreatedAt = reader.GetDateTime(10)
                });
            }

            return list;
        }
    }
}
