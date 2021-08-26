using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("dish_in_menu_history")]
    public class DishInMenuHistory : EntityBaseDeletable<long>
    {
        /// <summary>
        /// идентификатор блюда
        /// </summary>
        [Column("dish_id")]
        public long DishId { get; set; }

        /// <summary>
        /// автор изменения
        /// </summary>
        [Column("user_id")]
        public long UserId { get; set; }

        /// <summary>
        /// время изменения
        /// </summary>
        [Column("last_upd_date")]
        public DateTime LastUpdDate { get; set; }

        /// <summary>
        /// Тип расписания
        /// Тип расписания:
        ///D - daily(блюдо доступно ежедневно),
        ///W - weekly(блюдо доступно в определенные дни недели),
        ///M - monthly(блюдо доступно в определенные дни месяца)
        ///S - simple(one time run)
        /// </summary>
        [Column("s_type")]
        [StringLength(1)]
        public string Type { get; set; }
        /// <summary>
        /// цена блюда
        /// </summary>
        [Column("price")]
        public double Price { get; set; }

        [ForeignKey("DishId")]
        public virtual Dish Dish { get; set; }

        [ForeignKey("UserId")]
        public virtual User Creator { get; set; }
    }
}
