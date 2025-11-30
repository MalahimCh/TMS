using System;

namespace TMS.DTO
{
    public class SupportRequestResponseDTO
    {
        public int ResponseId { get; set; }          // Identity column
        public int RequestId { get; set; }           // FK to SupportRequests
        public int UserId { get; set; }              // Customer or SupportStaff
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
