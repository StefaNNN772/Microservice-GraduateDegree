using Prometheus;
using System.Diagnostics.Metrics;

namespace ApiGateway.Services
{
    public class ProxyService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProxyService> _logger;
        private readonly ServiceDiscoveryService _serviceDiscovery;

        // Prometheus metrics
        private static readonly Counter ProxyRequestsTotal = Metrics
            .CreateCounter("gateway_proxy_requests_total", "Total number of proxy requests", new[] { "service", "method", "status" });

        private static readonly Histogram ProxyRequestDuration = Metrics
            .CreateHistogram("gateway_proxy_request_duration_seconds", "Duration of proxy requests", new[] { "service" });

        private static readonly Counter ProxyErrorsTotal = Metrics
            .CreateCounter("gateway_proxy_errors_total", "Total number of proxy errors", new[] { "service", "error_type" });

        public ProxyService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<ProxyService> logger,
            ServiceDiscoveryService serviceDiscovery)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
            _serviceDiscovery = serviceDiscovery;
        }

        public async Task ProxyRequestAsync(HttpContext context, string serviceUrlKey, string basePath)
        {
            var serviceName = GetServiceNameFromUrlKey(serviceUrlKey);
            using var timer = ProxyRequestDuration.WithLabels(serviceName).NewTimer();

            try
            {
                var serviceUrl = await _serviceDiscovery.GetServiceUrlAsync(serviceUrlKey);

                if (string.IsNullOrEmpty(serviceUrl))
                {
                    throw new InvalidOperationException($"Service URL not found for {serviceUrlKey}");
                }

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                // Konstruiši target URL
                var targetPath = context.Request.Path.ToString();
                if (!string.IsNullOrEmpty(basePath) && targetPath.StartsWith(basePath))
                {
                    targetPath = targetPath.Substring(basePath.Length);
                }

                var targetUrl = $"{serviceUrl.TrimEnd('/')}{targetPath}{context.Request.QueryString}";

                _logger.LogInformation($"Proxying {context.Request.Method} request to: {targetUrl}");

                // Kreiraj request message
                var requestMessage = new HttpRequestMessage();
                requestMessage.Method = new HttpMethod(context.Request.Method);
                requestMessage.RequestUri = new Uri(targetUrl);

                // Kopiraj headers
                foreach (var header in context.Request.Headers)
                {
                    if (!IsRestrictedHeader(header.Key))
                    {
                        try
                        {
                            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Failed to add header {header.Key}: {ex.Message}");
                        }
                    }
                }

                // Kopiraj body za POST/PUT/PATCH zahteve
                if (context.Request.ContentLength > 0 &&
                    (context.Request.Method == "POST" || context.Request.Method == "PUT" || context.Request.Method == "PATCH"))
                {
                    context.Request.Body.Position = 0;
                    var bodyContent = new StreamContent(context.Request.Body);

                    if (!string.IsNullOrEmpty(context.Request.ContentType))
                    {
                        bodyContent.Headers.TryAddWithoutValidation("Content-Type", context.Request.ContentType);
                    }

                    requestMessage.Content = bodyContent;
                }

                // Pošalji zahtev
                var response = await httpClient.SendAsync(requestMessage);

                // Record metrics
                ProxyRequestsTotal.WithLabels(serviceName, context.Request.Method, ((int)response.StatusCode).ToString()).Inc();

                // Kopiraj response status
                context.Response.StatusCode = (int)response.StatusCode;

                // Kopiraj response headers
                foreach (var header in response.Headers)
                {
                    if (!IsRestrictedResponseHeader(header.Key))
                    {
                        try
                        {
                            context.Response.Headers.TryAdd(header.Key, header.Value.ToArray());
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Failed to add response header {header.Key}: {ex.Message}");
                        }
                    }
                }

                foreach (var header in response.Content.Headers)
                {
                    if (!IsRestrictedResponseHeader(header.Key))
                    {
                        try
                        {
                            context.Response.Headers.TryAdd(header.Key, header.Value.ToArray());
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Failed to add content header {header.Key}: {ex.Message}");
                        }
                    }
                }

                // Kopiraj response body
                await response.Content.CopyToAsync(context.Response.Body);

                _logger.LogInformation($"Successfully proxied request to {serviceName} with status {response.StatusCode}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"HTTP error while proxying request to {serviceName}");
                ProxyErrorsTotal.WithLabels(serviceName, "http_error").Inc();

                context.Response.StatusCode = 503;
                await context.Response.WriteAsync($"Service temporarily unavailable: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, $"Timeout while proxying request to {serviceName}");
                ProxyErrorsTotal.WithLabels(serviceName, "timeout").Inc();

                context.Response.StatusCode = 504;
                await context.Response.WriteAsync("Gateway timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error while proxying request to {serviceName}");
                ProxyErrorsTotal.WithLabels(serviceName, "unexpected").Inc();

                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"Gateway Error: {ex.Message}");
            }
        }

        private static bool IsRestrictedHeader(string headerName)
        {
            var restrictedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Connection", "Content-Length", "Date", "Expect", "Host", "If-Modified-Since",
                "Range", "Transfer-Encoding", "Upgrade", "Via", "Warning", "Proxy-Connection"
            };

            return restrictedHeaders.Contains(headerName);
        }

        private static bool IsRestrictedResponseHeader(string headerName)
        {
            var restrictedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Transfer-Encoding", "Connection", "Upgrade", "Server"
            };

            return restrictedHeaders.Contains(headerName);
        }

        private static string GetServiceNameFromUrlKey(string serviceUrlKey)
        {
            return serviceUrlKey switch
            {
                "USER_SERVICE_URL" => "user-service",
                "ROUTE_SERVICE_URL" => "route-service",
                "RESERVATION_SERVICE_URL" => "reservation-service",
                _ => "unknown-service"
            };
        }
    }
}
