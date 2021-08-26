using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace Food.Services
{
    /// <summary>
    ///     Базовый класс отправки уведомлений
    /// </summary>
    public abstract class NotificationBase
    {
        /// <summary>
        ///     Тело уведомления
        /// </summary>
        protected NotificationBodyBase Notification;

        /// <summary>
        ///     Формирование уведомления
        /// </summary>
        /// <param name="notification"></param>
        public abstract void FormNotification(NotificationBodyBase notification);

        /// <summary>
        ///     Отправка уведомления
        /// </summary>
        protected abstract Task SendNotification();

        /// <summary>
        ///     Асинхронная отправка уведомлений
        /// </summary>
        public async Task SendNotificationAsync()
        {
            await SendNotification();
        }

        /// <summary>
        /// Асинхронная отправка уведомления куратору кафе
        /// </summary>
        /// <param name="address">коллекция содержащая емайл адрес и имя куратора</param>
        /// <returns></returns>
        public async Task SendNotificationCuratorAsync((Dictionary<string, string>, string) address)
        {
            await SendNotificationCurator(address);
        }

        /// <summary>
        /// Отправка уведомления
        /// </summary>
        /// <param name="address">коллекция содержащая емайл адрес и имя куратора</param>
        /// <returns></returns>
        public abstract Task SendNotificationCurator((Dictionary<string, string>, string) address);
    }
}
