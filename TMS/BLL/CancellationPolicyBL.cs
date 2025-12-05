using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMS.DAL;
using TMS.DTO;

namespace TMS.BLL
{
    public class CancellationPolicyBL
    {
        private readonly CancellationPolicyDAL _dal;

        public CancellationPolicyBL(CancellationPolicyDAL dal)
        {
            _dal = dal;
        }

        // ---------------- GET ALL ----------------
        public async Task<List<CancellationPolicyDTO>> GetAllPoliciesAsync()
        {
            return await _dal.GetAllPoliciesAsync();
        }

        // ---------------- ADD ----------------
        public async Task<bool> AddPolicyAsync(int refundPercentage, int cutoffHours, bool isActive = true)
        {
            var policy = new CancellationPolicyDTO
            {
                RefundPercentage = refundPercentage,
                CutoffHoursBeforeDeparture = cutoffHours,
                IsActive = isActive
            };

            int id = await _dal.AddPolicyAsync(policy);
            return id > 0;
        }

        // ---------------- UPDATE ----------------
        public async Task<bool> UpdatePolicyAsync(CancellationPolicyDTO policy)
        {
            return await _dal.UpdatePolicyAsync(policy);
        }

        // ---------------- DELETE ----------------
        public async Task<bool> DeletePolicyAsync(int policyId)
        {
            return await _dal.DeletePolicyAsync(policyId);
        }

        // ---------------- GET BY ID ----------------
        public async Task<CancellationPolicyDTO> GetPolicyByIdAsync(int id)
        {
            return await _dal.GetPolicyByIdAsync(id);
        }
    }
}
