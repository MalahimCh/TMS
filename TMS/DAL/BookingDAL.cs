using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using TMS.BLL;
using TMS.Design_Patterns;
using TMS.DTO;

namespace TMS.DAL
{
    public class BookingDAL
    {
        private readonly DBConnection _db;

        public BookingDAL()
        {
            _db = new DBConnection();
        }


        // ----------------------------------------------------------
        // CHECK IF SEAT IS AVAILABLE
        // ----------------------------------------------------------
        public async Task<bool> IsSeatAvailableAsync(int scheduleId, int seatId)
        {
            string query = @"
                SELECT COUNT(*)
                FROM BookingSeats bs
                INNER JOIN Bookings b ON bs.BookingId = b.Id
                WHERE b.ScheduleId = @ScheduleId
                AND bs.SeatId = @SeatId
                AND b.BookingStatus IN ('Pending','Confirmed')
            ";

            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@ScheduleId", scheduleId);
            cmd.Parameters.AddWithValue("@SeatId", seatId);

            int count = (int)await cmd.ExecuteScalarAsync();
            return count == 0;
        }

        //Get bookings by user ID
        public async Task<List<BookingDTO>> GetBookingsByUserIdAsync(int userID)
        {
            string query = @"
        SELECT 
            b.Id, b.UserId, b.BookingReference, b.BookingDate, b.ScheduleId,
            b.TotalAmount, b.DiscountAmount, b.FinalAmount, b.BookingStatus,
            b.PaymentStatus, b.PromotionCode, b.TransactionId, b.PaymentMethod,
            s.DepartureTime,
            bus.BusNumber,
            CONCAT(locOrigin.Name, N' → ', locDest.Name) AS RouteDisplay
        FROM Bookings b
        JOIN Schedules s      ON b.ScheduleId = s.Id
        JOIN Buses bus        ON s.BusId = bus.Id
        JOIN Routes r         ON s.RouteId = r.Id
        JOIN Locations locOrigin ON r.OriginId = locOrigin.Id
        JOIN Locations locDest   ON r.DestinationId = locDest.Id
        WHERE b.UserId = @UserId
        ORDER BY b.BookingDate DESC";

            var bookings = new List<BookingDTO>();

            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userID;

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                // Safely read nullable columns
                string? promo = reader.IsDBNull(reader.GetOrdinal("PromotionCode")) ? null : reader.GetString(reader.GetOrdinal("PromotionCode"));
                string? txn = reader.IsDBNull(reader.GetOrdinal("TransactionId")) ? null : reader.GetString(reader.GetOrdinal("TransactionId"));
                string? payM = reader.IsDBNull(reader.GetOrdinal("PaymentMethod")) ? null : reader.GetString(reader.GetOrdinal("PaymentMethod"));

                var departureTime = reader.GetDateTime(reader.GetOrdinal("DepartureTime"));
                var busNumber = reader.GetString(reader.GetOrdinal("BusNumber"));
                var routeDisplay = reader.GetString(reader.GetOrdinal("RouteDisplay"));

                bookings.Add(new BookingDTO
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    ScheduleId = reader.GetInt32(reader.GetOrdinal("ScheduleId")),
                    BookingDate = reader.GetDateTime(reader.GetOrdinal("BookingDate")),
                    TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                    DiscountAmount = reader.GetDecimal(reader.GetOrdinal("DiscountAmount")),
                    PromotionCode = promo,
                    FinalAmount = reader.GetDecimal(reader.GetOrdinal("FinalAmount")),
                    BookingStatus = reader.GetString(reader.GetOrdinal("BookingStatus")),
                    PaymentStatus = reader.GetString(reader.GetOrdinal("PaymentStatus")),
                    TransactionId = txn,
                    PaymentMethod = payM,
                    BookingReference = reader.GetString(reader.GetOrdinal("BookingReference")),

                    ScheduleDisplay = $"{departureTime:yyyy-MM-dd HH:mm} | Bus: {busNumber} | Route: {routeDisplay}"
                });
            }

            return bookings;
        }


        // ----------------------------------------------------------
        // INSERT BOOKING (returns new Id)
        // ----------------------------------------------------------

        public async Task<int> InsertBookingAsync(BookingDTO booking)
        {
            string query = @"
                INSERT INTO Bookings
                (UserId, ScheduleId, TotalAmount, DiscountAmount,
                 PromotionCode, BookingStatus, PaymentStatus,
                 TransactionId, PaymentMethod, BookingReference)
                OUTPUT INSERTED.Id
                VALUES
                (@UserId, @ScheduleId, @TotalAmount, @DiscountAmount,
                 @Promo, @BookStatus, @PayStatus,
                 @Txn, @Method, @Ref)
            ";

            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@UserId", booking.UserId);
            cmd.Parameters.AddWithValue("@ScheduleId", booking.ScheduleId);
            cmd.Parameters.AddWithValue("@TotalAmount", booking.TotalAmount);
            cmd.Parameters.AddWithValue("@DiscountAmount", booking.DiscountAmount);
            cmd.Parameters.AddWithValue("@Promo", (object?)booking.PromotionCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BookStatus", booking.BookingStatus);
            cmd.Parameters.AddWithValue("@PayStatus", booking.PaymentStatus);
            cmd.Parameters.AddWithValue("@Txn", (object?)booking.TransactionId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Method", (object?)booking.PaymentMethod ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Ref", booking.BookingReference);

            return (int)await cmd.ExecuteScalarAsync();


        }


        // ----------------------------------------------------------
        // INSERT BOOKING SEATS AND MARK THEM TEMPORARILY BOOKED
        // ----------------------------------------------------------
        public async Task InsertBookingSeatsAsync(int bookingId, List<BookingSeatDTO> seats)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            using var tran = conn.BeginTransaction(); // ensure atomicity

            try
            {
                // Insert booking seats
                string insertQuery = @"
            INSERT INTO BookingSeats (BookingId, SeatId, SeatPrice,Gender)
            VALUES (@BookingId, @SeatId, @Price,@Gender)
        ";

                using var cmd = new SqlCommand(insertQuery, conn, tran);
                foreach (var seat in seats)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@BookingId", bookingId);
                    cmd.Parameters.AddWithValue("@SeatId", seat.SeatId);
                    cmd.Parameters.AddWithValue("@Price", seat.SeatPrice);
                    cmd.Parameters.AddWithValue("@Gender", seat.Gender);

                    await cmd.ExecuteNonQueryAsync();
                }

                // Update seat statuses to TemporarilyBooked
                string updateSeatsQuery = @"
            UPDATE Seats
            SET Status = 'TemporarilyBooked'
            WHERE Id IN (" + string.Join(",", seats.Select(s => s.SeatId)) + @")
        ";

                using var cmdUpdate = new SqlCommand(updateSeatsQuery, conn, tran);
                await cmdUpdate.ExecuteNonQueryAsync();

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        // ----------------------------------------------------------
        // UPDATE PAYMENT STATUS AND MARK SEATS AS BOOKED IF PAID
        // ----------------------------------------------------------
        public async Task UpdatePaymentAsync(int bookingId, string status, string txnId, string method)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            using var tran = conn.BeginTransaction(); // atomic operation

            try
            {
                string bookingStatus;
                string seatStatus;

                // Determine new booking and seat status
                if (status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
                {
                    bookingStatus = "Confirmed";
                    seatStatus = "Booked";
                }
                else if (status.Equals("Failed", StringComparison.OrdinalIgnoreCase))
                {
                    bookingStatus = "Expired";
                    seatStatus = "Available";
                }
                else if (status.Equals("Refunded", StringComparison.OrdinalIgnoreCase))
                {
                    bookingStatus = "Cancelled";
                    seatStatus = "Available";
                }
                else
                {
                    // Default fallback
                    bookingStatus = "Pending";
                    seatStatus = "TemporarilyBooked";
                }

                // 1. Update booking info
                string updateBookingQuery = @"
            UPDATE Bookings
            SET BookingStatus = @BookingStatus,
                PaymentStatus = @PaymentStatus,
                TransactionId = @Txn,
                PaymentMethod = @Method
            WHERE Id = @Id
        ";

                using var cmd = new SqlCommand(updateBookingQuery, conn, tran);
                cmd.Parameters.AddWithValue("@Id", bookingId);
                cmd.Parameters.AddWithValue("@BookingStatus", bookingStatus);
                cmd.Parameters.AddWithValue("@PaymentStatus", status);
                cmd.Parameters.AddWithValue("@Txn", (object?)txnId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Method", (object?)method ?? DBNull.Value);
                await cmd.ExecuteNonQueryAsync();

                // 2. Update seat status
                string updateSeatsQuery = @"
            UPDATE s
            SET s.Status = @SeatStatus
            FROM Seats s
            INNER JOIN BookingSeats bs ON s.Id = bs.SeatId
            WHERE bs.BookingId = @BookingId
        ";

                using var cmdSeats = new SqlCommand(updateSeatsQuery, conn, tran);
                cmdSeats.Parameters.AddWithValue("@BookingId", bookingId);
                cmdSeats.Parameters.AddWithValue("@SeatStatus", seatStatus);
                await cmdSeats.ExecuteNonQueryAsync();

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }


        // ----------------------------------------------------------
        // GET BOOKING BY REFERENCE
        // ----------------------------------------------------------
        public async Task<BookingDTO?> GetBookingByReferenceAsync(string reference)
        {
            string query = @"
                SELECT *
                FROM Bookings
                WHERE BookingReference = @Ref
            ";

            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Ref", reference);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return new BookingDTO
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                ScheduleId = reader.GetInt32(reader.GetOrdinal("ScheduleId")),
                BookingDate = reader.GetDateTime(reader.GetOrdinal("BookingDate")),
                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                DiscountAmount = reader.GetDecimal(reader.GetOrdinal("DiscountAmount")),
                PromotionCode = reader["PromotionCode"] as string,
                FinalAmount = reader.GetDecimal(reader.GetOrdinal("FinalAmount")),
                BookingStatus = reader.GetString(reader.GetOrdinal("BookingStatus")),
                PaymentStatus = reader.GetString(reader.GetOrdinal("PaymentStatus")),
                TransactionId = reader["TransactionId"] as string,
                PaymentMethod = reader["PaymentMethod"] as string,
                BookingReference = reader.GetString(reader.GetOrdinal("BookingReference"))
            };
        }
    }
}
