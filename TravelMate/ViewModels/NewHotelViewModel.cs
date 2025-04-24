using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TravelMate.Models;
using TravelMate.Services;


namespace TravelMate.ViewModels
{
    public class NewHotelViewModel : BindableObject
    {
        public int UserId { get; set; }
        public ObservableCollection<HotelDetails> HotelResults { get; set; } = new();


        private string hotelName;
        private string city;
        private DateTime checkInDate = DateTime.Today;
        private DateTime checkOutDate = DateTime.Today.AddDays(1);
        private bool isHotelResultVisible;
        private HotelDetails selectedHotel;

        public string HotelName
        {
            get => hotelName;
            set { hotelName = value; OnPropertyChanged(); }
        }

        public string City
        {
            get => city;
            set { city = value; OnPropertyChanged(); }
        }

        public DateTime CheckInDate
        {
            get => checkInDate;
            set { checkInDate = value; OnPropertyChanged(); }
        }

        public DateTime CheckOutDate
        {
            get => checkOutDate;
            set { checkOutDate = value; OnPropertyChanged(); }
        }

        public bool IsHotelResultVisible
        {
            get => isHotelResultVisible;
            set { isHotelResultVisible = value; OnPropertyChanged(); }
        }

        public HotelDetails SelectedHotel
        {
            get => selectedHotel;
            set
            {
                selectedHotel = value;
                OnPropertyChanged();
            }
        }
        public ICommand SearchHotelCommand { get; }
        public ICommand AddHotelCommand { get; }
        public ICommand SkipCommand { get; }
        public INavigation Navigation { get; set; }

        public NewHotelViewModel(int userId, INavigation navigation)
        {
            UserId = userId;
            Navigation = navigation;

            SearchHotelCommand = new Command(async () => await SearchHotel());
            AddHotelCommand = new Command(async () => await AddHotel());
            SkipCommand = new Command(async () => await Navigation.PushAsync(new HomePage(UserId)));
        }

        private async Task SearchHotel()
        {
            if (string.IsNullOrEmpty(HotelName) || string.IsNullOrEmpty(City))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please enter both hotel name and city.", "OK");
                return;
            }

            var hotelSearch = $"{HotelName} {City}";
            var checkIn = CheckInDate.ToString("yyyy-MM-dd");
            var checkOut = CheckOutDate.ToString("yyyy-MM-dd");

            var results = await HotelService.GetHotelDetailsAsync(hotelSearch, checkIn, checkOut);

            HotelResults.Clear();
            if (results != null && results.Count > 0)
            {
                SelectedHotel = results[0];
                HotelResults.Add(SelectedHotel);
                IsHotelResultVisible = true;
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("No Results", "No hotels found matching your criteria.", "OK");
            }
        }

        private async Task AddHotel()
        {
            if (SelectedHotel == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please search and select a hotel first.", "OK");
                return;
            }

            var hotel = new Hotel
            {
                UserId = UserId,
                HotelName = SelectedHotel.HotelName,
                Address = SelectedHotel.Address,
                Phone = SelectedHotel.Phone,
                CheckInDate = CheckInDate.ToString("yyyy-MM-dd"),
                CheckOutDate = CheckOutDate.ToString("yyyy-MM-dd"),
                LogoUrl = SelectedHotel.LogoUrl,
                Class = SelectedHotel.HotelClass,
                NearbyPlaces = SelectedHotel.NearbyPlaces,
                Latitude = SelectedHotel.Latitude,
                Longitude = SelectedHotel.Longitude,
                Amenities = string.Join(", ", SelectedHotel.Amenities?.Take(3) ?? [])
            };

            await DatabaseHelper.AddHotel(hotel, UserId);
            await Application.Current.MainPage.DisplayAlert("Success", "Hotel added to your trip!", "OK");

            bool addAnother = await Application.Current.MainPage.DisplayAlert("Add Another Hotel?", "Do you want to add another hotel?", "Yes", "No");

            if (!addAnother)
                await Navigation.PushAsync(new HomePage(UserId));
            else
            {
                HotelName = string.Empty;
                City = string.Empty;
                CheckInDate = DateTime.Today;
                CheckOutDate = DateTime.Today.AddDays(1);
                HotelResults.Clear();
            }
        }
    }
}
