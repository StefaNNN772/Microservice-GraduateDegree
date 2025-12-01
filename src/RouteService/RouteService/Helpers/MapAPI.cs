using System.Globalization;
using System.Text.Json;

namespace RouteService.Helpers
{
    public class MapAPI
    {
        private readonly HttpClient _httpClient;
        //private readonly string _apiKey = "5b3ce3597851110001cf624806b1c4ec9694495bb916687a8eb4dc19";
        private readonly string _apiKey;

        public MapAPI(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["MapAPI:ApiKey"];
        }

        public async Task<Tuple<double, TimeSpan>> GetDistanceAndDurationAsync(string departure, string arrival)
        {
            try
            {
                // Dobijanje koordinata za polazak
                var departureUrl = $"https://api.openrouteservice.org/geocode/search?api_key={_apiKey}&text={Uri.EscapeDataString(departure)}&size=1";
                var departureResponseMessage = await _httpClient.GetAsync(departureUrl);
                departureResponseMessage.EnsureSuccessStatusCode();

                var departureResponse = await departureResponseMessage.Content.ReadFromJsonAsync<JsonDocument>();

                // Dobijanje koordinata za dolazak
                var arrivalUrl = $"https://api.openrouteservice.org/geocode/search?api_key={_apiKey}&text={Uri.EscapeDataString(arrival)}&size=1";
                var arrivalResponseMessage = await _httpClient.GetAsync(arrivalUrl);
                arrivalResponseMessage.EnsureSuccessStatusCode();

                var arrivalResponse = await arrivalResponseMessage.Content.ReadFromJsonAsync<JsonDocument>();

                if (departureResponse == null || arrivalResponse == null)
                    throw new Exception("Invalid API response");

                // Parsiranje koordinata
                JsonElement departureFeature = departureResponse.RootElement.GetProperty("features");
                JsonElement firstStartElement = departureFeature[0];
                var startGeometry = firstStartElement.GetProperty("geometry");
                var startCoordinates = startGeometry.GetProperty("coordinates");
                string startLon = startCoordinates[0].GetDouble().ToString(CultureInfo.InvariantCulture);
                string startLat = startCoordinates[1].GetDouble().ToString(CultureInfo.InvariantCulture);

                JsonElement arrivalFeature = arrivalResponse.RootElement.GetProperty("features");
                JsonElement firstEndElement = arrivalFeature[0];
                var endGeometry = firstEndElement.GetProperty("geometry");
                var endCoordinates = endGeometry.GetProperty("coordinates");
                string endLon = endCoordinates[0].GetDouble().ToString(CultureInfo.InvariantCulture);
                string endLat = endCoordinates[1].GetDouble().ToString(CultureInfo.InvariantCulture);

                // Dobijanje trajanja puta
                var routeUrl = $"https://api.openrouteservice.org/v2/directions/driving-hgv?api_key={_apiKey}&start={startLon},{startLat}&end={endLon},{endLat}";
                var routeResponseMessage = await _httpClient.GetAsync(routeUrl);

                var routeResponse = await routeResponseMessage.Content.ReadFromJsonAsync<JsonDocument>();
                JsonElement routeFeature = routeResponse.RootElement.GetProperty("features");
                JsonElement root = routeFeature[0];
                JsonElement properties = root.GetProperty("properties");
                JsonElement segments = properties.GetProperty("segments");
                JsonElement firstSegment = segments[0];
                double distance = firstSegment.GetProperty("distance").GetDouble();
                double duration = firstSegment.GetProperty("duration").GetDouble();

                int hours = (int)(duration / 3600);
                int minutes = (int)((duration % 3600) / 60);
                TimeSpan fullDuration = new TimeSpan(hours, minutes, 0);

                return new Tuple<double, TimeSpan>(distance / 1000, fullDuration);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"API request failed: {ex.Message}");
            }
            catch (JsonException ex)
            {
                throw new Exception($"Error parsing JSON response: {ex.Message}");
            }
        }

        public async Task<List<Coords>> GetCoords(List<string> stations)
        {
            List<Coords> coords = new List<Coords>(stations.Count);

            foreach (var station in stations)
            {
                var url = $"https://api.openrouteservice.org/geocode/search?api_key={_apiKey}&text={Uri.EscapeDataString(station)}&size=1";
                var resultMessage = await _httpClient.GetAsync(url);
                resultMessage.EnsureSuccessStatusCode();

                var result = await resultMessage.Content.ReadFromJsonAsync<JsonDocument>();

                if (result == null)
                {
                    throw new Exception("Failed geocoding.");
                }

                var feature = result.RootElement.GetProperty("features")[0];
                var geometry = feature.GetProperty("geometry");
                var coordinates = geometry.GetProperty("coordinates");
                double lon = coordinates[0].GetDouble();
                double lat = coordinates[1].GetDouble();
                coords.Add(new Coords { Lon = lon, Lat = lat, Station = station });
            }

            return coords;
        }

        public async Task<Tuple<double, TimeSpan>> GetDistanceAndDuration(List<Coords> coords)
        {
            var coordsArray = coords.Select(c => new[] { c.Lon, c.Lat }).ToArray();
            var body = new { coordinates = coordsArray };

            var content = new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openrouteservice.org/v2/directions/driving-hgv");
            request.Content = content;
            request.Headers.Add("Authorization", _apiKey);
            var routeResponseMessage = await _httpClient.SendAsync(request);
            routeResponseMessage.EnsureSuccessStatusCode();

            var routeResponse = await routeResponseMessage.Content.ReadFromJsonAsync<JsonDocument>();
            JsonElement routeFeature = routeResponse.RootElement.GetProperty("routes");
            JsonElement root = routeFeature[0];
            JsonElement summary = root.GetProperty("summary");
            double distance = summary.GetProperty("distance").GetDouble();
            double duration = summary.GetProperty("duration").GetDouble();

            int hours = (int)(duration / 3600);
            int minutes = (int)((duration % 3600) / 60);
            TimeSpan fullDuration = new TimeSpan(hours, minutes, 0);

            return new Tuple<double, TimeSpan>(distance / 1000, fullDuration);
        }
    }
}
