using Microsoft.EntityFrameworkCore;
using ReservationService.Data;
using ReservationService.DTOs;
using ReservationService.Models;
using System;

namespace ReservationService.Repository
{
    public class BusReservationRepository
    {
        private readonly AppDbContext _context;

        public BusReservationRepository(AppDbContext context)
        {
            this._context = context;
        }

        public async Task<BusLineDTO> GetBusLine(long id)
        {
            var busLine = await _context.BusLines.Include(b => b.Schedule)
                .ThenInclude(p => p.Provider)
                .FirstOrDefaultAsync(b => b.Id == id);

            BusLineDTO busLineDto = new BusLineDTO
            {
                Id = busLine.Id,
                Departure = busLine.Schedule.Departure,
                Arrival = busLine.Schedule.Arrival,
                Discount = busLine.Schedule.Discount,
                DepartureDate = busLine.DepartureDate.ToString("dd/MM/yyyy"),
                DepartureTime = busLine.Schedule.DepartureTime.ToString(@"hh\:mm"),
                ArrivalTime = busLine.Schedule.ArrivalTime.ToString(@"hh\:mm"),
                Provider = busLine.Schedule.Provider.Name,
                AvailableSeats = busLine.AvailableSeats,
                Price = busLine.Schedule.Price,
            };

            return busLineDto;
        }

        public async Task<Tuple<List<int>, BusLine>> GetBusSeats(long id)
        {
            var reserverSeats = await _context.ReservedSeats.Where(b => b.BusLineId == id).Select(b => b.SeatNumber).ToListAsync();
            var busLine = await _context.BusLines.Include(b => b.Schedule).Where(b => b.Id == id).FirstAsync();

            return Tuple.Create(reserverSeats, busLine);
        }

        public async Task<bool> AddReservation(Ticket ticket, List<int> numOfSeats)
        {
            bool result = false;

            _context.Tickets.Add(ticket);
            result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    foreach (var n in numOfSeats)
                    {
                        _context.ReservedSeats.AddRange(new ReservedSeat { SeatNumber = n, BusLineId = ticket.BusLineId, TicketId = ticket.Id });
                    }

                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    result = false;
                    await transaction.RollbackAsync();
                }
            }

            var busLine = await _context.BusLines.FindAsync(ticket.BusLineId);

            if (busLine.AvailableSeats != null)
            {
                busLine.AvailableSeats -= numOfSeats.Count();

                result = await _context.SaveChangesAsync() > 0;
            }

            return result;
        }

        public async Task<List<Ticket>> UserToNotify(long id)
        {
            var schedule = await _context.Schedules.FindAsync(id);

            var schedulesList = await _context.Schedules.Where(s => s.BusLineId == schedule.BusLineId &&
                                                                (s.Departure == schedule.Departure || s.Arrival == schedule.Departure)).ToListAsync();

            List<Ticket> ticketsToNotify = new List<Ticket>();

            foreach (var s in schedulesList)
            {
                var busLines = await _context.BusLines.Where(b => b.ScheduleId == s.Id).ToListAsync();

                foreach (var b in busLines)
                {
                    var tickets = await _context.Tickets
                            .Include(t => t.User)
                            .Include(t => t.BusLine)
                                .ThenInclude(bl => bl.Schedule)
                            .Where(t => t.BusLineId == b.Id)
                            .ToListAsync();

                    foreach (var t in tickets)
                    {
                        ticketsToNotify.Add(t);
                    }
                }
            }

            return ticketsToNotify;
        }

        public async Task<List<Ticket>> GetTicketsToNotifyForUpdate(long id)
        {
            var busLines = await _context.BusLines.Where(b => b.ScheduleId == id).ToListAsync();

            List<Ticket> ticketsToNotify = new List<Ticket>();

            foreach (var b in busLines)
            {
                var tickets = await _context.Tickets
                        .Include(t => t.User)
                        .Include(t => t.BusLine)
                            .ThenInclude(bl => bl.Schedule)
                        .Where(t => t.BusLineId == b.Id)
                        .ToListAsync();

                foreach (var t in tickets)
                {
                    ticketsToNotify.Add(t);
                }
            }

            return ticketsToNotify;
        }
    }
}
