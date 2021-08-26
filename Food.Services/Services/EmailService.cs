using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Food.Services.Config;

namespace Food.Services.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfigureSettings _config;
        private readonly SmtpClient _smtp;

        public EmailService(IConfigureSettings config)
        {
            _config = config;
            _smtp = new SmtpClient(config.Email.Host, config.Email.Port);
            _smtp.EnableSsl = config.Email.EnableSsl;

            if (_config.Email.Login != null && _config.Email.Password != null)
                _smtp.Credentials = new NetworkCredential(_config.Email.Login, _config.Email.Password);
        }

        public async Task SendAsync(string message, string subject, string to, bool html = true)
        {
            var msg = new MailMessage();
            msg.To.Add(new MailAddress(to));
            msg.From = new MailAddress(_config.Email.FromAddress, _config.Email.DisplayName);
            msg.Subject = subject;
            msg.Body = message;
            msg.IsBodyHtml = html;
            await _smtp.SendMailAsync(msg);
        }
    }
}