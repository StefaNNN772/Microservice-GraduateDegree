using RouteService.DTOs;
using RouteService.Models;
using RouteService.Repository;
using System.Net.Sockets;

namespace RouteService.Services
{
    public class SchedulesService
    {
        private readonly SchedulesRepository _scheduleRepository;

        public SchedulesService(SchedulesRepository schedulesRepository)
        {
            this._scheduleRepository = schedulesRepository;
        }

        public async Task<Schedules> AddSchedule(SchedulesDTO schedulesDto)
        {
            var newSchedule = new Schedules
            {
                ProviderId = schedulesDto.ProviderId,
                Departure = schedulesDto.Departure,
                Arrival = schedulesDto.Arrival,
                DepartureTime = TimeSpan.Parse(schedulesDto.DepartureTime),
                ArrivalTime = TimeSpan.Parse(schedulesDto.ArrivalTime),
                BusLineId = schedulesDto.BusLineId,
                Price = schedulesDto.Price,
                AvailableSeats = schedulesDto.AvailableSeats,
                PricePerKilometer = schedulesDto.PricePerKilometer,
                Days = schedulesDto.Days,
                Discount = schedulesDto.Discount
            };

            newSchedule = await _scheduleRepository.AddSchedule(newSchedule);

            return newSchedule;
        }

        public async Task<List<Schedules>> GetSchedules(int providerId)
        {
            var schedulesList = await _scheduleRepository.GetSchedules(providerId);

            return schedulesList;
        }

        public async Task<Schedules> GetScheduleById(long id)
        {
            var schedule = await _scheduleRepository.GetScheduleById(id);

            return schedule;
        }

        public async Task<List<Schedules>> GetScheduleByIdAndDeparture(string id, string departure)
        {
            var schedule = await _scheduleRepository.GetScheduleByIdAndDeparture(id, departure);

            return schedule;
        }

        public async Task<bool> DeleteSchedule(long id)
        {
            return await _scheduleRepository.DeleteScheduleById(id);
        }

        public async Task<Schedules> FindScheduleById(long id)
        {
            return await _scheduleRepository.FindScheduleById(id);
        }

        public async Task<bool> UpdateSchedule(Schedules schedule)
        {
            return await _scheduleRepository.UpdateSchedule(schedule);
        }

        public async Task<bool> UpdateAllSchedules(List<Schedules> schedules)
        {
            return await _scheduleRepository.UpdateAllSchedules(schedules);
        }

        public async Task<List<Schedules>> GetAllSchedulesByBusLineId(string busLineId)
        {
            return await _scheduleRepository.GetAllSchedulesByBusLineId(busLineId);
        }

        public async Task<bool> GenerateBusLines(BusLine busLine)
        {
            return await _scheduleRepository.GenerateBusLines(busLine);
        }

        public async Task<bool> UpdateBusLinesAvailableSeats(int availableSeatsDifference, long scheduleId)
        {
            return await _scheduleRepository.UpdateBusLinesAvailableSeats(availableSeatsDifference, scheduleId);
        }

        //public async Task<List<Ticket>> UserToNotify(long id)
        //{
        //    return await _scheduleRepository.UserToNotify(id);
        //}

        //public async Task<List<Ticket>> GetTicketsToNotifyForUpdate(long id)
        //{
        //    return await _scheduleRepository.GetTicketsToNotifyForUpdate(id);
        //}
    }
}
