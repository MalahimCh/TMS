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

                var cmd = new SqlCommand(
                    @"INSERT INTO Buses (Id, BusNumber, BusType, TotalSeats, CreatedAt)
                      VALUES (@Id, @BusNumber, @BusType, @TotalSeats, @CreatedAt)", conn);

                cmd.Parameters.AddWithValue("@Id", bus.Id);
                cmd.Parameters.AddWithValue("@BusNumber", bus.BusNumber);
                cmd.Parameters.AddWithValue("@BusType", bus.BusType);
                cmd.Parameters.AddWithValue("@TotalSeats", bus.TotalSeats);
                cmd.Parameters.AddWithValue("@CreatedAt", bus.CreatedAt);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        // ---------------- UPDATE BUS ----------------
        public async Task<bool> UpdateBusAsync(BusDTO bus)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand(
                    @"UPDATE Buses 
                      SET BusNumber=@BusNumber,
                          BusType=@BusType,
                          TotalSeats=@TotalSeats
                      WHERE Id=@Id", conn);

                cmd.Parameters.AddWithValue("@BusNumber", bus.BusNumber);
                cmd.Parameters.AddWithValue("@BusType", bus.BusType);
                cmd.Parameters.AddWithValue("@TotalSeats", bus.TotalSeats);
                cmd.Parameters.AddWithValue("@Id", bus.Id);

                int rowsAffected=await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0; 
            }
        }

        // ---------------- DELETE BUS ----------------
        public async Task<bool> DeleteBusAsync(Guid busId)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand("DELETE FROM Buses WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", busId);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0; // true if a row was deleted
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
