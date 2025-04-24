using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using TravelMate.Models;
using TravelMate.Services;
using TravelMate.Controls;
using Microsoft.Maui.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace TravelMate.ViewModels
{
    public class HomeViewModel : BindableObject
    {
        // The ViewModel for the Home page.  
        // Loads the next upcoming flight, starts a live countdown,
        // fetches weather for the destination, and exposes
        // collections & commands for the main dashboard.

        private readonly int userId;
        private System.Timers.Timer countdownTimer;
        private Flight closestFlight;
        private readonly INavigation _navigation;

        public ObservableCollection<Hotel> Hotels { get; set; } = new();

        public string CityName { get; set; } // The display name of the next flight’s destination city.
        public string CountdownText { get; set; } // A formatted countdown string (days hours minutes).
        public string DepartureLabel { get; set; } = "Days   hours  minutes"; //secondary label for countdown.
        public string WeatherCity { get; set; } // The display name of the destination city for weather info.
        public string WeatherTemp { get; set; } // The current temperature in the destination city.
        public string WeatherIcon { get; set; }  // Icon key representing current weather.
        public ImageSource DestinationImage { get; set; } // An image of the destination (embedded resource).

        public ICommand ShowChecklistCommand { get; set; } //navigate to the checklist page.
        public ICommand ShowMapCommand { get; set; } //navigate to the map page.
        public ICommand ShowHotelsCommand { get; set; } //navigate to the hotels page.
        public ICommand ShowConverterCommand { get; set; } //navigate to the currency converter page.   

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

        public HomeViewModel(int userId, INavigation navigation)
        {
            this.userId = userId;
            _navigation = navigation;

            ShowMapCommand = new Command(async () => await _navigation.PushAsync(new MapPage(userId,CityName)));
            ShowChecklistCommand = new Command(async () => await _navigation.PushAsync(new ChecklistPage(userId, CityName)));
            ShowHotelsCommand = new Command(async () => await _navigation.PushAsync(new MyHotelsPage(userId,CityName)));
            ShowConverterCommand = new Command(async () => await _navigation.PushAsync(new CurrencyConverterPage(userId,CityName)));
            
            LoadClosestFlight();
            LoadHotels();
        }

        // Finds the next upcoming flight, sets CityName and DestinationImage,
        /// starts the countdown timer, and loads weather for that city.
        private async void LoadClosestFlight()
        {
            var flights = await DatabaseHelper.GetFlightsByUserId(userId);
            var upcomingFlights = flights?
                .Select(f => new
                {
                    Flight = f,
                    IsValid = DateTime.TryParseExact($"{f.DepartureDate} {f.DepartureTime}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt),
                    DepartureDate = dt
                })
                .Where(f => f.IsValid && f.DepartureDate > DateTime.Now)
                .OrderBy(f => f.DepartureDate)
                .Select(f => f.Flight)
                .ToList();

            if (upcomingFlights == null || !upcomingFlights.Any())
            {
                CountdownText = "No upcoming flights.";
                OnPropertyChanged(nameof(CountdownText));
                return;
            }

            closestFlight = upcomingFlights.First();
            string arrivalCity = IATAToCityMap.ContainsKey(closestFlight.ArrivalCity) ? IATAToCityMap[closestFlight.ArrivalCity] : closestFlight.ArrivalCity;
            CityName = arrivalCity;
            OnPropertyChanged(nameof(CityName));

            // Load image
            var imagePath = $"TravelMate.Resources.Images.Destinations.{closestFlight.ArrivalCity.Replace(" ", "")}.jpg";
            DestinationImage = ImageSource.FromResource(imagePath, typeof(HomeViewModel).Assembly);
            OnPropertyChanged(nameof(DestinationImage));

            StartCountdown();
            await LoadWeatherInfo(arrivalCity);
        }

        // Starts a 1s timer that updates the countdown text until departure.
        private void StartCountdown()
        {
            countdownTimer = new System.Timers.Timer(1000);
            countdownTimer.Elapsed += (s, e) =>
            {
                var departureDateTime = DateTime.ParseExact($"{closestFlight.DepartureDate} {closestFlight.DepartureTime}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                var remaining = departureDateTime - DateTime.Now;

                if (remaining <= TimeSpan.Zero)
                {
                    CountdownText = "Flight departed";
                    DepartureLabel = "";
                }
                else
                {
                    CountdownText = $"{remaining.Days}    {remaining.Hours:D2}  {remaining.Minutes:D2}";
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    OnPropertyChanged(nameof(CountdownText));
                    OnPropertyChanged(nameof(DepartureLabel));
                });
            };
            countdownTimer.Start();
        }

        // Loads the list of hotels for this trip from the database.
        private async Task LoadHotels()
        {
            var hotelList = await DatabaseHelper.GetHotelsByUserId(userId);
            Hotels.Clear();
            foreach (var h in hotelList)
            {
                h.LogoUrl = h.LogoUrl?.Replace("Uri: ", "").Trim();
                Hotels.Add(h);
            }

            OnPropertyChanged(nameof(Hotels));
        }

        // Fetches current weather for the given city, sets WeatherTemp, WeatherCity and WeatherIcon.
        private async Task LoadWeatherInfo(string city)
        {
            var (currentTemp, locationAddress) = await WeatherService.GetWeather(city);
            WeatherTemp = currentTemp.HasValue ? $"{currentTemp.Value}°C" : "N/A";
            WeatherCity = locationAddress ?? "Unknown";
            WeatherIcon = currentTemp switch
            {
                >= 22 => "sunny.png",
                >= 12 => "cloudy.png",
                _ => "cold.png"
            };

            OnPropertyChanged(nameof(WeatherCity));
            OnPropertyChanged(nameof(WeatherTemp));
            OnPropertyChanged(nameof(WeatherIcon));
        }
    }
}
