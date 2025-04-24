using System;
using Microsoft.Maui.Controls;
using TravelMate.ViewModels;
using System.Windows.Input;

namespace TravelMate
{
    public partial class NewHotelPage : ContentPage
    {
        private readonly NewHotelViewModel _viewModel;
        public NewHotelPage(int userId)
        {

            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            _viewModel = new NewHotelViewModel(userId, Navigation);
            BindingContext = _viewModel;

            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.SelectedHotel) && _viewModel.SelectedHotel != null)
                {
                    ShowStars(_viewModel.SelectedHotel.HotelClass);
                }
            };
        }


        private async void OnHotelTapped(object sender, EventArgs e)
        {
            var answer = await DisplayAlert("Add Hotel", "Do you want to add this Hotel to your trip?", "Yes", "No");

            if (answer)
            {
                await _viewModel.AddHotelCommand.ExecuteAsync(null);
            }
        }

        private void ShowStars(int hotelClass)
        {
            Star1.IsVisible = hotelClass >= 1;
            Star2.IsVisible = hotelClass >= 2;
            Star3.IsVisible = hotelClass >= 3;
            Star4.IsVisible = hotelClass >= 4;
            Star5.IsVisible = hotelClass >= 5;
        }
    }

    public static class CommandExtensions
    {
        public static async Task ExecuteAsync(this ICommand command, object parameter)
        {
            if (command is Command cmd && cmd.CanExecute(parameter))
                cmd.Execute(parameter);

            await Task.CompletedTask;
        }

       

    }
}


