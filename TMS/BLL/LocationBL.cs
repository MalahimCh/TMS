using System.Collections.Generic;
using System.Threading.Tasks;
using TMS.DAL;
using TMS.DTO;

namespace TMS.BLL
{
    public class LocationBL
    {
        private readonly LocationDAL _locationDAL;

        public LocationBL(LocationDAL locationDAL)
        {
            _locationDAL = locationDAL;
        }

        // Get all locations
        public async Task<List<LocationDTO>> GetAllLocationsAsync()
        {
            return await _locationDAL.GetAllLocationsAsync();
        }


    }
}
