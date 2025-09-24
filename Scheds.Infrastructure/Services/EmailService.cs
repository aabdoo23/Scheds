using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scheds.Application.Interfaces.Services;
using System.Net;
using System.Net.Mail;

namespace Scheds.Infrastructure.Services
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

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            if (string.IsNullOrEmpty(toEmail))
            {
                _logger.LogWarning("Email address is null or empty; skipping email.");
                return;
            }

            var sanitized = System.Text.RegularExpressions.Regex.Replace(toEmail, @"[^a-zA-Z0-9@._\-+]", "");
            if (sanitized != toEmail)
            {
                _logger.LogWarning("Sanitized email: '{original}' -> '{sanitized}' (Bytes: {bytes})", toEmail, sanitized, BitConverter.ToString(System.Text.Encoding.UTF8.GetBytes(sanitized)));
            }
            toEmail = sanitized.Trim();

            var smtpSection = _config.GetSection("Smtp");
            var host = smtpSection.GetValue<string>("Host");
            var port = smtpSection.GetValue<int?>("Port") ?? 25;
            var enableSsl = smtpSection.GetValue<bool?>("EnableSsl") ?? false;
            var user = smtpSection.GetValue<string>("User");
            var pass = smtpSection.GetValue<string>("Password");

            var fromRaw = smtpSection.GetValue<string>("From") ?? user ?? throw new InvalidOperationException("From email address is not configured");
            
            var fromSanitized = System.Text.RegularExpressions.Regex.Replace(fromRaw, @"[^a-zA-Z0-9@._\-+]", "").Trim();
            if (fromSanitized != fromRaw)
            {
                _logger.LogWarning("Sanitized FROM address: '{original}' -> '{sanitized}' (Bytes: {bytes})", fromRaw, fromSanitized, BitConverter.ToString(System.Text.Encoding.UTF8.GetBytes(fromSanitized)));
            }
            var from = fromSanitized;

            if (!IsValidEmail(from))
            {
                _logger.LogError("Invalid FROM email address: '{from}' (Bytes: {bytes})", from, BitConverter.ToString(System.Text.Encoding.UTF8.GetBytes(from)));
                throw new InvalidOperationException($"Configured FROM address is invalid: {from}");
            }

            if (string.IsNullOrEmpty(host))
            {
                _logger.LogWarning("SMTP host not configured; skipping email.");
                return;
            }

            if (!IsValidEmail(toEmail))
            {
                _logger.LogWarning("Invalid email address format: '{toEmail}' (Length: {length}, Bytes: {bytes}); skipping email.", toEmail, toEmail.Length, BitConverter.ToString(System.Text.Encoding.UTF8.GetBytes(toEmail)));
                return;
            }

            try
            {
                using var msg = new MailMessage();
                msg.From = new MailAddress(from);
                msg.To.Add(new MailAddress(toEmail));
                msg.Subject = subject;
                msg.Body = htmlBody ?? string.Empty;
                msg.IsBodyHtml = true;

                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = enableSsl
                };

                if (!string.IsNullOrEmpty(user))
                {
                    client.Credentials = new NetworkCredential(user, pass);
                }

                await client.SendMailAsync(msg);
                _logger.LogInformation("Email sent to {to}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {to}", toEmail);
                throw;
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
