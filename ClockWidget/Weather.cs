using Microsoft.VisualBasic;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ClockWidget
{
    internal static class Weather
    {
        public record WeatherData
        {
            public string City;
            public string CountryCode;
            public double Temperature;
            public DateTime LastUpdatedAt;

            public WeatherData(string city, string countryCode, double temperature, DateTime lastUpdatedAt)
            {
                this.City = city;
                this.CountryCode = countryCode;
                this.Temperature = temperature;
                this.LastUpdatedAt = lastUpdatedAt;
            }
        }
        public static WeatherData CurrentData { get; private set; } = new("Unknown", "UN", 0.0, DateTime.Now);

        // Listeners
        public delegate void Listener(WeatherData data);

        private static readonly HashSet<Listener> listeners = [];
        
        public static void AddListener(Listener newListener)
        {
            _ = listeners.Add(newListener);
        }

        public static void RemoveListener(Listener oldListener)
        {
            _ = listeners.Remove(oldListener);
            if (listeners.Count == 0)
            {
                Stop();
            }
        }

        public static void RemoveAllListeners()
        {
            listeners.Clear();
            Stop();
        }

        private static void NotifyListeners(WeatherData current)
        {
            if (listeners.Count == 0)
            {
                return;
            }
            foreach (Listener handler in listeners)
            {
                handler(current);
            }
        }

        // Start() and Stop()
        private static bool canUpdateWeatherData = false;
        public static void Start()
        {
            if (canUpdateWeatherData)
            {
                return;
            }
            canUpdateWeatherData = true;
            _ = PeriodicallyUpdateData();
        }

        public static void Stop()
        {
            canUpdateWeatherData = false;
        }

#pragma warning disable IDE1006 // Naming Styles
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
#pragma warning restore IDE1006 // Naming Styles

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
            if (!canUpdateWeatherData)
            {
                return;
            }

            string ip = await GetIP();
            if (ip == ERROR_DETECTING_IP )
            {
                return;
            }

            GeoData? geoData = await GetGeoData(ip);
            if (geoData?.status != "success" || geoData?.lat == null || geoData?.lon == null || geoData?.city == null || geoData?.countryCode == null)
            {
                return;
            }

            MeteoData? meteoData = await GetMeteoData(geoData.lat, geoData.lon);
            if (meteoData?.current?.temperature_2m == null)
            {
                return;
            }

            CurrentData = new (geoData.city, geoData.countryCode, meteoData.current.temperature_2m, DateTime.Now);
            NotifyListeners(CurrentData);

            await Task.Delay(UPDATE_TIMEOUT);
            _ = PeriodicallyUpdateData();
        }
    }
}
