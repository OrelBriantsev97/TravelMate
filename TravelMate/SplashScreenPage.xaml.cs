using System;
using Microsoft.Maui.Controls;

namespace TravelMate
{
    public partial class SplashScreenPage : ContentPage
    {
        public SplashScreenPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Wait 2 seconds, then navigate to the main page
            Device.StartTimer(TimeSpan.FromSeconds(2), () =>
            {
                Application.Current.MainPage = new NavigationPage(new LoginPage());
                return false;
            });
        }
    }
}
