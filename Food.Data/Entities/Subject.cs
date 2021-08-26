using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("subject_rf")]
    public class Subject : EntityBaseDeletable<int>
    {
        [Column("name")]
        public string Name { get; set; }

        public virtual List<City> Cities { get; set; }
    }
}
