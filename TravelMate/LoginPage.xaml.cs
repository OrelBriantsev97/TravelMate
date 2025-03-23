
using TravelMate.Models;

namespace TravelMate
{
    /// Represents the login page for new and existing users.
    /// Flow:
    /// - New user: Displays welcome popup -> Navigates to NewFlightPage -> NewHotelPage -> HomePage
    /// - Existing user: Navigates directly to HomePage
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        /// Handles the login button click event.
        /// Retrieves user input for email and password.
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text;
            var password = PasswordEntry.Text;
            int userId = -1;
            string errorMessage = await DatabaseHelper.CheckCredentials(email, password);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                await DisplayAlert("Error", "errorMessage", "OK");
                return;
            }
            userId = await DatabaseHelper.GetUserIdByEmail(email);
            if (userId != -1)
            {
                await Navigation.PushAsync(new HomePage(userId));
            }
            else
            {
                await DisplayAlert("Error", "User ID does not exist.", "OK");
            }
        }

        /// Handles the "Sign Up"  tap event.
        private void OnSignUpTapped(object sender, EventArgs e)
        {
            LoginForm.IsVisible = false;
            SignUpForm.IsVisible = true;
        }

        /// Handles the "ForgotPassword"  tap event.
        private void OnForgotPasswordTapped(object sender, EventArgs e)
        {
            // TODO: Implement navigation to Forgot Password Page
            // Navigation.PushAsync(new ForgotPasswordPage());
        }
        private void OnLoginTapped(object sender, EventArgs e)
        {
            SignUpForm.IsVisible = false;
            LoginForm.IsVisible = true;
        }

        /// Handles the sign-up button click event.
        /// Validates user input, registers a new user, and navigates to NewFlightPage .

        private async void OnSignUpClicked(object sender, EventArgs e)
        {
            var name = UsernameEntry.Text;
            var email = SignUpEmailEntry.Text;
            var password = SignUpPasswordEntry.Text;
            var confirmPassword = ConfirmPasswordEntry.Text;

            string errorMessage = ValidationHelper.UserValidation(email, password, confirmPassword);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                await DisplayAlert("Error", "errorMessage", "OK");
                return;
            }
            bool exist = await DatabaseHelper.IsEmailExist(email);
            if (exist)
            {
                await DisplayAlert("Error", "email already exist", "OK");
                return;
            }

            var newUser = new User
            {
                Name = name,
                Email = email,
                Password = password,
            };
            DatabaseHelper.AddNewUser(newUser);
            var userID = newUser.Id;

            var popup = new WelcomePopUp(userID);
            await Navigation.PushModalAsync(popup);

            await Navigation.PushAsync(new NewFlightPage(userID));

        }
    }
}