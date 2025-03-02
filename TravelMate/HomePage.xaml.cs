using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Timers;
using Microsoft.Maui.Controls;
using TravelMate.Models;
using TravelMate.Services;


namespace TravelMate
{
    public partial class HomePage : ContentPage
    {
        private readonly int userId;
        private Flight closestFlight;
        private System.Timers.Timer countdownTimer;


        public HomePage(int userID)
        {
            InitializeComponent();
            userId = userID;
            LoadClosestFlight();
        }

        private async void LoadClosestFlight()
        {
            try { 
            // Retrieve the closest upcoming flight for the user
                var flights = await DatabaseHelper.GetFlightsByUserId(userId);
                if (flights != null && flights.Any())
                {
                    Console.WriteLine($"Found {flights.Count} flights:");
                    foreach (var flight in flights)
                    {
                        //Customize to print the properties you want.
                        Console.WriteLine($"Flight: FlightNumber={flight.FlightNumber}, Destination={flight.ArrivalCity}, Departure={flight.DepartureTime}");
                    }
                }
                else
                {
                    Console.WriteLine("No flights found for the user.");
                }
                var closestFlight= flights

    .Where(f => DateTime.TryParse(f.DepartureTime, out var departureDate) && departureDate > DateTime.Now)

    .OrderBy(f => DateTime.Parse(f.DepartureTime))

    .FirstOrDefault();
                if (closestFlight != null)
                {
                // Set the destination image
                    var imageName = $"{closestFlight.DepartureCity.Replace(" ", string.Empty).ToLower()}.jpg";
                    var imageSource = ImageSource.FromResource($"TravelMate.Resources.Images.Destinations.{imageName}", typeof(HomePage).GetTypeInfo().Assembly);
                    DestinationImage.Source = imageSource;

                // Start the countdown timer
                    StartCountdown();
                }
                else
                {
                countdownLabel.Text = "No upcoming flights.";
                }
            }
            catch (Exception ex)
            {

                // Log the exception (e.g., to console or a logging service)
                Console.WriteLine($"Error loading closest flight: {ex.Message}");
                // Optionally, display an error message to the user
                await DisplayAlert("Error", "An error occurred while loading the flight information.", "OK");
        }
    }

        private void StartCountdown()
        {
            countdownTimer = new System.Timers.Timer(1000);
            countdownTimer.Elapsed += OnTimerElapsed;
            countdownTimer.Start();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var departureTime = DateTime.Parse(closestFlight.DepartureTime);
            var timeRemaining = departureTime - DateTime.Now;

            if (timeRemaining <= TimeSpan.Zero)
            {
                countdownTimer.Stop();
                countdownLabel.Text = "Flight departed";
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    countdownLabel.Text = $"{timeRemaining.Days}d {timeRemaining.Hours}h {timeRemaining.Minutes}m {timeRemaining.Seconds}s until departure";
                });
            }
        }
    }
}
