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
using Microsoft.Maui.Dispatching;
using TravelMate.Controls;
using TravelMate.ViewModels;

namespace TravelMate
{
    public partial class MapPage : ContentPage
    {
        private readonly int userId;
        private List<Hotel> hotels = new();
        private List<Flight> flights = new();
        private readonly MapViewModel viewModel;    

        public MapPage(int userId, string destination)
        {
            InitializeComponent();
            this.userId = userId;

            NavigationContainer.Content = new NavigationBar(userId, destination);

            _ = SetupMapAsync(); // Start map setup
            BindingContext = new MapViewModel(userId);

            LoadMapWithSpinner();
        }

        private async Task LoadMapWithSpinner()
        {
            viewModel.IsLoading = true;

            await Task.Delay(300); 

            await SetupMapAsync();

            viewModel.IsLoading = false;
        }

        private async Task SetupMapAsync()
        {
            var tileSource = new HttpTileSource(new GlobalSphericalMercator(), "https://tile.openstreetmap.org/{z}/{x}/{y}.png");

            mapView.Map = new Mapsui.Map();
            mapView.Map.Layers.Add(new TileLayer(tileSource) { Name = "OpenStreetMap" });
            mapView.Map.Navigator.ZoomTo(7);
            mapView.Map.Home = n => n.ZoomTo(20);

            flights = await DatabaseHelper.GetFlightsByUserId(userId);
            hotels = await DatabaseHelper.GetHotelsByUserId(userId);

            if (hotels.Any())
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LoadHotelPins();
                    ZoomToHotels();
                });
            }
        }

        private void LoadHotelPins()
        {
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

            mapView.Map.Layers.Add(pinLayer);
        }

        private void ZoomToHotels()
        {
            var allPoints = hotels.Select(h => SphericalMercator.FromLonLat(h.Longitude, h.Latitude)).ToList();
            if (allPoints.Any())
            {
                var bounds = new MRect(
                    allPoints.Min(p => p.x), allPoints.Min(p => p.y),
                    allPoints.Max(p => p.x), allPoints.Max(p => p.y));
                mapView.Map.Navigator.ZoomToBox(bounds);
            }
        }

        private void ZoomIn(object sender, EventArgs e) => mapView.Map.Navigator.ZoomIn();
        private void ZoomOut(object sender, EventArgs e) => mapView.Map.Navigator.ZoomOut();
    }
}
