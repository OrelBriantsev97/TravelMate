using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TravelMate.Models;
using TravelMate.Services;

namespace TravelMate.ViewModels
{
    // ViewModel responsible for loading and exposing categorized place data
    // (attractions, restaurants, bars) for a given user and destination.
    public class MyPlacesViewModel : BindableObject
    {
        private readonly int userId;
        private readonly string destination;
        private bool isLoading;

        public ObservableCollection<Place> Attractions { get; set; } = new();
        public ObservableCollection<Place> Restaurants { get; set; } = new();
        public ObservableCollection<Place> Bars { get; set; } = new();

        // Indicates whether the ViewModel is currently loading data.
        public bool IsLoading
        {
            get => isLoading;
            set { isLoading = value; OnPropertyChanged(); }
        }

        public MyPlacesViewModel(int userId, string destination)
        {
            this.userId = userId;
            this.destination = destination;

            LoadPlaces();
        }

        // Asynchronously fetches lists of attractions, restaurants, and bars
        // from the PlaceService and updates the corresponding collections.
        private async Task LoadPlaces()
        {
            IsLoading = true;

            var attractions = await PlaceService.GetPlaces(userId, destination, "attractions");
            var restaurants = await PlaceService.GetPlaces(userId, destination, "restaurants");
            var bars = await PlaceService.GetPlaces(userId, destination, "bars");

            Attractions.Clear();
            Restaurants.Clear();
            Bars.Clear();

            foreach (var item in attractions) Attractions.Add(item);
            foreach (var item in restaurants) Restaurants.Add(item);
            foreach (var item in bars) Bars.Add(item);

            IsLoading = false;
        }

    }
}
