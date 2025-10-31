using Microsoft.EntityFrameworkCore;
using RouteService.Data;
using RouteService.Models;
using System.Net.Sockets;

namespace RouteService.Repository
{
    public class SchedulesRepository
    {
        private readonly AppDbContext _context;

        public SchedulesRepository(AppDbContext context)
        {
            this._context = context;
        }

        public async Task<Schedules> AddSchedule(Schedules schedule)
        {
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();
            return schedule;
        }

        public async Task<List<Schedules>> GetSchedules(int providerId)
        {
            return await _context.Schedules.Where(u => u.ProviderId == providerId).ToListAsync();
        }

        public async Task<bool> DeleteScheduleById(long id)
        {
            var schedule = await _context.Schedules.FindAsync(id);

            if (schedule == null)
            {
                return false;
            }

            var schedulesToDelete = await _context.Schedules.Where(s => s.BusLineId == schedule.BusLineId &&
                                                                (s.Departure == schedule.Departure || s.Arrival == schedule.Departure)).ToListAsync();

            foreach (var s in schedulesToDelete)
            {
                var busLinesToDelete = await _context.BusLines.Where(b => b.ScheduleId == s.Id).ToListAsync();
                _context.BusLines.RemoveRange(busLinesToDelete);
                await _context.SaveChangesAsync();
            }

            _context.Schedules.RemoveRange(schedulesToDelete);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Schedules> FindScheduleById(long id)
        {
            return await _context.Schedules.FindAsync(id);
        }

        public async Task<bool> UpdateSchedule(Schedules schedule)
        {
            _context.Schedules.Update(schedule);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> UpdateAllSchedules(List<Schedules> schedules)
        {
            _context.Schedules.UpdateRange(schedules);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<List<Schedules>> GetAllSchedulesByBusLineId(string busLineId)
        {
            return await _context.Schedules.Where(s => s.BusLineId == busLineId).ToListAsync();
        }

        public async Task<bool> GenerateBusLines(BusLine busLine)
        {
            _context.BusLines.Add(busLine);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateBusLinesAvailableSeats(int availableSeatsDifference, long scheduleId)
        {
            var allBusLinesById = await _context.BusLines.Where(b => b.ScheduleId == scheduleId).ToListAsync();

            foreach (var busLine in allBusLinesById)
            {
                if ((busLine.AvailableSeats + availableSeatsDifference) >= 0)
                {
                    busLine.AvailableSeats += availableSeatsDifference;
                }
            }

            _context.BusLines.UpdateRange(allBusLinesById);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        //public async Task<List<Ticket>> UserToNotify(long id)
        //{
        //    var schedule = await _context.Schedules.FindAsync(id);

        //    var schedulesList = await _context.Schedules.Where(s => s.BusLineId == schedule.BusLineId &&
        //                                                        (s.Departure == schedule.Departure || s.Arrival == schedule.Departure)).ToListAsync();

        //    List<Ticket> ticketsToNotify = new List<Ticket>();

        //    foreach (var s in schedulesList)
        //    {
        //        var busLines = await _context.BusLines.Where(b => b.ScheduleId == s.Id).ToListAsync();

        //        foreach (var b in busLines)
        //        {
        //            var tickets = await _context.Tickets
        //                    .Include(t => t.User)
        //                    .Include(t => t.BusLine)
        //                        .ThenInclude(bl => bl.Schedule)
        //                    .Where(t => t.BusLineId == b.Id)
        //                    .ToListAsync();

        //            foreach (var t in tickets)
        //            {
        //                ticketsToNotify.Add(t);
        //            }
        //        }
        //    }

        //    return ticketsToNotify;
        //}

        //public async Task<List<Ticket>> GetTicketsToNotifyForUpdate(long id)
        //{
        //    var busLines = await _context.BusLines.Where(b => b.ScheduleId == id).ToListAsync();

        //    List<Ticket> ticketsToNotify = new List<Ticket>();

        //    foreach (var b in busLines)
        //    {
        //        var tickets = await _context.Tickets
        //                .Include(t => t.User)
        //                .Include(t => t.BusLine)
        //                    .ThenInclude(bl => bl.Schedule)
        //                .Where(t => t.BusLineId == b.Id)
        //                .ToListAsync();

        //        foreach (var t in tickets)
        //        {
        //            ticketsToNotify.Add(t);
        //        }
        //    }

        //    return ticketsToNotify;
        //}
    }
}
