using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TravelMate.Services
{
    // Provides functionality to retrieve current weather information for a specified location
    // using the Visual Crossing Weather API.
    public static class WeatherService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string ApiKey = "";
        private const string BaseUrl = "https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline";

        // Retrieves the current day's temperature and resolved address for a given location.
        /// <param name="location">The location query (e.g., "Tel Aviv,Israel").</param>
        /// <returns> A tuple of (<c>double?</c> temperature in Celsius rounded to one decimal place,
        /// <c>string</c> resolved address). If the API call fails or data is unavailable, returns (null, "Location Unknown").
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
    // Represents the JSON response from the Visual Crossing Weather API.
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
