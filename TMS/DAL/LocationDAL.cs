using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMS.DTO;

namespace TMS.DAL
{
    public class LocationDAL
    {
        private readonly DBConnection _db;

        public LocationDAL()
        {
            _db = new DBConnection();
        }

        // Get all locations
        public async Task<List<LocationDTO>> GetAllLocationsAsync()
        {
            var locations = new List<LocationDTO>();

            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT Id, Name FROM Locations ORDER BY Name", conn);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        locations.Add(new LocationDTO
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }

            return locations;
        }

    }
}
