using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// История заказов
    /// </summary>
    [Table("order_info")]
    public class OrderInfo : EntityBase<long>
    {
        [Column("delivery_summ")]
        public double DeliverySumm { get; set; }

        [Column("discount_summ")]
        public double DiscountSumm { get; set; }

        [Column("total_summ")]
        public double TotalSumm { get; set; }

        [Column("payment_type")]
        public long? PaymentType { get; set; }

        [Column("create_date")]
        public DateTime CreateDate { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdate { get; set; }

        [Column("last_upd_by")]
        public long? LastUpdateBy { get; set; }

        [Column("order_email")]
        public string OrderEmail { get; set; }

        [Column("order_phone")]
        public string OrderPhone { get; set; }

        [Column("order_address")]
        public string OrderAddress { get; set; }
        /// <summary>
        /// Возвращает или задает идентификатор города.
        /// </summary>
        [Column("city_id")]
        public long CityId { get; set; }
        [ForeignKey("CityId")]
        public virtual City City { get; set; }

        public virtual Order Order { get; set; }
    }
}
