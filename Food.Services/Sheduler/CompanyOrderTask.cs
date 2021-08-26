using System;
using System.Linq;
using Food.Data;
using Food.Data.Entities;
using Food.Services.Config;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Mvc;

namespace Food.Services
{
    internal class CompanyOrderTask : ScheduledTask
    {
        private readonly Cafe _cafe;
        private readonly long _companyOrderId;
        private readonly long _isRepeatable;
        private readonly Order _order;

        /// <summary>
        ///     Конструктор
        /// </summary>
        /// <param name="executeTime"></param>
        /// <param name="isRepeatable"></param>
        /// <param name="isDeleted"></param>
        /// <param name="companyOrderId"></param>
        /// <param name="cafe"></param>
        /// <param name="order"></param>
        public CompanyOrderTask(
            DateTime? executeTime,
            long isRepeatable,
            bool isDeleted,
            long companyOrderId,
            Cafe cafe,
            Order order
            ) : base(executeTime, isRepeatable, isDeleted)
        {
            _isRepeatable = isRepeatable;
            _companyOrderId = companyOrderId;
            _cafe = cafe;
            _order = order;
        }

        public long Id { get; set; }

        public DateTime ExecuteTime { get; set; }
        public long IsRepeatable { get; set; }
        public bool IsDeleted { get; set; }
        public long CompanyOrderId { get; set; }
        public Cafe Cafe { get; set; }
        public Order Order { get; set; }

        /// <summary>
        ///     Запуск выполнения задачи
        /// </summary>
        public override void Run()
        {
            try
            {
                var orders = Accessor.Instance.GetOrdersByCompanyOrderId(_companyOrderId);
                var companyOrder = Accessor.Instance.GetCompanyOrderById(_companyOrderId);
                var taskCompleted = false;

                if (orders.Count() > 0 && companyOrder.State == (long)EnumOrderStatus.Created)
                {
                    if (SendNotice(companyOrder))
                    {
                        Accessor.Instance.SetCompanyOrderStatus(
                            companyOrder.Id,
                            (long)EnumOrderStatus.Accepted,
                            userId: 0);
                        taskCompleted = true;
                    }
                }
                else
                {
                    taskCompleted = true;
                }

                if (taskCompleted && _isRepeatable == (long)TaskRepeatableEnum.Once)
                {
                    // удаляем задачу
                    var taskId = (long)Accessor.Instance.GetTaskIdByParameters(
                        _order.Id,
                        _cafe.Id,
                        _companyOrderId
                        );

                    CancelTask(taskId);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        ///     Запись в лог-файл.
        /// </summary>
        protected override void WriteLog()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Отмена задачи
        /// </summary>
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

        /// <summary>
        ///     Отправка увведомления в кафе о компанейском заказе.
        /// </summary>
        /// <param name="companyOrder"></param>
        /// <returns>
        ///     true - отправлено без ошибок
        ///     false - при отправке возникли ошибки
        /// </returns>
        private bool SendNotice(CompanyOrder companyOrder, [FromServices] IConfigureSettings configureSettings = null)
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
                notification.SendNotificationAsync().GetAwaiter().GetResult();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
