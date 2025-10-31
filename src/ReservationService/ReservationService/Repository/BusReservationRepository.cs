using Microsoft.EntityFrameworkCore;
using ReservationService.Clients.Interfaces;
using ReservationService.Data;
using ReservationService.DTOs;
using ReservationService.Models;
using System;

namespace ReservationService.Repository
{
    public class BusReservationRepository
    {
        private readonly AppDbContext _context;
        private readonly IRouteServiceClient _routeServiceClient;

        public BusReservationRepository(AppDbContext context, IRouteServiceClient routeServiceClient)
        {
            this._context = context;
            this._routeServiceClient = routeServiceClient;
        }

        public async Task<List<int>> GetBusLineSeats(long id)
        {
            return await _context.ReservedSeats.Where(b => b.BusLineId == id).Select(b => b.SeatNumber).ToListAsync();
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

            var busLine = await _routeServiceClient.GetBusLineAsync(ticket.BusLineId);

            if (busLine.AvailableSeats != null)
            {
                busLine.AvailableSeats -= numOfSeats.Count();

                result = await _context.SaveChangesAsync() > 0;
            }

            return result;
        }

        public async Task<List<Ticket>> UserToNotify(long id)
        {
            var schedule = await _routeServiceClient.GetSchedule(id);

            var schedulesList = await _routeServiceClient.GetSchedules(schedule.BusLineId, schedule.Departure);

            List<Ticket> ticketsToNotify = new List<Ticket>();

            foreach (var s in schedulesList)
            {
                var busLines = await _routeServiceClient.GetBusLines(s.Id);

                foreach (var b in busLines)
                {
                    var tickets = await _context.Tickets
                            //.Include(t => t.User)
                            //.Include(t => t.BusLine)
                                //.ThenInclude(bl => bl.Schedule)
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
            var busLines = await _routeServiceClient.GetBusLines(id);

            List<Ticket> ticketsToNotify = new List<Ticket>();

            foreach (var b in busLines)
            {
                var tickets = await _context.Tickets
                        //.Include(t => t.User)
                        //.Include(t => t.BusLine)
                            //.ThenInclude(bl => bl.Schedule)
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
