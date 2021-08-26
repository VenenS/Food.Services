using Food.Data.Entities;
using Food.Services.Config;
using Food.Services.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ITWebNet.FoodService.Food.DbAccessor;

namespace Food.Services
{
    /// <summary>
    ///     Email уведомление
    /// </summary>
    public class EmailNotification : NotificationBase
    {
        private IConfigureSettings ConfigureSettings { get; }

        public EmailNotification(IConfigureSettings configureSettings)
        {
            ConfigureSettings = configureSettings;
        }

        /// <summary>
        /// Формирование уведомления
        /// </summary>
        /// <param name="notification"></param>
        public override void FormNotification(NotificationBodyBase notification)
        {
            Notification = notification;
        }

        /// <summary>
        ///     Отправка уведомления на указанные в теле сообщения ящики.
        ///     Выставление статуса отправки уведомления
        /// </summary>
        protected override Task SendNotification()
        {
            return Task.Run(
            () =>
            {
                try
                {
                    using (var smtpClient = new SmtpClient())
                    {
                        smtpClient.Host = ConfigureSettings?.Email.Host;
                        smtpClient.Port = ConfigureSettings.Email.Port;
                        smtpClient.EnableSsl = ConfigureSettings.Email.EnableSsl;
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = new NetworkCredential(ConfigureSettings?.Email.Login,
                            ConfigureSettings?.Email.Password);
                        var msg = new MailMessage();

                        Notification
                            .GetReceiverAddress()
                            .ForEach(a => msg.To.Add(new MailAddress(a)));
                        msg.From = new MailAddress(ConfigureSettings?.Email.FromAddress,
                            ConfigureSettings?.Email.DisplayName);

                        msg.Subject = Notification.GetSubject();
                        msg.Body = Notification.GetMessageBody();
                        msg.IsBodyHtml = false;
                        smtpClient.Send(msg);
                    }
                }
                catch (SmtpException exc)
                {
                    Notification.SetSendError(exc);
                    Accessor.Instance.LogError("EmailNotification",
                        $"Письма небыли отправлены по адресам. Ошибка = {exc}," +
                        $" Время отправления - {DateTime.Now},");
                }
                catch (Exception exc)
                {
                    Notification.SetSendError(exc);
                    Accessor.Instance.LogError("EmailNotification",
                        $"Письма небыли отправлены по адресам. Ошибка = {exc}," +
                        $" Время отправления - {DateTime.Now},");
                }

                Notification.FormLogInfo();
            }
            );
        }

        /// <summary>
        /// Отправка емайл кураторам
        /// </summary>
        /// <param name="emailAddress">Адреса кураторов</param>
        /// <returns></returns>
        public override Task SendNotificationCurator((Dictionary<string, string>, string) emailAddress)
        {
            return Task.Run(
            () =>
            {
                try
                {
                    using (SmtpClient smtpClient = new SmtpClient())
                    {
                        smtpClient.Host = ConfigurationManager.Configuration.GetSection("Email:Host")?.Value;
                        smtpClient.Port = int.Parse(ConfigurationManager.Configuration.GetSection("Email:Port")?.Value);
                        smtpClient.EnableSsl = bool.Parse(ConfigurationManager.Configuration.GetSection("Email:EnableSSL")?.Value);
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = new NetworkCredential(
                            ConfigurationManager.Configuration.GetSection("Email:Login")?.Value,
                            ConfigurationManager.Configuration.GetSection("Email:Password")?.Value);
                        var msg = new MailMessage();


                        foreach (var item in emailAddress.Item1)
                        {
                            msg.To.Add(item.Key);
                            msg.From = new MailAddress(
                                ConfigurationManager.Configuration.GetSection("Email:FromAddress")?.Value,
                                ConfigurationManager.Configuration.GetSection("Email:DisplayName")?.Value);
                            msg.Subject = Notification.GetSubject();
                            msg.Body= ($"Добрый день {item.Value}.\n\n" +
                                      $"Уведомление для куратора компании {emailAddress.Item2} о завершении формирования корпоративного заказа.\n\n" +
                                      $"Для оформления Вашего корпоративного заказа" +
                                      $"осталось 30 минут.\nНа данный момент минимальная " +
                                      $"сумма для заказа не набрана. \nПожалуйста дополните заказ или заказ " +
                                      $"будет отменен кафе в связи с недостаточной суммой корпоративного заказа");
                            msg.IsBodyHtml = false;
                            smtpClient.Send(msg);

                            Accessor.Instance.LogInfo("EmailNotification",
                                $"Письмо по адресу {item} успешно отправлено в {DateTime.Now},");
                        }
                    }
                }
                catch (SmtpException exc)
                {
                    Accessor.Instance.LogError("EmailNotification",
                        $"Письма небыли отправлены по адресам. Ошибка = {exc}," +
                        $" Время отправления - {DateTime.Now}," +
                        $"адрес - {emailAddress}");
                }
                catch (Exception exc)
                {
                    Accessor.Instance.LogError("EmailNotification",
                        $"Письма небыли отправлены по адресам. Ошибка = {exc}," +
                        $" Время отправления - {DateTime.Now}," +
                        $"адрес - {emailAddress}");
                }
            }
            );
        }

        
    }
}
