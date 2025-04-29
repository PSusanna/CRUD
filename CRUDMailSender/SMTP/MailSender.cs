using CRUDShared.Settings;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace CRUDMailSender.SMTP
{
    public class MailSender
    {
        private readonly SMTPConfig _settings;

        public MailSender(IOptions<SMTPConfig> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using (SmtpClient client = new SmtpClient(_settings.Server, _settings.Port))
                {
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
                    client.EnableSsl = true;

                    MailMessage mailMessage = new MailMessage()
                    {
                        From = new MailAddress(_settings.From),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = false,
                    };
                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    Console.WriteLine("Email sent successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }
    }
}
