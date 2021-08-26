using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Таблица - справочник типов уведомлений, например: создание нового пользователя, новый заказ.
    /// </summary>
    [Table("notification_type")]
    public class NotificationType
    {
        /// <summary>
        /// Идентификатор типа уведомления
        /// </summary>
        [Key]
        [Column("id")]
        public Int16 Id { get; set; }

        /// <summary>
        /// Код уведомления
        /// </summary>
        [Column("notification_type_code")]
        public string NotificationTypeCode { get; set; }

        /// <summary>
        /// Описание типа уведомления
        /// </summary>
        [Column("notification_type_name")]
        public string NotificationTypeName { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
