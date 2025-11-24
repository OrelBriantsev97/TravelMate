using System.Collections.ObjectModel;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace TravelMate.Services
{
    /// The FlightService class provides functionality for retrieving flight details from the Google Flights API using SERP API
    public static class FlightService
    {
        private const string ApiKey = "";
        private const string ApiUrl = "https://serpapi.com/search?engine=google_flights";

        /// Retrieves flight details based on the specified flight number, origin, destination, and departure time.
        /// <param name="flightNumber">The flight number for which details are to be fetched.</param>
        /// <param name="origin">The airport code of the origin airport.</param>
        /// <param name="destination">The airport code of the destination airport.</param>
        /// <param name="departureTime">The scheduled departure time of the flight.</param>
        /// <returns>A Task representing the asynchronous operation, with a <see cref="FlightDetails"/> object containing the flight details if found, or null if no matching flight is found.</returns>
        public static async Task<FlightDetails> GetFlightDetailsAsync(string flightNumber, string origin, string destination, DateTime departureTime)
        {
            var client = new HttpClient();
            string departureDate = departureTime.ToString("yyyy-MM-dd");

            var requestUrl = $"{ApiUrl}&apikey={ApiKey}&flight_number={flightNumber}&departure_id={origin}&arrival_id={destination}&outbound_date={departureDate}&type=2";
            Console.WriteLine($"Request URL: {requestUrl}");
            var response = await client.GetStringAsync(requestUrl);

            if (string.IsNullOrEmpty(response))
                return null;

            // Deserialize the JSON response into a FlightApiResponse object.
            var flightResponse = JsonConvert.DeserializeObject<FlightApiResponse>(response);

            if (flightResponse?.BestFlights == null || !flightResponse.BestFlights.Any())
                return null;


            //// Find the first flight matching the provided flight number.
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

    /// Represents the API response containing flight data.
    public class FlightApiResponse
    {
        [JsonProperty("best_flights")]
        public List<BestFlight> BestFlights { get; set; }
    }

    //Represents a collection of flights in a specific flight listing.
    public class BestFlight
    {
        [JsonProperty("flights")]
        public List<FlightDetails> Flights { get; set; }
    }

    /// Represents the details of a flight
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

    //Represents details of an airport, including its name, ID, and time-related information.
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
