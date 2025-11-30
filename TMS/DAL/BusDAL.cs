using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMS.DTO;
using TMS.Design_Patterns;

namespace TMS.DAL
{
    public class BusDAL
    {
        private readonly DBConnection _db;

        public BusDAL()
        {
            _db = new DBConnection();
        }

        //------------- GET ALL BUS TYPES ----------------
        public async Task<List<string>> GetAllBusTypesAsync()
        {
            var busTypes = new List<string>();

            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT DISTINCT BusType FROM Buses ORDER BY BusType", conn);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        busTypes.Add(reader.GetString(0));
                    }
                }
            }

            return busTypes;
        }
        // ---------------- ADD BUS ----------------
        public async Task AddBusAsync(BusDTO bus)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();
            try
            {
                // Insert bus
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

                // Generate seats using factory
                var generator = SeatGeneratorFactory.GetGenerator(bus.BusType);
                var seats = generator.GenerateSeats();

                foreach (var s in seats)
                {
                    var seatCmd = new SqlCommand(@"
                INSERT INTO Seats (BusId, SeatNumber, IsSide, BunkType,Status)
                VALUES (@BusId, @SeatNumber, @IsSide, @BunkType,@Status);
            ", conn, trans);
                    seatCmd.Parameters.AddWithValue("@BusId", newBusId);
                    seatCmd.Parameters.AddWithValue("@SeatNumber", s.SeatNumber);
                    seatCmd.Parameters.AddWithValue("@IsSide", s.IsSide);
                    seatCmd.Parameters.AddWithValue("@BunkType", (object?)s.BunkType ?? DBNull.Value);
                    seatCmd.Parameters.AddWithValue("@Status", s.Status);
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