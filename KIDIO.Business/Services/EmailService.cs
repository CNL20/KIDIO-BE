using MailKit.Net.Smtp;
using MailKit;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using System.Net.Sockets;

namespace KIDIO.Business.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            // Validate cấu hình SMTP trước khi kết nối
            // Nếu AppPassword rỗng → throw AppException(503) thay vì để SmtpException bubble up thành 500
            var smtpServer  = _config["EmailSettings:SmtpServer"] ?? "";
            var portStr     = _config["EmailSettings:Port"] ?? "587";
            var senderEmail = _config["EmailSettings:SenderEmail"] ?? "";
            var senderName  = _config["EmailSettings:SenderName"] ?? "KIDIO";
            var appPassword = _config["EmailSettings:AppPassword"] ?? "";

            if (string.IsNullOrWhiteSpace(smtpServer) || string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(appPassword))
            {
                _logger.LogWarning("Email service is not configured (missing SMTP credentials). Email to {To} with subject '{Subject}' was NOT sent.", toEmail, subject);
                // Ném AppException với 503 thay vì để crash — caller quyết định có cần bail ra hay không
                throw new AppException("Email service is not configured. Please contact the administrator.", 503);
            }

            if (!int.TryParse(portStr, out var port))
                port = 587;

            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(senderName, senderEmail));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;
                email.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

                using var smtp = new SmtpClient();
                smtp.Timeout = 5000; // Set timeout to 5 seconds to prevent hanging on cloud platforms (e.g. Render) where SMTP ports are blocked
                await smtp.ConnectAsync(smtpServer, port, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(senderEmail, appPassword);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email sent to {To} — subject: '{Subject}'", toEmail, subject);
            }
            catch (AuthenticationException ex)
            {
                _logger.LogError(ex, "SMTP authentication failed for {Email}. Check EmailSettings:AppPassword.", senderEmail);
                throw new AppException("Email authentication failed. Please check the SMTP configuration.", 503);
            }
            catch (SmtpCommandException ex)
            {
                _logger.LogError(ex, "SMTP command error while sending email to {To}: {StatusCode}", toEmail, ex.StatusCode);
                throw new AppException($"Email delivery failed (SMTP error {ex.StatusCode}). Please try again later.", 503);
            }
            catch (SocketException ex)
            {
                _logger.LogError(ex, "Network error connecting to SMTP server {Server}:{Port}", smtpServer, port);
                throw new AppException("Cannot connect to email server. Please try again later.", 503);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending email to {To}", toEmail);
                throw new AppException("Failed to send email. Please try again later.", 503);
            }
        }
    }
}