using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace KIDIO.Business.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public EmailService(IConfiguration config, ILogger<EmailService> logger, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var apiKey      = _config["EmailSettings:ApiKey"] ?? "";
            var senderEmail = _config["EmailSettings:SenderEmail"] ?? "";
            var senderName  = _config["EmailSettings:SenderName"] ?? "KIDIO";

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(senderEmail))
            {
                _logger.LogWarning("Brevo Email service is not configured (missing API Key or Sender Email). Email to {To} with subject '{Subject}' was NOT sent.", toEmail, subject);
                throw new AppException("Email service is not configured. Please contact the administrator.", 503);
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
                requestMessage.Headers.Add("api-key", apiKey);
                requestMessage.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var payload = new
                {
                    sender = new { name = senderName, email = senderEmail },
                    to = new[] { new { email = toEmail } },
                    subject = subject,
                    htmlContent = body
                };

                var json = JsonSerializer.Serialize(payload);
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.SendAsync(requestMessage);
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Brevo API returned error status code {StatusCode}: {Error}", response.StatusCode, errorResponse);
                    throw new AppException($"Failed to send email via Brevo API (Status: {response.StatusCode}).", 503);
                }

                _logger.LogInformation("Email sent successfully via Brevo API to {To} — subject: '{Subject}'", toEmail, subject);
            }
            catch (AppException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending email via Brevo to {To}", toEmail);
                throw new AppException("Failed to send email. Please try again later.", 503);
            }
        }
    }
}