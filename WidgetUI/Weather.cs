using System.Net.Http;
using System.Net.Http.Json;

namespace WidgetUI
{
    internal static class Weather
    {
        public record WeatherData(string City, string CountryCode, double Temperature, DateTime LastUpdatedAt)
        {
            public readonly string City = City;
            public readonly string CountryCode = CountryCode;
            public readonly double Temperature = Temperature;
            public DateTime LastUpdatedAt = LastUpdatedAt;
        }
        public static WeatherData CurrentData { get; private set; } = new("Unknown", "UN", 0.0, DateTime.Now);

        // Listeners
        public delegate void Listener(WeatherData data);

        private static readonly HashSet<Listener> Listeners = [];

        public static void AddListener(Listener newListener)
        {
            _ = Listeners.Add(newListener);
        }

        public static void RemoveListener(Listener oldListener)
        {
            _ = Listeners.Remove(oldListener);
            if (Listeners.Count == 0)
            {
                Stop();
            }
        }

        public static void RemoveAllListeners()
        {
            Listeners.Clear();
            Stop();
        }

        private static void NotifyListeners(WeatherData current)
        {
            if (Listeners.Count == 0)
            {
                return;
            }
            foreach (var handler in Listeners)
            {
                handler(current);
            }
        }

        // Start() and Stop()
        private static bool _canUpdateWeatherData = false;
        public static void Start()
        {
            if (_canUpdateWeatherData)
            {
                return;
            }
            _canUpdateWeatherData = true;
            _ = PeriodicallyUpdateData();
        }

        public static void Stop()
        {
            _canUpdateWeatherData = false;
        }

        private record GeoData
        {
            public required string city { get; set; }
            public required string countryCode { get; set; }
            public required double lat { get; set; }
            public required double lon { get; set; }
            public required string status { get; set; }
        }

        private record MeteoDataCurrent
        {
            public required float temperature_2m { get; set; }
        }

        private record MeteoData
        {
            public required MeteoDataCurrent current { get; set; }
        }

        private const string ERROR_DETECTING_IP = "127.0.0.1";
        private static readonly HttpClient client = new();

        private static async Task<string> GetIP()
        {
            try
            {
                return await client.GetStringAsync("https://api.ipify.org");
            }
            catch (HttpRequestException)
            {
                return ERROR_DETECTING_IP;
            }
        }

        private static async Task<GeoData?> GetGeoData(string IP)
        {
            return await client.GetFromJsonAsync<GeoData>($"http://ip-api.com/json/{IP}?fields=status,countryCode,city,lat,lon");
        }

        private static async Task<MeteoData?> GetMeteoData(double latitude, double longitude)
        {
            return await client.GetFromJsonAsync<MeteoData>($"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m");
        }

        private const int UPDATE_TIMEOUT = 1800000; // 30 min.
        private static async Task PeriodicallyUpdateData()
        {
            if (!_canUpdateWeatherData)
            {
                return;
            }

            try
            {
                var ip = await GetIP();
                if (ip == ERROR_DETECTING_IP)
                {
                    return;
                }

                var geoData = await GetGeoData(ip);
                if (geoData?.status != "success" || geoData?.lat == null || geoData?.lon == null || geoData?.city == null || geoData?.countryCode == null)
                {
                    return;
                }

                MeteoData? meteoData = await GetMeteoData(geoData.lat, geoData.lon);
                if (meteoData?.current?.temperature_2m == null)
                {
                    return;
                }

                CurrentData = new(geoData.city, geoData.countryCode, meteoData.current.temperature_2m, DateTime.Now);
                NotifyListeners(CurrentData);
            }
            catch (Exception)
            {
                // Do nothing
            }



            await Task.Delay(UPDATE_TIMEOUT);
            _ = PeriodicallyUpdateData();
        }
    }
}
