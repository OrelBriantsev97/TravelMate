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
        public int UserId { get; set; }
        public ObservableCollection<Hotel> HotelResults { get; set; } = new ObservableCollection<Hotel>();

        public NewHotelPage(int userId)
        {

            InitializeComponent();
            UserId = userId;
            this.BindingContext = this;
        }


        private async void OnSearchHotelClicked(object sender, EventArgs e)
        {
            var hotelName = HotelNameEntry.Text;
            var city = CityEntry.Text;
            var checkInDate = CheckInDatePicker.Date;
            var checkOutDate = CheckOutDatePicker.Date;

            if (string.IsNullOrEmpty(hotelName) || string.IsNullOrEmpty(city))
            {
                await DisplayAlert("Error", "Please enter both hotel name and city.", "OK");
                return;
            }
            var checkIn = CheckInDatePicker.Date.ToString("yyyy-MM-dd");
            var checkOut = CheckOutDatePicker.Date.ToString("yyyy-MM-dd");
            string hotelAndCity = $"{hotelName} {city}";
            var hotelDetailsList = await HotelService.GetHotelDetailsAsync(hotelName, checkIn, checkOut);
            if (hotelDetailsList != null && hotelDetailsList.Count > 0)
            {
                var hotel = hotelDetailsList[0]; // Take the first hotel

                HotelResultStack.IsVisible = true;
                HotelNameLabel.Text = hotel.HotelName;
                HotelAddressLabel.Text = hotel.Address;
                HotelRatingLabel.Text = $"Rating: {hotel.Rate}/5";
                HotelPhoneLabel.Text = $"Phone: {hotel.Phone}";

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
    }
}


