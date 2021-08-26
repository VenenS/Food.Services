using Food.Data.Entities;
using Food.Services.Config;
using Food.Services.Utils;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Food.Services
{
    /// <summary>
    ///     Формирование уведомления о новом заказе о кафе
    /// </summary>
    public class NewOrderToCafeNotificationBody : NotificationBodyBase
    {
        private readonly Cafe _cafe;
        private List<CafeNotificationContact> _cafeNotificationContacts;
        private readonly CompanyOrder _companyOrder;
        private readonly Banket _banketOrder;
        private Exception _currentException;
        private readonly Order _order;
        private IConfigureSettings ConfigureSettings { get; }
        /// <summary>
        ///     Конструктор
        /// </summary>
        /// <param name="cafe"></param>
        /// <param name="order"></param>
        public NewOrderToCafeNotificationBody(Cafe cafe, Order order, IConfigureSettings config) : this(config)
        {
            _cafe = cafe;
            _order = order;
            _currentException = null;
        }

        /// <summary>
        /// </summary>
        /// <param name="cafe"></param>
        /// <param name="companyOrder"></param>
        public NewOrderToCafeNotificationBody(Cafe cafe, CompanyOrder companyOrder, IConfigureSettings config) : this(config)
        {
            _cafe = cafe;
            _companyOrder = companyOrder;
            _order = Accessor.Instance.GetOrdersByCompanyOrderId(companyOrder.Id).FirstOrDefault();
            _currentException = null;
        }

        /// <summary>
        /// </summary>
        /// <param name="cafe"></param>
        /// <param name="banketOrder"></param>
        public NewOrderToCafeNotificationBody(Cafe cafe, Banket banketOrder, IConfigureSettings config) : this(config)
        {
            _cafe = cafe;
            _banketOrder = banketOrder;
            _order = Accessor.Instance.ListOrdersInBanket(banketOrder.Id).FirstOrDefault();
            _currentException = null;
        }

        public NewOrderToCafeNotificationBody(IConfigureSettings configureSettings)
        {
            ConfigureSettings = configureSettings;
        }

        /// <summary>
        ///     Получение текста сообщения о новом заказе с ссылкой-переходом на него
        /// </summary>
        /// <returns></returns>
        public override string GetMessageBody()
        {
            if (_banketOrder != null)
            {
                return string.Format(
                    @"Уважаемое кафе {0}.
Вам поступил в {1} новый банкетный заказ под номером {2}.
Вы можете его просмотреть, перейдя по ссылке {3}{4}{5}{6}{7}{8}",
                    _cafe.CafeFullName,
                    _banketOrder.EventDate,
                    _banketOrder.Id,
                    ConfigureSettings?.SiteName ?? "edovoz.com",
                    "/manager/cafe/",
                    _cafe.Id,
                    "/reports/details/",
                    _banketOrder.Id,
                    "/?orderType=Banket"
                );
            }

            if (_order.CompanyOrderId != null)
                return string.Format(
                    @"Уважаемое кафе {0}.
Вам поступил в {1} новый корпоративный заказ под номером {2}.
Вы можете его просмотреть, перейдя по ссылке {3}{4}{5}{6}{7}{8}",
                    _cafe.CafeFullName,
                    _companyOrder.AutoCloseDate,
                    _companyOrder.Id,
                    ConfigureSettings?.SiteName ?? "edovoz.com",
                    "/manager/cafe/",
                    _cafe.Id,
                    "/reports/details/",
                    _companyOrder.Id,
                    "/?orderType=Collective"
                );

            return string.Format(
                @"Уважаемое кафе {0}.
Вам поступил в {1} новый заказ под номером {2}.
Вы можете его просмотреть, перейдя по ссылке {3}{4}{5}{6}{7}{8}",
                    _cafe.CafeFullName,
                    _order.CreationDate,
                    _order.Id,
                    ConfigureSettings?.SiteName ?? "edovoz.com",
                    "/manager/cafe/",
                    _cafe.Id,
                    "/reports/details/",
                    _order.Id,
                    "/?orderType=Individual"
            );
        }

        /// <summary>
        ///     Получить списка контактов кафе для уведомления
        /// </summary>
        /// <returns></returns>
        public override List<string> GetReceiverAddress()
        {
            _cafeNotificationContacts =
                Accessor.Instance.GetCafeNotificationContactByCafeId(
                    _cafe.Id,
                    NotificationChannelEnum.Email
                    );

            return
                _cafeNotificationContacts
                    .Select(cnt => cnt.NotificationContact)
                    .ToList();
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

            if (_cafeNotificationContacts.Count > 0)
            {
                foreach (var contact in _cafeNotificationContacts)
                {
                    var notification = new Notification
                    {
                        CafeId = _cafe.Id,
                        CreateDate = sendDate,
                        CreatedBy = 0,
                        ErrorMessage = errorMessage,
                        NotificationChannelId = (int)ITWebNet.Food.Core.DataContracts.Common.NotificationChannelModel.Email,
                        NotificationTypeId = (int)ITWebNet.Food.Core.DataContracts.Common.NotificationType.OrderCreate,
                        OrderId = _order.Id,
                        SendContact = contact.NotificationContact,
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
            }
            else
            {
                var notification = new Notification
                {
                    CafeId = _cafe.Id,
                    CreateDate = sendDate,
                    CreatedBy = 0,
                    ErrorMessage = errorMessage,
                    NotificationChannelId = (int)ITWebNet.Food.Core.DataContracts.Common.NotificationChannelModel.Email,
                    NotificationTypeId = (int)ITWebNet.Food.Core.DataContracts.Common.NotificationType.OrderCreate,
                    OrderId = _order.Id,
                    SendContact = string.Empty,
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
        }

        /// <summary>
        ///     Получение темы сообщения
        /// </summary>
        /// <returns></returns>
        public override string GetSubject()
        {
            return "Поступление нового заказа";
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
