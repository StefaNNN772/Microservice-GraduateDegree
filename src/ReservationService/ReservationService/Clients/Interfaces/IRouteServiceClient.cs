using ReservationService.DTOs;

namespace ReservationService.Clients.Interfaces
{
    public interface IRouteServiceClient
    {
        Task<BusLineDTO> GetBusLineAsync(long busLineId);
        Task<bool> UpdateAvailableSeatsAsync(long busLineId, int seatsDifference);
        Task<List<long>> GetTicketUserIdsForScheduleAsync(long scheduleId);
        Task<BusLineDTO> GetBusSeats(long busLineId);
        Task<ScheduleDTO> GetSchedule(long id);
        Task<List<ScheduleDTO>> GetSchedules(string id, string departure);
        Task<List<BusLineDTO>> GetBusLines(long id);
    }
}
