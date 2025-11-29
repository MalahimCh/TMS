using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMS.DAL;
using TMS.DTO;

namespace TMS.BLL
{
    public class BusBL
    {
        private readonly BusDAL _busRepo;

        public BusBL(BusDAL busRepo)
        {
            _busRepo = busRepo;
        }

        // ---------------- ADD BUS ----------------
        public async Task<bool> AddBusAsync(string busNumber, string busType, int totalSeats)
        {
            // Check duplicate bus number
            var allBuses = await _busRepo.GetAllBusesAsync();
            if (allBuses.Exists(b => b.BusNumber.Equals(busNumber, StringComparison.OrdinalIgnoreCase)))
                return false;

            var bus = new BusDTO
            {
                BusNumber = busNumber,
                BusType = busType,
                TotalSeats = totalSeats
            };

            await _busRepo.AddBusAsync(bus);
            return true;
        }

        // ---------------- UPDATE BUS ----------------
        public async Task<bool> UpdateBusAsync(BusDTO bus)
        {
            return await _busRepo.UpdateBusAsync(bus);
        }

        // ---------------- DELETE BUS ----------------
        public async Task<bool> DeleteBusAsync(int busId)
        {
            return await _busRepo.DeleteBusAsync(busId);
        }

        // ---------------- GET BY ID ----------------
        public async Task<BusDTO> GetBusByIdAsync(int id)
        {
            return await _busRepo.GetBusByIdAsync(id);
        }

        // ---------------- GET ALL ----------------
        public async Task<List<BusDTO>> GetAllBusesAsync()
        {
            return await _busRepo.GetAllBusesAsync();
        }
    }
}
