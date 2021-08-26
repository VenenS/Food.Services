using Food.Data.Entities;
using ITWebNet.FoodService.Food.DbAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using Castle.Core.Internal;

namespace Food.Services
{
    public class AbortOrderToCafeNotificationBody : NotificationBodyBase
    {
        private readonly Dictionary<string, string> _emailAddressAndFullName;
        private readonly IEnumerable<Order> _orders;
        private Exception _exception;
        private string _cafeName;
        private string _address;

        public AbortOrderToCafeNotificationBody(IEnumerable<Order> orders)
        {
            _exception = null;
            _orders = orders;
            _cafeName = orders.FirstOrDefault()?.Cafe.CafeName;
            _address = orders.FirstOrDefault()?.OrderInfo.OrderAddress;
        }

        public AbortOrderToCafeNotificationBody((Dictionary<string, string>,string) emailsAddress)
        {
            _emailAddressAndFullName = emailsAddress.Item1;
            _cafeName = emailsAddress.Item2;
        }

        /// <summary>
        ///     Формирование записи об уведомлении
        /// </summary>
        public override void FormLogInfo()
        {
            var sendDate = DateTime.Now;
            var sendStatus =
                _exception == null
                    ? "S"
                    : "E";
            var errorMessage =
                _exception == null
                    ? string.Empty
                    : string.Format(
                        @"При отправке сообщения кафе возникла следующая ошибка
                        Код ошибки: {0}",
                        _exception is SmtpException
                            ? ((SmtpException)_exception).StatusCode.ToString()
                            : _exception.Message
                        );

            if (!_orders.IsNullOrEmpty()&&_orders.Any())
            {
                foreach (var order in _orders)
                {
                    var notification = new Notification
                    {
                        CafeId = order.CafeId,
                        CreateDate = sendDate,
                        CreatedBy = order.UserId,
                        ErrorMessage = errorMessage,
                        NotificationChannelId = (int)ITWebNet.Food.Core.DataContracts.Common.NotificationChannelModel.Email,
                        NotificationTypeId = (int)ITWebNet.Food.Core.DataContracts.Common.NotificationType.OrderCreate,
                        OrderId = order.Id,
                        SendContact = order.User.Email,
                        SendDate = sendDate,
                        SendStatus = sendStatus,
                        UserId = order.UserId
                    };

                    try
                    {
                        Accessor.Instance.AddNotification(notification);
                    }
                    catch (Exception ex)
                    {
                        _exception = ex;
                    }
                }
            }
        }

        /// <summary>
        ///     Получение текста сообщения об отмене заказа по конкретному адресу
        /// </summary>
        /// <returns></returns>
        public override string GetMessageBody()
        {
            return string.Format(
            @"Уважаемый клиент.
Ваш заказ по адресу {1} из кафе {0} отменен в связи с недостаточной суммой корпоративного заказа",
                _cafeName,
                _address);
        }

        /// <summary>
        ///     Получить списка контактов пользователей для уведомления
        /// </summary>
        /// <returns></returns>
        public override List<string> GetReceiverAddress()
        {
            return _orders.Select(o => o.User.Email).ToList();
        }

        /// <summary>
        ///     Получение темы сообщения
        /// </summary>
        /// <returns></returns>
        public override string GetSubject()
        {
            return "Отмена заказа в сервисе edovoz.com";
        }

        /// <summary>
        ///     Выставление ошибки при отправке
        /// </summary>
        /// <param name="exc"></param>
        public override void SetSendError(Exception exc)
        {
            _exception = exc;
        }
    }
}
