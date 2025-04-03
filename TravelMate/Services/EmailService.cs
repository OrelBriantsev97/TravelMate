using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TravelMate.Services
{
    public class EmailService
    {
        private const string ApiUserId = "b064f3200a2883c533da0ae6ae0f0ea0";  // Replace with your API User ID
        private const string ApiSecret = "3d02e0948a8483daaa1c5ff6dae02180";  // Replace with your API Secret

        private const string SendPulseApiUrl = "https://api.sendpulse.com";
        private static readonly HttpClient _httpClient = new HttpClient();

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

        private string GeneratePinCode()
        {
            Random rand = new Random();
            int pinCode = rand.Next(100000, 999999);  // Generate a random 6-digit number
            return pinCode.ToString();
        }

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
