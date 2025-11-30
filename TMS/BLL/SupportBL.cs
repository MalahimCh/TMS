using System.Collections.Generic;
using System.Threading.Tasks;
using TMS.DAL;
using TMS.DTO;

namespace TMS.BLL
{
    public class SupportBL
    {
        private readonly SupportDAL _dal;

        public SupportBL()
        {
            _dal = new SupportDAL();
        }

  
        public async Task<int> AddRequest(SupportRequestDTO request)
        {
            // Assign staff automatically using round-robin
            var staffId = await _dal.GetNextStaffIdAsync();
            request.AssignedStaffId = staffId;
            request.Status = "Assigned"; // default status

            return await _dal.AddSupportRequestAsync(request);
        }

        public async Task<int> AddResponse(SupportRequestResponseDTO response)
        {
            return await _dal.AddResponseAsync(response);
        }

        public async Task<List<SupportRequestDTO>> GetRequestsByCustomer(int customerId)
        {
            return await _dal.GetRequestsByCustomerAsync(customerId);
        }

        public async Task<List<SupportRequestResponseDTO>> GetResponsesByRequest(int requestId)
        {
            return await _dal.GetResponsesByRequestAsync(requestId);
        }

        public async Task<List<SupportRequestDTO>> GetAssignedSupportRequests(string email)
        {
            return await _dal.GetAssignedSupportRequestsAsync(email);
        }


        public async Task<SupportRequestDTO> GetRequestById(int requestId)
        {
            return await _dal.GetRequestByIdAsync(requestId);
        }

        public async Task<bool> UpdateRequestStatus(int requestId, string status)
        {
            return await _dal.UpdateRequestStatusAsync(requestId, status);
        }
    }
}
