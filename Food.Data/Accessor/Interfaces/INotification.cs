using Food.Data.Entities;
using System;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface INotification
    {
        #region Notification
        /// <summary>
        /// Получить уведомление по идентификатору
        /// </summary>
        /// <param name="id">идентификатор уведомления</param>
        /// <returns></returns>
        Notification GetNotificationById(long id);

        /// <summary>
        /// Возвращает список всех уведомлений кафе за указанный период
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="startDate">начало периода</param>
        /// <param name="endDate">окончание периода</param>
        /// <returns></returns>
        List<Notification> GetNotificationsToCafe(
            long cafeId,
            DateTime? startDate = null,
            DateTime? endDate = null
        );

        /// <summary>
        /// Возвращает все уведомления для пользователя за указанный период
        /// </summary>
        /// <param name="userId">идентификатор пользователя</param>
        /// <param name="startDate">начало периода</param>
        /// <param name="endDate">окончание периода</param>
        /// <returns></returns>
        List<Notification> GetNotificationsToUser(
            Int64 userId,
            DateTime? startDate = null,
            DateTime? endDate = null
        );

        /// <summary>
        /// Воозвращает список уведомлений по фильтру за указанный период
        /// </summary>
        /// <param name="startDate">начало периода</param>
        /// <param name="endDate">окончание периода</param>
        /// <param name="sendStatus">статус отправки</param>
        /// <param name="channel">тип канала уведомлений</param>
        /// <param name="type">тип уведомления</param>
        /// <returns></returns>
        List<Notification> GetNotificationHistory(
            string sendStatus,
            NotificationChannelEnum? channel,
            NotificationTypeEnum? type,
            DateTime? startDate = null,
            DateTime? endDate = null
        );

        /// <summary>
        /// Добавить уведомление
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        long AddNotification(Notification notification);

        /// <summary>
        /// Удалить уведомление
        /// </summary>
        /// <param name="id">идентификатор уведомления</param>
        /// <returns></returns>
        bool RemoveNotification(long id);

        /// <summary>
        /// Редактировать уведомление
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        bool UpdateNotification(
            Notification notification
        );
        #endregion
    }
}
