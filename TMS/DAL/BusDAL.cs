using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
                        // Insert bus (Id identity generated automatically)
                        var cmd = new SqlCommand(@"
                            INSERT INTO Buses (BusNumber, BusType, TotalSeats)
                            OUTPUT INSERTED.Id
                            VALUES (@BusNumber, @BusType, @TotalSeats);
                        ", conn, trans);

                        cmd.Parameters.AddWithValue("@BusNumber", bus.BusNumber);
                        cmd.Parameters.AddWithValue("@BusType", bus.BusType);
                        cmd.Parameters.AddWithValue("@TotalSeats", bus.TotalSeats);

                        int newBusId = (int)await cmd.ExecuteScalarAsync();
                        bus.Id = newBusId;

                        // Insert seats
                        for (int i = 1; i <= bus.TotalSeats; i++)
                        {
                            var seatCmd = new SqlCommand(@"
                                INSERT INTO Seats (BusId, SeatNumber, Status)
                                VALUES (@BusId, @SeatNumber, 'Available');
                            ", conn, trans);

                            seatCmd.Parameters.AddWithValue("@BusId", newBusId);
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
                        var cmd = new SqlCommand(@"
                            UPDATE Buses SET 
                                BusNumber = @BusNumber,
                                BusType   = @BusType,
                                TotalSeats= @TotalSeats
                            WHERE Id = @Id;
                        ", conn, trans);

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

                        // Count existing seats
                        var countCmd = new SqlCommand(
                            "SELECT COUNT(*) FROM Seats WHERE BusId = @BusId",
                            conn, trans);

                        countCmd.Parameters.AddWithValue("@BusId", bus.Id);
                        int existing = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

                        // Add seats
                        if (bus.TotalSeats > existing)
                        {
                            for (int i = existing + 1; i <= bus.TotalSeats; i++)
                            {
                                var addCmd = new SqlCommand(@"
                                    INSERT INTO Seats (BusId, SeatNumber, Status)
                                    VALUES (@BusId, @SeatNumber, 'Available');
                                ", conn, trans);

                                addCmd.Parameters.AddWithValue("@BusId", bus.Id);
                                addCmd.Parameters.AddWithValue("@SeatNumber", i.ToString());
                                await addCmd.ExecuteNonQueryAsync();
                            }
                        }

                        // Remove seats
                        if (bus.TotalSeats < existing)
                        {
                            int removeCount = existing - bus.TotalSeats;

                            string selectQuery = $@"
                                SELECT TOP({removeCount}) Id
                                FROM Seats
                                WHERE BusId = @BusId AND Status = 'Available' 
                                      AND TRY_CAST(SeatNumber AS INT) IS NOT NULL
                                ORDER BY TRY_CAST(SeatNumber AS INT) DESC;
                            ";

                            var selectCmd = new SqlCommand(selectQuery, conn, trans);
                            selectCmd.Parameters.AddWithValue("@BusId", bus.Id);

                            var idsToDelete = new List<int>();
                            using (var reader = await selectCmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                    idsToDelete.Add(reader.GetInt32(0));
                            }

                            if (idsToDelete.Count < removeCount)
                            {
                                trans.Rollback();
                                return false;
                            }

                            var paramNames = new List<string>();
                            for (int i = 0; i < idsToDelete.Count; i++)
                                paramNames.Add($"@id{i}");

                            string deleteQuery =
                                $"DELETE FROM Seats WHERE Id IN ({string.Join(",", paramNames)})";

                            var deleteCmd = new SqlCommand(deleteQuery, conn, trans);

                            for (int i = 0; i < idsToDelete.Count; i++)
                                deleteCmd.Parameters.AddWithValue(paramNames[i], idsToDelete[i]);

                            await deleteCmd.ExecuteNonQueryAsync();
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
            }
        }

        // ---------------- DELETE BUS ----------------
        public async Task<bool> DeleteBusAsync(int busId)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        var cmdSeats = new SqlCommand(
                            "DELETE FROM Seats WHERE BusId=@Id",
                            conn, trans);

                        cmdSeats.Parameters.AddWithValue("@Id", busId);
                        await cmdSeats.ExecuteNonQueryAsync();

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
        public async Task<BusDTO> GetBusByIdAsync(int busId)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand(
                    "SELECT * FROM Buses WHERE Id=@Id",
                    conn);

                cmd.Parameters.AddWithValue("@Id", busId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new BusDTO
                        {
                            Id = reader.GetInt32(0),
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

                var cmd = new SqlCommand(
                    "SELECT * FROM Buses ORDER BY CreatedAt DESC",
                    conn);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        buses.Add(new BusDTO
                        {
                            Id = reader.GetInt32(0),
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
