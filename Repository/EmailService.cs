using System.Net.Mail;
using System.Net;

namespace EcommApp.Repository
{
    public class EmailService : IEmailService
    {
        private readonly SmtpClient smtpClient;
        private readonly string fromAddress; // The email address you are sending from

        public EmailService(IConfiguration configuration)
        {
            smtpClient = new SmtpClient
            {
                Host = configuration["EmailSettings:Host"],
                Port = int.Parse(configuration["EmailSettings:Port"]),
                EnableSsl = bool.Parse(configuration["EmailSettings:EnableSsl"]),
                Credentials = new NetworkCredential(
                    configuration["EmailSettings:Username"],
                    configuration["EmailSettings:Password"])
            };
            fromAddress = configuration["EmailSettings:FromAddress"];
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromAddress),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }

}
