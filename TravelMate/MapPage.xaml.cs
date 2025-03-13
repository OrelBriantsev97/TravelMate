using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using TravelMate.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

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
            var hotels = await DatabaseHelper.GetHotelsByUserId(userId);;
            if (hotels.Count > 0)
            {
                // Open Google Maps with the first hotel location
                var firstHotel = hotels[0];
                var location = new Location(firstHotel.Latitude, firstHotel.Longitude);
                var options = new MapLaunchOptions { Name = firstHotel.HotelName };

                //await Map.OpenAsync(location, options);
            }
            else
            {
                await DisplayAlert("No Hotels", "You don't have any saved hotels.", "OK");
            }
        }
    }

}
