using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.DTO
{

    public class OtpDTO
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string OtpCode { get; set; }
        public string Purpose { get; set; } // "Register" or "ResetPassword"
        public DateTime CreatedAt { get; set; }
        public bool IsUsed { get; set; }
    }
   



}
