using System.Collections.ObjectModel;
using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using TravelMate.Models;
using TravelMate.Services;

namespace TravelMate
{
    public partial class NewHotelPage : ContentPage
    {
        public int userId { get; set; }
        public ObservableCollection<Hotel> HotelResults { get; set; } = new ObservableCollection<Hotel>();

        public NewHotelPage(int userID)
        {

            InitializeComponent();
            userId = userID;
            this.BindingContext = this;
        }


        private async void OnSearchHotelClicked(object sender, EventArgs e)
        {
            var hotelName = HotelNameEntry.Text;
            var city = CityEntry.Text;
            var checkInDate = CheckInDatePicker.Date;
            var checkOutDate = CheckOutDatePicker.Date;
            Console.WriteLine($"userid is  in hotel1 {userId}");
            if (string.IsNullOrEmpty(hotelName) || string.IsNullOrEmpty(city))
            {
                await DisplayAlert("Error", "Please enter both hotel name and city.", "OK");
                return;
            }
            var checkIn = CheckInDatePicker.Date.ToString("yyyy-MM-dd");
            var checkOut = CheckOutDatePicker.Date.ToString("yyyy-MM-dd");
            string hotelSearch = $"{hotelName} {city}";
            var hotelDetailsList = await HotelService.GetHotelDetailsAsync(hotelSearch, checkIn, checkOut);
            if (hotelDetailsList != null && hotelDetailsList.Count > 0)
            {
                var hotel = hotelDetailsList[0]; // Take the first hotel

                HotelResultStack.IsVisible = true;
                NameLabel.Text = hotel.HotelName;
                AddressLabel.Text = hotel.Address;
                HotelClassLabel.Text = hotel.HotelClass.ToString();
                PhoneLabel.Text = $"Phone: {hotel.Phone}";
                LatitudeLabel.Text = hotel.Latitude.ToString();
                LongitudeLabel.Text = hotel.Longitude.ToString();
                NearbyPlacesLabel.Text = hotel.NearbyPlaces;
                HotelAmenitiesLabel.Text = string.Join(", ", hotel.Amenities);
                Star1.IsVisible = hotel.HotelClass >= 1;
                Star2.IsVisible = hotel.HotelClass >= 2;
                Star3.IsVisible = hotel.HotelClass >= 3;
                Star4.IsVisible = hotel.HotelClass >= 4;
                Star5.IsVisible = hotel.HotelClass >= 5;


                if (!string.IsNullOrEmpty(hotel.LogoUrl))
                {
                    HotelImage.Source = hotel.LogoUrl;
                }

            }
            else
            {
                await DisplayAlert("No Results", "No hotels found matching your criteria.", "OK");
            }

        }

        private async void OnHotelTapped(object sender, EventArgs e)
        {
            var answer = await DisplayAlert("Add Hotel", "Do you want to add this Hotel to your trip?", "Yes", "No");
            Console.WriteLine($"userid is  in hotel6 {userId}");
            if (answer)
            {
                var hotel = new Hotel
                {
                    UserId = userId,
                    HotelName = NameLabel.Text,
                    Address = AddressLabel.Text,
                    Phone = PhoneLabel.Text.Replace("Phone: ", ""),
                    CheckInDate = CheckInDatePicker.Date.ToString("yyyy-MM-dd"),
                    CheckOutDate = CheckOutDatePicker.Date.ToString("yyyy-MM-dd"),
                    LogoUrl = HotelImage.Source.ToString(),
                    Class = int.TryParse(HotelClassLabel.Text, out var hotelClass) ? hotelClass : 1,
                    NearbyPlaces = NearbyPlacesLabel.Text.Replace("Nearby: ", ""),
                    Latitude = double.TryParse(LatitudeLabel.Text, out var lat) ? lat : 0.0,
                    Longitude = double.TryParse(LongitudeLabel.Text, out var lon) ? lon : 0.0,
                    Amenities = HotelAmenitiesLabel.Text,

                };
                DatabaseHelper.AddHotel(hotel, userId);
                
                await DisplayAlert("Success", "Hotel added to your trip!", "OK");
                bool addAnotherHotel = await DisplayAlert("Add Another Hotel?", "Do you want to add another Hotel?", "Yes", "No");
                if(addAnotherHotel)
                {
                    NameLabel.Text = string.Empty;
                    CityEntry.Text = string.Empty;
                    CheckInDatePicker.Date = DateTime.Now;
                    CheckOutDatePicker.Date = DateTime.Now;


                }
                else
                {
                    await Navigation.PushAsync(new HomePage(userId));
                }
            }
            // TODO: fix data colors and show full address


        }
    }
}


