namespace TMS.DTO
{
    public class BookingDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ScheduleId { get; set; }

        public DateTime BookingDate { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string PromotionCode { get; set; }
        public decimal FinalAmount { get; set; }

        public string BookingStatus { get; set; }
        // Pending, Confirmed, Cancelled, Expired
        public string PaymentStatus { get; set; }
        //-- Pending, Paid, Failed, Refunded

        public string TransactionId { get; set; }
        public string PaymentMethod { get; set; }

        public string BookingReference { get; set; }

        public List<BookingSeatDTO> Seats { get; set; }

        public string ScheduleDisplay { get; set; }
    }

  
}
