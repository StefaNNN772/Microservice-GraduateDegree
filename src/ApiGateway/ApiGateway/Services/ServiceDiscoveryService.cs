namespace ApiGateway.Services
{
    public class ServiceDiscoveryService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServiceDiscoveryService> _logger;
        private readonly Dictionary<string, string> _serviceUrls;

        public ServiceDiscoveryService(IConfiguration configuration, ILogger<ServiceDiscoveryService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _serviceUrls = new Dictionary<string, string>();

            InitializeServiceUrls();
        }

        private void InitializeServiceUrls()
        {
            var services = new[]
            {
                "USER_SERVICE_URL",
                "ROUTE_SERVICE_URL",
                "RESERVATION_SERVICE_URL"
            };

            foreach (var service in services)
            {
                var url = Environment.GetEnvironmentVariable(service) ?? _configuration[service];
                if (!string.IsNullOrEmpty(url))
                {
                    _serviceUrls[service] = url;
                    _logger.LogInformation($"Registered service {service}: {url}");
                }
                else
                {
                    _logger.LogWarning($"Service URL not found for {service}");
                }
            }
        }

        public async Task<string?> GetServiceUrlAsync(string serviceKey)
        {
            if (_serviceUrls.TryGetValue(serviceKey, out var url))
            {
                // Ovde možete dodati logiku za health check servisa
                if (await IsServiceHealthyAsync(url))
                {
                    return url;
                }
                else
                {
                    _logger.LogWarning($"Service {serviceKey} is not healthy at {url}");
                    return null;
                }
            }

            _logger.LogError($"Service URL not found for key: {serviceKey}");
            return null;
        }

        private async Task<bool> IsServiceHealthyAsync(string serviceUrl)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);

                var response = await httpClient.GetAsync($"{serviceUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Health check failed for {serviceUrl}: {ex.Message}");
                return false; // Assume service is available if health check fails
            }
        }

        public IEnumerable<string> GetRegisteredServices()
        {
            return _serviceUrls.Keys;
        }
    }
}
