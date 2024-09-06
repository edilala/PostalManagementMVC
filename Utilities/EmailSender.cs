using MailKit.Net.Smtp;
using MimeKit;
using PostalManagementMVC.Interfaces;

namespace PostalManagementMVC.Utilities
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string body)
        {
            //https://support.google.com/accounts/answer/185833?hl=en&sjid=9130859724922440077-EU
            // enable 2 step verification
            // add app passwords
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Postal Management MVC", "developertirana@gmail.com"));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = subject;

            message.Body = new TextPart("plain")
            {
                Text = body
            };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate("developertirana@gmail.com", "sadm rygw mivc hmcc");

                client.Send(message);
                client.Disconnect(true);
                
                return Task.CompletedTask;
            }
        }
    }
}
