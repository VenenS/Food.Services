using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Представляет скидку.
    /// </summary>
    [Table("discount")]
    public class Discount : EntityBase<long>
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
        public long? CompanyId { get; set; }

        /// <summary>
        /// Возвращает или задает идентификатор пользователя.
        /// </summary>
        [Column("user_id")]
        public long? UserId { get; set; }

        public virtual Cafe Cafe { get; set; }
        public virtual Company Company { get; set; }
        public virtual User User { get; set; }

        [Column("discount")]
        public double Value { get; set; }

        [Column("discount_begin_date")]
        public DateTime BeginDate { get; set; }

        [Column("discount_end_date")]
        public DateTime? EndDate { get; set; }

        [Column("create_date")]
        public DateTime? CreationDate { get; set; }

        [Column("created_by")]
        public Int64? CreatorId { get; set; }

        [Column("last_upd_by")]
        public Int64? LastUpdateByUserId { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }
    }
}
