using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using TravelMate.Controls;

namespace TravelMate
{
    public partial class MyProfilePage : ContentPage
    {
        private readonly int userId;

        public MyProfilePage(int userId,string destination)
        {
            InitializeComponent();
            this.userId = userId;
            var navBar = new NavigationBar(userId, destination);
            NavigationContainer.Content = navBar;
        }

        private async void UpdateName(object sender, EventArgs e)
        {
            string newName = NameEntry.Text;

            if (string.IsNullOrWhiteSpace(newName))
            {
                await DisplayAlert("Error", "Name cannot be empty.", "OK");
                return;
            }

            bool success = await DatabaseHelper.UpdateUserName(userId, newName);

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

            bool success = await DatabaseHelper.UpdateUserPassword(userId, oldPassword, newPassword);

            if (success)
                await DisplayAlert("Success", "Password updated successfully.", "OK");
            else
                await DisplayAlert("Error", "Failed to update password. Check your old password.", "OK");
        }


    }
}