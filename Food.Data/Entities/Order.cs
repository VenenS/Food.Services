using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Представляет заказ.
    /// </summary>
    [Table("order")]
    public class Order : EntityBase<long>
    {

        [Column("banket_id")]
        public long? BanketId { get; set; }

        /// <summary>
        /// Возвращает или задает идентификатор кафе.
        /// </summary>
        [Column("cafe_id")]
        public long CafeId { get; set; }

        [Column("comment")]
        public string Comment { get; set; }

        /// <summary>
        /// Возвращает или задает идентификатор заказа компании.
        /// </summary>
        [Column("company_order_id")]
        public long? CompanyOrderId { get; set; }

        [Column("state")]
        public EnumOrderState State { get; set; }

        [Column("total_price")]
        public double TotalPrice { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        public virtual Cafe Cafe { get; set; }
        public virtual CompanyOrder CompanyOrder { get; set; }
        public virtual User User { get; set; }

        [Column("created_by")]
        public long? CreatorId { get; set; }

        [Column("order_create_date")]
        public DateTime CreationDate { get; set; }

        [Column("deliver_datetime")]
        public DateTime? DeliverDate { get; set; }

        [Column("order_delivery_address")]
        public long? DeliveryAddressId { get; set; }

        [Column("order_item_count")]
        public int? ItemsCount { get; set; }

        [Column("last_upd_by")]
        public long? LastUpdateByUserId { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }

        [Column("odd_money_comment")]
        [StringLength(100)]
        public string OddMoneyComment { get; set; }

        [Column("order_phone_number")]
        [StringLength(50)]
        public string PhoneNumber { get; set; }

        [Column("order_info_id")]
        public long? OrderInfoId { get; set; }

        [ForeignKey(nameof(OrderInfoId))]
        public virtual OrderInfo OrderInfo { get; set; }

        [ForeignKey("BanketId")]
        public virtual Banket Banket { get; set; }
        /// <summary>
        /// Возвращает или задает идентификатор города.
        /// </summary>
        [Column("city_id")]
        public long CityId { get; set; }
        [ForeignKey("CityId")]
        public virtual City City { get; set; }

        [ForeignKey(nameof(DeliveryAddressId))]
        public virtual Address DeliveryAddress { get; set; }

        public virtual List<OrderItem> OrderItems { get; set; }

        /// <summary>
        /// Комментарий менеджера к заказу
        /// </summary>
        [Column("manager_comment")]
        public string ManagerComment { get; set; }

        /// <summary>
        /// Тип оплаты
        /// </summary>
        [Column("pay_type")]
        public string PayType { get; set; }
    }
}
