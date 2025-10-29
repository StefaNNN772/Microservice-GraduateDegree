using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ApiGateway.Middleware
{
    public class JwtValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtValidationMiddleware> _logger;

        public JwtValidationMiddleware(RequestDelegate next, ILogger<JwtValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = ExtractTokenFromHeader(context.Request);

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jsonToken = tokenHandler.ReadJwtToken(token);

                    var userId = jsonToken.Claims.FirstOrDefault(x => x.Type == "userId" || x.Type == "nameid")?.Value;
                    var userEmail = jsonToken.Claims.FirstOrDefault(x => x.Type == "email" || x.Type == ClaimTypes.Email)?.Value;
                    var userRole = jsonToken.Claims.FirstOrDefault(x => x.Type == "role" || x.Type == ClaimTypes.Role)?.Value;

                    context.Items["UserId"] = userId;
                    context.Items["UserEmail"] = userEmail;
                    context.Items["UserRole"] = userRole;
                    context.Items["Token"] = token;

                    _logger.LogInformation($"JWT token validated for user: {userEmail} (ID: {userId})");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Invalid JWT token: {ex.Message}");
                }
            }

            await _next(context);
        }

        private string? ExtractTokenFromHeader(HttpRequest request)
        {
            var authHeader = request.Headers["Authorization"].FirstOrDefault();
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }
            return null;
        }
    }
}
