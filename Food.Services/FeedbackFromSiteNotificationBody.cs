using Food.Data.Entities;
using Food.Services.Config;
using Food.Services.Utils;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Food.Services
{
    public class FeedbackFromSiteNotificationBody : NotificationBodyBase
    {
        private Exception _currentException;

        private readonly FeedbackModel _feedback;

        private IConfigureSettings ConfigureSettings { get; }

        /// <summary>
        ///     Конструктор
        /// </summary>
        public FeedbackFromSiteNotificationBody(FeedbackModel feedback, IConfigureSettings config)
        {
            _feedback = feedback;
            _currentException = null;
            ConfigureSettings = config;
        }

        /// <summary>
        ///     Получение текста сообщения о новом заказе с ссылкой-переходом на него
        /// </summary>
        /// <returns></returns>
        public override string GetMessageBody()
        {
            var builder = new StringBuilder();
            builder.Append("Поступило новое сообщение от пользователя с сайта:");
            if (!string.IsNullOrWhiteSpace(_feedback.UserName))
                builder.Append($"\nОт пользователя: {_feedback.UserName}\n");
            builder.Append($"\nТема: {_feedback.Title}\n");
            builder.Append($"\nСообщение: {_feedback.Message}\n");
            if (!string.IsNullOrWhiteSpace(_feedback.Email))
                builder.Append($"\nКонтакты для обратной связи: {_feedback.Email}");
            return builder.ToString();
        }

        /// <summary>
        ///     Получить списка контактов кафе для уведомления
        /// </summary>
        /// <returns></returns>
        public override List<string> GetReceiverAddress()
        {
            var email = ConfigureSettings?.Feedback.Email ?? "support@edovoz.com";
            return
                new List<string>
                {
                    email
                };
        }

        /// <summary>
        ///     Формирование записи об уведомлении
        /// </summary>
        public override void FormLogInfo()
        {
            var sendDate = DateTime.Now;
            var sendStatus =
                _currentException == null
                    ? "S"
                    : "E";
            var errorMessage =
                _currentException == null
                    ? string.Empty
                    : string.Format(
                        @"При отправке сообщения кафе возникла следующая ошибка
                        Код ошибки: {0}",
                        _currentException is SmtpException
                            ? ((SmtpException)_currentException).StatusCode.ToString()
                            : _currentException.Message
                        );

            var notification = new Notification
            {
                CafeId = null,
                CreateDate = sendDate,
                CreatedBy = 0,
                ErrorMessage = errorMessage,
                NotificationChannelId = (int)NotificationChannelModel.Email,
                NotificationTypeId = (int)ITWebNet.Food.Core.DataContracts.Common.NotificationType.UserReply,
                OrderId = null,
                SendContact = "support@edovoz.com",
                SendDate = sendDate,
                SendStatus = sendStatus,
                UserId = null
            };

            try
            {
                Accessor.Instance.AddNotification(notification);
            }
            catch (Exception)
            {
                //TODO СДЕЛАТЬ АДЕКВАТНУЮ ОБРАБОТКУ ОШИБКИ ЛОГИРОВАНИЯ АХТУНГ!!!
            }
        }

        /// <summary>
        ///     Получение темы сообщения
        /// </summary>
        /// <returns></returns>
        public override string GetSubject()
        {
            return "Отзыв с сайта";
        }

        /// <summary>
        ///     Выставление ошибки при отправке
        /// </summary>
        /// <param name="exc"></param>
        public override void SetSendError(Exception exc)
        {
            _currentException = exc;
        }
    }
}
