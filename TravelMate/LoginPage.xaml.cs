using System;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using TravelMate.Models;

namespace TravelMate
{
    /// <summary>
    /// LoginPage handles user authentication, signup, and PIN verification.
    /// new user flow ->sign in -> send pin -> verify pin ->welcome pop up page
    /// exisiting user -> sign in -> home page
    /// </summary>
    public partial class LoginPage : ContentPage
    {
        private string _generatedPin;
        private int _pinAttempts = 0;
        private int userId = -1;
        private const int MaxPinAttempts = 2;
        private const string SenderEmail = "travel365mate@gmail.com";

        /// Initializes the LoginPage.
        public LoginPage()
        {
            InitializeComponent();
        }

        /// Handles login button click.
        /// Verifies user credentials and navigates to HomePage if successful.
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text;
            var password = PasswordEntry.Text;
            userId = await DatabaseHelper.GetUserIdByEmail(email);
            string errorMessage = await DatabaseHelper.CheckCredentials(email, password);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                await DisplayAlert("Error", errorMessage, "OK");
                return;
            }

            if (userId != -1)
            {
                // Existing user - Directly navigate to home
                await Navigation.PushAsync(new HomePage(userId));
            }
            else
            {
                await DisplayAlert("Error", "User not found. Please sign up.", "OK");
            }
        }

        // Handles user signup process.
        // Validates input, creates a new user, and sends a PIN for verification
        private async void OnSignUpClicked(object sender, EventArgs e)
        {
            var username = UsernameEntry.Text;
            var email = SignUpEmailEntry.Text;
            var password = SignUpPasswordEntry.Text;
            var confirmPassword = ConfirmPasswordEntry.Text;

            string errorMessage = ValidationHelper.UserValidation(email, password, confirmPassword);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                await DisplayAlert("Error", errorMessage, "OK");
                return;
            }

            bool exist = await DatabaseHelper.IsEmailExist(email);
            if (exist)
            {
                await DisplayAlert("Error", "Email already exists", "OK");
                return;
            }

            var newUser = new User { Name = username, Email = email, Password = password };
            await DatabaseHelper.AddNewUser(newUser);
            userId = newUser.Id;

            
            _generatedPin = GeneratePin();
            await SendPinEmail(email, username, _generatedPin);

            
            SignUpForm.IsVisible = false;
            PinInputForm.IsVisible = true;
        }

        // Sends a PIN code + welcome email to the user for verification.
        private async Task SendPinEmail(string recipientEmail, string username, string pin)
        {
            try
            {
                Environment.SetEnvironmentVariable("EMAIL_PASSWORD", "atsm eavo slgi feot", EnvironmentVariableTarget.Process);
                var emailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
                if (string.IsNullOrEmpty(emailPassword))
                    throw new Exception("SMTP email password is not set in environment variables.");

                using (var smtpClient = new SmtpClient("smtp.gmail.com"))
                {
                    smtpClient.Port = 587;
                    smtpClient.Credentials = new NetworkCredential(SenderEmail, emailPassword);
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(SenderEmail),
                        Subject = "Welcome to TravelMate! 🌍 Your PIN Code Inside",
                        Body = $"<h2>Welcome to TravelMate, {username}! 🎉</h2>" +
                               $"<p>We're excited to have you on board. Start exploring and planning your travels now!</p>"+
                               $"<p>Your 6-digit PIN code is: <strong>{pin}</strong></p>" +
                               $"<p>Please enter this PIN to complete your registration.</p>",
                        IsBodyHtml = true,
                    };

                    mailMessage.To.Add(recipientEmail);
                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to send email: {ex.Message}", "OK");
            }
        }

        // Handles PIN verification process.
        /// If correct, navigates to Welcome and Flight pages. Otherwise, tracks attempts.
        private async void OnPinVerifiedClicked(object sender, EventArgs e)
        {
            var enteredPin = PinCodeEntry.Text;

            if (enteredPin == _generatedPin)
            {
                var popup = new WelcomePopUp(userId);
                await Navigation.PushModalAsync(popup);

                await Navigation.PushAsync(new NewFlightPage(userId));
            }
            else
            {
                _pinAttempts++;

                if (_pinAttempts >= MaxPinAttempts)
                {
                    await DisplayAlert("Authentication Failed", "Please sign up again.", "OK");
                    PinInputForm.IsVisible = false;
                    LoginForm.IsVisible = true;
                }
                else
                {
                    await DisplayAlert("Error", "Incorrect PIN. Please try again.", "OK");
                }
            }
        }

        // Generates a random 6-digit PIN for authentication.
        private string GeneratePin()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        // Switches UI from login to signup form.
        private void OnSignUpTapped(object sender, EventArgs e)
        {
            LoginForm.IsVisible = false;
            SignUpForm.IsVisible = true;
        }

        // Switches UI from signup to login form.
        private void OnLoginTapped(object sender, EventArgs e)
        {
            SignUpForm.IsVisible = false;
            LoginForm.IsVisible = true;
        }
        private void OnForgotPasswordTapped(object sender, EventArgs e)
        {
            // TODO: Implement navigation to Forgot Password Page
            // Navigation.PushAsync(new ForgotPasswordPage());
        }
    }
}
