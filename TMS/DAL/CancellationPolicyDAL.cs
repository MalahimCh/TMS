using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMS.DTO;
using TMS.Design_Patterns;

namespace TMS.DAL
{
    public class CancellationPolicyDAL
    {
        private readonly DBConnection _db;

        public CancellationPolicyDAL()
        {
            _db = new DBConnection();
        }

        // ---------------- GET ALL POLICIES ----------------
        public async Task<List<CancellationPolicyDTO>> GetAllPoliciesAsync()
        {
            var policies = new List<CancellationPolicyDTO>();

            using (var conn = new SqlConnection(_db.ConnectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(
                    "SELECT * FROM CancellationPolicies ORDER BY CreatedAt DESC", conn);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        policies.Add(new CancellationPolicyDTO
                        {
                            Id = reader.GetInt32(0),
                            RefundPercentage = reader.GetInt32(1),
                            CutoffHoursBeforeDeparture = reader.GetInt32(2),
                            IsActive = reader.GetBoolean(3),
                            CreatedAt = reader.GetDateTime(4)
                        });
                    }
                }
            }

            return policies;
        }

        // ---------------- ADD POLICY ----------------
        public async Task<int> AddPolicyAsync(CancellationPolicyDTO policy)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();

            try
            {
                var cmd = new SqlCommand(@"
                    INSERT INTO CancellationPolicies 
                        (RefundPercentage, CutoffHoursBeforeDeparture, IsActive)
                    OUTPUT INSERTED.Id
                    VALUES (@RefundPercentage, @CutoffHoursBeforeDeparture, @IsActive);
                ", conn, trans);

                cmd.Parameters.AddWithValue("@RefundPercentage", policy.RefundPercentage);
                cmd.Parameters.AddWithValue("@CutoffHoursBeforeDeparture", policy.CutoffHoursBeforeDeparture);
                cmd.Parameters.AddWithValue("@IsActive", policy.IsActive);

                int newId = (int)await cmd.ExecuteScalarAsync();
                trans.Commit();
                return newId;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        // ---------------- UPDATE POLICY ----------------
        public async Task<bool> UpdatePolicyAsync(CancellationPolicyDTO policy)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();

            try
            {
                var cmd = new SqlCommand(@"
                    UPDATE CancellationPolicies SET
                        RefundPercentage = @RefundPercentage,
                        CutoffHoursBeforeDeparture = @CutoffHoursBeforeDeparture,
                        IsActive = @IsActive
                    WHERE Id = @Id
                ", conn, trans);

                cmd.Parameters.AddWithValue("@RefundPercentage", policy.RefundPercentage);
                cmd.Parameters.AddWithValue("@CutoffHoursBeforeDeparture", policy.CutoffHoursBeforeDeparture);
                cmd.Parameters.AddWithValue("@IsActive", policy.IsActive);
                cmd.Parameters.AddWithValue("@Id", policy.Id);

                int rows = await cmd.ExecuteNonQueryAsync();
                trans.Commit();
                return rows > 0;
            }
            catch
            {
                trans.Rollback();
                return false;
            }
        }

        // ---------------- DELETE POLICY ----------------
        public async Task<bool> DeletePolicyAsync(int policyId)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();
            using var trans = conn.BeginTransaction();

            try
            {
                var cmd = new SqlCommand("DELETE FROM CancellationPolicies WHERE Id=@Id", conn, trans);
                cmd.Parameters.AddWithValue("@Id", policyId);

                int rows = await cmd.ExecuteNonQueryAsync();
                trans.Commit();
                return rows > 0;
            }
            catch
            {
                trans.Rollback();
                return false;
            }
        }

        // ---------------- GET POLICY BY ID ----------------
        public async Task<CancellationPolicyDTO> GetPolicyByIdAsync(int policyId)
        {
            using var conn = new SqlConnection(_db.ConnectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand("SELECT * FROM CancellationPolicies WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", policyId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new CancellationPolicyDTO
                {
                    Id = reader.GetInt32(0),
                    RefundPercentage = reader.GetInt32(1),
                    CutoffHoursBeforeDeparture = reader.GetInt32(2),
                    IsActive = reader.GetBoolean(3),
                    CreatedAt = reader.GetDateTime(4)
                };
            }

            return null;
        }
    }
}
