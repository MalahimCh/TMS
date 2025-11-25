using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using TMS.DAL;
using TMS.DTO;

namespace TMS.BLL
{
    public class OtpBL
    {
        private readonly OtpDAL _repo;

        public OtpBL(OtpDAL repo)
        {
            _repo = repo;
        }

        // Generate 6-digit OTP
        public string GenerateOtp()
        {
            Random rnd = new Random();
            return rnd.Next(100000, 999999).ToString();
        }

        public async Task<string> GenerateAndStoreOtpAsync(string email, string purpose)
        {
            string otpCode = GenerateOtp();

            var otp = new OtpDTO
            {
                Email = email,
                OtpCode = otpCode,
                Purpose = purpose
            };

            await _repo.InsertOtpAsync(otp);
            return otpCode;
        }

        // Send OTP email and save to DB
        public async Task SendOtpEmailAsync(string email, string otpCode, string purpose)
        {
          

            using (var mail = new MailMessage())
            {
                mail.From = new MailAddress("noreply.tickit@gmail.com", "Tickit Support");
                mail.To.Add(email);
                mail.Subject = purpose == "Register" ? "Your Tickit Verification Code" : "Tickit Password Reset Code";
                mail.IsBodyHtml = true;

                mail.Body = $@"
                <div style='font-family:Segoe UI, sans-serif; padding:20px; background:#f7f9fc;'>
                    <div style='max-width:500px; margin:auto; background:white; padding:25px; border-radius:8px; 
                         box-shadow:0 2px 8px rgba(0,0,0,0.1);'>

                <h2 style='color:#17358A; text-align:center; margin-bottom:10px;'>Tickit Verification</h2>

                <p style='font-size:15px; color:#333;'>
                    Dear User,<br/><br/>
                    Please use the following One-Time Password (OTP) to complete your 
                    <b>{purpose}</b> process.
                </p>

                <div style='text-align:center; margin:25px 0;'>
                    <div style='font-size:30px; font-weight:bold; color:#17358A; letter-spacing:4px;'>
                        {otpCode}
                    </div>
                </div>

                <p style='font-size:14px; color:#555;'>
                    This OTP is valid for 5 minutes. If you did not request this, please ignore the email.
                </p>

                <hr style='margin:20px 0;'/>

                <p style='font-size:12px; color:#999; text-align:center;'>
                    © {DateTime.Now.Year} Tickit. All rights reserved.
                </p>
            </div>
        </div>";

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("noreply.tickit@gmail.com", "tmdj obqo ntko wppf");
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mail);
                }
            }
        }

        public async Task<bool> ValidateOtpAsync(string email, string otpCode, string purpose)
        {
            var otp = await _repo.GetLatestOtpAsync(email, purpose);

            if (otp != null && otp.OtpCode == otpCode)
            {
                // Check expiry: 5 minutes
                if ((DateTime.UtcNow - otp.CreatedAt).TotalMinutes > 5)
                {
                    return false; // OTP expired
                }

                await _repo.MarkOtpAsUsedAsync(otp.Id);
                return true;
            }

            return false;
        }

    }
}
