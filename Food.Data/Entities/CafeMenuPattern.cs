using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("cafe_menu_patterns")]
    public class CafeMenuPattern : EntityBase<long>
    {
        [Column("name")]
        public string Name { get; set; }

        [Column("cafe_id")]
        public long CafeId { get; set; }
        [ForeignKey("CafeId")]
        public virtual Cafe Cafe { get; set; }

        [Column("pattern_to_date")]
        public DateTime PatternDate { get; set; }

        [Column("is_banket")]
        public bool IsBanket { get; set; }


        public virtual List<CafeMenuPatternDish> Dishes { get; set; }

        public virtual List<Banket> Bankets { get; set; }
    }
}