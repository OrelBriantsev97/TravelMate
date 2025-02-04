using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Controls;
using System.Globalization;
using TravelMate.Models;
using TravelMate.Services;

namespace TravelMate
{
    public partial class NewFlightPage : ContentPage
    {
        public int userId { get; set; }
        private List<string> airportIds = new List<string>();

        private readonly List<string> airportOptions = new List<string>
        {
            "Tel Aviv - TLV","New York - JFK","Los Angeles - LAX","London - LHR","Paris - CDG","Tokyo - HND","Berlin - BER","Sydney - SYD","Dubai - DXB","Rome - FCO",
            "Bangkok - BKK","Phuket - HKT","Samui - USM","Chiang Mai - CNX","Manila - MNL","Cebu - CEB","Singapore - SIN","Jakarta - CGK","Kuala Lumpur - KUL",
            "Hanoi - HAN","Ho Chi Minh City - SGN","Vientiane - VTE","Rio de Janeiro - GIG","São Paulo - GRU","Buenos Aires - EZE","Lima - LIM","Quito - UIO","Caracas - CCS",
            "Montevideo - MVD"
        };

        public NewFlightPage(int userID) 
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            userId = userID;
            airportIds = new List<string>(airportOptions);
            AirportListOrigin.ItemsSource = airportIds;
            AirportListDest.ItemsSource = airportIds;
        }

        private async void OnSearchFlightClicked(object sender, EventArgs e)
        {
            var flightNumber = FlightNumberEntry.Text;
            var departureDate = DepartureDatePicker.Date;
            var origin = ExtractAirportCode(OriginEntry.Text);
            var destination = ExtractAirportCode(DestinationEntry.Text);
                    
            string errorMessage = ValidationHelper.validateFlightEntries(flightNumber, origin, destination);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                await DisplayAlert("Error", errorMessage, "OK");
                return;
            }

            try
            { 
                var flightDetails = await FlightService.GetFlightDetailsAsync(flightNumber, origin,destination,departureDate);

                if (flightDetails == null)
                {
                    await DisplayAlert("No Results", "No flight details found for the provided information.", "OK");
                    FlightDetailsLayout.IsVisible = false;
                }
                else
                {
                    var flight = flightDetails;

                    OriginLabel.Text = flight.Departure.Id ?? "N/A";
                    DestinationLabel.Text = flight.Arrival?.Id ?? "N/A";
                    AirlineLabel.Text = flight.Airline ?? "N/A";

                    ExtractFlightHours(flight);
                    calcDuration(flight);
                    FlightDetailsLayout.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions like API errors or parsing issues
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        // Event handler for when a flight is selected
        private async void OnFlightTapped(object sender, EventArgs e)
        {
            var flightNumber = FlightNumberEntry.Text;
            var answer = await DisplayAlert("Add Flight", "Do you want to add this flight to your trip?", "Yes", "No");
            if (answer)
            {

                var flight = new Flight
                {
                    UserId = userId,
                    FlightNumber = flightNumber,
                    DepartureTime = DepartureTimeLabel.Text ?? "N/A",
                    DepartureDate = DepartureDateLabel.Text ?? "N/A",
                    ArrivalTime = ArrivalTimeLabel.Text ?? "N/A",
                    ArrivalDate = ArrivalDateLabel.Text ?? "N/A",
                    DepartureCity = OriginLabel.Text ?? "N/A",
                    ArrivalCity = DestinationLabel.Text ?? "N/A",
                    Duration = DurationLabel.Text ?? "N/A",
                    Airline = AirlineLabel.Text ?? "N/A"

                };
                DatabaseHelper.AddFlight(flight, userId);
                await DisplayAlert("Success", "Flight added to your trip!", "OK");

            }
            await Navigation.PushAsync(new NewHotelPage(userId));

        }

        private void ExtractFlightHours(FlightDetails flight)
        {
            DateTime departureDateTime, arrivalDateTime;

            // Extract departure time
            if (DateTime.TryParse(flight.Departure?.Time, out departureDateTime))
            {
                DepartureTimeLabel.Text = departureDateTime.ToString("hh:mm tt", CultureInfo.InvariantCulture);
                DepartureDateLabel.Text = departureDateTime.ToString("yyyy-MM-dd"); // Only show date
            }
            else
            {
                DepartureTimeLabel.Text = "N/A";
                DepartureDateLabel.Text = "N/A";
            }

            // Extract arrival time
            if (DateTime.TryParse(flight.Arrival?.Time, out arrivalDateTime))
            {
                ArrivalTimeLabel.Text = arrivalDateTime.ToString("hh:mm tt", CultureInfo.InvariantCulture);
                ArrivalDateLabel.Text = arrivalDateTime.ToString("yyyy-MM-dd"); // Only show date
            }
            else
            {
                ArrivalTimeLabel.Text = "N/A";
                ArrivalDateLabel.Text = "N/A";
            }
        }
        
        //calculating flight duration. api returns duration as minutes
        //ex: duration 220 is 3 hours and 40 minutes.
        private void calcDuration(FlightDetails flight)
        {
            if (flight.Duration > 0)
            {
                int hours = (flight.Duration) / 60;
                int minutes = flight.Duration % 60;
                DurationLabel.Text = $"Duration: {hours}h {minutes}m";
            }
            else
            {
                DurationLabel.Text = "Duration: N/A";
            }
        }

        private string ExtractAirportCode(string airportString)
        {
            if (string.IsNullOrEmpty(airportString)) return null;

            // The airport code is always at the end after " - "
            var parts = airportString.Split(" - ");
            return parts.Length > 1 ? parts[^1] : airportString;
        }

        private void OnAirportSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;
            if (entry != null)
            {
                string searchText = entry.Text.ToLower();

                // Filter airports based on search text
                var filteredAirports = airportOptions
                    .Where(a => a.ToLower().Contains(searchText))
                    .ToList();

                // Show suggestions for the relevant entry
                if (entry == OriginEntry)
                {
                    AirportListOrigin.ItemsSource = filteredAirports;
                    AirportListOrigin.IsVisible = !string.IsNullOrEmpty(searchText);
                }
                else if (entry == DestinationEntry)
                {
                    AirportListDest.ItemsSource = filteredAirports;
                    AirportListDest.IsVisible = !string.IsNullOrEmpty(searchText);
                }
            }
        }

        private void OnAirportSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // Handle airport selection
            if (e.SelectedItem != null)
            {
                var selectedAirport = e.SelectedItem.ToString();

                // Set the selected airport to the corresponding entry
                if (sender == AirportListOrigin)
                {
                    OriginEntry.Text = selectedAirport;
                    AirportListOrigin.IsVisible = false;
                }
                else if (sender == AirportListDest)
                {
                    DestinationEntry.Text = selectedAirport;
                    AirportListDest.IsVisible = false;
                }
            }
        }




        private async void OnHelpClicked(object sender, EventArgs e)
        {
            // Show a simple help message in an alert box
            await DisplayAlert("Help",
                               "enter the airport IATA code (e.g., DXB) or the city name (e.g., Dubai) to search for the airport.",
                               "OK");
        }

    }
}
