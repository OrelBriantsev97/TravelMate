using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TravelMate.Services
{
    public static class FlightService
    {
        private const string ApiKey = "90a7e3bc24bfeb081ad2f4bab00f252e396a3b555b066ad6b71b4809d1d23a60"; 
        private const string ApiUrl = "https://serpapi.com/search?engine=google_flights";

        public static async Task<FlightDetails> GetFlightDetailsAsync(string flightNumber,string origin , string destination ,DateTime departureTime)
        {
            var client = new HttpClient();
            string departureDate = departureTime.ToString("yyyy-MM-dd");

            var requestUrl = $"{ApiUrl}&apikey={ApiKey}&flight_number={flightNumber}&departure_id={origin}&arrival_id={destination}&outbound_date={departureDate}&type=2";

            Console.WriteLine($"[DEBUG] API Request URL: {requestUrl}");
            // Sending the HTTP GET request to the API
            var response = await client.GetStringAsync(requestUrl);

            Console.WriteLine($"[DEBUG] respons{response}");
            if (string.IsNullOrEmpty(response))
                return null;

            // Deserializing the response into FlightApiResponse object
            var flightResponse = JsonConvert.DeserializeObject<FlightApiResponse>(response);

            if (flightResponse?.BestFlights == null || !flightResponse.BestFlights.Any())
                return null;

            foreach (var flight in flightResponse.BestFlights.SelectMany(f => f.Flights))
            {
                Console.WriteLine(flight.FlightNumber);  // Log flight numbers
            }

            var matchingFlight = flightResponse.BestFlights.SelectMany(f => f.Flights)
                                .FirstOrDefault(f => !string.IsNullOrEmpty(f.FlightNumber) &&
                         f.FlightNumber.Replace(" ", "").Equals(flightNumber.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));

            if (matchingFlight == null)
            {
                return null;
            }

            return matchingFlight;
        }
    }

    // Response model to deserialize API response
    public class FlightApiResponse
    {
        [JsonProperty("best_flights")]
        public List<BestFlight> BestFlights { get; set; }
    }

    public class BestFlight
    {
        [JsonProperty("flights")]
        public List<FlightDetails> Flights { get; set; }
    }
    public class FlightDetails
    {
        [JsonProperty("flight_number")]
        public string FlightNumber { get; set; }

        [JsonProperty("airline")]
        public string Airline { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("departure_airport")]
        public AirportDetails Departure { get; set; }

        [JsonProperty("arrival_airport")]
        public AirportDetails Arrival { get; set; }
    }

    public class AirportDetails
    {
        [JsonProperty("name")]
        public string AirportName { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }
    }
}
