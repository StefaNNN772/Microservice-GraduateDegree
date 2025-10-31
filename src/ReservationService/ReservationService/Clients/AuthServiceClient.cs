using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using ReservationService.Clients.Interfaces;
using ReservationService.DTOs;

namespace ReservationService.Clients
{
    public class AuthServiceClient : IAuthServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthServiceClient(HttpClient httpClient, IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(config["Services:AuthService"]);
            _httpContextAccessor = httpContextAccessor;
        }

        //private void CopyAuthHeader()
        //{
        //    var ctx = _httpContextAccessor.HttpContext;
        //    if (ctx != null && ctx.Request.Headers.TryGetValue("Authorization", out var authValues))
        //    {
        //        _httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authValues.First());
        //    }
        //    else
        //    {
        //        _httpClient.DefaultRequestHeaders.Authorization = null;
        //    }
        //}

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

        public async Task<UserDTO> GetUserByEmailAsync(string email)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/users/by-email/{email}");

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
