using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repository;

namespace AuthService.Services
{
    public class ProviderService
    {
        private readonly ProviderRepository _providerRepository;

        public ProviderService(ProviderRepository providerRepository)
        {
            _providerRepository = providerRepository;
        }


        public async Task<Provider> FindProviderByEmailOrNameAsync(string email, string name)
        {
            var provider = await _providerRepository.FindByEmailOrNameAsync(email, name);

            return provider;
        }

        public async Task<Provider> FindProviderAsync(string email)
        {
            var provider = await _providerRepository.FindByEmailAsync(email);

            if (provider != null)
            {
                return provider;
            }

            return null;
        }

        public async Task<Provider?> CreateProviderAsync(AddProviderDTO provider)
        {
            var newProvider = new Provider
            {
                Email = provider.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(provider.Password),
                Name = provider.Name,
                Address = provider.Address,
                PhoneNumber = provider.PhoneNumber,
                Role = UserRole.TransportProvider
            };

            newProvider = await _providerRepository.CreateProviderAsync(newProvider);

            return newProvider;
        }
    }
}
