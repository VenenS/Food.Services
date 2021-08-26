using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("cafe_menu_patterns_dishes")]
    public class CafeMenuPatternDish : EntityBase<long>
    {
        [Column("dish_id")]
        public long DishId { get; set; }

        [Column("pattern_id")]
        public long PatternId { get; set; }

        [Column("price")]
        public double Price { get; set; }
        
        [Column("name")]
        public string Name { get; set; }

        public virtual CafeMenuPattern Pattern { get; set; }
        public virtual Dish Dish { get; set; }
    }
}