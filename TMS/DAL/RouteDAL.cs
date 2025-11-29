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
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(
                @"INSERT INTO Routes (OriginId, DestinationId, DistanceKm, EstimatedTimeMinutes)
                  OUTPUT INSERTED.Id
                  VALUES (@OriginId, @DestinationId, @DistanceKm, @EstimatedTimeMinutes)", conn);

            cmd.Parameters.AddWithValue("@OriginId", route.OriginId);
            cmd.Parameters.AddWithValue("@DestinationId", route.DestinationId);
            cmd.Parameters.AddWithValue("@DistanceKm", route.DistanceKm);
            cmd.Parameters.AddWithValue("@EstimatedTimeMinutes", route.EstimatedTimeMinutes);

            route.Id = (int)await cmd.ExecuteScalarAsync();
        }

        // ---------------- UPDATE ROUTE ----------------
        public async Task<bool> UpdateRouteAsync(RouteDTO route)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(
                @"UPDATE Routes
                  SET OriginId = @OriginId,
                      DestinationId = @DestinationId,
                      DistanceKm = @DistanceKm,
                      EstimatedTimeMinutes = @EstimatedTimeMinutes
                  WHERE Id = @Id", conn);

            cmd.Parameters.AddWithValue("@Id", route.Id);
            cmd.Parameters.AddWithValue("@OriginId", route.OriginId);
            cmd.Parameters.AddWithValue("@DestinationId", route.DestinationId);
            cmd.Parameters.AddWithValue("@DistanceKm", route.DistanceKm);
            cmd.Parameters.AddWithValue("@EstimatedTimeMinutes", route.EstimatedTimeMinutes);

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        // ---------------- DELETE ROUTE ----------------
        public async Task<bool> DeleteRouteAsync(int routeId)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand("DELETE FROM Routes WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", routeId);

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        // ---------------- GET SINGLE ROUTE ----------------
        public async Task<RouteDTO> GetRouteByIdAsync(int routeId)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(
                @"SELECT r.Id, r.OriginId, r.DestinationId, o.Name AS OriginName, d.Name AS DestinationName,
                         r.DistanceKm, r.EstimatedTimeMinutes, r.CreatedAt
                  FROM Routes r
                  INNER JOIN Locations o ON r.OriginId = o.Id
                  INNER JOIN Locations d ON r.DestinationId = d.Id
                  WHERE r.Id = @Id", conn);

            cmd.Parameters.AddWithValue("@Id", routeId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new RouteDTO
                {
                    Id = reader.GetInt32(0),
                    OriginId = reader.GetInt32(1),
                    DestinationId = reader.GetInt32(2),
                    OriginName = reader.GetString(3),
                    DestinationName = reader.GetString(4),
                    DistanceKm = reader.GetInt32(5),
                    EstimatedTimeMinutes = reader.GetInt32(6),
                    CreatedAt = reader.GetDateTime(7)
                };
            }

            return null;
        }

        // ---------------- GET ALL ROUTES ----------------
        public async Task<List<RouteDTO>> GetAllRoutesAsync()
        {
            var routes = new List<RouteDTO>();

            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(
                @"SELECT r.Id, r.OriginId, r.DestinationId, o.Name AS OriginName, d.Name AS DestinationName,
                         r.DistanceKm, r.EstimatedTimeMinutes, r.CreatedAt
                  FROM Routes r
                  INNER JOIN Locations o ON r.OriginId = o.Id
                  INNER JOIN Locations d ON r.DestinationId = d.Id
                  ORDER BY r.CreatedAt DESC", conn);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                routes.Add(new RouteDTO
                {
                    Id = reader.GetInt32(0),
                    OriginId = reader.GetInt32(1),
                    DestinationId = reader.GetInt32(2),
                    OriginName = reader.GetString(3),
                    DestinationName = reader.GetString(4),
                    DistanceKm = reader.GetInt32(5),
                    EstimatedTimeMinutes = reader.GetInt32(6),
                    CreatedAt = reader.GetDateTime(7)
                });
            }

            return routes;
        }
    }
}
