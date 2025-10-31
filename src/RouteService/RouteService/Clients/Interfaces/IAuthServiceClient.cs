using RouteService.DTOs;

namespace RouteService.Clients.Interfaces
{
    public interface IAuthServiceClient
    {
        Task<UserDTO> GetUserByIdAsync(long userId);
        Task<ProviderDTO> GetProviderByIdAsync(long providerId);
    }
}
