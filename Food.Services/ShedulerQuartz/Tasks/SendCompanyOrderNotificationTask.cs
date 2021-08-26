using Food.Data.Entities;
using Food.Services.Config;
using Food.Services.ShedulerQuartz.Scaffolding;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.ShedulerQuartz.Tasks
{
    public class SendCompanyOrderNotificationTask : PersistentOneshotTask
    {
        private readonly IConfigureSettings configureSettings;

        public SendCompanyOrderNotificationTask(IConfigureSettings configureSettings)
        {
            this.configureSettings = configureSettings;
        }

        //TODO: Refactoring переименовать задачу в CloseCompanyOrder
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
                        "Уведомление по корпоративному заказу не было отправлено потому что заказ не был найден в БД "
                            + $"(OrderId={companyOrderId})");
                    return true;
                }

                CancelOrdersByAddressTask cancelOrders = new CancelOrdersByAddressTask();
                var cancelationOrders = cancelOrders.CheckCompanyOrderAmountByAdderss(orders, companyOrder);
                if (cancelationOrders == null)
                {
                    Accessor.Instance.LogError(
                        "Scheduler",
                        "Ошибка при поиске заказов для отмены"
                            + $"(OrderId={companyOrderId} CompanyName='{companyOrder.Company.Name}' "
                            + $"CafeName='{companyOrder.Cafe.CafeName}')");
                    return true;
                }
                if (cancelOrders.SendNotification(cancelationOrders))
                {
                    foreach (var order in cancelationOrders.SelectMany(g => g.Value))
                    {
                        Accessor.Instance.SetOrderStatus(
                        order.Id,
                        (long)EnumOrderStatus.Abort,
                        userId: 0);
                    }
                    Accessor.Instance.LogInfo(
                        "Scheduler",
                        $"Уведомления об отмене заказов были успешно отправлены пользователям"
                            + $"(CompanyOrderId={companyOrderId} CompanyName='{companyOrder.Company.Name}' "
                            + $"CafeName='{companyOrder.Cafe.CafeName}')");
                    return true;
                }

                if (companyOrder.State != (long)EnumOrderStatus.Created)
                {
                    Accessor.Instance.LogError(
                        "Scheduler",
                        "Уведомление по корпоративному заказу не было отправлено потому что статус заказа изменился "
                            + $"(OrderId={companyOrderId} CompanyName='{companyOrder.Company.Name}' "
                            + $"CafeName='{companyOrder.Cafe.CafeName} OrderStatus={companyOrder.State}')");
                    return true;
                }

                if (orders == null || !orders.Any())
                {
                    Accessor.Instance.LogError(
                        "Scheduler",
                        "Уведомление по корпоративному заказу не было отправлено потому что заказ был пустой "
                            + $"(OrderId={companyOrderId} CompanyName='{companyOrder.Company.Name}' "
                            + $"CafeName='{companyOrder.Cafe.CafeName}')");
                    return true;
                }


                if (await SendNotification(companyOrder))
                {
                    Accessor.Instance.SetCompanyOrderStatus(
                        companyOrder.Id,
                        (long)EnumOrderStatus.Accepted,
                        userId: 0);
                    Accessor.Instance.LogInfo(
                        "Scheduler",
                        $"Уведомление по корпоративному заказу было успешно отправлено в кафе "
                            + $"(CompanyOrderId={companyOrderId} CompanyName='{companyOrder.Company.Name}' "
                            + $"CafeName='{companyOrder.Cafe.CafeName}')");
                    return true;
                }
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
        ///     Отправка увведомления в кафе о компанейском заказе.
        /// </summary>
        /// <param name="companyOrder"></param>
        /// <returns>
        ///     true - отправлено без ошибок
        ///     false - при отправке возникли ошибки
        /// </returns>
        private async Task<bool> SendNotification(CompanyOrder companyOrder)
        {
            try
            {
                NotificationBase notification =
                    new EmailNotification(configureSettings);

                NotificationBodyBase notificationBody =
                    new NewOrderToCafeNotificationBody(
                        companyOrder.Cafe,
                        companyOrder,
                        configureSettings
                        );

                notification.FormNotification(notificationBody);
                await notification.SendNotificationAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

namespace Food.Services.ShedulerQuartz
{
    public static class SendCompanyOrderNotificationSchedulerExtensions
    {
        /// <summary>
        /// Отправляет уведомление в кафе в заданное время о корпоративном заказе.
        /// 
        /// Если задача по уведомлению о корпоративном заказе с идентификатором <see cref="companyOrderId"/>
        /// уже существует, новая задача создана не будет.
        /// </summary>
        /// <param name="when">Когда отправлять уведомление</param>
        /// <param name="banquetId">Идентификатор корпоративного заказа</param>
        /// <returns>Время, когда новая задача будет запущена или null, если задача уже существует.</returns>
        public static async Task<DateTimeOffset?> DispatchCompanyOrderNotificationAt(this IFoodScheduler scheduler, DateTime when, long companyOrderId)
        {
            var jobId = MakeJobKey(companyOrderId);
            var triggerId = MakeTriggerKey(companyOrderId);
            var data = new JobDataMap();
            data.Put("CompanyOrderId", companyOrderId);

            return await scheduler.ScheduleOneshot<Tasks.SendCompanyOrderNotificationTask>(
                jobId, triggerId, when, data,
                maxRetries: 10,
                retryInterval: TimeSpan.FromMinutes(5)
            );
        }

        /// <summary>
        /// Проверяет есть ли задача по отправке уведомления по корпоративному заказу с идентификатором
        /// <see cref="companyOrderId"/>.
        /// </summary>
        /// <param name="companyOrderId">Идентификатор копроративного заказа</param>
        public static async Task<bool> IsCompanyOrderNotificationScheduled(this IFoodScheduler scheduler, long companyOrderId)
        {
            return await scheduler.IsTriggerExists(MakeTriggerKey(companyOrderId)).ConfigureAwait(false);
        }

        /// <summary>
        /// Отменяет задачу по отправке уведомления о корпоративном заказе с идентификатором
        /// <see cref="companyOrderId"/>.
        /// </summary>
        /// <param name="companyOrderId">Идентификатор заказа</param>
        public static async Task<bool> CancelCompanyOrderNotification(this IFoodScheduler scheduler, long companyOrderId)
        {
            return await scheduler.CancelJob(MakeTriggerKey(companyOrderId)).ConfigureAwait(false);
        }

        private static JobKey MakeJobKey(long orderId) => new JobKey($"SendCompanyOrderNotification-JOB-{orderId}");

        private static TriggerKey MakeTriggerKey(long companyOrderId)
        {
            return new TriggerKey($"SendCompanyOrderNotification-{companyOrderId}");
        }
    }
}
