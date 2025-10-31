using System.Globalization;
using System.Text.Json;

namespace RouteService.Helpers
{
    public class MapAPI
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "5b3ce3597851110001cf624806b1c4ec9694495bb916687a8eb4dc19";

        public MapAPI(HttpClient httpClient)
        {
            _httpClient = httpClient;
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

        /*POKUSAJ SAMO PREKO MAPAPI*/
        //    public async Task<List<(string Stanica, double DistanceKm, TimeSpan Departure, TimeSpan Arrival)>>
        //GetRouteDetailsAsync(List<string> stanice, TimeSpan departureTime)
        //    {
        //        // 1. Geokodiranje svih stanica
        //        var coords = new List<(double Lon, double Lat, string Name)>();

        //        foreach (var stanica in stanice)
        //        {
        //            var url = $"https://api.openrouteservice.org/geocode/search?api_key={_apiKey}&text={Uri.EscapeDataString(stanica)}&size=1";
        //            var resMsg = await _httpClient.GetAsync(url);
        //            resMsg.EnsureSuccessStatusCode();
        //            var res = await resMsg.Content.ReadFromJsonAsync<JsonDocument>();
        //            if (res == null)
        //                throw new Exception("Geokodiranje nije uspelo");

        //            var feature = res.RootElement.GetProperty("features")[0];
        //            var geometry = feature.GetProperty("geometry");
        //            var coordinates = geometry.GetProperty("coordinates");
        //            double lon = coordinates[0].GetDouble();
        //            double lat = coordinates[1].GetDouble();
        //            coords.Add((lon, lat, stanica));
        //        }

        //        // 2. Pripremi telo za POST directions
        //        var coordsArray = coords.Select(c => new[] { c.Lon, c.Lat }).ToArray();
        //        var body = new { coordinates = coordsArray };

        //        var content = new StringContent(JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json");

        //        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openrouteservice.org/v2/directions/driving-hgv");
        //        request.Content = content;
        //        request.Headers.Add("Authorization", _apiKey); // OVDJE JE KLJUČ!

        //        var routeMsg = await _httpClient.SendAsync(request);
        //        routeMsg.EnsureSuccessStatusCode();
        //        var routeDoc = await routeMsg.Content.ReadFromJsonAsync<JsonDocument>();

        //        // 3. Parsiraj segmente
        //        if (!routeDoc.RootElement.TryGetProperty("routes", out var routes) || routes.ValueKind != JsonValueKind.Array || routes.GetArrayLength() == 0)
        //            throw new Exception("OpenRouteService API did not return any routes. Full response: " + routeDoc.RootElement.ToString());

        //        var route = routes[0];

        //        // summary: ukupna dužina/vreme
        //        var summary = route.GetProperty("summary");
        //        double totalDistance = summary.GetProperty("distance").GetDouble();
        //        double totalDuration = summary.GetProperty("duration").GetDouble();

        //        // segmenti: svaki segment između stanica
        //        var segments = route.GetProperty("segments");

        //        var result = new List<(string Stanica, double DistanceKm, TimeSpan Departure, TimeSpan Arrival)>();
        //        TimeSpan currentDeparture = departureTime;
        //        TimeSpan currentArrival = departureTime;

        //        // Prva stanica
        //        result.Add((stanice[0], 0, currentDeparture, currentDeparture));

        //        for (int i = 0; i < segments.GetArrayLength(); i++)
        //        {
        //            var seg = segments[i];
        //            double distance = seg.GetProperty("distance").GetDouble() / 1000.0; // km
        //            double duration = seg.GetProperty("duration").GetDouble(); // sekunde
        //            var ts = TimeSpan.FromSeconds(duration);

        //            currentDeparture = result[i].Arrival;
        //            currentArrival = currentDeparture + ts;
        //            result.Add((stanice[i + 1], distance, currentDeparture, currentArrival));
        //        }

        //        return result;
        //    }
    }
}
