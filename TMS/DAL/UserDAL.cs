using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using TMS.DTO;

namespace TMS.DAL
{
    public class UserDAL
    {
        private readonly DBConnection _db;

        public UserDAL()
        {
            _db = new DBConnection();
        }

        // Register User
        public async Task AddUserAsync(UserDTO user)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(
                    @"INSERT INTO Users (Id, FullName, Email, PhoneNumber, PasswordHash, Role, CreatedAt)
                      VALUES (@Id, @FullName, @Email, @PhoneNumber, @PasswordHash, @Role, @CreatedAt)", conn);

                cmd.Parameters.AddWithValue("@Id", user.Id);
                cmd.Parameters.AddWithValue("@FullName", user.FullName);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@PhoneNumber", (object)user.PhoneNumber ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                cmd.Parameters.AddWithValue("@Role", user.Role);
                cmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        // Login User
        public async Task<UserDTO> GetUserByEmailAsync(string email)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT * FROM Users WHERE Email=@Email", conn);
                cmd.Parameters.AddWithValue("@Email", email);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new UserDTO
                        {
                            Id = reader.GetGuid(0),
                            FullName = reader.GetString(1),
                            Email = reader.GetString(2),
                            PhoneNumber = reader.IsDBNull(3) ? null : reader.GetString(3),
                            PasswordHash = reader.GetString(4),
                            Role = reader.GetString(5),
                            CreatedAt = reader.GetDateTime(6)
                        };
                    }
                }
            }

            return null;
        }
    }
}
