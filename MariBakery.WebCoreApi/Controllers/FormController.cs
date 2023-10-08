using MariBakery.WebCoreApi.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using Telegram.Bot;

namespace MariBakery.WebCoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FormController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public FormController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("submit")]
        public IActionResult SubmitForm([FromBody] FormData formData)
        {
            try
            {
                string toEmail = "maribakery.wroclaw@gmail.com"; // Replace with the recipient's email address

                var emailSettings = _configuration.GetSection("EmailSettings").Get<EmailSettings>();
                var botSettings = _configuration.GetSection("BotSettings").Get<BotSettings>();

                var botClient = new Telegram.Bot.TelegramBotClient(botSettings.Token);

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(emailSettings.UserName);
                    message.To.Add(toEmail);
                    message.Subject = "Form Submission";
                    message.Body = $"Name: {formData.Name}\nNumber: {formData.Number}";

                    using (var smtpClient = new SmtpClient(emailSettings.SmtpServer))
                    {
                        smtpClient.Port = emailSettings.Port;
                        smtpClient.Credentials = new NetworkCredential(emailSettings.UserName, emailSettings.Password);
                        smtpClient.EnableSsl = true; // Use SSL for secure email sending

                        smtpClient.Send(message);   
                    }

                    botClient.SendTextMessageAsync(botSettings.ChatId, message.Body);
                }
                return Ok("Form submitted successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
