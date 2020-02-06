using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Notes2021Blazor.Client
{
    public static class EmailSender
    {
        public static async Task SendEmailAsync(string email, string subject, string message)
        {

            // get "Globals" From server

            var apiKey = Globals.SendGridApiKey;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(Globals.SendGridEmail, Globals.EmailName);
            var to = new EmailAddress(email);
            var htmlStart = "<!DOCTYPE html>";
            var isHtml = message.StartsWith(htmlStart);
            SendGridMessage msg = MailHelper.CreateSingleEmail(from, to, subject, isHtml ? "See Html." : message, message);

            Response response = await client.SendEmailAsync(msg);
        }
    }
}
