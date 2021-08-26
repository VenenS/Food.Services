using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("company_order_status_history")]
    public class CompanyOrderStateTransition : EntityBase<long>
    {
        /// <summary>
        /// Возвращает или задает идентификатор заказа компании.
        /// </summary>
        [Column("company_order_id")]
        public long CompanyOrderId { get; set; }

        public virtual CompanyOrder CompanyOrder { get; set; }

        [Column("order_status")]
        public Int64 Status { get; set; }

        [Column("created_by")]
        public Int64? CreatedBy { get; set; }

        [Column("create_date")]
        public DateTime CreateDate { get; set; }

        [Column("last_upd_by")]
        public Int64? LastUpdateByUserId { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }
    }
}
