using System;
using Microsoft.Maui.Controls;

namespace TravelMate.Controls
{
    public partial class NavigationBar : ContentView
    {
        private int userId;
        private string destination;

        
        public NavigationBar()
        {
            InitializeComponent();
        }
        public NavigationBar(int userId,string destination) : this()
        {
            this.userId = userId;
            this.destination = destination;
        }

        // Event handlers for the navigation buttons
        private async void ShowHotels(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MyHotelsPage(userId,destination));
        }

        private async void ShowFlights(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MyFlightsPage(userId, destination));
        }

        private async void ShowHome(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HomePage(userId));
        }

        private async void ShowProfileOptions(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MyProfilePage(userId,destination));
        }

        private async void ShowPlaces(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MyPlacesPage(userId, destination)); 
        }
    }
}
