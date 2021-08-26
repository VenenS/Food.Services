using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Представляет заказ компании.
    /// </summary>
    [Table("company_order")]
    public class CompanyOrder : EntityBase<long>
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

        [Column("state")]
        public long State { get; set; }

        [Column("total_price")]
        public double? TotalPrice { get; set; }

        public virtual Cafe Cafe { get; set; }
        public virtual Company Company { get; set; }

        [Column("order_auto_close_date")]
        public DateTime? AutoCloseDate { get; set; }

        [Column("order_delivery_address")]
        public long? DeliveryAddress { get; set; }

        [Column("order_delivery_date")]
        public DateTime? DeliveryDate { get; set; }

        [Column("contact_email")]
        public string ContactEmail { get; set; }

        [Column("contact_phone")]
        public string ContactPhone { get; set; }

        [Column("order_open_date")]
        public DateTime? OpenDate { get; set; }

        [Column("order_create_date")]
        public DateTime? OrderCreateDate { get; set; }

        [Column("create_date")]
        public DateTime? CreationDate { get; set; }

        [Column("created_by")]
        public long? CreatorId { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdate { get; set; }

        [Column("last_upd_by")]
        public long? LastUpdateByUserId { get; set; }

        /// <summary>
        /// Заказы сотрудников, входящие в состав корпоративного заказа
        /// </summary>
        public virtual ICollection<Order> Orders { get; set; }

        /// <summary>
        /// Общая стоимость доставки корпоративного заказа
        /// </summary>
        [Column("total_delivery_cost")]
        public double TotalDeliveryCost { get; set; }
    }
}
