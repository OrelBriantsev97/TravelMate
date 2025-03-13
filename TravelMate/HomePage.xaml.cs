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
        private List<Hotel> hotels;
        private System.Timers.Timer countdownTimer;

        // Dictionary to map IATA codes to city names
        private readonly Dictionary<string, string> IATAToCityMap = new()
        {
            { "TLV", "Tel Aviv" }, { "JFK", "New York" }, { "LAX", "Los Angeles" }, { "LHR", "London" },
            { "CDG", "Paris" }, { "HND", "Tokyo" }, { "BER", "Berlin" }, { "SYD", "Sydney" },
            { "DXB", "Dubai" }, { "AUH", "Abu Dhabi" }, { "FCO", "Rome" }, { "BKK", "Bangkok" },
            { "HKT", "Phuket" }, { "KBV", "Krabi" }, { "USM", "Samui" }, { "CNX", "Chiang Mai" },
            { "MNL", "Manila" }, { "CEB", "Cebu" }, { "SIN", "Singapore" }, { "HAN", "Hanoi" },
            { "SGN", "Ho Chi Minh City" }, { "VTE", "Vientiane" }, { "GIG", "Rio de Janeiro" },
            { "GRU", "São Paulo" }, { "EZE", "Buenos Aires" }, { "LIM", "Lima" }, { "UIO", "Quito" },
            { "MVD", "Montevideo" }
        };
        public HomePage(int userID)
        {
            InitializeComponent();
            userId = userID;
            LoadClosestFlight();
            LoadHotels();
        }

        private async void LoadClosestFlight()
        {
            try
            {
                var flights = await DatabaseHelper.GetFlightsByUserId(userId);
                if (flights == null || !flights.Any())
                {
                    countdownLabel.Text = "No upcoming flights.";
                    return;
                }
                var upcomingFlights = flights
                    .Select(f => new
                    {
                        Flight = f,
                        IsValid = DateTime.TryParseExact(
                            $"{f.DepartureDate} {f.DepartureTime}",
                            "yyyy-MM-dd HH:mm",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var departureDate),
                        DepartureDate = departureDate
                    })
                    .Where(f => f.IsValid && f.DepartureDate > DateTime.Now)
                    .OrderBy(f => f.DepartureDate)
                    .Select(f => f.Flight)
                    .ToList();

                if (!upcomingFlights.Any())
                {
                    countdownLabel.Text = "No upcoming flights.";
                    return;
                }

                // Get the closest flight
                closestFlight = upcomingFlights.First();
                var imageName = $"{closestFlight.ArrivalCity.Replace(" ", string.Empty)}.jpg";
                var imagePath = $"TravelMate.Resources.Images.Destinations.{imageName}";
                var imageSource = ImageSource.FromResource(imagePath, typeof(HomePage).GetTypeInfo().Assembly);
                if (imageSource != null)
                {
                    MainThread.BeginInvokeOnMainThread(() => DestinationImage.Source = imageSource);
                }

                string arrivalCity = closestFlight.ArrivalCity;
                if (IATAToCityMap.TryGetValue(arrivalCity, out string cityName))
                {
                    arrivalCity = cityName;
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    cityNameLabel.Text = arrivalCity;
                });
                StartCountdown();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "An error occurred while loading the flight information.", "OK");
            }
        }

        private async void LoadHotels()
        {
            try
            {

                var hotels = await DatabaseHelper.GetHotelsByUserId(userId);
                if (hotels != null && hotels.Count > 0)
                {
                    HotelView.ItemsSource = hotels;
                    Console.WriteLine("im hereeeeeeeeee");

                    for (int i = 0; i < hotels.Count; i++)
                    {
                        var dot = this.FindByName<Label>($"Dot{i + 1}");
                        if (dot != null)
                            dot.IsVisible = true;
                    }
                }
                else
                {
                    await DisplayAlert("No Hotels", "No hotels found for this user.", "OK");
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                await DisplayAlert("Error", $"An error occurred while loading hotels: {ex.Message}", "OK");
            }
        }

        private void HotelScrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            int currentIndex = e.CenterItemIndex;
            Console.WriteLine("im he111");
        }

  


        private void StartCountdown()
        {
            countdownTimer = new System.Timers.Timer(1000);
            countdownTimer.Elapsed += OnTimerElapsed;
            countdownTimer.Start();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (closestFlight == null) return;

            var departureDateTime = DateTime.ParseExact(
                $"{closestFlight.DepartureDate} {closestFlight.DepartureTime}",
                "yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture);

            var timeRemaining = departureDateTime - DateTime.Now;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (timeRemaining <= TimeSpan.Zero)
                {
                    countdownLabel.Text = "Flight departed";
                    departureLabel.Text = "";
                    countdownTimer.Stop();
                }
                else
                {
                    string countdownText = $"{timeRemaining.Days}    {timeRemaining.Hours:D2}  {timeRemaining.Minutes:D2} ";
                    countdownLabel.Text = countdownText;
                }
            });
        }

        private async void ShowMap(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MapPage(userId));
        }

        private async void ShowHotels(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MyHotelsPage(userId));
        }

        private async void ShowFlights(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MyFlightsPage(userId));
        }

        private async void ShowHome(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HomePage(userId));
        }

        private async void ShowProfileOptions(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MyProfilePage(userId));
        }

    }
}
