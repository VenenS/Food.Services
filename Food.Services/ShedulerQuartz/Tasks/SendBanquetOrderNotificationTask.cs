using Food.Data.Entities;
using Food.Services.Config;
using Food.Services.ShedulerQuartz;
using Food.Services.ShedulerQuartz.Scaffolding;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Food.Services.ShedulerQuartz.Tasks
{
    public class SendBanquetOrderNotificationTask : PersistentOneshotTask
    {
        private readonly IConfigureSettings configureSettings;

        public SendBanquetOrderNotificationTask(IConfigureSettings config)
        {
            configureSettings = config;
        }

        protected override async Task<bool> TryExecute(IJobExecutionContext context)
        {
            long banquetId = context.MergedJobDataMap.GetLong("BanquetId");

            try
            {
                var banket = Accessor.Instance.GetBanketById(banquetId);
                if (banket == null)
                {
                    Accessor.Instance.LogError(
                        "Scheduler",
                        "Уведомление по банкету не было отправлено потому что банкет не был найден в БД "
                            + $"(BanquetId={banquetId})");
                    return true;
                }

                if (await SendNotification(banket))
                {
                    Accessor.Instance.LogInfo(
                        "Scheduler",
                        $"Уведомление по банкету было успешно отправлено (BanquetId={banquetId})");
                    return true;
                }
            }
            catch (Exception)
            {
                Accessor.Instance.LogError(
                    "Scheduler",
                    $"Исключение при отправке уведомления по банкету (BanquetId={banquetId})");
                throw;
            }
            return false;
        }

        /// <summary>
        ///     Отправка увведомления в кафе о банкетном заказе.
        /// </summary>
        /// <returns>
        ///     true - отправлено без ошибок
        ///     false - при отправке возникли ошибки
        /// </returns>
        private async Task<bool> SendNotification(Banket banketOrder)
        {
            try
            {
                var notification = new EmailNotification(configureSettings);
                var notificationBody = new NewOrderToCafeNotificationBody(banketOrder.Cafe, banketOrder, configureSettings);

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
    public static class SendBanquetOrderNotificationSchedulerExtensions
    {
        /// <summary>
        /// Отправляет банкетное уведомление в кафе в заданное время.
        /// 
        /// Если задача по уведомлению о банкете с идентификатором <see cref="banquetId"/> уже существует,
        /// новая задача создана не будет.
        /// </summary>
        /// <param name="when">Когда отправлять уведомление</param>
        /// <param name="banquetId">Идентификатор банкета</param>
        /// <returns>Время, когда новая задача будет запущена или null, если задача уже существует.</returns>
        public static async Task<DateTimeOffset?> DispatchBanquetOrderNotificationAt(this IFoodScheduler scheduler, DateTime when, long banquetId)
        {
            var jobId = MakeJobKey(banquetId);
            var triggerId = MakeTriggerKey(banquetId);
            var data = new JobDataMap();
            data.Put("BanquetId", banquetId);

            return await scheduler.ScheduleOneshot<Tasks.SendBanquetOrderNotificationTask>(
                jobId, triggerId, when, data,
                maxRetries: 10,
                retryInterval: TimeSpan.FromMinutes(5)
            );
        }

        /// <summary>
        /// Проверяет есть ли задача по отправке уведомления по банкету с идентификатором <see cref="banquetId"/>.
        /// </summary>
        /// <param name="banquetId">Идентификатор банкета</param>
        public static async Task<bool> IsBanquetNotificationScheduled(this IFoodScheduler scheduler, long banquetId)
        {
            return await scheduler.IsTriggerExists(MakeTriggerKey(banquetId)).ConfigureAwait(false);
        }

        /// <summary>
        /// Отменяет задачу по отправке уведомления об индивидуальном заказе с идентификатором
        /// <see cref="orderId"/>.
        /// </summary>
        /// <param name="orderId">Идентификатор заказа</param>
        public static async Task<bool> CancelBanquetOrderNotification(this IFoodScheduler scheduler, long banquetId)
        {
            return await scheduler.CancelJob(MakeTriggerKey(banquetId)).ConfigureAwait(false);
        }

        private static JobKey MakeJobKey(long banquetId) =>
            new JobKey($"SendBanquetOrderNotification-JOB-{banquetId}");

        private static TriggerKey MakeTriggerKey(long banquetId)
        {
            return new TriggerKey($"SendBanquetOrderNotification-{banquetId}");
        }
    }
}
