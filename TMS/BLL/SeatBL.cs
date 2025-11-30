using System.Collections.Generic;
using System.Threading.Tasks;
using TMS.DAL;
using TMS.DTO;

namespace TMS.BLL
{
    public class SeatBL
    {
        private readonly SeatDAL _seatDAL;

        public SeatBL(SeatDAL seatDAL)
        {
            _seatDAL = seatDAL;
        }

        public Task<List<SeatModel>> GetSeatsAsync(int busId)
        {
            return _seatDAL.GetSeatsByBusAsync(busId);
        }

        public Task UpdateSeatStatusAsync(int seatId, string status)
        {
            return _seatDAL.UpdateSeatStatusAsync(seatId, status);
        }
    }
}
