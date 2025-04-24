using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TravelMate.Models; 

namespace TravelMate.Services
{
    // Provides methods to retrieve and cache places (attractions, restaurants, bars)
    /// for a given user and destination.
    public class PlaceService
    {
        private const string ApiKey = "90a7e3bc24bfeb081ad2f4bab00f252e396a3b555b066ad6b71b4809d1d23a60";
        private const string BaseUrl = "https://serpapi.com/search.json";

        // Gets a list of places for the specified user, destination, and type.
        // Attempts to load from the local database first; if none are found,
        // fetches from the external API, normalizes, stores them, and returns the list.
        // <param name="userId">The user identifier for whom to fetch places.</param>
        // <param name="destination">The destination city or location string.</param>
        // <param name="type">
        // The category of places to retrieve: "attractions", "restaurants", or "bars".
        // <returns> A <see cref="List{Place}"/> containing up to three places of the specified type.
   
        public static async Task<List<Place>> GetPlaces(int userId, string destination, string type)
        {
            // Try getting places from the DB
            var placesdb = await DatabaseHelper.GetPlaces(userId, type);
            if (placesdb.Any())
            {
                return placesdb;
            }

            // If not in DB, fetch from API
            var placesDetails = await FetchPlacesFromApi(destination, type);
            var placesToStore = ConvertToPlaceModel(placesDetails, userId, destination);

            // Normalize category (important step!)
            foreach (var place in placesToStore)
            {
                place.Category = type; // Only "attractions", "restaurants", or "bars"
            }

            // Store in DB
            await DatabaseHelper.AddPlaces(placesToStore);

            return placesToStore;
        }

        // Fetches raw place details from the external API for the given destination and category.
        // <param name="destination">The location to search within.</param>
        // <param name="type">The category of places ("attractions", "restaurants", "bars").</param>
        // <returns>A list of <see cref="PlaceDetails"/> parsed from the API response.</returns>
        private static async Task<List<PlaceDetails>> FetchPlacesFromApi(string destination,string type)
        {
            var places = new List<PlaceDetails>();
            var response = await GetApiResponse(type, destination);
            places.AddRange(ParsePlaces(response));

            return places;
        }

        // Sends an HTTP GET to SerpApi for local results of the specified category and destination.
        // <param name="category">The place type ("google_local").</param>
        // <param name="destination">The location query string.</param>
        // <returns>A <see cref="JObject"/> representing the parsed JSON response.</returns>
        private static async Task<JObject> GetApiResponse(string category, string destination)
        {
            string url = $"{BaseUrl}?engine=google_local&q={category}&location={destination}&apikey={ApiKey}";
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetStringAsync(url);
                return JObject.Parse(response);
            }
        }

        // Parses up to three place records from the JSON response.
        // <param name="response">The JSON object returned by the API.</param>
        // <returns>A list of <see cref="PlaceDetails"/> instances.</returns>
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

        /// Converts raw "PlaceDetails" into the application's Place model,
        /// adding user and destination information.
        /// <param name="detailsList" >List of PlaceDetails from the API.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="destination">The destination string.</param>
        /// <returns>A list of "Place" ready for database storage.</returns>
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

    // Represents the raw details of a place as returned by the external API.
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
