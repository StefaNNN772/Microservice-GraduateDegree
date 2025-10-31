using ReservationService.DTOs;
using ReservationService.Models;
using ReservationService.Repository;

namespace ReservationService.Services
{
    public class BusReservationService
    {
        private readonly BusReservationRepository _busReservationRepository;

        public BusReservationService(BusReservationRepository busReservationRepository)
        {
            this._busReservationRepository = busReservationRepository;
        }

        public async Task<BusLineDTO> GetBusLine(long id)
        {
            return await _busReservationRepository.GetBusLine(id);
        }

        public async Task<Tuple<List<int>, BusLine>> GetBusSeats(long id)
        {
            return await _busReservationRepository.GetBusSeats(id);
        }

        public async Task<bool> AddReservation(Ticket ticket, List<int> numOfSeat)
        {
            return await _busReservationRepository.AddReservation(ticket, numOfSeat);
        }

        public async Task<List<Ticket>> UserToNotify(long id)
        {
            return await _scheduleRepository.UserToNotify(id);
        }

        public async Task<List<Ticket>> GetTicketsToNotifyForUpdate(long id)
        {
            return await _scheduleRepository.GetTicketsToNotifyForUpdate(id);
        }
    }
}
