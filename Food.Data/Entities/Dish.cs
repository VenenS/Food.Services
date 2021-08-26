using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Food.Data.Entities
{
    /// <summary>
    /// Представляет блюдо.
    /// </summary>
    [Table("dish")]
    public class Dish : EntityBase<long>
    {
        /// <summary>
        /// Возвращает или задает описание.
        /// </summary>
        [Column("description")]
        public string Description { get; set; }

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

        [Column("version_from")]
        public DateTime? VersionFrom { get; set; }

        [Column("version_to")]
        public DateTime? VersionTo { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; }

        [Column("base_price_rub")]
        public double BasePrice { get; set; }

        [Column("image_id")]
        public string ImageId { get; set; }

        [Column("guid")]
        public Guid Uuid { get; set; }

        [Column("dish_rating_sum")]
        public Int64 DishRatingSumm { get; set; }

        [Column("dish_rating_count")]
        public Int64 DishRatingCount { get; set; }

        [Column("composition")]
        public string Composition { get; set; }

        [Column("create_date")]
        public DateTime? CreationDate { get; set; }

        [Column("created_by")]
        public Int64? CreatorId { get; set; }

        [Column("last_upd_by")]
        public Int64? LastUpdateByUserId { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }
        
        public virtual ICollection<DishInMenu> Schedules { get; set; }
        public virtual ICollection<DishVersion> Versions { get; set; }
        public virtual List<DishCategoryLink> DishCategoryLinks { get; set; }

        [NotMapped]
        public Cafe Cafe => DishCategoryLinks?.FirstOrDefault()?.CafeCategory?.Cafe;
    }
}
