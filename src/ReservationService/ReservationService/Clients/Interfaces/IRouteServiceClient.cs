using ReservationService.DTOs;

namespace ReservationService.Clients.Interfaces
{
    public interface IRouteServiceClient
    {
        Task<BusLineDTO> GetBusLineAsync(long busLineId);
        Task<bool> UpdateAvailableSeatsAsync(long busLineId, int seatsDifference);
        Task<List<long>> GetTicketUserIdsForScheduleAsync(long scheduleId);
    }
}
