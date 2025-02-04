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
            LoadUserNameAsync();
        }

        // On button click, navigate to the login page or the desired page
        private async void OnStartAddingClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
            await Navigation.PushAsync(new NewFlightPage(userId));
        }
        private async void LoadUserNameAsync()
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

            // Notify the UI that the UserName property has changed, which will refresh the Label
            OnPropertyChanged(nameof(UserName));
        }
    }
}
