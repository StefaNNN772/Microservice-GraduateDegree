using ReservationService.Repository;

namespace ReservationService.Services
{
    public class TicketService
    {
        private readonly TicketRepository _ticketRepository;

        public TicketService(TicketRepository ticketRepository)
        {
            this._ticketRepository = ticketRepository;
        }
    }
}
