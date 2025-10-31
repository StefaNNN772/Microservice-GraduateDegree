using Microsoft.EntityFrameworkCore;
using RouteService.Data;
using RouteService.Models;
using System;

namespace RouteService.Repository
{
    public class BusLinesRepository
    {
        private readonly AppDbContext _context;

        public BusLinesRepository(AppDbContext context)
        {
            this._context = context;
        }

        public async Task<List<string>> GetRoutes()
        {
            return _context.Schedules.Select(s => s.Departure + "-" + s.Arrival)
                .Distinct()
                .OrderBy(s => s.Substring(0, 1))
                .ToList();
        }

        public async Task<List<BusLine>> GetBusLinesForRoute(string departure, string arrival)
        {
            return await _context.BusLines.Include(b => b.Schedule)
                .Where(b => b.Schedule.Departure == departure && b.Schedule.Arrival == arrival)
                .ToListAsync();
        }

        public async Task<string> GetProviderName(long id)
        {
            return _context.Providers.Where(p => p.Id == id)
                .Select(p => p.Name)
                .FirstOrDefault();
        }
    }
}
