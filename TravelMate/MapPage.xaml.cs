using Mapsui.Layers;
using Mapsui.Tiling.Layers;
using Mapsui.Tiling;
using Mapsui.Projections;
using Mapsui.UI.Maui;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelMate.Models;
using BruTile.Predefined;
using BruTile.Web;
using Mapsui;
using Mapsui.Styles;
using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Maui.Dispatching;
using TravelMate.Controls;

namespace TravelMate
{
    public partial class MapPage : ContentPage
    {
        private readonly int userId;
        private List<Hotel> hotels = new();
        private List<Flight> flights = new();

        public MapPage(int userId,string destination)
        {
            InitializeComponent();
            this.userId = userId;
            var navBar = new NavigationBar(userId, destination);
            NavigationContainer.Content = navBar;
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
            mapView.Map.Navigator.ZoomTo(7);
            mapView.Map.Home = n => n.ZoomTo(90);

            flights = await DatabaseHelper.GetFlightsByUserId(userId);
            hotels = await DatabaseHelper.GetHotelsByUserId(userId);

            if (flights.Any() || hotels.Any())
            {
                await Task.Delay(500);
                MainThread.BeginInvokeOnMainThread(LoadTripOverview);
                MainThread.BeginInvokeOnMainThread(LoadHotelPins);
                Debug.WriteLine($"Number of layers: {mapView.Map.Layers.Count}");
            }
            else
            {
                await DisplayAlert("No Data", "No flights or hotels found.", "OK");
            }
        }

        private void LoadTripOverview()
        {
            var tripEvents = new List<TripEvent>();

            // Add flights to trip overview
            foreach (var flight in flights)
            {
                if (DateTime.TryParse(flight.DepartureDate, out DateTime flightDate))
                {
                    tripEvents.Add(new TripEvent
                    {
                        Date = flightDate,
                        Icon = "✈️",
                        Title = $"Flight {flight.FlightNumber}",
                        Subtitle = $"{flightDate:MMM dd, yyyy} at {flight.DepartureTime:hh\\:mm tt}"
                    });
                }
            }

            // Add hotels to trip overview
            int hotelNumber = 1;
            foreach (var hotel in hotels)
            {
                if (DateTime.TryParse(hotel.CheckInDate, out DateTime checkInDate) &&
                    DateTime.TryParse(hotel.CheckOutDate, out DateTime checkOutDate))
                {
                    Debug.WriteLine($"Hotel: {hotel.HotelName}, Check-in: {hotel.CheckInDate}");
                    tripEvents.Add(new TripEvent
                    {
                        Date = checkInDate,
                        Icon = "🏕️",
                        Title = $"{hotelNumber} → {hotel.HotelName}",
                        Subtitle = $"Check In {checkInDate:MMM dd} → Check Out: {checkOutDate:MMM dd}"
                    });
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

        private void ZoomIn(object sender, EventArgs e)
        {
            mapView.Map.Navigator.ZoomIn();
        }

        private void ZoomOut(object sender, EventArgs e)
        {
            mapView.Map.Navigator.ZoomOut();
        }
    }

    public class TripEvent
    {
        public DateTime Date { get; set; }
        public string Icon { get; set; } 
        public string Title { get; set; } 
        public string Subtitle { get; set; }
    }
}
