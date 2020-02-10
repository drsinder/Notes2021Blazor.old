using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Notes2021.Services
{
    //public interface IEmailSender
    //{
    //    Task SendEmailListAsync(List<string> emails, string subject, string htmlMessage);
    //}

    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var apiKey = Globals.SendGridApiKey;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(Globals.SendGridEmail, Globals.EmailName);
            var to = new EmailAddress(email);
            var htmlStart = "<!DOCTYPE html>";
            var isHtml = message.StartsWith(htmlStart);
            SendGridMessage msg = MailHelper.CreateSingleEmail(from, to, subject, isHtml ? "See Html." : message, message);

            // ReSharper disable once UnusedVariable
            Response response = await client.SendEmailAsync(msg);
        }

        public async Task SendEmailListAsync(List<string> emails, string subject, string message)
        {
            var apiKey = Globals.SendGridApiKey;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(Globals.SendGridEmail, Globals.EmailName);
            List<EmailAddress> tos = new List<EmailAddress>();

            foreach (string item in emails)
            {
                tos.Add(new EmailAddress(item));
            }

            var htmlStart = "<!DOCTYPE html>";
            var isHtml = message.StartsWith(htmlStart);

            SendGridMessage msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, isHtml ? "See Html." : message, message);


            // ReSharper disable once UnusedVariable
            var response = await client.SendEmailAsync(msg);
        }

    }
}
