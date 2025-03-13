using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelMate.Models;
using TravelMate.Services;

namespace TravelMate
{
    public partial class MyHotelsPage : ContentPage
    {
        private readonly int userId;

        public MyHotelsPage(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            LoadHotels();
        }

        private async void LoadHotels()
        {
            try
            {
                var hotels = await DatabaseHelper.GetHotelsByUserId(userId);

                if (hotels == null || !hotels.Any())
                {
                    await DisplayAlert("No Hotels", "No future hotels found.", "OK");
                    return;
                }

                // Sort by check-in date
                var sortedHotels = hotels.OrderBy(h => DateTime.Parse(h.CheckInDate)).ToList();
                HotelsCollectionView.ItemsSource = sortedHotels;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred while loading hotels: {ex.Message}", "OK");
            }
        }

        private async void ShowMap(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MapPage(userId));
        }

        private async void ShowHotels(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MyHotelsPage(userId));
        }

        private async void ShowFlights(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MyFlightsPage(userId));
        }

        private async void ShowHome(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HomePage(userId));
        }

        private async void ShowProfileOptions(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MyProfilePage(userId));
        }
    }
}
