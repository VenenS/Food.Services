using Food.Data.Entities;
using Food.Services.Services;
using ITWebNet.Food.Core.DataContracts.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services
{
    using OrderCancelledFormatter = Func<User, IEnumerable<Order>, string>;

    public class CommonSmsNotifications
    {

        /// <summary>
        /// Проверяет доступна ли смс отправка для данных заказов
        /// </summary>
        /// <param name="orders">заказы</param>
        /// <returns>
        ///     true - отправка доступна для всех заказов
        ///     false - отправка не доступна хотябы для одного заказа
        /// </returns>
        /// 
        public static bool IsSmsNotificationAvailable(IEnumerable<Order> orders)
        {
            return orders.All(o => o.User.SmsNotify);
        }

        public static async Task<bool> InformAboutCancelledOrders(ISmsSender sender,
                                                                  IEnumerable<Order> cancelledOrders,
                                                                  OrderCancelledFormatter formatter)
        {
            var ok = true;

            if (!cancelledOrders.Any())
                return true;

            var userOrders = from o in cancelledOrders
                             group o by o.UserId into g
                             let user = g.First().User
                             select new
                             {
                                 User = user,
                                 // Сгруппировать по номеру чтобы отправлять только 1 СМС сообщение
                                 // на телефон.
                                 ByPhoneNumber = g
                                    // Сначала удалить дубликаты заказов по ID.
                                    .GroupBy(x => x.Id)
                                    .Select(x => x.First())
                                    .GroupBy(x => x.PhoneNumber)
                             };

            foreach (var grouping in userOrders)
            {
                foreach (var phoneGrouping in grouping.ByPhoneNumber)
                {
                    // Если номер телефона связанный с заказом пуст - взять из профиля.
                    var isPhoneInProfileUsable = grouping.User.SmsNotify && grouping.User.PhoneNumberConfirmed;
                    var phoneNo = !String.IsNullOrWhiteSpace(phoneGrouping.Key)
                        ? phoneGrouping.Key
                        : (isPhoneInProfileUsable ? grouping.User.PhoneNumber : null);
                    if (phoneNo == null)
                        continue;

                    // TODO: обрезать текст сообщения если он слишком длинный.
                    var message = formatter.Invoke(grouping.User, phoneGrouping.AsEnumerable());
                    var response = await sender.SmsSend(grouping.User.PhoneNumber, message);
                    if (response.Status != 0)
                    {
                        if (response.Status == 1)
                            ok = false;
                        LogResponse(grouping.User.PhoneNumber, response);
                    }
                }
            }

            return ok;
        }

        private static void LogResponse(string phoneNr, ResponseModel response)
        {
            switch (response.Status)
            {
                case 1:
                    Log.Logger.Error($"Sending SMS to '{phoneNr}' resulted in error: {response.Message}");
                    break;
                case 2:
                    Log.Logger.Warning($"Sending SMS to '{phoneNr}' resulted in warning: {response.Message}");
                    break;
                case 3:
                    Log.Logger.Information($"Sending SMS to '{phoneNr}' resulted in message: {response.Message}");
                    break;
            }
        }
    }
}
