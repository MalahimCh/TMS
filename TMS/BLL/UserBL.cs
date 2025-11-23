using TMS.DAL;
using TMS.DTO;
using BCrypt.Net;
using System.Threading.Tasks;

namespace TMS.BLL
{
    public class UserBL
    {
        private readonly UserDAL _userRepo;
        private readonly OtpBL _otpBL;

        public UserBL(UserDAL userRepo, OtpBL otpBL)
        {
            _userRepo = userRepo;
            _otpBL = otpBL;
        }

        // REGISTER
        public async Task<bool> RegisterUserAsync(string fullName, string email, string phone, string password, string role = "customer")
        {
            var existing = await _userRepo.GetUserByEmailAsync(email);
            if (existing != null) return false; 

            var user = new UserDTO
            {
                FullName = fullName,
                Email = email,
                PhoneNumber = phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role,
                IsEmailVerified = false
            };

            await _userRepo.AddUserAsync(user);

            // Send OTP for email verification
            await _otpBL.SendOtpEmailAsync(email, "Register");

            return true;
        }

        // RESET PASSWORD
        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
                      
            var user = await _userRepo.GetUserByEmailAsync(email);
            if (user == null) return false;

           
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepo.UpdateUserAsync(user);

            return true;
        }



        // VERIFY User for Login
        public async Task<bool> VerifyUserOtpAsync(string email, string otpCode,string purpose)
        {
            bool valid = await _otpBL.ValidateOtpAsync(email, otpCode, purpose);
            if (!valid) return false;

            // Mark user as verified
            var user = await _userRepo.GetUserByEmailAsync(email);
            if (user != null)
            {
                user.IsEmailVerified = true;
                await _userRepo.UpdateUserAsync(user); 
                return true;
            }
            return false;
        }

        // LOGIN
        public async Task<UserDTO> LoginAsync(string email, string password)
        {
            var user = await _userRepo.GetUserByEmailAsync(email);
            if (user == null) return null;

            if (!user.IsEmailVerified)
                throw new Exception("NOT_VERIFIED");

            bool valid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return valid ? user : null;
        }

        // Get user by email
        public async Task<UserDTO> GetUserByEmailAsync(string email)
        {
            return await _userRepo.GetUserByEmailAsync(email);
        }

    }
}
