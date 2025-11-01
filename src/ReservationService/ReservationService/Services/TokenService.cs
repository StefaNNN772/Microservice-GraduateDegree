using Microsoft.IdentityModel.Tokens;
using ReservationService.DTOs;
using ReservationService.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ReservationService.Services
{
    public class TokenService
    {
        private string APP_NAME = "serbiaBus";  // Izdavač tokena
        private string SECRET = "ThisIsAVeryLongSecretKeyThatIsAtLeast64BytesLongForHmacSha512ABCDEFGHIJK";  // Tajna za potpisivanje
        private int EXPIRES_IN = 180000;  // Period važenja tokena u sekundama (50 sati)
        private string AUTH_HEADER = "Authorization";

        private static readonly string AUDIENCE_WEB = "web";
        private static readonly string SIGNATURE_ALGORITHM = SecurityAlgorithms.HmacSha512;

        public string GenerateToken(string username, long userId, UserRole userRole)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Iss, APP_NAME),
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim("id", userId.ToString()),
            new Claim("role", userRole.ToString()),
            new Claim(JwtRegisteredClaimNames.Aud, AUDIENCE_WEB),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(), ClaimValueTypes.DateTime),
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SECRET));
            var creds = new SigningCredentials(key, SIGNATURE_ALGORITHM);

            var token = new JwtSecurityToken(
                issuer: APP_NAME,
                audience: AUDIENCE_WEB,
                claims: claims,
                expires: GenerateExpirationDate(),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private DateTime GenerateExpirationDate()
        {
            return DateTime.UtcNow.AddSeconds(EXPIRES_IN);
        }

        public int GetExpiresIn()
        {
            return EXPIRES_IN;
        }

        public UserTokenDTO DecodeToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
            {
                throw new ArgumentException("Invalid token");
            }

            var jwtToken = handler.ReadJwtToken(token);

            var userId = Int32.Parse(jwtToken?.Claims?.FirstOrDefault(c => c.Type == "id")?.Value);
            var email = jwtToken?.Claims?.FirstOrDefault(c => c.Type == "sub")?.Value;

            return new UserTokenDTO { Id = userId, Email = email };
        }
    }
}
