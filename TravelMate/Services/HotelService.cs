using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TravelMate.Models;

namespace TravelMate.Services
{
    // Provides functionality for retrieving hotel details from the Google Hotels API via SerpApi.
    public static class HotelService
    {
        private const string ApiKey = ""; 
        private const string ApiUrl = "https://serpapi.com/search?engine=google_hotels";

        // Retrieves hotel details based on the specified name and booking dates.
        // <param name="name">
        // The hotel name or search query (e.g., "Hilton Tel Aviv"). </param>
        // <param name="checkIn">
        // Desired check‑in date as a string formatted "yyyy-MM-dd".</param>
        // <param name="checkOut">
        // Desired check‑out date as a string formatted "yyyy-MM-dd". </param>
        // <returns> / A <see cref="List{HotelDetails}"/> containing up to one matching hotel’s details,
        // or <c>null</c> if the response was empty or no hotels were found. </returns>
        public static async Task<List<HotelDetails>> GetHotelDetailsAsync(string name, string checkIn, string checkOut)
        {
            var client = new HttpClient();

            // Construct the request URL using the hotel name
            var requestUrl = $"{ApiUrl}"+
                 $"&q={Uri.EscapeDataString(name)}" +
                 $"&check_in_date={checkIn:yyyy-MM-dd}" +
                 $"&check_out_date={checkOut:yyyy-MM-dd}"+
                 $"&api_key={ApiKey}";

            var response = await client.GetStringAsync(requestUrl);
            Console.WriteLine($"[DEBUG] API Request URL: {requestUrl}");
            if (string.IsNullOrEmpty(response))
                return null;

            var jsonResponse = JObject.Parse(response);
            var hotelList = new List<HotelDetails>();
            JToken properties = jsonResponse["properties"];
            if (properties != null)
            {
                var hotelCount = 0;
                foreach (var property in properties)
                {
                    if (hotelCount == 1) 
                        break;

                    var hotel = new HotelDetails
                    {
                        HotelName = property["name"]?.ToString(),
                        LogoUrl = property["images"]?.FirstOrDefault()?["original_image"]?.ToString(),
                        Latitude = property["gps_coordinates"]?["latitude"].Value<double>() ?? 0.0,
                        Address = property["address"]?.ToString(),
                        Phone = property["phone"]?.ToString(),
                        CheckInTime = property["check_in_time"]?.ToString(),
                        CheckOutTime = property["check_out_time"]?.ToString(),
                        Longitude = property["gps_coordinates"]?["longitude"].Value<double>() ?? 0.0,
                        NearbyPlaces = string.Join(", ", property["nearby_places"]?.Select(np => np["name"].ToString())),
                        Amenities = property["amenities"]?.Select(a => a.ToString()).ToList() ?? new List<string>(),
                        HotelClass = int.TryParse(property["extracted_hotel_class"]?.ToString(), out var hotelClass) ? hotelClass : 1
                    };

                    hotelList.Add(hotel);
                    hotelCount++;
                }
            }
            else
            {
                // if reponse have only only one result
                var property = jsonResponse;
                var hotel = new HotelDetails
                {
                    HotelName = property["name"]?.ToString(),
                    LogoUrl = property["images"]?.FirstOrDefault()?["original_image"]?.ToString(),
                    Rate = property["overall_rating"]?.Value<double>() ?? 0.0,
                    Latitude = property["gps_coordinates"]?["latitude"].Value<double>() ?? 0.0,
                    Longitude = property["gps_coordinates"]?["longitude"].Value<double>() ?? 0.0,
                    Address = property["address"]?.ToString(),
                    Phone = property["phone"]?.ToString(),
                    CheckInTime = property["check_in_time"]?.ToString(),
                    CheckOutTime = property["check_out_time"]?.ToString(),
                    NearbyPlaces = string.Join(", ", property["nearby_places"]?.Select(np => np["name"].ToString())),
                    Amenities = property["amenities"]?.Select(a => a.ToString()).ToList() ?? new List<string>(),
                    HotelClass = int.TryParse(property["extracted_hotel_class"]?.ToString(), out var hotelClass) ? hotelClass : 1
                };

                hotelList.Add(hotel);
            }

            return hotelList;
        }

        // Represents the structure of a hotel-search response
        public class HotelApiResponse
        {
            public List<HotelDetails> Data { get; set; }
        }

    }
}
