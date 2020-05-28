using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
namespace VMenu.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        private readonly IHostingEnvironment _env;

        public EmailSender(IOptions<EmailSettings> emailSettings, IHostingEnvironment env)
        {
            _emailSettings = emailSettings.Value;
            _env = env;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            MailMessage message = new MailMessage();
            message.SubjectEncoding = System.Text.Encoding.UTF8;

            message.Subject = subject;
            message.Body = htmlMessage;
            message.IsBodyHtml = true;
            message.To.Add(email);

            string host = _emailSettings.SmtpServer;
            int port = _emailSettings.Port;
            string fromAddress = _emailSettings.From;
            message.From = new MailAddress(fromAddress);

            NetworkCredential NC = new NetworkCredential();
            NC.UserName = _emailSettings.Username;
            NC.Password = _emailSettings.Password;


            using (var smtpClient = new SmtpClient(host, port))
            {
                smtpClient.Credentials = NC;
                await smtpClient.SendMailAsync(message);
            }
        }
    }
}
