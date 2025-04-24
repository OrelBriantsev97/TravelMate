using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TravelMate.Services
{
    // Provides functionality to retrieve latest currency exchange rates
    // from the ExchangeRate-API for a given base currency.
    public class CurrencyConverterService
    {
        private const string ApiKey = "047873fe9f6621de7e117e0e";
        private const string BaseUrl = "https://v6.exchangerate-api.com/v6/";

        // Retrieves a dictionary of exchange rates relative to the specified base currency.
        // <param name="baseCurrency">
        // The three-letter ISO code of the base currency (e.g., "USD", "EUR").
        // </param>
        // <returns>  A "Dictionary{TKey, TValue}"  where keys are currency codes
        // and values are the conversion rates from <param name="baseCurrency"/>.
        // Returns <c>null</c> if the HTTP request fails or the response cannot be parsed.
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
