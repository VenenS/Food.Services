using System;
using Food.Data.Entities;
using Food.Services.Config;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Mvc;

namespace Food.Services
{
    public class BanketOrderTask : ScheduledTask
    {
        private readonly long _isRepeatable;
        private readonly long _banketId;
        private readonly Cafe _cafe;

        public BanketOrderTask(DateTime? executeTime, long isRepeatable, bool isDeleted, long banketId, Cafe cafe) : base(
            executeTime,
            isRepeatable, isDeleted)
        {
            _isRepeatable = isRepeatable;
            _banketId = banketId;
            _cafe = cafe;
        }

        public override void Run()
        {
            try
            {
                var banket = Accessor.Instance.GetBanketById(_banketId);

                 var status = SendNotice(banket);

                if (status && _isRepeatable == (long) TaskRepeatableEnum.Once)
                {
                    var taskId = (long) Accessor.Instance.GetTaskIdByParameters(
                        banket.Id,
                        _cafe.Id,
                        null,
                        _banketId
                    );
                    // удаляем задачу
                    CancelTask(taskId);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        ///     Отправка увведомления в кафе о банкетном заказе.
        /// </summary>
        /// <returns>
        ///     true - отправлено без ошибок
        ///     false - при отправке возникли ошибки
        /// </returns>
        private bool SendNotice(Banket banketOrder, [FromServices] IConfigureSettings configureSettings = null)
        {
            try
            {
                NotificationBase notification =
                    new EmailNotification(configureSettings);

                NotificationBodyBase notificationBody =
                    new NewOrderToCafeNotificationBody(
                        banketOrder.Cafe,
                        banketOrder,
                        configureSettings
                    );

                notification.FormNotification(notificationBody);

                notification.SendNotificationAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected override void WriteLog()
        {
            throw new NotImplementedException();
        }

        protected override void CancelTask(long taskId)
        {
            try
            {
                Accessor.Instance.RemoveTask(taskId);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}