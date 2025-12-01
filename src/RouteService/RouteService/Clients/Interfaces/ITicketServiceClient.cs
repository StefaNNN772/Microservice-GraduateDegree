using RouteService.DTOs;

namespace RouteService.Clients.Interfaces
{
    public interface ITicketServiceClient
    {
        Task<List<TicketDTO>> GetTickets(long scheduleId);
        Task<List<TicketDTO>> GetTicketsForUpdate(long scheduleId);
    }
}
