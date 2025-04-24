using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using TravelMate.Controls;

namespace TravelMate
{
    public partial class MyFlightsPage : ContentPage
    {
        public int userId { get; set; }

        public MyFlightsPage(int userID, string destination)
        {
            InitializeComponent();
            userId = userID;
            var navBar = new NavigationBar(userId, destination);
            NavigationContainer.Content = navBar;
            LoadFlights();
        }

        private async void LoadFlights()
        {
            try
            {
                var flights = await DatabaseHelper.GetFlightsByUserId(userId);

                // Combine DepartureDate and DepartureTime into DateTime for proper sorting
                var sortedFlights = flights
                    .Select(f =>
                    {
                        DateTime.TryParse($"{f.DepartureDate} {f.DepartureTime}", out DateTime fullDateTime);
                        return new { Flight = f, FullDateTime = fullDateTime };
                    })
                    .OrderBy(f => f.FullDateTime)
                    .Select(f => f.Flight)
                    .ToList();

                FlightsListView.ItemsSource = sortedFlights;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred while loading flights: {ex.Message}", "OK");
            }
        }


        private void addFlight(object sender, EventArgs e)
        {
            Navigation.PushAsync(new NewFlightPage(userId));
        }
    }
}