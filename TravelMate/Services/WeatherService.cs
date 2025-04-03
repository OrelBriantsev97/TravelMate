using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TravelMate.Services
{
    public static class WeatherService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string ApiKey = "R9E3B83PHVA6XFK8XDH8STPLF";
        private const string BaseUrl = "https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline";

        public static async Task<(double? temperature, string address)> GetWeather(string location)
        {
            string url = $"{BaseUrl}/{location}/?key={ApiKey}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Weather API Error: {response.StatusCode}");
                return (null, "Location Unknown");
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var weatherData = JsonConvert.DeserializeObject<WeatherResponse>(jsonResponse);

            if (weatherData == null || weatherData.Days == null || weatherData.Days.Count == 0)
            {
                return (null, "Location Unknown");
            }

            double temperatureF = weatherData.Days[0].Temp;
            double temperatureC = (temperatureF - 32) * 5 / 9;

            return (Math.Round(temperatureC, 1), weatherData.Address);
        }
    }

    public class WeatherResponse
    {
        [JsonProperty("address")]
        public string Address { get; set; } // Fetch Address

        [JsonProperty("days")]
        public List<DayWeather> Days { get; set; }
    }

    public class DayWeather
    {
        [JsonProperty("temp")]
        public double Temp { get; set; }
    }
}
