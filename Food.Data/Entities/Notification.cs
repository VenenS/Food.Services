using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Представляет уведомление.
    /// </summary>
    [Table("notifications")]
    public class Notification : EntityBase<long>
    {
        /// <summary>
        /// Возвращает или задает идентификатор заказа.
        /// </summary>
        [Column("order_id")]
        public long? OrderId { get; set; }

        public virtual Order Order { get; set; }

        /// <summary>
        /// Идентификатор пользователя, относительно которого сделано уведомление
        /// Может быть пустым, если уведомление не было связано с пользователем
        /// </summary>
        [Column("user_id")]
        public Int64? UserId { get; set; }

        /// <summary>
        /// Идентификатор кафе, относительно которого сделано уведомление
        /// Может быть пустым, если уведомление не было связано с кафе
        /// </summary>
        [Column("cafe_id")]
        public Int64? CafeId { get; set; }

        /// <summary>
        /// Идентификатор канала отправки
        /// </summary>
        [Column("notification_channel_id")]
        public Int16 NotificationChannelId { get; set; }

        /// <summary>
        /// Идентификатор типа сообщения
        /// </summary>
        [Column("notification_type_id")]
        public Int16 NotificationTypeId { get; set; }

        /// <summary>
        /// Адрес, кому было отправлено сообщение
        /// Допустимые значение: email, номер сотового
        /// </summary>
        [Column("send_contact")]
        public string SendContact { get; set; }

        /// <summary>
        /// Дата отправки уведомления
        /// </summary>
        [Column("send_date")]
        public DateTime SendDate { get; set; }

        /// <summary>
        /// Статус отправки сообщения
        /// Допустимые значения: S - success, E - error
        /// </summary>
        [Column("send_status")]
        public string SendStatus { get; set; }

        /// <summary>
        /// Текст ошибки отправки, если таковая есть
        /// </summary>
        [Column("error_message")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        [Column("create_date")]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Кем создана
        /// </summary>
        [Column("created_by")]
        public Int64 CreatedBy { get; set; }

        /// <summary>
        /// Дата последнего обновления записи
        /// </summary>
        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }

        /// <summary>
        /// Кем обновлена
        /// </summary>
        [Column("last_upd_by")]
        public Int64? LastUpdateByUserId { get; set; }

        /// <summary>
        /// Связанный объект типа уведомления
        /// </summary>
        [ForeignKey("NotificationTypeId")]
        public virtual NotificationType NotificationType { get; set; }

        /// <summary>
        /// Связанный объект канала уведомления
        /// </summary>
        [ForeignKey("NotificationChannelId")]
        public virtual NotificationChannel NotificationChannel { get; set; }
    }
}
