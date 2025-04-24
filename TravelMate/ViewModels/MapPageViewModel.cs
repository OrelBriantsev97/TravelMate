using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Tiling.Layers;
using Mapsui.Tiling;
using Mapsui.Projections;
using TravelMate.Models;
using TravelMate.Services;
using BruTile.Predefined;
using BruTile.Web;

namespace TravelMate.ViewModels
{
    public class MapViewModel : BindableObject
    {
        // ViewModel responsible for initializing and managing the map,
        // loading user flights and hotels, placing map pins, and providing
        // TripEvents for the trip overview.
        public ObservableCollection<TripEvent> TripEvents { get; set; } = new();
        public Mapsui.Map Map { get; private set; }
        public Task init { get; private set; }

        private List<Hotel> hotels;
        private List<Flight> flights;
        
        private readonly int userId;

        private bool isLoading;
        // Indicates whether the map data is currently loading.
        public bool IsLoading
        {
            get => isLoading;
            set { isLoading = value; OnPropertyChanged(); }
        }


        public MapViewModel(int userId)
        {
            this.userId = userId;
            Map = new Mapsui.Map();
            init = InitializeMap();
        }

        // Asynchronously initializes the map:
        // - Adds OpenStreetMap tiles
        // - Loads flights and hotels from the database
        // - Populates TripEvents and map pins on the UI thread
        private async Task InitializeMap()
        {
            var tileSource = new HttpTileSource(new GlobalSphericalMercator(), "https://tile.openstreetmap.org/{z}/{x}/{y}.png");
            var tileLayer = new TileLayer(tileSource) { Name = "OpenStreetMap" };


            Map.Layers.Add(tileLayer);
            Map.Navigator.ZoomTo(7);
            Map.Home = n => n.ZoomTo(90);

            flights = await DatabaseHelper.GetFlightsByUserId(userId);
            hotels = await DatabaseHelper.GetHotelsByUserId(userId);

            if (flights.Any() || hotels.Any())
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LoadTripOverview();
                    LoadHotelPins(Map);
                });
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                OnPropertyChanged(nameof(Map));
            });
        }

        // Builds the TripEvents list by combining upcoming flights and hotel stays,
        // sorting them by date.
        private void LoadTripOverview()
        {
            TripEvents.Clear();
            var events = new List<TripEvent>();

            foreach (var flight in flights)
            {
                if (DateTime.TryParse(flight.DepartureDate, out DateTime date))
                {
                    events.Add(new TripEvent
                    {
                        Date = date,
                        Icon = "✈️",
                        Title = $"Flight {flight.FlightNumber}",
                        Subtitle = $"{date:MMM dd, yyyy} at {flight.DepartureTime:hh\\:mm tt}"
                    });
                }
            }

            int hotelNumber = 1;
            foreach (var hotel in hotels)
            {
                if (DateTime.TryParse(hotel.CheckInDate, out DateTime checkIn) &&
                    DateTime.TryParse(hotel.CheckOutDate, out DateTime checkOut))
                {
                    events.Add(new TripEvent
                    {
                        Date = checkIn,
                        Icon = "🏕️",
                        Title = $"{hotelNumber} → {hotel.HotelName}",
                        Subtitle = $"Check In {checkIn:MMM dd} → Check Out: {checkOut:MMM dd}"
                    });
                    hotelNumber++;
                }
            }

            foreach (var ev in events.OrderBy(e => e.Date))
                TripEvents.Add(ev);
        }

        // Places pin markers on the map for each hotel location
        // and adjusts the map's viewport to fit all pins.
        private void LoadHotelPins(Mapsui.Map map)
        {
            if (!hotels.Any()) return;

            var pinLayer = new MemoryLayer("Hotel Pins")
            {
                Features = hotels.Select((hotel, i) =>
                {
                    var pos = SphericalMercator.FromLonLat(hotel.Longitude, hotel.Latitude);
                    var feature = new PointFeature(new MPoint(pos.x, pos.y));
                    feature["Label"] = $"{i + 1} → {hotel.HotelName}";
                    return feature;
                }).ToList(),
                Style = new SymbolStyle
                {
                    SymbolScale = 0.7,
                    Fill = new Mapsui.Styles.Brush(Mapsui.Styles.Color.Red),
                    Outline = new Pen(Mapsui.Styles.Color.Black, 1)

                }
            };

            map.Layers.Add(pinLayer);

            var allPoints = hotels.Select(h => SphericalMercator.FromLonLat(h.Longitude, h.Latitude)).ToList();
            var bounds = new MRect(allPoints.Min(p => p.x), allPoints.Min(p => p.y), allPoints.Max(p => p.x), allPoints.Max(p => p.y));
            map.Navigator.ZoomToBox(bounds);
        }

        public void ZoomIn() => Map.Navigator.ZoomIn();
        public void ZoomOut() => Map.Navigator.ZoomOut();
    }

    public class TripEvent
    {
        public DateTime Date { get; set; }
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
    }
}
