using TravelMate;
using Microsoft.Maui.Controls;

namespace TravelMate
{
    public partial class WelcomePopUp : ContentPage
    {
        private int userId { get; set; }
        public string UserName { get; set; }
        public WelcomePopUp(int UserId)
        {
            InitializeComponent();
            userId = UserId;
            BindingContext = this;
            LoadUserName();
        }
        // TODO: remove shadow and fix username not appering
        // On button click, navigate to the login page or the desired page
        private async void OnStartAddingClicked(object sender, EventArgs e)
        {
            WelcomeLayout.IsVisible = false;
            await Navigation.PopModalAsync();
            await Task.Delay(100);
            await Navigation.PushAsync(new NewFlightPage(userId));
        }
        private async void LoadUserName()
        {
            // Fetch the username from the database using the userId
            string userName = await DatabaseHelper.GetUserNameById(userId);

            // If we found a valid userName, set the UserName property
            if (!string.IsNullOrEmpty(userName))
            {
                // Set the UserName property to "Welcome {userName} ✈️"
                UserName = $"Welcome {userName} ✈️";
            }
            else
            {
                // If no userName is found, set a default welcome message
                UserName = "Welcome Traveler ✈️";
            }

            BindingContext = null;  // Clear first
            BindingContext = this;
        }
    }
}
