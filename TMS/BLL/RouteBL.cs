using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMS.DAL;
using TMS.DTO;

namespace TMS.BLL
{
    public class RouteBL
    {
        private readonly RouteDAL _routeRepo;

        public RouteBL(RouteDAL routeRepo)
        {
            _routeRepo = routeRepo;
        }

        // ---------------- ADD ROUTE ----------------
        public async Task<bool> AddRouteAsync(string origin, string destination, int distanceKm, int estimatedTimeMinutes)
        {
            var allRoutes = await _routeRepo.GetAllRoutesAsync();

            // prevent duplicate routes
            if (allRoutes.Exists(r =>
                r.Origin.Equals(origin, StringComparison.OrdinalIgnoreCase) &&
                r.Destination.Equals(destination, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            var route = new RouteDTO
            {
                Origin = origin,
                Destination = destination,
                DistanceKm = distanceKm,
                EstimatedTimeMinutes = estimatedTimeMinutes
            };

            await _routeRepo.AddRouteAsync(route);
            return true;
        }

        // ---------------- UPDATE ROUTE ----------------
        public async Task<bool> UpdateRouteAsync(RouteDTO route)
        {
            return await _routeRepo.UpdateRouteAsync(route);
        }

        // ---------------- DELETE ROUTE ----------------
        public async Task<bool> DeleteRouteAsync(int routeId)
        {
            return await _routeRepo.DeleteRouteAsync(routeId);
        }

        // ---------------- GET BY ID ----------------
        public async Task<RouteDTO> GetRouteByIdAsync(int id)
        {
            return await _routeRepo.GetRouteByIdAsync(id);
        }

        // ---------------- GET ALL ----------------
        public async Task<List<RouteDTO>> GetAllRoutesAsync()
        {
            return await _routeRepo.GetAllRoutesAsync();
        }
    }
}
