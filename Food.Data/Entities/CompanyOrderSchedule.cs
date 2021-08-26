using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Расписание корпоративного заказа. Создает Куратор для кафе.
    /// </summary>
    [Table("company_order_schedule")]
    public class CompanyOrderSchedule : EntityBase<long>
    {
        /// <summary>
        /// Возвращает или задает идентификатор кафе.
        /// </summary>
        [Column("cafe_id")]
        public long CafeId { get; set; }

        /// <summary>
        /// Возвращает или задает идентификатор компании.
        /// </summary>
        [Column("company_id")]
        public long CompanyId { get; set; }

        public virtual Cafe Cafe { get; set; }
        public virtual Company Company { get; set; }

        [Column("company_delivery_address_id")]
        public Int64 CompanyDeliveryAdress { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Дата, с которого начинает действовать расписание
        /// </summary>
        [Column("schedule_begin_date")]
        public DateTime? BeginDate { get; set; }

        /// <summary>
        /// Конечная дата действия расписания
        /// </summary>
        [Column("schedule_end_date")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Время начала корпоративного заказа
        /// </summary>
        [Column("order_start_time")]
        public TimeSpan OrderStartTime { get; set; }

        /// <summary>
        /// Время окончания корпоративного заказа
        /// </summary>
        [Column("order_stop_date")]
        public TimeSpan OrderStopTime { get; set; }

        /// <summary>
        /// Время отправки корпоративного заказа
        /// </summary>
        [Column("order_send_time")]
        public TimeSpan OrderSendTime { get; set; }

        [Column("create_date")]
        public DateTime? CreateDate { get; set; }

        [Column("created_by")]
        public Int64? CreatorId { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }

        [Column("last_upd_by")]
        public Int64? LastUpdateByUserId { get; set; }
    }
}
