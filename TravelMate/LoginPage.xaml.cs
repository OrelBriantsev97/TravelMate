
using TravelMate.Models;

namespace TravelMate
{
    //login page for new and exisiting user.
    //flow is : new user -> welcome pop up -> NewFlightPage -> NewHotelPage ->HomePage
    //exsiting user -> HomePage
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void OnLoginClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text;
            var password = PasswordEntry.Text;



        }

        private void OnSignUpTapped(object sender, EventArgs e)
        {
            LoginForm.IsVisible = false;
            SignUpForm.IsVisible = true;
        }

        private void OnForgotPasswordTapped(object sender, EventArgs e)
        {
            // Navigate to Forgot Password Page
            //Navigation.PushAsync(new ForgotPasswordPage());
        }
        private async void OnSignUpClicked(object sender, EventArgs e)
        {
            var name = UsernameEntry.Text;
            var email = SignUpEmailEntry.Text;
            var password = SignUpPasswordEntry.Text;
            var confirmPassword = ConfirmPasswordEntry.Text;

            string errorMessage = ValidationHelper.UserValidation(email, password, confirmPassword);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                await DisplayAlert("Error", errorMessage, "OK");
                return;
            }
            var newUser = new User
            {
                Name = name,
                Email = email,
                Password = password,
                PhoneNumber = null
            };
            DatabaseHelper.AddNewUser(newUser);
            var userID = newUser.Id;

            var popup = new WelcomePopUp(userID);
            await Navigation.PushModalAsync(popup);

            await Navigation.PushAsync(new NewHotelPage(userID));

        }
    }
}
