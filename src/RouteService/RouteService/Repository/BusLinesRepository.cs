using Microsoft.EntityFrameworkCore;
using RouteService.Clients.Interfaces;
using RouteService.Data;
using RouteService.DTOs;
using RouteService.Models;
using System;

namespace RouteService.Repository
{
    public class BusLinesRepository
    {
        private readonly AppDbContext _context;
        private readonly IAuthServiceClient _authServiceClient;

        public BusLinesRepository(AppDbContext context, IAuthServiceClient authServiceClient)
        {
            this._context = context;
            this._authServiceClient = authServiceClient;
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
            var provider = await _authServiceClient.GetProviderByIdAsync(id);

            return provider.Name;
        }

        public async Task<BusLineDTO> GetBusLine(long id)
        {
            var busLine = await _context.BusLines.Include(b => b.Schedule)
                .FirstOrDefaultAsync(b => b.Id == id);

            var provider = await _authServiceClient.GetProviderByIdAsync(busLine.Schedule.ProviderId);

            BusLineDTO busLineDto = new BusLineDTO
            {
                Id = busLine.Id,
                Departure = busLine.Schedule.Departure,
                Arrival = busLine.Schedule.Arrival,
                Discount = busLine.Schedule.Discount,
                DepartureDate = busLine.DepartureDate.ToString("dd/MM/yyyy"),
                DepartureTime = busLine.Schedule.DepartureTime.ToString(@"hh\:mm"),
                ArrivalTime = busLine.Schedule.ArrivalTime.ToString(@"hh\:mm"),
                Provider = provider.Name,
                AvailableSeats = busLine.AvailableSeats,
                Price = busLine.Schedule.Price,
            };

            return busLineDto;
        }

        public async Task<List<BusLine>> GetBusLinesByScheduleId(long id)
        {
            return await _context.BusLines.Where(b => b.ScheduleId == id).ToListAsync();
        }

        public async Task<BusLine> GetBusSeats(long id)
        {
            var busLine = await _context.BusLines.Include(b => b.Schedule).Where(b => b.Id == id).FirstAsync();

            return busLine;
        }
    }
}
