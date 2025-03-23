using Mapsui.Layers;
using Mapsui.Tiling.Layers;
using Mapsui.Tiling;
using Mapsui.Projections;
using Mapsui.UI.Maui;
using System.Threading.Tasks;
using TravelMate.Models;
using BruTile.Predefined;
using BruTile.Web;
using Mapsui;
using Mapsui.Styles;
using NetTopologySuite.Index.HPRtree;

namespace TravelMate
{
    public partial class MapPage : ContentPage
    {
        private readonly int userId;
        private List<Hotel> hotels = new();
        private List<Flight> flights = new();

        public MapPage(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            SetupMap();
        }

        private async Task SetupMap()
        {
            var tileSource = new HttpTileSource(
                new GlobalSphericalMercator(),
                "https://tile.openstreetmap.org/{z}/{x}/{y}.png");

            var tileLayer = new TileLayer(tileSource)
            {
                Name = "OpenStreetMap"
            };

            mapView.Map = new Mapsui.Map();
            mapView.Map.Layers.Add(tileLayer);
            mapView.Map.Navigator.ZoomTo(100);
            
            flights = await DatabaseHelper.GetFlightsByUserId(userId);
            hotels = await DatabaseHelper.GetHotelsByUserId(userId);

            if (flights.Any() || hotels.Any())
            {
                MainThread.BeginInvokeOnMainThread(LoadTripOverview);
                MainThread.BeginInvokeOnMainThread(LoadHotelPins);
                LoadHotelPins();
            }
            else
            {
                await DisplayAlert("No Data", "No flights or hotels found.", "OK");
            }
        }

        private void LoadTripOverview()
        {
            var tripEvents = new List<(DateTime Date, string Description)>();

            foreach (var flight in flights)
            {
                if (DateTime.TryParse(flight.DepartureDate, out DateTime flightDate))
                {
                    tripEvents.Add((flightDate, $"✈️ Flight {flight.FlightNumber} - {flightDate:MMM dd, yyyy}"));
                }
            }

            int hotelNumber = 1;
            foreach (var hotel in hotels)
            {
                if (DateTime.TryParse(hotel.CheckInDate, out DateTime checkInDate))
                {
                    tripEvents.Add((checkInDate, $"🏨 {hotelNumber} → {hotel.HotelName} (Check-in: {checkInDate:MMM dd, yyyy})"));
                    hotelNumber++;
                }
            }

            var sortedTripEvents = tripEvents.OrderBy(e => e.Date).ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                tripOverviewCollectionView.ItemsSource = sortedTripEvents;
            });
        }



        private void LoadHotelPins()
        {
            if (!hotels.Any()) return;

            // Clear previous pins
            mapView.Map.Layers.Remove(mapView.Map.Layers.FirstOrDefault(l => l.Name == "Hotel Pins"));

            // Add new pins
            var pinLayer = new MemoryLayer("Hotel Pins")
            {
                Features = hotels.Select((hotel, index) =>
                {
                    var position = SphericalMercator.FromLonLat(hotel.Longitude, hotel.Latitude);
                    var feature = new PointFeature(new MPoint(position.x, position.y));
                    feature["Label"] = $"{index + 1} → {hotel.HotelName}";
                    return feature;
                }).ToList(),
                Style = new SymbolStyle
                {
                    SymbolScale = 0.7,
                    Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.Red),
                    Outline = new Mapsui.Styles.Pen(Mapsui.Styles.Color.Black, 1)
                }
            };

            mapView.Map.Layers.Add(pinLayer);

            // Zoom to fit all pins
            if (hotels.Any())
            {
                var allPoints = hotels.Select(h => SphericalMercator.FromLonLat(h.Longitude, h.Latitude)).ToList();
                var boundingBox = new MRect(
                    allPoints.Min(p => p.x), allPoints.Min(p => p.y),
                    allPoints.Max(p => p.x), allPoints.Max(p => p.y)
                );

                mapView.Map.Navigator.ZoomToBox(boundingBox);
            }

        }

        private async void ShowMap(object sender, EventArgs e) => await Navigation.PushAsync(new MapPage(userId));
        private async void ShowHotels(object sender, EventArgs e) => await Navigation.PushAsync(new MyHotelsPage(userId));
        private async void ShowFlights(object sender, EventArgs e) => await Navigation.PushAsync(new MyFlightsPage(userId));
        private async void ShowHome(object sender, EventArgs e) => await Navigation.PushAsync(new HomePage(userId));
        private async void ShowProfileOptions(object sender, EventArgs e) => await Navigation.PushAsync(new MyProfilePage(userId));
    }
}
