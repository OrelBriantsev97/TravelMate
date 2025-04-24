using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Timers;
using Microsoft.Maui.Controls;
using TravelMate.Controls;
using TravelMate.Models;
using TravelMate.Services;
using TravelMate.ViewModels;


namespace TravelMate
{
    public partial class HomePage : ContentPage
    {
        private readonly HomeViewModel _viewModel;

        public HomePage(int userId)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            _viewModel = new HomeViewModel(userId, Navigation);
            BindingContext = _viewModel;

            var navBar = new NavigationBar(userId, _viewModel.CityName);
            NavigationContainer.Content = navBar;
        }
    }

}
