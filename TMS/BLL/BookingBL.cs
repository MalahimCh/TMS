using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMS.DAL;
using TMS.DTO;

namespace TMS.BLL
{
    public class BookingBL
    {
        private readonly BookingDAL _dal;

        public BookingBL()
        {
            _dal = new BookingDAL();
        }

        // -------------------------------------------------------------
        // CREATE BOOKING (Async)
        // -------------------------------------------------------------
        public async Task<BookingDTO> CreateBookingAsync(BookingDTO booking)
        {
            if (booking == null)
                throw new ArgumentNullException(nameof(booking));

            if (booking.Seats == null || booking.Seats.Count == 0)
                throw new Exception("No seats selected.");

            if (booking.TotalAmount <= 0)
                throw new Exception("Total amount must be greater than zero.");

            if (booking.DiscountAmount < 0)
                throw new Exception("Invalid discount amount.");

            if (booking.DiscountAmount > booking.TotalAmount)
                throw new Exception("Discount cannot exceed total amount.");

            
            booking.BookingStatus = "Pending";
            booking.PaymentStatus = "Pending";

            foreach (var seat in booking.Seats)
            {
                bool available = await _dal.IsSeatAvailableAsync(booking.ScheduleId, seat.SeatId);

                if (!available)
                    throw new Exception($"Seat {seat.SeatId} is already booked.");
            }

            int bookingId = await _dal.InsertBookingAsync(booking);

            await _dal.InsertBookingSeatsAsync(bookingId, booking.Seats);

            booking.Id = bookingId;
            return booking;
        }


        //get bookings by userID
        public async Task<List<BookingDTO>> GetBookingsByUserIdAsync(int userId)
        {
            if (userId <= 0)
                throw new Exception("Invalid user ID.");

            return await _dal.GetBookingsByUserIdAsync(userId);
        }

        // -------------------------------------------------------------
        // GET BOOKING BY REFERENCE (Async)
        // -------------------------------------------------------------
        public async Task<BookingDTO?> GetBookingByReferenceAsync(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
                throw new Exception("Invalid booking reference.");

            return await _dal.GetBookingByReferenceAsync(reference);
        }


        // -------------------------------------------------------------
        // UPDATE PAYMENT STATUS (Async)
        // -------------------------------------------------------------
        public async Task UpdatePaymentAsync(int bookingId, string paymentStatus, string? txnId, string? method)
        {
            if (bookingId <= 0)
                throw new Exception("Invalid booking ID.");

            if (string.IsNullOrWhiteSpace(paymentStatus))
                throw new Exception("Payment status is required.");

            if (paymentStatus == "Paid")
            {
                if (string.IsNullOrWhiteSpace(txnId))
                    throw new Exception("Transaction ID must be provided when marking payment as Paid.");

                if (string.IsNullOrWhiteSpace(method))
                    throw new Exception("Payment method is required when marking payment as Paid.");
            }

            await _dal.UpdatePaymentAsync(bookingId, paymentStatus, txnId, method);
        }


        // -------------------------------------------------------------
        // CHECK SEAT AVAILABILITY (Async)
        // -------------------------------------------------------------
        public async Task<bool> IsSeatAvailableAsync(int scheduleId, int seatId)
        {
            return await _dal.IsSeatAvailableAsync(scheduleId, seatId);
        }
    }
}
