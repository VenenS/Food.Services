using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region Notification
        /// <summary>
        /// Получить уведомление по идентификатору
        /// </summary>
        /// <param name="id">идентификатор уведомления</param>
        /// <returns></returns>
        public Notification GetNotificationById(long id)
        {
            using (var fc = GetContext())
            {
                return fc.Notifications.AsNoTracking().FirstOrDefault(n => n.Id == id && !n.IsDeleted);
            }
        }

        /// <summary>
        /// Возвращает список всех уведомлений кафе за указанный период
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="startDate">начало периода</param>
        /// <param name="endDate">окончание периода</param>
        /// <returns></returns>
        public List<Notification> GetNotificationsToCafe(
            long cafeId,
            DateTime? startDate = null,
            DateTime? endDate = null
        )
        {
            List<Notification> notifications;
            using (var fc = GetContext())
            {
                IQueryable<Notification> query = fc.Notifications.AsNoTracking().Where(e => e.CafeId == cafeId && !e.IsDeleted);

                if (startDate != null)
                {
                    endDate =
                        endDate == null
                            ? DateTime.Now.AddYears(100)
                            : endDate.Value;

                    query = query.Where(n => n.SendDate >= startDate && n.SendDate <= endDate);
                }

                notifications = query.ToList();
            }

            return notifications;
        }

        /// <summary>
        /// Возвращает все уведомления для пользователя за указанный период
        /// </summary>
        /// <param name="userId">идентификатор пользователя</param>
        /// <param name="startDate">начало периода</param>
        /// <param name="endDate">окончание периода</param>
        /// <returns></returns>
        public List<Notification> GetNotificationsToUser(
            Int64 userId,
            DateTime? startDate = null,
            DateTime? endDate = null
        )
        {
            List<Notification> notifications;

            using (var fc = GetContext())
            {
                IQueryable<Notification> query = fc.Notifications.AsNoTracking().Where(n => n.UserId == userId && !n.IsDeleted);

                if (startDate != null)
                {
                    endDate =
                        endDate == null
                            ? DateTime.Now.AddYears(100)
                            : endDate.Value;

                    query = query.Where(n => n.SendDate >= startDate && n.SendDate <= endDate);
                }

                notifications = query.ToList();
            }

            return notifications;
        }

        /// <summary>
        /// Воозвращает список уведомлений по фильтру за указанный период
        /// </summary>
        /// <param name="startDate">начало периода</param>
        /// <param name="endDate">окончание периода</param>
        /// <param name="sendStatus">статус отправки</param>
        /// <param name="channel">тип канала уведомлений</param>
        /// <param name="type">тип уведомления</param>
        /// <returns></returns>
        public List<Notification> GetNotificationHistory(
            string sendStatus,
            NotificationChannelEnum? channel,
            NotificationTypeEnum? type,
            DateTime? startDate = null,
            DateTime? endDate = null
        )
        {
            List<Notification> notifications;

            using (var fc = GetContext())
            {
                IQueryable<Notification> query = fc.Notifications.AsNoTracking().Where(
                    n =>    (
                                channel == null
                                || n.NotificationChannelId == (Int16)channel
                            )
                            &&
                            (
                                type == null
                                || n.NotificationTypeId == (Int16)type
                            )
                            &&
                            (
                                String.IsNullOrWhiteSpace(sendStatus)
                                || n.SendStatus == sendStatus
                            )
                            && !n.IsDeleted);

                if (startDate != null)
                {
                    endDate =
                        endDate == null
                            ? DateTime.Now.AddYears(100)
                            : endDate.Value;

                    query = query.Where(n => n.SendDate >= startDate && n.SendDate <= endDate);

                }

                notifications = query.ToList();
            }

            return notifications;
        }

        /// <summary>
        /// Добавить уведомление
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        public long AddNotification(Notification notification)
        {
            long newItemId;

            notification.ErrorMessage =
                (!string.IsNullOrEmpty(notification.ErrorMessage) && notification.ErrorMessage.Length > 255)
                    ? notification.ErrorMessage.Remove(255)
                    : notification.ErrorMessage;

            using (var fc = GetContext())
            {
                fc.Notifications.Add(notification);

                fc.SaveChanges();

                newItemId = notification.Id;
            }

            return newItemId;
        }

        /// <summary>
        /// Удалить уведомление
        /// </summary>
        /// <param name="id">идентификатор уведомления</param>
        /// <returns></returns>
        public bool RemoveNotification(long id)
        {
            using (var fc = GetContext())
            {
                var oldNotification =
                    fc.Notifications.FirstOrDefault(
                        c => c.Id == id && c.IsDeleted == false
                        );

                if (oldNotification != null)
                {
                    oldNotification.IsDeleted = false;

                    fc.SaveChanges();
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Редактировать уведомление
        /// </summary>
        /// <param name="notification"></param>
        /// <returns></returns>
        public bool UpdateNotification(
            Notification notification
        )
        {
            using (var fc = GetContext())
            {
                var oldNotification =
                    fc.Notifications.FirstOrDefault(
                        c => c.Id == notification.Id
                             && c.IsDeleted == false
                        );

                if (oldNotification != null)
                {
                    oldNotification.NotificationChannelId = notification.NotificationChannelId;
                    oldNotification.CafeId = notification.CafeId;
                    oldNotification.ErrorMessage = notification.ErrorMessage;
                    oldNotification.LastUpdateByUserId = notification.LastUpdateByUserId;
                    oldNotification.LastUpdDate = notification.LastUpdDate;
                    oldNotification.NotificationChannelId = notification.NotificationChannelId;
                    oldNotification.NotificationTypeId = notification.NotificationTypeId;
                    oldNotification.OrderId = notification.OrderId;
                    oldNotification.SendContact = notification.SendContact;
                    oldNotification.SendDate = notification.SendDate;
                    oldNotification.SendStatus = notification.SendStatus;
                    oldNotification.UserId = notification.UserId;
                    fc.SaveChanges();
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}
