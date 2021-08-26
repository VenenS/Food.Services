using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Представляет город.
    /// </summary>
    [Table("city")]
    public class City : EntityBase<long>
    {
        [Column("name")]
        public string Name { get; set; }

        [Column("subject_rf_id")]
        public int SubjectId { get; set; }

        public virtual Subject Subject { get; set; }

        public virtual List<Cafe> Cafes { get; set; }
    }
}
