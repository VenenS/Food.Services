using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Food.Data.Entities;
using Food.Services.ShedulerQuartz.Scaffolding;
using ITWebNet.FoodService.Food.DbAccessor;
using Quartz;
using Serilog;
using ITWebNet.Food.Core;
using Food.Services.Services;
using Food.Services.Config;

namespace Food.Services.ShedulerQuartz.Tasks
{
    public class SendIndividualOrderNotificationTask : PersistentOneshotTask
    {
        private readonly IConfigureSettings configureSettings;

        public SendIndividualOrderNotificationTask(IConfigureSettings configureSettings)
        {
            this.configureSettings = configureSettings;
        }

        protected override async Task<bool> TryExecute(IJobExecutionContext context)
        {
            var orderId = context.MergedJobDataMap.GetLong("OrderId");

            try
            {
                var order = Accessor.Instance.GetOrderById(orderId);

                if (order == null)
                {
                    Accessor.Instance.LogError(
                        "Scheduler",
                        "Уведомление по индивидальному заказу не было отправлено потому что заказ не был найден в БД "
                            + $"(OrderId={orderId})");
                    return true;
                }

                if (order.State != EnumOrderState.Created)
                {
                    Accessor.Instance.LogError(
                        "Scheduler",
                        "Уведомление по индивидальному заказу не было отправлено потому что статус заказа изменился "
                            + $"(OrderId={orderId} UserEmail='{order.User.Email}' CafeName='{order.Cafe.CafeName}')");
                    return true;
                }

                if (await SendNotification(order))
                {
                    Accessor.Instance.SetOrderStatus(order.Id, (long)EnumOrderState.Accepted, userId: 0);
                    Accessor.Instance.LogInfo(
                        "Scheduler",
                        $"Уведомление по индивидальному заказу было успешно отправлено в кафе "
                            + $"(OrderId={orderId} UserEmail='{order.User.Email}' CafeName='{order.Cafe.CafeName}')");
                    return true;
                }
            }
            catch (Exception e)
            {
                Accessor.Instance.LogError(
                    "Scheduler",
                    $"Исключение при отправке уведомления по индивидальному заказу (Order.Id={orderId} Exception='{e}' "
                        + $"Backtrace='{e.StackTrace}')");
                throw;
            }
            return false;
        }

        /// <summary>
        ///     Отправка уведомления в кафе об индивидуальном заказе.
        /// </summary>
        /// <param name="order">Задача.</param>
        /// <returns>
        ///     true - отправлено без ошибок
        ///     false - при отправке возникли ошибки
        /// </returns>
        private async Task<bool> SendNotification(Order order)
        {
            try
            {
                var notification = new EmailNotification(configureSettings);
                var cafe = Accessor.Instance.GetCafeById(order.CafeId);
                var notificationBody = new NewOrderToCafeNotificationBody(cafe, order, configureSettings);

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
    public static class SendIndividualOrderNotificationSchedulerExtensions
    {
        /// <summary>
        /// Отправляет уведомление в кафе в заданное время об индивидуальном заказе.
        /// 
        /// Если задача по уведомлению об индивидуальном заказе с идентификатором <see cref="orderId"/>
        /// уже существует, новая задача создана не будет.
        /// </summary>
        /// <param name="when">Когда отправлять уведомление</param>
        /// <param name="banquetId">Идентификатор индивидуального заказа</param>
        /// <returns>Время, когда новая задача будет запущена или null, если задача уже существует.</returns>
        public static async Task<DateTimeOffset?> DispatchIndividualOrderNotificationAt(this IFoodScheduler scheduler, DateTime when, long orderId)
        {
            //Создается задача с уникальным именем
            var jobId = MakeJobKey(orderId);
            var triggerId = MakeTriggerKey(orderId);
            var data = new JobDataMap();
            data.Put("OrderId", orderId);

            return await scheduler.ScheduleOneshot<Tasks.SendIndividualOrderNotificationTask>(
                jobId, triggerId, when, data,
                maxRetries: 10,
                retryInterval: TimeSpan.FromMinutes(5)
            );
        }

        /// <summary>
        /// Проверяет есть ли задача по отправке уведомления по индивидуальному заказу с идентификатором
        /// <see cref="orderId"/>.
        /// </summary>
        /// <param name="orderId">Идентификатор индивидуального заказа</param>
        public static async Task<bool> IsIndividualOrderNotificationScheduled(this IFoodScheduler scheduler, long orderId)
        {
            return await scheduler.IsTriggerExists(MakeTriggerKey(orderId)).ConfigureAwait(false);
        }

        /// <summary>
        /// Отменяет задачу по отправке уведомления об индивидуальном заказе с идентификатором
        /// <see cref="orderId"/>.
        /// </summary>
        /// <param name="orderId">Идентификатор заказа</param>
        public static async Task<bool> CancelIndividualOrderNotification(this IFoodScheduler scheduler, long orderId)
        {
            return await scheduler.CancelJob(MakeTriggerKey(orderId)).ConfigureAwait(false);
        }

        private static JobKey MakeJobKey(long orderId) =>
            new JobKey($"SendIndividualOrderNotification-JOB-{orderId}");

        private static TriggerKey MakeTriggerKey(long orderId)
        {
            return new TriggerKey($"SendIndividualOrderNotification-{orderId}");
        }
    }
}
