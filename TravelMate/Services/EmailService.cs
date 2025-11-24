
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TravelMate.Services
{
    // Provides functionality to send transactional emails via the SendPulse API.
    public class EmailService
    {

        private const string ApiUserId = ""; 
        private const string ApiSecret = "";  

        private const string SendPulseApiUrl = "https://api.sendpulse.com";
        private static readonly HttpClient _httpClient = new HttpClient();

        // Sends an email to the specified recipient. If param "isPasswordReset" is true,
        // a 6-digit PIN code for password reset is included in the body.
        // <param name="toEmail">The recipient's email address.</param>
        // <param name="subject">The email subject line.</param>
        // <param name="isPasswordReset">Determines if the email is for password reset (includes PIN).</param>
        // <returns>True if the email was sent successfully; otherwise, false.</returns>
        public async Task<bool> SendEmailAsync(string toEmail, string subject, bool isPasswordReset = false)
        {
            try
            {
                // Step 1: Authenticate and get access token
                var accessToken = await GetAccessToken();

                if (string.IsNullOrEmpty(accessToken))
                {
                    Console.WriteLine("Authentication failed.");
                    return false;
                }

                // Step 2: Prepare the email data
                string pinCode = string.Empty;

                if (isPasswordReset)
                {
                    pinCode = GeneratePinCode();  // Generate a 6-digit PIN
                }

                string emailBody = isPasswordReset
                    ? $"<p>Your password reset PIN code is: <strong>{pinCode}</strong></p>"
                    : $@"
                    <div style='font-family: Arial, sans-serif; text-align: center; padding: 20px;'>
                        <img src='https://yourapp.com/images/mainlogo.png' alt='TravelMate Logo' width='200' style='margin-bottom: 20px;'/>
                        <h1 style='color: #32545c;'>Hi Traveler! ✈️ Welcome to TravelMate</h1>
                        <p style='font-size: 18px; color: #555;'>We're thrilled to have you onboard! 🎉</p>
                        <p style='font-size: 16px; color: #666;'>Let’s start your journey now!</p>
                        <a href='https://yourapp.com' style='display: inline-block; background-color: #32545c; color: white; padding: 10px 20px; text-decoration: none; font-size: 18px; border-radius: 5px;'>Get Started</a>
                        <p style='margin-top: 20px; font-size: 14px; color: #888;'>Safe travels, The TravelMate Team</p>
                    </div>";

                var emailData = new
                {
                    subject = subject,
                    from = new { email = "travel365mate@gmail.com", name = "TravelMate" },
                    to = new[] { new { email = toEmail, name = toEmail } },
                    html = emailBody
                };

                // Step 3: Send the email using the SendPulse API
                var sendEmailResult = await SendEmailAsync(accessToken, emailData);
                return sendEmailResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }

        // Generates a random 6-digit PIN code for password reset.
        // <returns> A string representation of the 6-digit PIN.</returns>
        private string GeneratePinCode()
        {
            Random rand = new Random();
            int pinCode = rand.Next(100000, 999999);  // Generate a random 6-digit number
            return pinCode.ToString();
        }

        // Authenticates with SendPulse and retrieves an OAuth access token.
        //<returns>The access token string if successful; otherwise, an empty string.</returns>
        private async Task<string> GetAccessToken()
        {
            try
            {
                var authUrl = $"{SendPulseApiUrl}/oauth/access_token";
                var requestData = new StringContent(
                    $"grant_type=client_credentials&client_id={ApiUserId}&client_secret={ApiSecret}",
                    Encoding.UTF8,
                    "application/x-www-form-urlencoded"
                );

                var response = await _httpClient.PostAsync(authUrl, requestData);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseJson = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return responseJson.access_token;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication failed: {ex.Message}");
                return string.Empty;
            }
        }

        // Sends the prepared email payload to the SendPulse SMTP API.
        /// <param name="accessToken">The OAuth access token.</param>
        /// <param name="emailData">An object representing the email payload.</param>
        /// <returns>True if the HTTP call succeeded; otherwise, false.</returns>
        private async Task<bool> SendEmailAsync(string accessToken, object emailData)
        {
            try
            {
                var emailUrl = $"{SendPulseApiUrl}/smtp/email";
                var requestData = new StringContent(
                    JsonConvert.SerializeObject(emailData),
                    Encoding.UTF8,
                    "application/json"
                );

                requestData.Headers.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.PostAsync(emailUrl, requestData);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }
    }
}
