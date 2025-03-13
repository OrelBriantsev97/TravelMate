using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;

namespace TravelMate
{
    public partial class MyFlightsPage : ContentPage
    {
        public int userId { get; set; }

        public MyFlightsPage(int userID)
        {
            InitializeComponent();
            userId = userID;
            LoadFlights();
        }

        private async void LoadFlights()
        {
            try
            {
                var flights = await DatabaseHelper.GetFlightsByUserId(userId); // Replace with actual DB call

                // Sort flights by departure date and time
                var sortedFlights = flights.OrderBy(f => f.DepartureTime).ToList();
                FlightsListView.ItemsSource = sortedFlights;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred while loading hotels: {ex.Message}", "OK");
            }
        }

        // Navigation methods for the bottom navigation bar
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