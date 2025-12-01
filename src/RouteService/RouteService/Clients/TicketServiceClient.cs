using RouteService.Clients.Interfaces;
using RouteService.DTOs;
using RouteService.Models;

namespace RouteService.Clients
{
    public class TicketServiceClient : ITicketServiceClient
    {
        private readonly HttpClient _httpClient;

        public TicketServiceClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(config["Services:ReservationService"]);
        }

        public async Task<List<TicketDTO>> GetTickets(long scheduleId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"busReservation/getTickets/{scheduleId}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<List<TicketDTO>>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling ReservationService: {ex.Message}");
                return null;
            }
        }

        public async Task<List<TicketDTO>> GetTicketsForUpdate(long scheduleId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"busReservation/getTicketsForUpdate/{scheduleId}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<List<TicketDTO>>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling ReservationService: {ex.Message}");
                return null;
            }
        }
    }
}
