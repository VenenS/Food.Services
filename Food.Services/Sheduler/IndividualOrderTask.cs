using Food.Data.Entities;
using Food.Services.Config;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Food.Services
{
    public class IndividualOrderTask: ScheduledTask
    {
        private readonly long _isRepeatable;
        private readonly Order _order;
        private readonly Cafe _cafe;

        public IndividualOrderTask(DateTime? executeTime, long isRepeatable, bool isDeleted, Cafe cafe, Order order) : base(executeTime, isRepeatable, isDeleted)
        {
            _isRepeatable = isRepeatable;
            _cafe = cafe;
            _order = order;
        }

        public override void Run()
        {
            try
            {
                var status = SendNotice(_order);

                if (status && _isRepeatable == (long)TaskRepeatableEnum.Once)
                {
                    var taskId = (long)Accessor.Instance.GetTaskIdByParameters(
                        _order.Id,
                        _cafe.Id,
                        null
                    );
                    // удаляем задачу
                    CancelTask(taskId);
                }
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        ///     Отправка уведомления в кафе об индивидуальном заказе.
        /// </summary>
        /// <param name="order">Задача.</param>
        /// <returns>
        ///     true - отправлено без ошибок
        ///     false - при отправке возникли ошибки
        /// </returns>
        private bool SendNotice(Order order, [FromServices] IConfigureSettings configureSettings = null)
        {
            try
            {
                NotificationBase notification =
                    new EmailNotification(configureSettings);

                NotificationBodyBase notificationBody =
                    new NewOrderToCafeNotificationBody(_cafe, order, configureSettings);

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