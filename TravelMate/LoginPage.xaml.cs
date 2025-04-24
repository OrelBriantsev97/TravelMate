using System;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using TravelMate.Models;
using TravelMate.ViewModels;

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
        private readonly LoginViewModel _viewModel;

        /// Initializes the LoginPage.
        public LoginPage()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            BindingContext = new LoginViewModel();
        }
        


    }
}