using Microsoft.Maui.Controls;
using System;
using System.ComponentModel;
using System.Linq;
using TravelMate.ViewModels;

namespace TravelMate
{
    public partial class NewFlightPage : ContentPage
    {
        private NewFlightViewModel _viewModel;

        public NewFlightPage(int userId)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            _viewModel = new NewFlightViewModel(userId, Navigation);
            BindingContext = _viewModel;

            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_viewModel.SelectedFlight) && _viewModel.SelectedFlight != null)
            {
                var flight = _viewModel.SelectedFlight;

                AirlineLabel.Text = flight.Airline ?? "N/A";
                DepartureTimeLabel.Text = flight.Departure?.Time?.Substring(11, 5) ?? "N/A";
                ArrivalTimeLabel.Text = flight.Arrival?.Time?.Substring(11, 5) ?? "N/A";
                OriginLabel.Text = flight.Departure?.Id ?? "N/A";
                DepartureDateLabel.Text = flight.Departure?.Time?.Substring(0, 10) ?? "";
                DestinationLabel.Text = flight.Arrival?.Id ?? "N/A";
                ArrivalDateLabel.Text = flight.Arrival?.Time?.Substring(0, 10) ?? "";
                DurationLabel.Text = $"Duration: {_viewModel.FormatDuration(flight.Duration)}";

                FlightDetailsLayout.IsVisible = true;
            }
        }
        private void OnOriginFocused(object sender, FocusEventArgs e)
        {
            AirportListOrigin.IsVisible = true;
            AirportListDest.IsVisible = false;
        }

        private void OnDestinationFocused(object sender, FocusEventArgs e)
        {
            AirportListDest.IsVisible = true;
            AirportListOrigin.IsVisible = false;
        }

        private void OnAirportSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = ((Entry)sender).Text?.ToLower();
            IEnumerable<AirportItem> filtered = string.IsNullOrWhiteSpace(searchText)
       ? _viewModel.AirportItems
       : _viewModel.AirportItems
           .Where(item => item.CityName.ToLower().Contains(searchText) ||
                          item.FullString.ToLower().Contains(searchText));

            if (((Entry)sender).Placeholder?.ToLower().Contains("origin") == true)
            {
                AirportListOrigin.ItemsSource = filtered;
                AirportListOrigin.IsVisible = filtered.Any();
            }
            else
            {
                AirportListDest.ItemsSource = filtered;
                AirportListDest.IsVisible = filtered.Any();
            }
        }

        private void OnAirportSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is not AirportItem selectedAirport)
                return;

            if (sender == AirportListOrigin)
            {
                _viewModel.Origin = selectedAirport.FullString;
                AirportListOrigin.IsVisible = false;
            }
            else if (sender == AirportListDest)
            {
                _viewModel.Destination = selectedAirport.FullString;
                AirportListDest.IsVisible = false;
            }

            ((ListView)sender).SelectedItem = null;
        }

        private async void OnFlightTapped(object sender, EventArgs e)
        {
            var answer = await DisplayAlert("Add Flight", "Do you want to add this flight to your trip?", "Yes", "No");

            if (answer)
            {
                // Execute the AddFlightCommand in your ViewModel
                if (BindingContext is NewFlightViewModel vm && vm.AddFlightCommand.CanExecute(null))
                {
                    vm.AddFlightCommand.Execute(null);
                    AirportListOrigin.IsVisible = false;
                    AirportListDest.IsVisible = false;
                }
            }
            else
            {
                await DisplayAlert("Continue Searching", "Feel free to search for a different flight.", "OK");
            }
        }

        private async void OnHelpClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Help", "Enter the city name or IATA code (e.g., 'Dubai' or 'DXB')", "OK");
        }
    }
}
