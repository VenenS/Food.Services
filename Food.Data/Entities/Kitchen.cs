using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Представляет кухню.
    /// </summary>
    [Table("kitchen")]
    public class Kitchen : EntityBase<long>
    {
        [StringLength(50)]
        [Column("kitchen_name")]
        public string Name { get; set; }

        public virtual ICollection<KitchenInCafe> KitchenInCafes { get; set; }
    }
}
