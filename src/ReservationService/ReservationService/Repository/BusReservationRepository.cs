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
        private readonly IAuthServiceClient _authServiceClient;

        public BusReservationRepository(AppDbContext context, IRouteServiceClient routeServiceClient, IAuthServiceClient authServiceClient)
        {
            this._context = context;
            this._routeServiceClient = routeServiceClient;
            this._authServiceClient = authServiceClient;
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

            result = await _routeServiceClient.UpdateBusLine(ticket.BusLineId, numOfSeats.Count());

            //var busLine = await _routeServiceClient.GetBusLineAsync(ticket.BusLineId);

            //if (busLine.AvailableSeats != null)
            //{
            //    busLine.AvailableSeats -= numOfSeats.Count();

            //    result = await _context.SaveChangesAsync() > 0;
            //}

            return result;
        }

        public async Task<List<TicketDTO>> UserToNotify(long id)
        {
            var schedule = await _routeServiceClient.GetSchedule(id);

            var schedulesList = await _routeServiceClient.GetSchedules(schedule.BusLineId, schedule.Departure);

            List<TicketDTO> ticketsToNotify = new List<TicketDTO>();

            foreach (var s in schedulesList)
            {
                var busLines = await _routeServiceClient.GetBusLines(s.Id);

                foreach (var b in busLines)
                {
                    var tickets = await _context.Tickets
                            .Where(t => t.BusLineId == b.Id)
                            .ToListAsync();


                    foreach (var t in tickets)
                    {
                        var userDto = await _authServiceClient.GetUserByIdAsync(t.UserId);
                        var buslineDto = await _routeServiceClient.GetBusLineAsync(t.BusLineId);
                        var scheduleDto = await _routeServiceClient.GetSchedule(buslineDto.ScheduleId);

                        ticketsToNotify.Add(new TicketDTO
                        {
                            Id = t.Id,
                            BusLineId = buslineDto.Id,
                            UserId = userDto.Id,
                            UserEmail = userDto.Email,
                            QRCodeValue = t.QRCodeValue,
                            NumberOfSeats = t.NumberOfSeats,
                            IsChecked = t.IsChecked,
                            IsPaid = t.IsPaid,
                            PurchaseTime = t.PurchaseTime,
                            Price = t.Price,
                            PaymentMethod = t.PaymentMethod,
                            Departure = scheduleDto.Departure,
                            Arrival = scheduleDto.Arrival,
                            DepartureTime = scheduleDto.DepartureTime,
                            ArrivalTime = scheduleDto.ArrivalTime,
                            DepartureDate = buslineDto.DepartureDate
                        });
                    }
                }
            }

            return ticketsToNotify;
        }

        public async Task<List<TicketDTO>> GetTicketsToNotifyForUpdate(long id)
        {
            var busLines = await _routeServiceClient.GetBusLines(id);

            List<TicketDTO> ticketsToNotify = new List<TicketDTO>();

            foreach (var b in busLines)
            {
                var tickets = await _context.Tickets
                        .Where(t => t.BusLineId == b.Id)
                        .ToListAsync();

                foreach (var t in tickets)
                {
                    var userDto = await _authServiceClient.GetUserByIdAsync(t.UserId);
                    var buslineDto = await _routeServiceClient.GetBusLineAsync(t.BusLineId);
                    var scheduleDto = await _routeServiceClient.GetSchedule(buslineDto.ScheduleId);

                    ticketsToNotify.Add(new TicketDTO
                    {
                        Id = t.Id,
                        BusLineId = buslineDto.Id,
                        UserId = userDto.Id,
                        UserEmail = userDto.Email,
                        QRCodeValue = t.QRCodeValue,
                        NumberOfSeats = t.NumberOfSeats,
                        IsChecked = t.IsChecked,
                        IsPaid = t.IsPaid,
                        PurchaseTime = t.PurchaseTime,
                        Price = t.Price,
                        PaymentMethod = t.PaymentMethod,
                        Departure = scheduleDto.Departure,
                        Arrival = scheduleDto.Arrival,
                        DepartureTime = scheduleDto.DepartureTime,
                        ArrivalTime = scheduleDto.ArrivalTime,
                        DepartureDate = buslineDto.DepartureDate
                    });
                }
            }

            return ticketsToNotify;
        }
    }
}
