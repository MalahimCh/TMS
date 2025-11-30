using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMS.DTO;

namespace TMS.DAL
{
    public class SupportDAL
    {
        private readonly DBConnection _db;

        public SupportDAL()
        {
            _db = new DBConnection();
        }

        //----------------- GET ALL SUPPORT STAFF IDS ----------------
        public async Task<List<int>> GetAllSupportStaffIdsAsync()
        {
            var staffIds = new List<int>();
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT Id FROM Users WHERE Role='supportstaff' ORDER BY Id", conn);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        staffIds.Add(reader.GetInt32(0));
                }
            }
            return staffIds;
        }

        //----------------- GET NEXT STAFF ID FOR ASSIGNMENT ----------------
        public async Task<int?> GetNextStaffIdAsync()
        {
            var staffIds = await GetAllSupportStaffIdsAsync();
            if (staffIds.Count == 0)
                return null;

            // Get last assigned staff
            int? lastAssignedStaff = null;
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT TOP 1 AssignedStaffId FROM SupportRequests WHERE AssignedStaffId IS NOT NULL ORDER BY CreatedAt DESC", conn);
                var result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                    lastAssignedStaff = (int)result;
            }

            if (lastAssignedStaff == null)
                return staffIds[0];

            int index = staffIds.IndexOf(lastAssignedStaff.Value);
            return staffIds[(index + 1) % staffIds.Count]; // next staff
        }

        //----------------- ADD NEW SUPPORT REQUEST ----------------
        public async Task<int> AddSupportRequestAsync(SupportRequestDTO request)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    INSERT INTO SupportRequests
                    (CustomerId, Category, Subject, Description, Status, AssignedStaffId, CreatedAt, UpdatedAt)
                    OUTPUT INSERTED.RequestId
                    VALUES (@CustomerId, @Category, @Subject, @Description, @Status, @AssignedStaffId, GETDATE(), GETDATE());
                ", conn);

                cmd.Parameters.AddWithValue("@CustomerId", (object)request.CustomerId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Category", request.Category);
                cmd.Parameters.AddWithValue("@Subject", request.Subject);
                cmd.Parameters.AddWithValue("@Description", request.Description);
                cmd.Parameters.AddWithValue("@Status", request.Status);
                cmd.Parameters.AddWithValue("@AssignedStaffId", (object)request.AssignedStaffId ?? DBNull.Value);

                int newId = (int)await cmd.ExecuteScalarAsync();
                return newId;
            }
        }

        //----------------- ADD RESPONSE ----------------
        public async Task<int> AddResponseAsync(SupportRequestResponseDTO response)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    INSERT INTO SupportRequestResponses
                    (RequestId, UserId, Message, CreatedAt)
                    OUTPUT INSERTED.ResponseId
                    VALUES (@RequestId, @UserId, @Message, GETDATE());
                ", conn);

                cmd.Parameters.AddWithValue("@RequestId", response.RequestId);
                cmd.Parameters.AddWithValue("@UserId", response.UserId);
                cmd.Parameters.AddWithValue("@Message", response.Message);

                int newId = (int)await cmd.ExecuteScalarAsync();
                return newId;
            }
        }

        //----------------- GET ALL REQUESTS FOR CUSTOMER ----------------
        public async Task<List<SupportRequestDTO>> GetRequestsByCustomerAsync(int customerId)
        {
            var list = new List<SupportRequestDTO>();
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    SELECT * FROM SupportRequests
                    WHERE CustomerId = @CustomerId
                    ORDER BY CreatedAt DESC
                ", conn);
                cmd.Parameters.AddWithValue("@CustomerId", customerId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new SupportRequestDTO
                        {
                            RequestId = reader.GetInt32(0),
                            CustomerId = reader.IsDBNull(1) ? null : (int?)reader.GetInt32(1),
                            Category = reader.GetString(2),
                            Subject = reader.GetString(3),
                            Description = reader.GetString(4),
                            Status = reader.GetString(5),
                            AssignedStaffId = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                            CreatedAt = reader.GetDateTime(7),
                            UpdatedAt = reader.GetDateTime(8)
                        });
                    }
                }
            }
            return list;
        }

        //----------------- GET RESPONSES FOR A REQUEST ----------------
        public async Task<List<SupportRequestResponseDTO>> GetResponsesByRequestAsync(int requestId)
        {
            var list = new List<SupportRequestResponseDTO>();
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    SELECT * FROM SupportRequestResponses
                    WHERE RequestId = @RequestId
                    ORDER BY CreatedAt ASC
                ", conn);
                cmd.Parameters.AddWithValue("@RequestId", requestId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new SupportRequestResponseDTO
                        {
                            ResponseId = reader.GetInt32(0),
                            RequestId = reader.GetInt32(1),
                            UserId = reader.GetInt32(2),
                            Message = reader.GetString(3),
                            CreatedAt = reader.GetDateTime(4)
                        });
                    }
                }
            }
            return list;
        }

        //----------------- GET SINGLE REQUEST ----------------
        public async Task<SupportRequestDTO> GetRequestByIdAsync(int requestId)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT * FROM SupportRequests WHERE RequestId=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", requestId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new SupportRequestDTO
                        {
                            RequestId = reader.GetInt32(0),
                            CustomerId = reader.IsDBNull(1) ? null : (int?)reader.GetInt32(1),
                            Category = reader.GetString(2),
                            Subject = reader.GetString(3),
                            Description = reader.GetString(4),
                            Status = reader.GetString(5),
                            AssignedStaffId = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                            CreatedAt = reader.GetDateTime(7),
                            UpdatedAt = reader.GetDateTime(8)
                        };
                    }
                }
            }
            return null;
        }

        //----------------- UPDATE STATUS ----------------
        public async Task<bool> UpdateRequestStatusAsync(int requestId, string status)
        {
            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    UPDATE SupportRequests 
                    SET Status=@Status, UpdatedAt=GETDATE() 
                    WHERE RequestId=@Id
                ", conn);

                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@Id", requestId);

                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        //----------------GET ASSIGNED SUPPORT REQUESTS FOR STAFF BY EMAIL ----------------
        public async Task<List<SupportRequestDTO>> GetAssignedSupportRequestsAsync(string email)
        {
            var list = new List<SupportRequestDTO>();

            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand(@"
            SELECT 
                SR.RequestId,
                SR.Subject,
                SR.Category,
                SR.Description,
                SR.Status,
                SR.CustomerId,
                SR.CreatedAt,
                U.FullName AS CustomerName
            FROM SupportRequests SR
            INNER JOIN Users S ON S.Id = SR.AssignedStaffId
            LEFT JOIN Users U ON U.Id = SR.CustomerId
            WHERE S.Email = @Email
            ORDER BY SR.CreatedAt DESC;
        ", conn);

                cmd.Parameters.AddWithValue("@Email", email);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new SupportRequestDTO
                        {
                            RequestId = reader.GetInt32(0),
                            Subject = reader.GetString(1),
                            Category = reader.GetString(2),
                            Description = reader.GetString(3),
                            Status = reader.GetString(4),
                            CustomerId = reader.IsDBNull(5) ? null : (int?)reader.GetInt32(5),
                            CreatedAt = reader.GetDateTime(6),
                            CustomerName = reader.IsDBNull(7) ? "Guest User" : reader.GetString(7),
                        });
                    }
                }
            }

            return list;
        }

    }
}
