using ReservationService.DTOs;

namespace ReservationService.Clients.Interfaces
{
    public interface IAuthServiceClient
    {
        Task<UserDTO> GetUserByIdAsync(long userId);
    }
}
