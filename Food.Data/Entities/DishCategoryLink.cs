using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Food.Data.Entities
{
    /// <summary>
    /// Связь блюда и категории кафе
    /// </summary>
    [Table("dish_cafe_category_link")]
    public class DishCategoryLink : EntityBase<long>
    {
        [Column("dish_id")]
        public long DishId { get; set; }

        public virtual Dish Dish { get; set; }

        [Column("cafe_category_link_id")]
        public long CafeCategoryId { get; set; }

        public virtual DishCategoryInCafe CafeCategory { get; set; }

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

        [Column("is_active")]
        public bool? IsActive { get; set; }

        /// <summary>
        /// Возвращает true если связанные блюдо и категория активны
        /// </summary>
        [NotMapped]
        public bool IsActiveLink =>
            Dish?.IsDeleted == false
            && Dish?.IsActive == true
            && CafeCategory?.IsDeleted == false
            && CafeCategory?.IsActive == true
            && CafeCategory?.DishCategory?.IsDeleted == false
            && CafeCategory?.DishCategory?.IsActive == true;
    }
}
