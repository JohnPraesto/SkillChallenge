using SkillChallenge.Interfaces;
using System.Net;
using System.Net.Mail;

namespace SkillChallenge.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpUser;
        private readonly string _smtpSecret;
        private readonly string _smtpFromEmail;

        public EmailService(IConfiguration config)
        {
            _smtpUser = config["Mailjet:Key"];
            _smtpSecret = config["Mailjet:Secret"];
            _smtpFromEmail = config["Mailjet:FromEmail"];
        }

        public async Task SendEmailAsync(string toEmail, string subject, string bodyHtml)
        {
            using (var client = new SmtpClient("in.mailjet.com", 587))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(_smtpUser, _smtpSecret);

                var mail = new MailMessage();
                mail.From = new MailAddress(_smtpFromEmail, "SkillChallenge");
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.Body = bodyHtml;
                mail.IsBodyHtml = true;

                await client.SendMailAsync(mail);
            }
        }
    }
}
