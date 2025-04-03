using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TravelMate.Models; 

namespace TravelMate.Services
{
    public class PlaceService
    {
        private const string ApiKey = "90a7e3bc24bfeb081ad2f4bab00f252e396a3b555b066ad6b71b4809d1d23a60";
        private const string BaseUrl = "https://serpapi.com/search.json";

        public static async Task<List<Place>> GetPlaces(int userId, string destination,string type)
        {
            var placesdb = await DatabaseHelper.GetPlaces(userId, type);
            if (placesdb.Any())
            {
                return placesdb; 
            }
            var placesDetails = await FetchPlacesFromApi(destination,type);

            // Convert PlaceDetails to Place model for database insertion
            var placesToStore = ConvertToPlaceModel(placesDetails, userId, destination);

            // Store places in database
            DatabaseHelper.AddPlaces(placesToStore);

            return placesToStore;
        }

        private static async Task<List<PlaceDetails>> FetchPlacesFromApi(string destination,string type)
        {
            var places = new List<PlaceDetails>();
            var response = await GetApiResponse(type, destination);
            places.AddRange(ParsePlaces(response));

            return places;
        }

        private static async Task<JObject> GetApiResponse(string category, string destination)
        {
            string url = $"{BaseUrl}?engine=google_local&q={category}&location={destination}&apikey={ApiKey}";
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetStringAsync(url);
                return JObject.Parse(response);
            }
        }

        private static List<PlaceDetails> ParsePlaces(JObject response)
        {
            var places = new List<PlaceDetails>();
            var localResults = response["local_results"];
            int count = 0;

            foreach (var place in localResults)
            {
                if (count >= 3)
                {
                    break;
                }
                places.Add(new PlaceDetails
                {
                    Name = place["title"]?.ToString(),
                    Category = place["type"]?.ToString(),
                    Address = place["address"]?.ToString(),
                    Rating = place["rating"]!=null ? place["rating"].ToObject<double>(): 0.0,
                    OpeningHours = place["hours"]?.ToString() ?? "Not Avialble",
                    Thumbnail = place["thumbnail"]?.ToString(),
                });
                count++;
            }

            return places;
        }

        private static List<Place> ConvertToPlaceModel(List<PlaceDetails> placeDetailsList, int userId, string destination)
        {
            return placeDetailsList.Select(pd => new Place
            {
                UserId = userId,
                Destination = destination,
                Name = pd.Name,
                Category = pd.Category,
                Address = pd.Address,
                Rating = pd.Rating,
                OpeningHours = pd.OpeningHours,
                Thumbnail = pd.Thumbnail
            }).ToList();
        }
    }

    public class PlaceDetails
    {
        public string Name { get; set; }
        public string Category { get; set; } // Restaurant, Attraction, Nightlife
        public string Address { get; set; }
        public double Rating { get; set; }
        public string OpeningHours { get; set; }
        public string Thumbnail { get; set; }
    }
}
