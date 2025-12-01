using RouteService.Clients.Interfaces;
using RouteService.DTOs;

namespace RouteService.Clients
{
    public class AuthServiceClient : IAuthServiceClient
    {
        private readonly HttpClient _httpClient;

        public AuthServiceClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(config["Services:AuthService"]);
        }

        public async Task<ProviderDTO> GetProviderByIdAsync(long providerId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/providers/{providerId}");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<ProviderDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling AuthService: {ex.Message}");
                return null;
            }
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
