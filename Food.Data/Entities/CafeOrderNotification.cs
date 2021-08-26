using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Список менеджеров, которых необходимо уведомить о пришедших заказов
    /// </summary>
    [Table("cafe_order_notification")]
    public class CafeOrderNotification :EntityBaseDeletable<long>
    {
        /// <summary>
        /// Идентификатор кафе
        /// </summary>
        [Column("cafe_id")]
        public long CafeId { get; set; }
        [ForeignKey("CafeId")]
        public virtual Cafe Cafe { get; set; }
        /// <summary>
        /// Время появления заказа
        /// </summary>
        [Column("deliver_datetime")]
        public DateTime DeliverDate { get; set; }

        /// <summary>
        /// Идентификатор пользователя (Менеджера кафе)
        /// </summary>
        [Column("user_id")]
        public long UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
