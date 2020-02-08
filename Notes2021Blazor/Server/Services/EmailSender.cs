using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.IO;
using System.Threading.Tasks;

namespace Notes2021Blazor.Server.Services
{
    public class EmailSender : IEmailSender
    {
        public StreamWriter StreamWriter { get; private set; }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var apiKey = Globals.SendGridApiKey;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(Globals.SendGridEmail, Globals.EmailName);
            var to = new EmailAddress(email);
            var htmlStart = "<!DOCTYPE html>";
            var isHtml = message.StartsWith(htmlStart);
            SendGridMessage msg = MailHelper.CreateSingleEmail(from, to, subject, isHtml ? "See Html." : message, message);

            if (isHtml)
            {
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                await sw.WriteAsync(message);
                await sw.FlushAsync();
                ms.Seek(0, SeekOrigin.Begin);
                await msg.AddAttachmentAsync("FromNotes2021.html", ms);
            }

            Response response = await client.SendEmailAsync(msg);
        }

        //public async Task SendEmailListAsync(List<string> emails, string subject, string message)
        //{
        //    var apiKey = Globals.SendGridApiKey;
        //    var client = new SendGridClient(apiKey);
        //    var from = new EmailAddress(Globals.SendGridEmail, Globals.EmailName);
        //    List<EmailAddress> tos = new List<EmailAddress>();

        //    foreach (string item in emails)
        //    {
        //        tos.Add(new EmailAddress(item));
        //    }

        //    var htmlStart = "<!DOCTYPE html>";
        //    var isHtml = message.StartsWith(htmlStart);

        //    SendGridMessage msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, isHtml ? "See Html." : message, message);


        //    // ReSharper disable once UnusedVariable
        //    var response = await client.SendEmailAsync(msg);
        //}

    }
}
