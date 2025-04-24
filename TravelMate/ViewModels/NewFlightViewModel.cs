using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TravelMate.Models;
using TravelMate.Services;

namespace TravelMate.ViewModels
{
    public class NewFlightViewModel : BindableObject
    {
        // ViewModel for the "Add New Flight" page.
        // Manages flight search via external API and adding selected flights to the user's trip.
 
        public int UserId { get; set; }

        private string flightNumber;
        private string origin;
        private string destination;
        private DateTime departureDate = DateTime.Now;
        private string arrivalDate;
        private FlightDetails selectedFlight;
        public ObservableCollection<AirportItem> AirportItems { get; set; } = new ObservableCollection<AirportItem>();

        // The flight number input by the user.
        public string FlightNumber
        {
            get => flightNumber;
            set { flightNumber = value; OnPropertyChanged(); }
        }

        // The flight origin,input by user.
        public string Origin
        {
            get => origin;
            set { origin = value; OnPropertyChanged(); }
        }

        // The flight destination,input by user.
        public string Destination
        {
            get => destination;
            set { destination = value; OnPropertyChanged(); }
        }

        //The flight departure date, input by user.
        public DateTime DepartureDate
        {
            get => departureDate;
            set { departureDate = value; OnPropertyChanged(); }
        }
        //The flight arrival date, input by user.
        public string ArrivalDate
        {
            get => arrivalDate;
            set { arrivalDate = value; OnPropertyChanged(); }
        }

        // The flight details returned by the API, if any.
        public FlightDetails SelectedFlight
        {
            get => selectedFlight;
            set { selectedFlight = value; OnPropertyChanged(); }
        }
        public ICommand SearchFlightCommand { get; } // Command to search for flights.
        public ICommand AddFlightCommand { get; } // Command to add selected flight to the user's trip.
        public ICommand SkipCommand { get; } // Command to skip flight selection and proceed to hotel booking.
        public INavigation Navigation { get; set; }

        public NewFlightViewModel(int userId, INavigation navigation)
        {
            UserId = userId;
            Navigation = navigation;
            SearchFlightCommand = new Command(async () => await SearchFlight());
            AddFlightCommand = new Command(async () => await AddFlight());
            SkipCommand = new Command(async () => await Navigation.PushAsync(new NewHotelPage(UserId)));

            var airportOptions = new List<string>
            {
                "Tel Aviv - TLV", "New York - JFK", "Los Angeles - LAX", "London - LHR", "Paris - CDG",
                "Tokyo - HND", "Berlin - BER", "Sydney - SYD", "Dubai - DXB", "Abu Dhabi - AUH", "Rome - FCO",
                "Bangkok - BKK", "Phuket - HKT", "Krabi - KBV", "Samui - USM", "Chiang Mai - CNX", "Manila - MNL",
                "Cebu - CEB", "Singapore - SIN", "Hanoi - HAN", "Ho Chi Minh City - SGN", "Vientiane - VTE",
                "Rio de Janeiro - GIG", "São Paulo - GRU", "Buenos Aires - EZE", "Lima - LIM", "Quito - UIO",
                "Montevideo - MVD"
            };

            foreach (var option in airportOptions)
            {
                var parts = option.Split(" - ");
                AirportItems.Add(new AirportItem { CityName = parts[0], FullString = option });
            }
        }
        // Validates inputs, calls the FlightService API, and populates SelectedFlight/>.
        /// Shows an alert if no results are found or an error occurs.
        private async Task SearchFlight()
        {
            string errorMessage = ValidationHelper.ValidateFlightEntries(FlightNumber, ExtractAirportCode(Origin), ExtractAirportCode(Destination));
            if (!string.IsNullOrEmpty(errorMessage))
            {
                await Application.Current.MainPage.DisplayAlert("Error", errorMessage, "OK");
                return;
            }

            try
            {
                var result = await FlightService.GetFlightDetailsAsync(FlightNumber, ExtractAirportCode(Origin), ExtractAirportCode(Destination), DepartureDate);
                if (result == null)
                {
                    await Application.Current.MainPage.DisplayAlert("No Results", "No flight details found.", "OK");
                    SelectedFlight = null;
                }
                else
                {
                    SelectedFlight = result;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        /// Adds the searched flight to the local database and optionally resets the form for a new entry.
        /// </summary>
        private async Task AddFlight()
        {
            if (SelectedFlight == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please search for a flight first.", "OK");
                return;
            }

            var flight = new Flight
            {
                UserId = UserId,
                FlightNumber = FlightNumber,
                Airline = SelectedFlight.Airline ?? "N/A",
                DepartureTime = ParseTime(SelectedFlight.Departure?.Time),
                DepartureDate = ParseDate(SelectedFlight.Departure?.Time),
                ArrivalTime = ParseTime(SelectedFlight.Arrival?.Time),
                ArrivalDate = ParseDate(SelectedFlight.Arrival?.Time),
                DepartureCity = SelectedFlight.Departure?.Id ?? "N/A",
                ArrivalCity = SelectedFlight.Arrival?.Id ?? "N/A",
                Duration = FormatDuration(SelectedFlight.Duration)
            };

            await DatabaseHelper.AddFlight(flight, UserId);
            await Application.Current.MainPage.DisplayAlert("Success", "Flight added to your trip!", "OK");

            bool addAnother = await Application.Current.MainPage.DisplayAlert("Add Another Flight?", "Do you want to add another flight?", "Yes", "No");
            if (!addAnother)
                await Navigation.PushAsync(new NewHotelPage(UserId));
            else
            {
                FlightNumber = string.Empty;
                DepartureDate = DateTime.Now;
                Origin = string.Empty;
                Destination = string.Empty;
                SelectedFlight = null;
            }
        }

        /// Extracts the IATA code (e.g. "TLV") from a "City - Code" string.
        /// </summary>
        /// <param name="input">The full airport string.</param>
        /// <returns>The IATA code or null if parsing fails.</returns>
        private string ExtractAirportCode(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            var parts = input.Split(" - ");
            return parts.Length > 1 ? parts[^1] : input;
        }

        // Formats a datetime string into "HH:mm" or returns "N/A".
        private string ParseTime(string timeString)
        {
            return DateTime.TryParse(timeString, out DateTime time) ? time.ToString("HH:mm") : "N/A";
        }

        // Formats a datetime string into "yyyy-MM-dd" or returns "N/A".
        private string ParseDate(string timeString)
        {
            return DateTime.TryParse(timeString, out DateTime time) ? time.ToString("yyyy-MM-dd") : "N/A";
        }

        /// <summary>
        /// Converts a duration in minutes into a "<hours>h <minutes>m" format.
        public string FormatDuration(int minutes)
        {
            if (minutes <= 0) return "N/A";
            int hrs = minutes / 60;
            int mins = minutes % 60;
            return $"{hrs}h {mins}m";
        }
    }

    public class AirportItem
    {
        public string CityName { get; set; }
        public string FullString { get; set; }
    }
}


