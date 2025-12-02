
using TMS.DTO;
using Microsoft.Data.SqlClient;

namespace TMS.DAL
{
    public class SeatDAL
    {
        private readonly DBConnection _db;

        public SeatDAL()
        {
            _db = new DBConnection();
        }
        public async Task<List<SeatModel>> GetSeatsByBusAsync(int busId)
        {
            var seats = new List<SeatModel>();

            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                string query = @"
          SELECT s.*, bs.Gender
FROM Seats s
LEFT JOIN BookingSeats bs
    ON s.Id = bs.SeatId
LEFT JOIN Bookings b
    ON bs.BookingId = b.Id AND b.BookingStatus IN ('Pending','Confirmed')
WHERE s.BusId = @BusId
ORDER BY s.SeatNumber

        ";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BusId", busId);
                    await conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            seats.Add(new SeatModel
                            {
                                Id = (int)reader["Id"],
                                BusId = (int)reader["BusId"],
                                SeatNumber = (int)reader["SeatNumber"],
                                IsSide = (bool)reader["IsSide"],
                                BunkType = reader["BunkType"] as string,
                                Status = (string)reader["Status"],
                                CreatedAt = (DateTime)reader["CreatedAt"],
                                Gender = reader["Gender"] as string ?? "" // only for booked seats
                            });
                        }
                    }
                }
            }

            return seats;
        }



        public async Task UpdateSeatStatusAsync(int seatId, string status)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                string query = "UPDATE Seats SET Status = @Status WHERE Id = @SeatId";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@SeatId", seatId);
                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}