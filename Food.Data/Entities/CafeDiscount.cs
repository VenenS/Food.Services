using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("cafe_discount")]
    public class CafeDiscount : EntityBase<long>
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

        public virtual Cafe Cafe { get; set; }
        public virtual Company Company { get; set; }

        [Column("summ_from")]
        public double SummFrom { get; set; }

        [Column("summ_to")]
        public double? SummTo { get; set; }

        [Column("percent")]
        public double? Percent { get; set; }

        [Column("summ")]
        public double? Summ { get; set; }

        [Column("create_date")]
        public DateTime CreateDate { get; set; }

        [Column("created_by")]
        public Int64 CreatorId { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }

        [Column("last_upd_by")]
        public Int64? LastUpdateByUserId { get; set; }

        [Column("discount_begin_date")]
        public DateTime BeginDate { get; set; }

        [Column("discount_end_date")]
        public DateTime? EndDate { get; set; }
    }
}
