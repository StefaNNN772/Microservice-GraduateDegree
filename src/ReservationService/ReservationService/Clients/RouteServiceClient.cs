using ReservationService.Clients.Interfaces;
using ReservationService.DTOs;

namespace ReservationService.Clients
{
    public class RouteServiceClient : IRouteServiceClient
    {
        private readonly HttpClient _httpClient;

        public RouteServiceClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(config["Services:RouteService"]);
        }

        public async Task<BusLineDTO> GetBusLineAsync(long busLineId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"busLines/getBusLine/{busLineId}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<BusLineDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling RouteService: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateAvailableSeatsAsync(long busLineId, int seatsDifference)
        {
            try
            {
                var dto = new { SeatsDifference = seatsDifference };
                var response = await _httpClient.PutAsJsonAsync($"/api/BusLines/{busLineId}/update-seats", dto);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating seats: {ex.Message}");
                return false;
            }
        }

        public async Task<BusLineDTO> GetBusSeats(long busLineId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"busLines/getSeats/{busLineId}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<BusLineDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling RouteService: {ex.Message}");
                return null;
            }
        }

        public async Task<ScheduleDTO> GetSchedule(long id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"schedules/getSchedule/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<ScheduleDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling RouteService: {ex.Message}");
                return null;
            }
        }

        public async Task<List<ScheduleDTO>> GetSchedules(string id, string departure)
        {
            try
            {
                var response = await _httpClient.GetAsync($"schedules/getSchedules/{id}/{departure}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<List<ScheduleDTO>>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling RouteService: {ex.Message}");
                return null;
            }
        }

        public async Task<List<long>> GetTicketUserIdsForScheduleAsync(long scheduleId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Schedules/{scheduleId}/ticket-users");

                if (!response.IsSuccessStatusCode)
                    return new List<long>();

                return await response.Content.ReadFromJsonAsync<List<long>>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting ticket users: {ex.Message}");
                return new List<long>();
            }
        }

        public async Task<List<BusLineDTO>> GetBusLines(long id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"busLines/getBusLines/by-scheduleId/{id}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<List<BusLineDTO>>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling RouteService: {ex.Message}");
                return null;
            }
        }
    }
}
