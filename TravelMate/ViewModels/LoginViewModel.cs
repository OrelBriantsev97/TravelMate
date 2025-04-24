using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TravelMate.Models;
using TravelMate.Services;
using System.Threading.Tasks;
using System;
using System.Net.Mail;
using System.Net;

namespace TravelMate.ViewModels
{
    /// ViewModel responsible for handling user login, signup and PIN verification flows.
    /// Implements INotifyPropertyChanged for data binding in XAML.
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string email;
        private string username;
        private string password;
        private string confirmPassword;
        private string pin;
        private string generatedPin;
        private int pinAttempts = 0;
        private const int MaxPinAttempts = 2;
        private bool isPinSectionVisible;
        private int userId = -1;
        private bool isLoginVisible = true;
        private bool isSignupVisible = false;
        private bool isForgotPasswordVisible;
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }

        private readonly EmailService emailService = new EmailService();

        // The email address entered by the user.
        public string Email
        {
            get => email;
            set { email = value; OnPropertyChanged(); }
        }
        // The password entered by the user
        public string Password
        {
            get => password;
            set { password = value; OnPropertyChanged(); }
        }

        //The PIN code entered by the user for verification.
        public string Pin
        {
            get => pin;
            set { pin = value; OnPropertyChanged(); }
        }

        // The username entered by the user during signup.
        public string Username
        {
            get => username;
            set { username = value; OnPropertyChanged(); }
        }

        // The confirmation password entered during signup.
        public string ConfirmPassword
        {
            get => confirmPassword;
            set { confirmPassword = value; OnPropertyChanged(); }
        }

        // Controls visibility of the PIN entry section.
        public bool IsPinSectionVisible
        {
            get => isPinSectionVisible;
            set { isPinSectionVisible = value; OnPropertyChanged(); }
        }
        // Controls visibility of the Login entry section.
        public bool IsLoginVisible
        {
            get => isLoginVisible;
            set { isLoginVisible = value; OnPropertyChanged(); }
        }

        // Controls visibility of the SignUp entry section.
        public bool IsSignupVisible
        {
            get => isSignupVisible;
            set { isSignupVisible = value; OnPropertyChanged(); }
        }

        // Controls visibility of the forgotPassword entry section.
        public bool IsForgotPasswordVisible
        {
            get => isForgotPasswordVisible;
            set { isForgotPasswordVisible = value; OnPropertyChanged(); }
        }
        public ICommand LoginCommand { get; } // execute the login process
        public ICommand SignupCommand { get; }  //execute the signup process
        public ICommand VerifyPinCommand { get; } // execute the verification process
        public ICommand ToggleLoginSignupCommand { get; }
        public ICommand ShowForgotPasswordCommand { get; }
        public ICommand ResetPasswordCommand { get; }

        //Initializes a new instance of the LoginViewModel
        public LoginViewModel()
        {
            LoginCommand = new Command(async () => await Login());
            SignupCommand = new Command(async () => await Signup());
            VerifyPinCommand = new Command(async () => await VerifyPin());
            ToggleLoginSignupCommand = new Command(ToggleLoginSignup);
            ShowForgotPasswordCommand = new Command(ShowForgotPassword);
            ResetPasswordCommand = new Command(async () => await ResetPassword());
        }

        /// Attempts to log in the user by validating credentials against the database.
        /// Navigates to HomePage on success or shows an error alert.
        private async Task Login()
        {
            userId = await DatabaseHelper.GetUserIdByEmail(Email);
            var result = await DatabaseHelper.CheckCredentials(Email, Password);

            if (!string.IsNullOrEmpty(result))
            {
                await App.Current.MainPage.DisplayAlert("Error", result, "OK");
                return;
            }

            if (userId != -1)
            {
                await App.Current.MainPage.Navigation.PushAsync(new HomePage(userId));
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", "User not found. Please sign up.", "OK");
            }
        }

        /// Handles user signup: validates input, checks email uniqueness,
        /// generates a PIN, sends it via email, and shows the PIN entry section.
        private async Task Signup()
        {
            var errorMessage = ValidationHelper.UserValidation(Email, Password, ConfirmPassword);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                await App.Current.MainPage.DisplayAlert("Error", errorMessage, "OK");
                return;
            }
            bool exists = await DatabaseHelper.IsEmailExist(Email);
            if (exists)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Email already exists", "OK");
                return;
            }

            var newUser = new User { Name = Username, Email = Email, Password = Password };
           

            generatedPin = GeneratePin();
            await SendPinEmail(Email, Username, generatedPin);
            await DatabaseHelper.AddNewUser(newUser);
            userId = newUser.Id;
            IsSignupVisible = false;
            IsPinSectionVisible = true;

        }

        // Verifies the entered PIN against the generated one.
        // Allows up to MaxPinAttempts tries before forcing a restart.
        private async Task VerifyPin()
        {
            if (Pin == generatedPin)
            {
                var popup = new WelcomePopUp(userId);
                await App.Current.MainPage.Navigation.PushModalAsync(popup);
                await App.Current.MainPage.Navigation.PushAsync(new NewFlightPage(userId));
            }
            else
            {
                pinAttempts++;
                if (pinAttempts >= MaxPinAttempts)
                {
                    await App.Current.MainPage.DisplayAlert("Authentication Failed", "Please sign up again.", "OK");
                    IsPinSectionVisible = false;
                    IsSignupVisible = true;
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Error", "Incorrect PIN. Please try again.", "OK");
                }
            }
        }

        //Sends the generated PIN code to the user's email address.
        /// <param name="recipientEmail">The user's email.</param>
        /// <param name="username">The user's display name.</param>
        /// <param name="pin">The PIN code to send.</param>
        private async Task SendPinEmail(string recipientEmail, string username, string pin)
        {
            try
            {
                Environment.SetEnvironmentVariable("EMAIL_PASSWORD", "atsm eavo slgi feot", EnvironmentVariableTarget.Process);
                var emailPassword = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
                if (string.IsNullOrEmpty(emailPassword))
                    throw new Exception("SMTP email password is not set.");

                using (var smtpClient = new SmtpClient("smtp.gmail.com"))
                {
                    smtpClient.Port = 587;
                    smtpClient.Credentials = new NetworkCredential("travel365mate@gmail.com", emailPassword);
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("travel365mate@gmail.com"),
                        Subject = "Welcome to TravelMate! 🌍 Your PIN Code Inside",
                        Body = $"<h2>Welcome to TravelMate, {username}! 🎉</h2>" +
                               $"<p>We're excited to have you on board.</p>" +
                               $"<p>Your 6-digit PIN code is: <strong>{pin}</strong></p>",
                        IsBodyHtml = true,
                    };

                    mailMessage.To.Add(recipientEmail);
                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"Failed to send email: {ex.Message}", "OK");
            }
        }

        // Generates a random 6-digit PIN code.
        private string GeneratePin()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        // Toggles between showing the login and signup forms.
        private void ToggleLoginSignup()
        {
            IsLoginVisible = !IsLoginVisible;
            IsSignupVisible = !IsSignupVisible;
        }

        // Displays the "forgot password" view, hiding other sections.
        private void ShowForgotPassword()
        {
            IsLoginVisible = false;
            IsSignupVisible = false;
            IsPinSectionVisible = false;
            IsForgotPasswordVisible = true;
        }

        // Resets the user's password after validating input and matching confirmation.
        private async Task ResetPassword()
        {
            if (NewPassword != ConfirmNewPassword)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Passwords do not match.", "OK");
                return;
            }

            int userId = await DatabaseHelper.GetUserIdByEmail(Email);
            if (userId == -1)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Email not found.", "OK");
                return;
            }

            bool success = await DatabaseHelper.ChangePassword(userId, NewPassword);
            if (success)
            {
                await App.Current.MainPage.DisplayAlert("Success", "Password changed successfully.", "OK");
                IsForgotPasswordVisible = false;
                IsLoginVisible = true;
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", "Something went wrong.", "OK");
            }
        }

        // Raises the PropertyChanged event for data binding.
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
