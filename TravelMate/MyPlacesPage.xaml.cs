using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TravelMate.Services;
using TravelMate.Models;
using TravelMate.Controls;

namespace TravelMate
{
    public partial class MyPlacesPage : ContentPage
    {
        private int userId;
        private string destination;

        public MyPlacesPage(int userId, string destination)
        {
            InitializeComponent();
            this.userId = userId;
            this.destination = destination;
            var navBar = new NavigationBar(userId, destination);
            NavigationContainer.Content = navBar;
            LoadPlaces();
        }

        private async void LoadPlaces()
        {
            var attractions = await PlaceService.GetPlaces(userId, destination,"attractions");
            var restaurants = await PlaceService.GetPlaces(userId, destination, "restaurants");
            var bars = await PlaceService.GetPlaces(userId, destination, "bars");

            AttractionsCollectionView.ItemsSource = attractions;
            RestaurantsCollectionView.ItemsSource = restaurants;
            BarsCollectionView.ItemsSource = bars;


        }
    }
}
