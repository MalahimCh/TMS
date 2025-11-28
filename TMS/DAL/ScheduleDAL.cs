using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMS.DTO;

namespace TMS.DAL
{
    public class ScheduleDAL
    {
        private readonly DBConnection _db = new DBConnection();

        // ---------------- ADD NORMAL SCHEDULE ----------------
        public async Task AddScheduleAsync(ScheduleDTO s)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                INSERT INTO Schedules
                (Id, RouteId, BusId, RecurringScheduleId, DepartureTime, ArrivalTime, Price, Completed, CreatedAt)
                VALUES (@Id,@RouteId,@BusId,@RecurringScheduleId,@DepartureTime,@ArrivalTime,@Price,@Completed,@CreatedAt)",
                conn);

            cmd.Parameters.AddWithValue("@Id", s.Id);
            cmd.Parameters.AddWithValue("@RouteId", s.RouteId);
            cmd.Parameters.AddWithValue("@BusId", s.BusId);
            cmd.Parameters.AddWithValue("@RecurringScheduleId", (object)s.RecurringScheduleId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DepartureTime", s.DepartureTime);
            cmd.Parameters.AddWithValue("@ArrivalTime", s.ArrivalTime);
            cmd.Parameters.AddWithValue("@Price", s.Price);
            cmd.Parameters.AddWithValue("@Completed", s.Completed);
            cmd.Parameters.AddWithValue("@CreatedAt", s.CreatedAt);

            await cmd.ExecuteNonQueryAsync();
        }

        // ---------------- UPDATE NORMAL SCHEDULE ----------------
        public async Task<bool> UpdateScheduleAsync(ScheduleDTO s)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"
                UPDATE Schedules SET
                    RouteId=@RouteId,
                    BusId=@BusId,
                    DepartureTime=@DepartureTime,
                    ArrivalTime=@ArrivalTime,
                    Price=@Price,
                    Completed=@Completed
                WHERE Id=@Id", conn);

            cmd.Parameters.AddWithValue("@Id", s.Id);
            cmd.Parameters.AddWithValue("@RouteId", s.RouteId);
            cmd.Parameters.AddWithValue("@BusId", s.BusId);
            cmd.Parameters.AddWithValue("@DepartureTime", s.DepartureTime);
            cmd.Parameters.AddWithValue("@ArrivalTime", s.ArrivalTime);
            cmd.Parameters.AddWithValue("@Price", s.Price);
            cmd.Parameters.AddWithValue("@Completed", s.Completed);

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        // ---------------- DELETE NORMAL SCHEDULE ----------------
        public async Task<bool> DeleteScheduleAsync(Guid id)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand("DELETE FROM Schedules WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        // ---------------- ADD RECURRING SCHEDULE ----------------
        public async Task AddRecurringScheduleAsync(RecurringScheduleDTO rec)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();

            try
            {
                // Insert recurring template
                var cmd = new SqlCommand(@"
                    INSERT INTO RecurringSchedules
                    (Id, RouteId, BusId, StartDate, EndDate, Frequency, DepartureTime, ArrivalTime, Price, NextRunDate, CreatedAt)
                    VALUES (@Id,@RouteId,@BusId,@StartDate,@EndDate,@Frequency,@DepartureTime,@ArrivalTime,@Price,@NextRunDate,@CreatedAt)",
                    conn, trans);

                cmd.Parameters.AddWithValue("@Id", rec.Id);
                cmd.Parameters.AddWithValue("@RouteId", rec.RouteId);
                cmd.Parameters.AddWithValue("@BusId", rec.BusId);
                cmd.Parameters.AddWithValue("@StartDate", rec.StartDate);
                cmd.Parameters.AddWithValue("@EndDate", rec.EndDate);
                cmd.Parameters.AddWithValue("@Frequency", rec.Frequency);
                cmd.Parameters.AddWithValue("@DepartureTime", rec.DepartureTime);
                cmd.Parameters.AddWithValue("@ArrivalTime", rec.ArrivalTime);
                cmd.Parameters.AddWithValue("@Price", rec.Price);
                cmd.Parameters.AddWithValue("@NextRunDate", rec.NextRunDate);
                cmd.Parameters.AddWithValue("@CreatedAt", rec.CreatedAt);

                await cmd.ExecuteNonQueryAsync();

                // Generate schedules
                var schedules = RecurringHelper.GenerateSchedules(rec);
                foreach (var s in schedules)
                {
                    s.RecurringScheduleId = rec.Id;

                    var scheduleCmd = new SqlCommand(@"
                        INSERT INTO Schedules
                        (Id, RouteId, BusId, RecurringScheduleId, DepartureTime, ArrivalTime, Price, Completed, CreatedAt)
                        VALUES (@Id,@RouteId,@BusId,@RecurringScheduleId,@DepartureTime,@ArrivalTime,@Price,@Completed,@CreatedAt)",
                        conn, trans);

                    scheduleCmd.Parameters.AddWithValue("@Id", s.Id);
                    scheduleCmd.Parameters.AddWithValue("@RouteId", s.RouteId);
                    scheduleCmd.Parameters.AddWithValue("@BusId", s.BusId);
                    scheduleCmd.Parameters.AddWithValue("@RecurringScheduleId", s.RecurringScheduleId);
                    scheduleCmd.Parameters.AddWithValue("@DepartureTime", s.DepartureTime);
                    scheduleCmd.Parameters.AddWithValue("@ArrivalTime", s.ArrivalTime);
                    scheduleCmd.Parameters.AddWithValue("@Price", s.Price);
                    scheduleCmd.Parameters.AddWithValue("@Completed", s.Completed);
                    scheduleCmd.Parameters.AddWithValue("@CreatedAt", s.CreatedAt);

                    await scheduleCmd.ExecuteNonQueryAsync();
                }

                trans.Commit();
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
                // 1) Update template
                var cmd = new SqlCommand(@"
                    UPDATE RecurringSchedules SET
                        RouteId=@RouteId,
                        BusId=@BusId,
                        StartDate=@StartDate,
                        EndDate=@EndDate,
                        Frequency=@Frequency,
                        DepartureTime=@DepartureTime,
                        ArrivalTime=@ArrivalTime,
                        Price=@Price,
                        NextRunDate=@NextRunDate
                    WHERE Id=@Id", conn, trans);

                cmd.Parameters.AddWithValue("@Id", rec.Id);
                cmd.Parameters.AddWithValue("@RouteId", rec.RouteId);
                cmd.Parameters.AddWithValue("@BusId", rec.BusId);
                cmd.Parameters.AddWithValue("@StartDate", rec.StartDate);
                cmd.Parameters.AddWithValue("@EndDate", rec.EndDate);
                cmd.Parameters.AddWithValue("@Frequency", rec.Frequency);
                cmd.Parameters.AddWithValue("@DepartureTime", rec.DepartureTime);
                cmd.Parameters.AddWithValue("@ArrivalTime", rec.ArrivalTime);
                cmd.Parameters.AddWithValue("@Price", rec.Price);
                cmd.Parameters.AddWithValue("@NextRunDate", rec.NextRunDate);

                await cmd.ExecuteNonQueryAsync();

                // 2) Delete all future schedules linked to this recurring schedule
                var delCmd = new SqlCommand(@"
                    DELETE FROM Schedules
                    WHERE RecurringScheduleId=@Id AND DepartureTime >= @Now", conn, trans);
                delCmd.Parameters.AddWithValue("@Id", rec.Id);
                delCmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);
                await delCmd.ExecuteNonQueryAsync();

                // 3) Generate new schedules
                var schedules = RecurringHelper.GenerateSchedules(rec);
                foreach (var s in schedules)
                {
                    s.RecurringScheduleId = rec.Id;

                    var addCmd = new SqlCommand(@"
                        INSERT INTO Schedules
                        (Id, RouteId, BusId, RecurringScheduleId, DepartureTime, ArrivalTime, Price, Completed, CreatedAt)
                        VALUES (@Id,@RouteId,@BusId,@RecurringScheduleId,@DepartureTime,@ArrivalTime,@Price,@Completed,@CreatedAt)",
                        conn, trans);

                    addCmd.Parameters.AddWithValue("@Id", s.Id);
                    addCmd.Parameters.AddWithValue("@RouteId", s.RouteId);
                    addCmd.Parameters.AddWithValue("@BusId", s.BusId);
                    addCmd.Parameters.AddWithValue("@RecurringScheduleId", s.RecurringScheduleId);
                    addCmd.Parameters.AddWithValue("@DepartureTime", s.DepartureTime);
                    addCmd.Parameters.AddWithValue("@ArrivalTime", s.ArrivalTime);
                    addCmd.Parameters.AddWithValue("@Price", s.Price);
                    addCmd.Parameters.AddWithValue("@Completed", s.Completed);
                    addCmd.Parameters.AddWithValue("@CreatedAt", s.CreatedAt);

                    await addCmd.ExecuteNonQueryAsync();
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
        public async Task<bool> DeleteRecurringScheduleAsync(Guid id)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();

            try
            {
                // Delete future schedules linked to this recurring schedule
                var delCmd = new SqlCommand(@"
                    DELETE FROM Schedules
                    WHERE RecurringScheduleId=@Id AND DepartureTime >= @Now", conn, trans);
                delCmd.Parameters.AddWithValue("@Id", id);
                delCmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);
                await delCmd.ExecuteNonQueryAsync();

                // Delete recurring schedule
                var cmd = new SqlCommand("DELETE FROM RecurringSchedules WHERE Id=@Id", conn, trans);
                cmd.Parameters.AddWithValue("@Id", id);
                int rows = await cmd.ExecuteNonQueryAsync();

                trans.Commit();
                return rows > 0;
            }
            catch
            {
                trans.Rollback();
                return false;
            }
        }

        // ---------------- GET ALL SCHEDULES ----------------
        public async Task<List<ScheduleDTO>> GetAllSchedulesAsync()
        {
            var list = new List<ScheduleDTO>();
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand("SELECT * FROM Schedules ORDER BY DepartureTime", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new ScheduleDTO
                {
                    Id = reader.GetGuid(0),
                    RouteId = reader.GetGuid(1),
                    BusId = reader.GetGuid(2),
                    RecurringScheduleId = reader.IsDBNull(3) ? null : reader.GetGuid(3),
                    DepartureTime = reader.GetDateTime(4),
                    ArrivalTime = reader.GetDateTime(5),
                    Price = reader.GetDecimal(6),
                    Completed = reader.GetBoolean(7),
                    CreatedAt = reader.GetDateTime(8)
                });
            }
            return list;
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

            return await cmd.ExecuteNonQueryAsync(); // returns number of schedules updated
        }

        // ---------------- GET SCHEDULE BY ID ----------------
        public async Task<ScheduleDTO?> GetScheduleByIdAsync(Guid id)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand("SELECT * FROM Schedules WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new ScheduleDTO
                {
                    Id = reader.GetGuid(0),
                    RouteId = reader.GetGuid(1),
                    BusId = reader.GetGuid(2),
                    RecurringScheduleId = reader.IsDBNull(3) ? null : reader.GetGuid(3),
                    DepartureTime = reader.GetDateTime(4),
                    ArrivalTime = reader.GetDateTime(5),
                    Price = reader.GetDecimal(6),
                    Completed = reader.GetBoolean(7),
                    CreatedAt = reader.GetDateTime(8)
                };
            }

            return null; // not found
        }

        // ---------------- GET RECURRING SCHEDULE BY ID ----------------
        public async Task<RecurringScheduleDTO?> GetRecurringScheduleByIdAsync(Guid id)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand("SELECT * FROM RecurringSchedules WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new RecurringScheduleDTO
                {
                    Id = reader.GetGuid(0),
                    RouteId = reader.GetGuid(1),
                    BusId = reader.GetGuid(2),
                    StartDate = reader.GetDateTime(3),
                    EndDate = reader.GetDateTime(4),
                    Frequency = reader.GetString(5),
                    DepartureTime = reader.GetTimeSpan(6),
                    ArrivalTime = reader.GetTimeSpan(7),
                    Price = reader.GetDecimal(8),
                    NextRunDate = reader.GetDateTime(9),
                    CreatedAt = reader.GetDateTime(10)
                };
            }

            return null; // not found
        }


    }
}
