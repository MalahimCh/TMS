using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using TMS.DTO;
using TMS.Design_Patterns;

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
        // INSERT BOOKING SEATS
        // ----------------------------------------------------------
        public async Task InsertBookingSeatsAsync(int bookingId, List<BookingSeatDTO> seats)
        {
            string query = @"
                INSERT INTO BookingSeats (BookingId, SeatId, SeatPrice)
                VALUES (@BookingId, @SeatId, @Price)
            ";

            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(query, conn);

            foreach (var seat in seats)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@BookingId", bookingId);
                cmd.Parameters.AddWithValue("@SeatId", seat.SeatId);
                cmd.Parameters.AddWithValue("@Price", seat.SeatPrice);

                await cmd.ExecuteNonQueryAsync();
            }
        }


        // ----------------------------------------------------------
        // UPDATE PAYMENT STATUS
        // ----------------------------------------------------------
        public async Task UpdatePaymentAsync(int bookingId, string status, string txnId, string method)
        {
            string query = @"
                UPDATE Bookings
                SET PaymentStatus = @Status,
                    TransactionId = @Txn,
                    PaymentMethod = @Method
                WHERE Id = @Id
            ";

            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            using var cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@Id", bookingId);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@Txn", (object?)txnId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Method", (object?)method ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
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
