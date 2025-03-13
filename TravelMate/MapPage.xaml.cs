using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using TravelMate.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TravelMate
{
    public partial class MapPage : ContentPage
    {
        private readonly int userId;

        public MapPage(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            LoadHotelPins();
        }

        private async void LoadHotelPins()
        {
            var hotels = await DatabaseHelper.GetHotelsByUserId(userId);

            if (hotels.Count > 0)
            {
                var firstHotel = hotels[0]; // Get first hotel
                mapView.MapType = MapType.Street;  // Ensure OpenStreetMap is used

                mapView.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Location(firstHotel.Latitude, firstHotel.Longitude),
                    Distance.FromKilometers(5)));

                // Add pins for each hotel
                foreach (var hotel in hotels)
                {
                    var pin = new Pin
                    {
                        Label = hotel.HotelName,
                        Address = hotel.Address,
                        Location = new Location(hotel.Latitude, hotel.Longitude),
                        Type = PinType.Place
                    };

                    mapView.Pins.Add(pin);
                }
            }
            else
            {
                await DisplayAlert("No Hotels", "You don't have any saved hotels.", "OK");
            }
        }
    }
}
