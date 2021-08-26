using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("object_type")]
    public class ObjectType: EntityBaseDeletable<long>
    {
        [Column("description")]
        [MaxLength(256)]
        public string Description { get; set; }
    }
}
