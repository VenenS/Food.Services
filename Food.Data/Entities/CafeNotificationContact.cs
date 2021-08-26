using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Таблица, в которой хранятся контактные данные для уведомления кафе.
    /// </summary>
    [Table("cafe_notification_contacts")]
    public class CafeNotificationContact : EntityBase<long>
    {
        /// <summary>
        /// Идентификатор кафе
        /// </summary>
        [Column("cafe_id")]
        public Int64 CafeId { get; set; }

        /// <summary>
        /// Идентификатор канала отправки
        /// </summary>
        [Column("notification_channel_id")]
        public Int16 NotificationChannelId { get; set; }

        /// <summary>
        /// Контактные данные для отправки
        /// Допустимые значение: email, номер сотового
        /// </summary>
        [Column("notification_contact")]
        public string NotificationContact { get; set; }

        /// <summary>
        /// Связанный объект кафе
        /// </summary>
        [ForeignKey("CafeId")]
        public virtual Cafe Cafe { get; set; }

        /// <summary>
        /// Связанный объект типа уведомления
        /// </summary>
        [ForeignKey("NotificationChannelId")]
        public virtual NotificationChannel NotificationChannel { get; set; }
    }
}
