using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMS.DAL;
using TMS.DTO;

namespace TMS.BLL
{
    public class ScheduleBL
    {
        private readonly ScheduleDAL _dal;

        public ScheduleBL(ScheduleDAL dal)
        {
            _dal = dal;
        }

        // ---------------- COMPLETED SCHEDULES ----------------
        public async Task<int> UpdateCompletedSchedulesAsync()
        {
            return await _dal.UpdateCompletedSchedulesAsync();
        }

        // ---------------- GET BY ID ----------------
        public async Task<ScheduleDTO?> GetScheduleByIdAsync(int id)
            => await _dal.GetScheduleByIdAsync(id);

        public async Task<RecurringScheduleDTO?> GetRecurringScheduleByIdAsync(int id)
            => await _dal.GetRecurringScheduleByIdAsync(id);

        // ---------------- NORMAL SCHEDULE ----------------
        public async Task AddScheduleAsync(ScheduleDTO s)
            => await _dal.AddScheduleAsync(s);

        public async Task<bool> UpdateScheduleAsync(ScheduleDTO s)
            => await _dal.UpdateScheduleAsync(s);

        public async Task<bool> DeleteScheduleAsync(int id)
            => await _dal.DeleteScheduleAsync(id);

        public async Task<List<ScheduleDTO>> GetAllSchedulesAsync()
            => await _dal.GetAllSchedulesAsync();

        public async Task<List<RecurringScheduleDTO>> GetAllRecurringSchedulesAsync()
            => await _dal.GetAllRecurringSchedulesAsync();

        // ---------------- RECURRING SCHEDULE ----------------
        public async Task AddRecurringScheduleAsync(RecurringScheduleDTO rec)
            => await _dal.AddRecurringScheduleAsync(rec);

        public async Task<bool> UpdateRecurringScheduleAsync(RecurringScheduleDTO rec)
            => await _dal.UpdateRecurringScheduleAsync(rec);

        public async Task<bool> DeleteRecurringScheduleAsync(int id)
            => await _dal.DeleteRecurringScheduleAsync(id);
    }
}
