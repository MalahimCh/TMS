using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMS.DTO;

namespace TMS.DAL
{
    public class RouteDAL
    {
        private readonly DBConnection _db;

        public RouteDAL()
        {
            _db = new DBConnection();
        }

        // ---------------- ADD ROUTE ----------------
        public async Task AddRouteAsync(RouteDTO route)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();

                // Use OUTPUT INSERTED.Id to get new identity
                var cmd = new SqlCommand(
                    @"INSERT INTO Routes (Origin, Destination, DistanceKm, EstimatedTimeMinutes)
                      OUTPUT INSERTED.Id
                      VALUES (@Origin, @Destination, @DistanceKm, @EstimatedTimeMinutes)",
                    conn);

                cmd.Parameters.AddWithValue("@Origin", route.Origin);
                cmd.Parameters.AddWithValue("@Destination", route.Destination);
                cmd.Parameters.AddWithValue("@DistanceKm", route.DistanceKm);
                cmd.Parameters.AddWithValue("@EstimatedTimeMinutes", route.EstimatedTimeMinutes);

                // Capture newly generated ID
                int newRouteId = (int)await cmd.ExecuteScalarAsync();
                route.Id = newRouteId;
            }
        }

        // ---------------- UPDATE ROUTE ----------------
        public async Task<bool> UpdateRouteAsync(RouteDTO route)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand(
                    @"UPDATE Routes
                      SET DistanceKm = @DistanceKm,
                          EstimatedTimeMinutes = @EstimatedTimeMinutes
                      WHERE Id = @Id",
                    conn);

                cmd.Parameters.AddWithValue("@Id", route.Id);
                cmd.Parameters.AddWithValue("@DistanceKm", route.DistanceKm);
                cmd.Parameters.AddWithValue("@EstimatedTimeMinutes", route.EstimatedTimeMinutes);

                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        // ---------------- DELETE ROUTE ----------------
        public async Task<bool> DeleteRouteAsync(int routeId)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand(
                    "DELETE FROM Routes WHERE Id=@Id",
                    conn);

                cmd.Parameters.AddWithValue("@Id", routeId);

                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        // ---------------- GET SINGLE ROUTE ----------------
        public async Task<RouteDTO> GetRouteByIdAsync(int routeId)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand("SELECT * FROM Routes WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", routeId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new RouteDTO
                        {
                            Id = reader.GetInt32(0),
                            Origin = reader.GetString(1),
                            Destination = reader.GetString(2),
                            DistanceKm = reader.GetInt32(3),
                            EstimatedTimeMinutes = reader.GetInt32(4),
                            CreatedAt = reader.GetDateTime(5)
                        };
                    }
                }
            }
            return null;
        }

        // ---------------- GET ALL ROUTES ----------------
        public async Task<List<RouteDTO>> GetAllRoutesAsync()
        {
            var routes = new List<RouteDTO>();

            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand("SELECT * FROM Routes ORDER BY CreatedAt DESC", conn);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        routes.Add(new RouteDTO
                        {
                            Id = reader.GetInt32(0),
                            Origin = reader.GetString(1),
                            Destination = reader.GetString(2),
                            DistanceKm = reader.GetInt32(3),
                            EstimatedTimeMinutes = reader.GetInt32(4),
                            CreatedAt = reader.GetDateTime(5)
                        });
                    }
                }
            }

            return routes;
        }
    }
}
