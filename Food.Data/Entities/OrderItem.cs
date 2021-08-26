using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Представляет позицию заказа.
    /// </summary>
    [Table("order_item")]
    public class OrderItem : EntityBase<long>
    {
        [Column("comment")]
        public string Comment { get; set; }

        /// <summary>
        /// Возвращает или задает идентификатор блюда.
        /// </summary>
        [Column("dish_id")]
        public long DishId { get; set; }

        /// <summary>
        /// Возвращает или задает идентификатор заказа.
        /// </summary>
        [Column("order_id")]
        public long OrderId { get; set; }

        [Column("total_price")]
        public double TotalPrice { get; set; }

        public virtual Dish Dish { get; set; }
        public virtual Order Order { get; set; }

        [Column("food_dish_name")]
        [StringLength(100)]
        public string DishName { get; set; }

        [Column("food_dish_kcalories")]
        public double? DishKcalories { get; set; }

        [Column("food_dish_weight")]
        public double? DishWeight { get; set; }

        [Column("food_dish_image_id")]
        public Int64? ImageId { get; set; }

        [Column("dish_count")]
        public int DishCount { get; set; }

        [Column("dish_base_price")]
        public double DishBasePrice { get; set; }

        [Column("dish_discount_prc")]
        public int? DishDiscountPrc { get; set; }

        [Column("created_by")]
        public Int64? CreatorId { get; set; }

        [Column("create_date")]
        public DateTime? CreationDate { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }

        [Column("last_upd_by")]
        public Int64? LastUpdateByUserId { get; set; }
    }
}
