using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region CafeNotificationContact

        /// <summary>
        /// Возвращает контакт кафе по идентификатору
        /// </summary>
        /// <param name="id">идентификатор контакта</param>
        /// <returns></returns>
        public CafeNotificationContact GetCafeNotificationContactById(long id)
        {
            CafeNotificationContact cafeNotification;

            using (var fc = GetContext())
            {
                cafeNotification = fc.CafeNotificationContact.AsNoTracking().FirstOrDefault(cnt => cnt.Id == id && cnt.IsDeleted == false);
            }

            return cafeNotification;
        }


        /// <summary>
        /// Возвращает список всех контактов кафе по идентификатору
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="notificationChannel">тип канала уведомления</param>
        /// <returns></returns>
        public List<CafeNotificationContact> GetCafeNotificationContactByCafeId(
            long cafeId,
            NotificationChannelEnum? notificationChannel
        )
        {
            List<CafeNotificationContact> cafeNotifications;

            using (var fc = GetContext())
            {
                var query = fc.CafeNotificationContact.AsNoTracking().Where(cnt => cnt.CafeId == cafeId);

                if (notificationChannel != null)
                {
                    query = query.Where(cnt => cnt.NotificationChannelId == (long)notificationChannel && cnt.IsDeleted == false);
                }

                cafeNotifications = query.ToList();
            }

            return cafeNotifications;
        }


        /// <summary>
        /// Добавить контакт для уведомлений
        /// </summary>
        /// <param name="cafeNotificatioContact">контакт (сущность)</param>
        /// <returns></returns>
        public long AddCafeNotificationContact(
            CafeNotificationContact cafeNotificatioContact
        )
        {
            long newItemId;

            using (var fc = GetContext())
            {
                fc.CafeNotificationContact.Add(cafeNotificatioContact);

                fc.SaveChanges();

                newItemId = cafeNotificatioContact.Id;
            }

            return newItemId;
        }

        /// <summary>
        /// Удалить контакт уведомлений
        /// </summary>
        /// <param name="id">идентификатор контакта</param>
        /// <returns></returns>
        public bool RemoveCafeNotificationContact(long id)
        {
            using (var fc = GetContext())
            {
                var oldCafeNotificationContact =
                    fc.CafeNotificationContact.FirstOrDefault(
                        c => c.Id == id && c.IsDeleted == false
                        );

                if (oldCafeNotificationContact != null)
                {
                    oldCafeNotificationContact.IsDeleted = true;

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
        /// Обновляет контакт уведомлений кафе
        /// </summary>
        /// <param name="cafeNotificationContact">контакт (сущность)</param>
        /// <returns></returns>
        public bool UpdateCafeNotificationContact(CafeNotificationContact cafeNotificationContact)
        {
            using (var fc = GetContext())
            {
                var oldCafeNotificationContact =
                    fc.CafeNotificationContact.FirstOrDefault(
                        c => c.Id == cafeNotificationContact.Id && c.IsDeleted == false
                        );

                if (oldCafeNotificationContact != null)
                {
                    oldCafeNotificationContact.NotificationChannelId = cafeNotificationContact.NotificationChannelId;
                    oldCafeNotificationContact.NotificationContact = cafeNotificationContact.NotificationContact;

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
