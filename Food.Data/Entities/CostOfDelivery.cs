using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("cost_of_delivery")]
    public class CostOfDelivery : EntityBase<long>
    {
        /// <summary>
        /// Возвращает или задает идентификатор кафе.
        /// </summary>
        [Column("cafe_id")]
        public long CafeId { get; set; }

        public virtual Cafe Cafe { get; set; }

        [Column("order_price_from")]
        public double OrderPriceFrom { get; set; }

        [Column("order_price_to")]
        public double OrderPriceTo { get; set; }

        [Column("delivery_price")]
        public double DeliveryPrice { get; set; }

        [Column("create_date")]
        public DateTime? CreateDate { get; set; }

        [Column("created_by")]
        public Int64? CreatorId { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }

        [Column("last_upd_by")]
        public Int64? LastUpdateByUserId { get; set; }

        [Column("for_company_orders")]
        public bool ForCompanyOrders { get; set; }
    }
}
