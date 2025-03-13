using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace TravelMate
{
    public partial class MyProfilePage : ContentPage
    {
        private readonly int userId;

        public MyProfilePage(int userId)
        {
            InitializeComponent();
            this.userId = userId;
        }

        private async void UpdateName(object sender, EventArgs e)
        {
            string newName = NameEntry.Text;

            if (string.IsNullOrWhiteSpace(newName))
            {
                await DisplayAlert("Error", "Name cannot be empty.", "OK");
                return;
            }

            // Call method to update the name in the database (replace with actual DB logic)
            bool success = await UpdateUserName(userId, newName);

            if (success)
                await DisplayAlert("Success", "Name updated successfully.", "OK");
            else
                await DisplayAlert("Error", "Failed to update name.", "OK");
        }

        private async void UpdatePassword(object sender, EventArgs e)
        {
            string oldPassword = OldPasswordEntry.Text;
            string newPassword = NewPasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                await DisplayAlert("Error", "Both fields must be filled.", "OK");
                return;
            }

            // Call method to update password in database (replace with actual DB logic)
            bool success = await UpdateUserPassword(userId, oldPassword, newPassword);

            if (success)
                await DisplayAlert("Success", "Password updated successfully.", "OK");
            else
                await DisplayAlert("Error", "Failed to update password. Check your old password.", "OK");
        }

        // Dummy method to simulate DB update (replace with actual database logic)
        private Task<bool> UpdateUserName(int userId, string newName)
        {
            // Simulate successful update
            return Task.FromResult(true);
        }

        private Task<bool> UpdateUserPassword(int userId, string oldPassword, string newPassword)
        {
            // Simulate successful password change
            return Task.FromResult(true);
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