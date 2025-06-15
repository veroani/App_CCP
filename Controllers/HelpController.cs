using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using App_CCP.Services.Config;
using Microsoft.Extensions.Options;

namespace App_CCP.Controllers
{
    public class HelpController : Controller
    {
        private readonly EmailSettings _emailSettings;

        public HelpController(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public IActionResult Index() => View();

        public IActionResult FAQ() => View();

        public IActionResult Contact() => View();

        [HttpPost]
        public async Task<IActionResult> Contact(string nume, string email, string mesaj)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_emailSettings.FromEmail))
                {
                    throw new InvalidOperationException("FromEmail is not configured in EmailSettings.");
                }

                var mail = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail!, _emailSettings.FromName),
                    Subject = $"Solicitare suport de la {nume}",
                    Body = $"Mesaj: {mesaj}\n\nEmail expeditor: {email}",
                    IsBodyHtml = false
                };

                mail.To.Add(_emailSettings.SupportEmail!);

                using var smtp = new SmtpClient(_emailSettings.SmtpServer)
                {
                    Port = _emailSettings.SmtpPort,
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                    EnableSsl = true
                };

                await smtp.SendMailAsync(mail);
                ViewBag.Message = "Mesaj trimis cu succes!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Eroare la trimiterea mesajului: " + ex.Message;
            }

            return View();
        }
    }
}