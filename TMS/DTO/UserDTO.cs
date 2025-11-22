using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.DTO
{
    public class UserDTO
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // unique user ID
        public string FullName { get; set; }           // Name
        public string Email { get; set; }              // Email (used for login)
        public string PhoneNumber { get; set; }        // Optional
        public string PasswordHash { get; set; }       // Store hashed password
        public string Role { get; set; }              // e.g., Admin, Customer, Support
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
