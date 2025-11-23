using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;
using TMS.DTO;

namespace TMS.DAL
{
    public class OtpDAL
    {
        private readonly DBConnection _db;

        public OtpDAL()
        {
            _db = new DBConnection();
        }

        // Insert OTP
        public async Task InsertOtpAsync(OtpDTO otp)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(
                    @"INSERT INTO OtpVerification (Email, OtpCode, Purpose)
                      VALUES (@Email, @OtpCode, @Purpose)", conn);

                cmd.Parameters.AddWithValue("@Email", otp.Email);
                cmd.Parameters.AddWithValue("@OtpCode", otp.OtpCode);
                cmd.Parameters.AddWithValue("@Purpose", otp.Purpose);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        // Get latest unused OTP
        public async Task<OtpDTO> GetLatestOtpAsync(string email, string purpose)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(
                    @"SELECT TOP 1 * FROM OtpVerification
                      WHERE Email=@Email AND Purpose=@Purpose AND IsUsed=0
                      ORDER BY CreatedAt DESC", conn);

                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Purpose", purpose);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new OtpDTO
                        {
                            Id = reader.GetInt32(0),
                            Email = reader.GetString(1),
                            OtpCode = reader.GetString(2),
                            Purpose = reader.GetString(3),
                            CreatedAt = reader.GetDateTime(4),
                            IsUsed = reader.GetBoolean(5)
                        };
                    }
                }
            }

            return null;
        }

        // Mark OTP as used
        public async Task MarkOtpAsUsedAsync(int otpId)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("UPDATE OtpVerification SET IsUsed=1 WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", otpId);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
