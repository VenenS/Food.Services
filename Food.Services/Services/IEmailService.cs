using System.Threading.Tasks;

namespace Food.Services.Services
{
    public interface IEmailService
    {
        /// <summary>
        /// Отправляет сообщение на указанный email.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="subject"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        Task SendAsync(string message, string subject, string to, bool html = true);
    }
}