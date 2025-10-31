using ReservationService.Clients.Interfaces;
using ReservationService.DTOs;

namespace ReservationService.Clients
{
    public class AuthServiceClient : IAuthServiceClient
    {
        private readonly HttpClient _httpClient;

        public AuthServiceClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(config["Services:AuthService"]);
        }

        public async Task<UserDTO> GetUserByIdAsync(long userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/users/{userId}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<UserDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling AuthService: {ex.Message}");
                return null;
            }
        }
    }
}
