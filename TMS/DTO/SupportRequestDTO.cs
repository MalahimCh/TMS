using System;

namespace TMS.DTO
{
     public class SupportRequestDTO
    {
        public int RequestId { get; set; }
        public string Subject { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }

        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }   // UI – optional
        public DateTime CreatedAt { get; set; }

        // --- UI ONLY FIELDS ---
        public string StatusColor { get; set; }     // For colored label in WPF
        public bool ShowUnreadDot { get; set; }
        public int? AssignedStaffId { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

}
