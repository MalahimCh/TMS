using TMS.DAL;
using TMS.DTO;
using BCrypt.Net;
using System.Threading.Tasks;

namespace TMS.BLL
{
    public class UserBL
    {
        private readonly UserDAL _repo;

        public UserBL(UserDAL repo)
        {
            _repo = repo;
        }

        // REGISTER
        public async Task<bool> RegisterUserAsync(string fullName, string email, string phone, string password, string role = "customer")
        {
            var existing = await _repo.GetUserByEmailAsync(email);
            if (existing != null) return false; // email already taken

            var user = new UserDTO
            {
                FullName = fullName,
                Email = email,
                PhoneNumber = phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role
            };

            await _repo.AddUserAsync(user);
            return true;
        }

        // LOGIN
        public async Task<UserDTO> LoginAsync(string email, string password)
        {
            var user = await _repo.GetUserByEmailAsync(email);
            if (user == null) return null;

            bool valid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return valid ? user : null;
        }
    }
}
