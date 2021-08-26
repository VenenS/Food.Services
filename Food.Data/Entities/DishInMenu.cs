using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Представляет блюдо в меню.
    /// </summary>
    [Table("dish_in_menu")]
    public class DishInMenu : EntityBase<long>
    {
        /// <summary>
        /// Возвращает или задает идентификатор блюда.
        /// </summary>
        [Column("dish_id")]
        public long DishId { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; }

        [ForeignKey("DishId")]
        public virtual Dish Dish { get; set; }

        [Column("s_type")]
        [StringLength(1)]
        public string Type { get; set; }

        [Column("schedule_begin_date")]
        public DateTime? BeginDate { get; set; }

        [Column("schedule_end_date")]
        public DateTime? EndDate { get; set; }

        [Column("one_time_date")]
        public DateTime? OneDate { get; set; }

        [StringLength(100)]
        [Column("s_month_day")]
        public string MonthDays { get; set; }

        [StringLength(30)]
        [Column("s_week_day")]
        public string WeekDays { get; set; }

        [Column("food_price_rub")]
        public double? Price { get; set; }

        [Column("create_date")]
        public DateTime? CreateDate { get; set; }

        [Column("created_by")]
        public Int64? CreatorId { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }

        [Column("last_upd_by")]
        public Int64? LastUpdateByUserId { get; set; }

        public virtual User Creator { get; set; }
    }
}
