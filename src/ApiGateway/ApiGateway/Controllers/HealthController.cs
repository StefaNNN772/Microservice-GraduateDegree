using ApiGateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ServiceDiscoveryService _serviceDiscovery;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            ServiceDiscoveryService serviceDiscovery,
            IHttpClientFactory httpClientFactory,
            ILogger<HealthController> logger)
        {
            _serviceDiscovery = serviceDiscovery;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var services = new Dictionary<string, object>();

            foreach (var serviceKey in _serviceDiscovery.GetRegisteredServices())
            {
                var serviceUrl = await _serviceDiscovery.GetServiceUrlAsync(serviceKey);
                services[serviceKey] = await CheckServiceHealth(serviceUrl);
            }

            var overallStatus = services.Values.All(s => ((dynamic)s).status == "Healthy") ? "Healthy" : "Degraded";

            var result = new
            {
                Status = overallStatus,
                Timestamp = DateTime.UtcNow,
                Services = services,
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            };

            return Ok(result);
        }

        [HttpGet("services")]
        public async Task<IActionResult> GetServicesHealth()
        {
            var services = new Dictionary<string, object>();

            foreach (var serviceKey in _serviceDiscovery.GetRegisteredServices())
            {
                var serviceUrl = await _serviceDiscovery.GetServiceUrlAsync(serviceKey);
                services[serviceKey] = await CheckServiceHealth(serviceUrl);
            }

            return Ok(services);
        }

        private async Task<object> CheckServiceHealth(string? serviceUrl)
        {
            if (string.IsNullOrEmpty(serviceUrl))
            {
                return new { status = "Not Configured", responseTime = 0 };
            }

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);

                var response = await httpClient.GetAsync($"{serviceUrl}/health");
                stopwatch.Stop();

                return new
                {
                    status = response.IsSuccessStatusCode ? "Healthy" : "Unhealthy",
                    responseTime = stopwatch.ElapsedMilliseconds,
                    statusCode = (int)response.StatusCode,
                    url = serviceUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Health check failed for {serviceUrl}: {ex.Message}");
                return new
                {
                    status = "Unhealthy",
                    responseTime = 0,
                    error = ex.Message,
                    url = serviceUrl
                };
            }
        }
    }
}
