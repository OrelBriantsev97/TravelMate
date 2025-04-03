using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TravelMate.Models;
using TravelMate.Services;
using TravelMate.Controls;

namespace TravelMate
{
    public partial class MyHotelsPage : ContentPage
    {
        private readonly int userId;

        public MyHotelsPage(int UserId,string destination)
        {
            InitializeComponent();
            userId = UserId;
            Console.WriteLine($"in my hotels  page user id{userId} and destination is {destination}");
            NavigationBar navBar = new NavigationBar(userId,destination);
            NavigationContainer.Content = navBar;
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
                foreach (var hotel in hotels)
                {
                    hotel.LogoUrl = hotel.LogoUrl?.Replace("Uri: ", "").Trim();
                    Console.WriteLine($"Hotel: {hotel.HotelName}, Cleaned Logo URL: {hotel.LogoUrl}");
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

        private async void addHotel(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new NewHotelPage(userId));
        }
    }
}
