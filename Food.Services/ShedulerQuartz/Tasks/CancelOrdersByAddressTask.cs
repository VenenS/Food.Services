using DocumentFormat.OpenXml.Office.CustomUI;
using Food.Data.Entities;
using Food.Services.Config;
using Food.Services.Services;
using Food.Services.ShedulerQuartz.Scaffolding;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Quartz;
using Remotion.Linq.Clauses;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.ShedulerQuartz.Tasks
{
    public class CancelOrdersByAddressTask : PersistentOneshotTask
    {
        private IConfigureSettings _configureSettings;

        public CancelOrdersByAddressTask(IConfigureSettings config)
        {
            _configureSettings = config;
        }
        public CancelOrdersByAddressTask() { }

        protected override async Task<bool> TryExecute(IJobExecutionContext context)
        {
            long companyOrderId = context.MergedJobDataMap.GetLong("CompanyOrderId");

            try
            {
                var orders = Accessor.Instance.GetOrdersByCompanyOrderId(companyOrderId);
                var companyOrder = Accessor.Instance.GetCompanyOrderById(companyOrderId);
                if (companyOrder == null)
                {
                    Accessor.Instance.LogError(
                        "Scheduler",
                        "Уведомление об отмене заказа не было отправлено потому что заказ не был найден в БД "
                            + $"(OrderId={companyOrderId})");
                    return true;
                }

                if (companyOrder.State != (long)EnumOrderStatus.Created)
                {
                    Accessor.Instance.LogError(
                        "Scheduler",
                        "Уведомление об отмене заказа не было отправлено потому что статус заказа изменился "
                            + $"(OrderId={companyOrderId} CompanyName='{companyOrder.Company.Name}' "
                            + $"CafeName='{companyOrder.Cafe.CafeName} OrderStatus={companyOrder.State}')");
                    return true;
                }

                if (orders == null || !orders.Any())
                {
                    Accessor.Instance.LogError(
                        "Scheduler",
                        "Уведомление об отмене заказа не было отправлено потому что заказ был пустой "
                            + $"(OrderId={companyOrderId} CompanyName='{companyOrder.Company.Name}' "
                            + $"CafeName='{companyOrder.Cafe.CafeName}')");
                    return true;
                }

                var cancelationOrders = CheckCompanyOrderAmountByAdderss(orders, companyOrder);
                if (cancelationOrders == null)
                {
                    Accessor.Instance.LogError(
                        "Scheduler",
                        "Ошибка при поиске заказов для отмены"
                            + $"(OrderId={companyOrderId} CompanyName='{companyOrder.Company.Name}' "
                            + $"CafeName='{companyOrder.Cafe.CafeName}')");
                    return true;
                }

                #region Отправка уведомления куратору

                //Если cancelationOrders не пустой значит внутри сформирован список адресов на отмену заказа
                if (cancelationOrders!=null)
                {
                    //Берем id заказа и вытаскиваем куратора которому нужно отправить уведомление
                    var emailCurator=Accessor.Instance.GetEmailsCuratorByCompanyOrderId(companyOrderId);

                    //Непосредственная отправка уведомления куратору
                    var flag = SendEmailNotificationCurator(emailCurator);

                    
                    if (flag == true)
                    {
                        Accessor.Instance.LogInfo(
                            "Scheduler",
                            $"Уведомления об отмене заказа были успешно отправлено куратору"
                                + $"(CompanyOrderId={companyOrderId} CompanyName='{companyOrder.Company.Name}' "
                                + $"CafeName='{companyOrder.Cafe.CafeName}')");
                        return true;
                    }
                    else
                    {
                        Accessor.Instance.LogInfo(
                            "Scheduler",
                            $"Уведомления об отмене заказа не были доставлены куратору"
                                + $"(CompanyorderIf={companyOrderId} CompanyName='{companyOrder.Company.Name}' "
                                + $"CafeName='{companyOrder.Cafe.CafeName}')");
                        return false;
                    }
                }

                #endregion

                //if (SendNotification(cancelationOrders))
                //{
                //    foreach (var order in cancelationOrders.SelectMany(g => g.Value))
                //    {
                //        Accessor.Instance.SetOrderStatus(
                //        order.Id,
                //        (long)EnumOrderStatus.Abort,
                //        userId: 0);
                //    }
                //    Accessor.Instance.LogInfo(
                //        "Scheduler",
                //        $"Уведомления об отмене заказов были успешно отправлены пользователям"
                //            + $"(CompanyOrderId={companyOrderId} CompanyName='{companyOrder.Company.Name}' "
                //            + $"CafeName='{companyOrder.Cafe.CafeName}')");
                //    return true;
                //}
            }
            catch (Exception e)
            {
                Accessor.Instance.LogError(
                    "Scheduler",
                    $"Исключение при отправке уведомления по корпоративному заказу (CompanyOrderId={companyOrderId} "
                        + $"Exception='{e}' Backtrace='{e.StackTrace}')");
                throw;
            }
            return false;
        }

        /// <summary>
        ///     Отправка увведомлений пользователям по смс или email.
        /// </summary>
        /// <param name="cancelationOrder"></param>
        /// <returns>
        ///     true - отправлено без ошибок
        ///     false - при отправке возникли ошибки
        /// </returns>
        internal bool SendNotification(Dictionary<string, IEnumerable<Order>> cancelationOrders)
        {
            try
            {
                var ok = true;
                foreach(var orders in cancelationOrders)
                {
                    var result = CommonSmsNotifications.IsSmsNotificationAvailable(orders.Value) ?
                            SendSmsNotification(orders.Value)
                          : SendEmailNotification(orders.Value);
                    if (!result)
                        ok = false;
                }
                return ok;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Отправка уведомления куратору на email
        /// </summary>
        /// <param name="emailCurator"></param>
        /// <returns>
        ///     true - отправлено без ошибок
        ///     false - отправлено с ошибками
        /// </returns>
        private bool SendEmailNotificationCurator((Dictionary<string, string>,string) emailCurator)
        {
            try
            {
                NotificationBase notification =
                    new EmailNotification(_configureSettings);

                NotificationBodyBase notificationBody =
                    new AbortOrderToCafeNotificationBody(emailCurator);

                notification.FormNotification(notificationBody);
                notification.SendNotificationCuratorAsync(emailCurator).GetAwaiter().GetResult();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Отправка увведомлений пользователям по Email.
        /// </summary>
        /// <param name="cancelationOrder"></param>
        /// <returns>
        ///     true - отправлено без ошибок
        ///     false - при отправке возникли ошибки
        /// </re
        private bool SendEmailNotification(IEnumerable<Order> orders)
        {
            try
            {
                NotificationBase notification =
                    new EmailNotification(_configureSettings);

                NotificationBodyBase notificationBody =
                    new AbortOrderToCafeNotificationBody(
                        orders
                        );

                notification.FormNotification(notificationBody);
                notification.SendNotificationAsync().GetAwaiter().GetResult();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Отправка увведомлений пользователям по смс.
        /// </summary>
        /// <param name="cancelationOrder"></param>
        /// <returns>
        ///     true - отправлено без ошибок
        ///     false - при отправке возникли ошибки
        /// </returns>
        private bool SendSmsNotification(IEnumerable<Order> cancelationOrders)
        {
            try
            {
                var smsSender = new NotificationService(_configureSettings);
                var message = "В связи с недостаточной суммой заказов, ваши заказы ({1}) из кафе {2} были отменены";
                var result = CommonSmsNotifications.InformAboutCancelledOrders(
                    smsSender,
                    cancelationOrders,
                    (u, o) => string.Format(message, o.Count())
                    ).GetAwaiter().GetResult();

                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Проверка на минимальную сумму по адресам.
        /// </summary>
        /// <param name="orders">все заказы из корпоративного заказа</param>
        /// <param name="companyOrder">корпоративный заказа</param>
        /// <returns>
        ///     null - ошибка при подсчете суммы
        ///     cancelationOrders - список заказов, которые нужно отменить сгруппированных по адресам
        /// </returns>
        internal Dictionary<string, IEnumerable<Order>> CheckCompanyOrderAmountByAdderss(List<Order> orders, CompanyOrder companyOrder)
        {
            try
            {
                var minAmount = Accessor.Instance.GetCafeById(companyOrder.CafeId)?.DailyCorpOrderSum;
                var cancelationOrders = new Dictionary<string, IEnumerable<Order>>();
                var amounts = Accessor.Instance.CalculateTotalPriceForCompanyOrderByAddresses(companyOrder.Id);
                foreach (var amount in amounts)
                {
                    if (amount.Item3 < minAmount)
                    {
                        var ordersByAddress = orders.Where(o => o.OrderInfo.OrderAddress == amount.Item1);
                        cancelationOrders.Add(amount.Item1, ordersByAddress);
                    }
                }
                return cancelationOrders;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

namespace Food.Services.ShedulerQuartz
{
    public static class CancelOrdersByAddressSchedulerExtensions
    {
        /// <summary>
        /// Выполняет проверку и отправляет уведомление пользователям в заданное время 
        /// об отмене заказа при недостаточной общей сумме корпоративного заказа.
        /// Если задача об отмене заказа с идентификатором <see cref="companyOrderId"/>
        /// уже существует, новая задача создана не будет.
        /// </summary>
        /// <param name="when">Когда отправлять уведомление</param>
        /// <param name="banquetId">Идентификатор корпоративного заказа</param>
        /// <returns>Время, когда задача будет запущена.</returns>
        public static async Task<DateTimeOffset?> AbortOrdersByAddressAt(this IFoodScheduler scheduler, DateTime when, long companyOrderId)
        {
            var jobId = MakeJobKey(companyOrderId);
            var triggerId = MakeTriggerKey(companyOrderId);
            var data = new JobDataMap();
            data.Put("CompanyOrderId", companyOrderId);

            return await scheduler.ScheduleOneshot<Tasks.CancelOrdersByAddressTask>(
                jobId, triggerId, when, data,
                maxRetries: 5,
                retryInterval: TimeSpan.FromMinutes(3)
            );
        }

        /// <summary>
        /// Проверяет есть ли задача об отмене заказов по корпоративному заказу с идентификатором
        /// <see cref="companyOrderId"/>.
        /// </summary>
        /// <param name="companyOrderId">Идентификатор копроративного заказа</param>
        public static async Task<bool> IsAbortOrdersByAddressScheduled(this IFoodScheduler scheduler, long companyOrderId)
        {
            return await scheduler.IsTriggerExists(MakeTriggerKey(companyOrderId)).ConfigureAwait(false);
        }

        /// <summary>
        /// Отменяет задачу по отмене заказов по корпоративному заказу с идентификатором
        /// <see cref="companyOrderId"/>.
        /// </summary>
        /// <param name="companyOrderId">Идентификатор заказа</param>
        public static async Task<bool> CancelAbortOrdersByAddress(this IFoodScheduler scheduler, long companyOrderId)
        {
            return await scheduler.CancelJob(MakeTriggerKey(companyOrderId)).ConfigureAwait(false);
        }

        private static JobKey MakeJobKey(long orderId) => new JobKey($"CancelOrdersByAddress-JOB-{orderId}");
        private static TriggerKey MakeTriggerKey(long orderId) =>
            new TriggerKey($"CancelOrdersByAddress-{orderId}");
    }
}
