using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Food.Services.Services;
using Food.Services.ShedulerQuartz.Scaffolding;
using ITWebNet.FoodService.Food.DbAccessor;
using Quartz;
using Serilog;

namespace Food.Services.ShedulerQuartz.Tasks
{
    public class SendDeliveryInfoNotificationTask : PersistentOneshotTask
    {
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public SendDeliveryInfoNotificationTask(IEmailService emailService,
                                                INotificationService notificationService)
        {
            _emailService = emailService;
            _notificationService = notificationService;
        }

        protected override async Task<bool> TryExecute(IJobExecutionContext context)
        {
            var orderId = context.MergedJobDataMap.GetLong("OrderId");

            try
            {
                // Информация о заказе
                var order = Accessor.Instance.GetOrderById(orderId);
                var message = $"{order.User.Name}, ваш заказ №{order.Id} из {order.Cafe.CafeName} будет доставлен сегодня до {order.DeliverDate.Value.AddMinutes(30):HH:mm}";
                // Отправка сообщений на почту указанную при оформлении или при ее отсутствии из профиля пользователя
                await _emailService.SendAsync(message, $"Уведомления о заказе №{order.Id}. \"Едовоз\"", order.OrderInfo.OrderEmail ?? order.User.Email);
                // Отправка sms Если номер подтвержден
                if (order.User.PhoneNumberConfirmed)
                    await _notificationService.SmsSend(order.OrderInfo.OrderPhone ?? order.User.PhoneNumber, message);
                Accessor.Instance.LogInfo("Scheduler", "Уведомление по доставке индивидуального заказа было успешно отправлено пользователю " +
                    $"(OrderId={orderId} UserEmail='{order.OrderInfo.OrderEmail ?? order.User.Email}' CafeName='{order.Cafe.CafeName}')");
            }
            catch (Exception e)
            {
                Accessor.Instance.LogError(
                    "Scheduler", $"Исключение при отправке уведомления по индивидальному заказу (Order.Id={orderId} Exception='{e}' "
                        + $"Backtrace='{e.StackTrace}')");
                throw;
            }

            return true;
        }
    }
}

namespace Food.Services.ShedulerQuartz
{
    public static class SendDeliveryInfoNotificationSchedulerExtensions
    {
        /// <summary>
        /// Отправляет пользователю информацию о доставке в указанное время.
        /// </summary>
        public static async Task<DateTimeOffset?> DispatchIndividualOrderNotification(this IFoodScheduler scheduler,
                                                                                      DateTime when,
                                                                                      long orderId)
        {
            var jobId = MakeJobKey(orderId);
            var triggerId = MakeTriggerKey(orderId);
            var data = MakeJobDataMap(orderId);

            return await scheduler.ScheduleOneshot<Tasks.SendDeliveryInfoNotificationTask>(
                jobId, triggerId, when, data);
        }

        /// <summary>
        /// Запускает задачу по отправке уведомлений о доставке заказа
        /// прямо сейчас. Полезно для отладки.
        /// </summary>
        public static async Task RunDispatchIndividualOrderNotificationTaskNow(this IFoodScheduler scheduler,
                                                                               long orderId)
        {
            var jobId = MakeJobKey(orderId);

            if (!await scheduler.IsJobExists(jobId))
                await scheduler.DispatchIndividualOrderNotification(DateTime.Now.AddMinutes(-1), orderId);
            else
                await scheduler.TriggerJob(jobId, MakeJobDataMap(orderId));
        }

        private static JobDataMap MakeJobDataMap(long orderId)
        {
            var data = new JobDataMap();
            data.Put("OrderId", orderId);
            return data;
        }

        private static JobKey MakeJobKey(long orderId) =>
            new JobKey($"SendDeliveryInfoNotification-JOB-{orderId}");

        private static TriggerKey MakeTriggerKey(long orderId) =>
            new TriggerKey($"SendDeliveryInfoNotification-{orderId}");
    }
}