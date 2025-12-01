using AuthService.Models;
using AuthService.Repository;

namespace AuthService.Services
{
    public class AuthenticationManager
    {
        private readonly UserRepository _userRepository;
        private readonly ProviderRepository _providerRepository;


        public AuthenticationManager(UserRepository userRepository, ProviderRepository providerRepository)
        {
            _userRepository = userRepository;
            _providerRepository = providerRepository;
        }

        public async Task<bool> AuthenticateAsync(string email, string password)
        {
            var user = await _userRepository.FindByEmailAsync(email);
            Provider provider = null;

            if (user == null)
            {
                provider = await _providerRepository.FindByEmailAsync(email);
                if (provider == null)
                {
                    return false;
                }
            }

            if (user == null && BCrypt.Net.BCrypt.Verify(password, provider.PasswordHash))
            {
                return true;
            }
            else if (provider == null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return true;
            }

            return true;
        }
    }
}
