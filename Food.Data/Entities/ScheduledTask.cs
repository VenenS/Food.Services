using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Представляет запланированную задачу.
    /// </summary>
    [Table("scheduled_tasks")]
    public class ScheduledTask : EntityBase<long>
    {
        [Column("created_by")]
        public long? CreatorId { get; set; }

        [Column("create_date")]
        public DateTime? CreateDate { get; set; }

        [Column("last_upd_by")]
        public long? LastUpdateBy { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }

        /// <summary>
        /// Повторное выполнения задачи
        /// </summary>
        [Column("is_repeatable")]
        public long IsRepeatable { get; set; }

        /// <summary>
        /// Запланированное время выполнения задачи.
        /// </summary>
        [Column("execution_time")]
        public DateTime? ScheduledExecutionTime { get; set; }

        /// <summary>
        /// Тип задачи
        /// </summary>
        [Column("type")]
        public EnumScheduledTaskType TaskType { get; set; }

        [Column("order_id")]
        public long? OrderId { get; set; }

        public virtual Order Order { get; set; }

        [Column("cafe_id")]
        public long? CafeId { get; set; }

        public virtual Cafe Cafe { get; set; }

        [Column("company_order_id")]
        public long? CompanyOrderId { get; set; }

        [Column("banket_id")]
        public long? BanketId { get; set; }

        public virtual Banket Banket { get; set; }
    }
}
