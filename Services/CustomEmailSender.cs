using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using App_CCP.Services.Interfaces;
using App_CCP.Services.Config;
using Microsoft.Extensions.Options;

namespace App_CCP.Services
{
    public class CustomEmailSender : ICustomEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public CustomEmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            if (string.IsNullOrEmpty(_emailSettings.FromEmail))
            {
                throw new InvalidOperationException("Sender email address (FromEmail) is not configured properly.");
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = message,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);

            var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = true,
            };

            await client.SendMailAsync(mailMessage);
        }
    }
}