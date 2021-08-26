using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Хранится вся история измений блюда. 
    /// </summary>
    [Table("dish_version")]
    public class DishVersion :EntityBase<long>
    {
        [Column("dish_id")]
        public Int64 DishId { get; set; }

        [Column("cafe_category_link_id")]
        public Int64 CafeCategoryId { get; set; }

        [StringLength(100)]
        [Column("food_dish_name")]
        public string DishName { get; set; }

        [Column("kcalories")]
        public double? Kcalories { get; set; }

        [Column("weight")]
        public double? Weight { get; set; }

        [Column("weight_description")]
        [StringLength(100)]
        public string WeightDescription { get; set; }

        [Column("version_id")]
        public Int64? VersionId { get; set; }

        [Column("version_from")]
        public DateTime? VersionFrom { get; set; }

        [Column("version_to")]
        public DateTime? VersionTo { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; }

        [Column("base_price_rub")]
        public double BasePrice { get; set; }

        [Column("food_dish_index")]
        public int? DishIndex { get; set; }

        [Column("create_date")]
        public DateTime? CreateDate { get; set; }

        [Column("created_by")]
        public Int64? CreatorId { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }

        [Column("last_upd_by")]
        public Int64? LastUpdateByUserId { get; set; }

        [Column("image_id")]
        public string ImageId { get; set; }

        [ForeignKey("DishId")]
        public virtual Dish Dish { get; set; }

        [ForeignKey("CafeCategoryId")]
        public virtual DishCategoryInCafe CafeCategory { get; set; }
    }
}
