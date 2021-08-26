using Food.Data.Entities;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface ICafeNotificatonContact
    {
        #region CafeNotificationContact

        /// <summary>
        /// Возвращает контакт кафе по идентификатору
        /// </summary>
        /// <param name="id">идентификатор контакта</param>
        /// <returns></returns>
        CafeNotificationContact GetCafeNotificationContactById(long id);


        /// <summary>
        /// Возвращает список всех контактов кафе по идентификатору
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="notificationChannel">тип канала уведомления</param>
        /// <returns></returns>
        List<CafeNotificationContact> GetCafeNotificationContactByCafeId(
            long cafeId,
            NotificationChannelEnum? notificationChannel
        );


        /// <summary>
        /// Добавить контакт для уведомлений
        /// </summary>
        /// <param name="cafeNotificatioContact">контакт (сущность)</param>
        /// <returns></returns>
        long AddCafeNotificationContact(
            CafeNotificationContact cafeNotificatioContact
        );

        /// <summary>
        /// Удалить контакт уведомлений
        /// </summary>
        /// <param name="id">идентификатор контакта</param>
        /// <returns></returns>
        bool RemoveCafeNotificationContact(long id);

        /// <summary>
        /// Обновляет контакт уведомлений кафе
        /// </summary>
        /// <param name="cafeNotificationContact">контакт (сущность)</param>
        /// <returns></returns>
        bool UpdateCafeNotificationContact(CafeNotificationContact cafeNotificationContact);

        #endregion
    }
}
