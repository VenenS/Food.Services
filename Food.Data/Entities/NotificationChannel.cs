using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Таблица - справочник каналов уведомления, например Email, SMS.
    /// </summary>
    [Table("notification_channel")]
    public class NotificationChannel : EntityBaseDeletable<short>
    {
        /// <summary>
        /// Код канала уведомления
        /// </summary>
        [Column("notification_channel_code")]
        public string NotificationTypeCode { get; set; }

        /// <summary>
        /// Описание канала уведомления
        /// </summary>
        [Column("notification_channel_name")]
        public string NotificationTypeName { get; set; }

        public virtual ICollection<CafeNotificationContact> CafeNotificationContacts { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
