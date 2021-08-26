using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Представляет категорию блюд.
    /// </summary>
    [Table("dish_category")]
    public class DishCategory : EntityBase<long>
    {
        [StringLength(1024)]
        [Column("category_description")]
        public string Description { get; set; }

        [StringLength(50)]
        [Column("category_name")]
        public string CategoryName { get; set; }

        [StringLength(100)]
        [Column("category_full_name")]
        public string CategoryFullName { get; set; }

        [NotMapped]
        public int? Index { get; set; }

        /// <summary>
        /// Общее количество блюд в данной категории, доступное для текущего запроса.
        /// Нужно для выбора 6 случайных блюд в каждой категории и отображения кнопки
        /// "+ ещё N" для показа остальных блюд (на главной странице).
        /// </summary>
        [NotMapped]
        public int CurrentDishCount { get; set; } = 0;

        [Column("is_active")]
        public bool? IsActive { get; set; }

        [Column("create_date")]
        public DateTime? CreationDate { get; set; }

        [Column("created_by")]
        public Int64? CreatorId { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }

        [Column("last_upd_by")]
        public Int64? LastUpdateByUserId { get; set; }

        [Column("guid")]
        public Guid Uuid { get; set; }

        public virtual ICollection<DishCategoryInCafe> DishCategoryInCafes { get; set; }
    }
}
