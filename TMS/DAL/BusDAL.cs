using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMS.BLL;
using TMS.DTO;

namespace TMS.DAL
{
    public class BusDAL
    {
        private readonly DBConnection _db;

        public BusDAL()
        {
            _db = new DBConnection();
        }

        // ---------------- ADD BUS ----------------
        public async Task AddBusAsync(BusDTO bus)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Insert Bus
                        var cmd = new SqlCommand(
                            @"INSERT INTO Buses (Id, BusNumber, BusType, TotalSeats, CreatedAt)
                      VALUES (@Id, @BusNumber, @BusType, @TotalSeats, @CreatedAt)",
                            conn, trans);

                        cmd.Parameters.AddWithValue("@Id", bus.Id);
                        cmd.Parameters.AddWithValue("@BusNumber", bus.BusNumber);
                        cmd.Parameters.AddWithValue("@BusType", bus.BusType);
                        cmd.Parameters.AddWithValue("@TotalSeats", bus.TotalSeats);
                        cmd.Parameters.AddWithValue("@CreatedAt", bus.CreatedAt);

                        await cmd.ExecuteNonQueryAsync();

                        // Insert Seats
                        for (int i = 1; i <= bus.TotalSeats; i++)
                        {
                            var seatCmd = new SqlCommand(
                                @"INSERT INTO Seats (BusId, SeatNumber) 
                          VALUES (@BusId, @SeatNumber)",
                                conn, trans);

                            seatCmd.Parameters.AddWithValue("@BusId", bus.Id);
                            seatCmd.Parameters.AddWithValue("@SeatNumber", i.ToString());
                            await seatCmd.ExecuteNonQueryAsync();
                        }

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        // ---------------- UPDATE BUS ----------------
        public async Task<bool> UpdateBusAsync(BusDTO bus)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 1) Update bus row
                        var cmd = new SqlCommand(
                            @"UPDATE Buses SET 
                         BusNumber = @BusNumber,
                         BusType   = @BusType,
                         TotalSeats= @TotalSeats
                      WHERE Id = @Id",
                            conn, trans);

                        cmd.Parameters.AddWithValue("@BusNumber", bus.BusNumber);
                        cmd.Parameters.AddWithValue("@BusType", bus.BusType);
                        cmd.Parameters.AddWithValue("@TotalSeats", bus.TotalSeats);
                        cmd.Parameters.AddWithValue("@Id", bus.Id);

                        int rows = await cmd.ExecuteNonQueryAsync();
                        if (rows == 0)
                        {
                            trans.Rollback();
                            return false;
                        }

                        // 2) Count existing seats
                        var countCmd = new SqlCommand(
                            "SELECT COUNT(*) FROM Seats WHERE BusId = @BusId",
                            conn, trans);
                        countCmd.Parameters.AddWithValue("@BusId", bus.Id);
                        int existing = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                        // 3) If need to add seats -> simple inserts
                        if (bus.TotalSeats > existing)
                        {
                            for (int i = existing + 1; i <= bus.TotalSeats; i++)
                            {
                                var addCmd = new SqlCommand(
                                    @"INSERT INTO Seats (Id, BusId, SeatNumber, Status, CreatedAt)
                              VALUES (NEWID(), @BusId, @SeatNumber, 'Available', SYSUTCDATETIME())",
                                    conn, trans);
                                addCmd.Parameters.AddWithValue("@BusId", bus.Id);
                                addCmd.Parameters.AddWithValue("@SeatNumber", i.ToString());
                                await addCmd.ExecuteNonQueryAsync();
                            }
                        }

                        // 4) If need to remove seats -> select removable seat Ids first (only AVAILABLE seats)
                        if (bus.TotalSeats < existing)
                        {
                            int removeCount = existing - bus.TotalSeats;

                            // Select TOP N available seats ordered by numeric SeatNumber descending.
                            // Use TRY_CAST (or TRY_CONVERT) to avoid exceptions for non-numeric seat labels.
                            string selectQuery = $@"
                        SELECT TOP({removeCount}) Id
                        FROM Seats
                        WHERE BusId = @BusId AND Status = 'Available' AND TRY_CAST(SeatNumber AS INT) IS NOT NULL
                        ORDER BY TRY_CAST(SeatNumber AS INT) DESC";

                            var selectCmd = new SqlCommand(selectQuery, conn, trans);
                            selectCmd.Parameters.AddWithValue("@BusId", bus.Id);

                            var idsToDelete = new List<Guid>();
                            using (var reader = await selectCmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    idsToDelete.Add(reader.GetGuid(0));
                                }
                            }

                            // If not enough available seats to remove, abort — cannot delete booked/held seats
                            if (idsToDelete.Count < removeCount)
                            {
                                // Optionally: you could choose to delete as many as possible, or notify admin which seats are blocked.
                                trans.Rollback();
                                return false;
                            }

                            // Build parameterized DELETE ... WHERE Id IN (@id0, @id1, ...)
                            var paramNames = new List<string>();
                            for (int i = 0; i < idsToDelete.Count; i++)
                            {
                                paramNames.Add($"@id{i}");
                            }

                            string deleteQuery = $"DELETE FROM Seats WHERE Id IN ({string.Join(",", paramNames)})";
                            var deleteCmd = new SqlCommand(deleteQuery, conn, trans);
                            for (int i = 0; i < idsToDelete.Count; i++)
                            {
                                deleteCmd.Parameters.AddWithValue(paramNames[i], idsToDelete[i]);
                            }

                            await deleteCmd.ExecuteNonQueryAsync();
                        }

                        trans.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Consider logging ex.Message here
                        trans.Rollback();
                        return false;
                    }
                }
            }
        }

        // ---------------- DELETE BUS ----------------
        public async Task<bool> DeleteBusAsync(Guid busId)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Delete seats
                        var cmdSeats = new SqlCommand(
                            "DELETE FROM Seats WHERE BusId=@Id",
                            conn, trans);
                        cmdSeats.Parameters.AddWithValue("@Id", busId);
                        await cmdSeats.ExecuteNonQueryAsync();

                        // Delete bus
                        var cmdBus = new SqlCommand(
                            "DELETE FROM Buses WHERE Id=@Id",
                            conn, trans);
                        cmdBus.Parameters.AddWithValue("@Id", busId);

                        int rows = await cmdBus.ExecuteNonQueryAsync();

                        trans.Commit();
                        return rows > 0;
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                }
            }
        }


        // ---------------- GET SINGLE BUS ----------------
        public async Task<BusDTO> GetBusByIdAsync(Guid busId)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand("SELECT * FROM Buses WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", busId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new BusDTO
                        {
                            Id = reader.GetGuid(0),
                            BusNumber = reader.GetString(1),
                            BusType = reader.GetString(2),
                            TotalSeats = reader.GetInt32(3),
                            CreatedAt = reader.GetDateTime(4)
                        };
                    }
                }
            }

            return null;
        }

        // ---------------- GET ALL BUSES ----------------
        public async Task<List<BusDTO>> GetAllBusesAsync()
        {
            var buses = new List<BusDTO>();

            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand("SELECT * FROM Buses ORDER BY CreatedAt DESC", conn);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        buses.Add(new BusDTO
                        {
                            Id = reader.GetGuid(0),
                            BusNumber = reader.GetString(1),
                            BusType = reader.GetString(2),
                            TotalSeats = reader.GetInt32(3),
                            CreatedAt = reader.GetDateTime(4)
                        });
                    }
                }
            }

            return buses;
        }
    }
}
