using System;

namespace TMS.DTO
{
    public class CancellationPolicyDTO
    {
        public int Id { get; set; }
        public int RefundPercentage { get; set; }
        public int CutoffHoursBeforeDeparture { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
