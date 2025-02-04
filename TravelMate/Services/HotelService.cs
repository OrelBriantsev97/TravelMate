using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TravelMate.Services
{
    public static class HotelService
    {
        private const string ApiKey = "90a7e3bc24bfeb081ad2f4bab00f252e396a3b555b066ad6b71b4809d1d23a60"; 
        private const string ApiUrl = "https://serpapi.com/search?engine=google_hotels"; 

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
                        Rate = property["overall_rating"]?.Value<double>() ?? 0.0,
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

        public class HotelApiResponse
        {
            public List<HotelDetails> Data { get; set; }
        }

        public class HotelDetails
        {
            public string HotelName { get; set; }
            public string Address { get; set; }
            public string Phone { get; set; }
            public string CheckInTime { get; set; }
            public string CheckOutTime { get; set; }    
            public string LogoUrl { get; set; }
            public double Rate { get; set; }
            public string NearbyPlaces { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public List<string> Amenities { get; set; }
            public int HotelClass { get; set;}

            public ObservableCollection<string> HotelClassStars
            {
                get
                {
                    return new ObservableCollection<string>(Enumerable.Repeat("star_icon.png", HotelClass));
                }
            }


        }
    }
}
