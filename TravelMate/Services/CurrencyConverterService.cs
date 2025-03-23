using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TravelMate.Services
{
    public class CurrencyConverterService
    {
        private const string ApiKey = "047873fe9f6621de7e117e0e";
        private const string BaseUrl = "https://v6.exchangerate-api.com/v6/";

        public static async Task<Dictionary<string, decimal>> GetExchangeRates(string baseCurrency)
        {
            string url = $"{BaseUrl}{ApiKey}/latest/{baseCurrency}";

            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<ExchangeRateResponse>(jsonResponse);

            return data?.ConversionRates;
        }
    }

    public class ExchangeRateResponse
    {
        [JsonProperty("conversion_rates")]
        public Dictionary<string, decimal> ConversionRates { get; set; }
    }
}
