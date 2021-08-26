using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("tags")]
    public class Tag : EntityBase<long>
    {
        [Column("name")]
        [MaxLength(256)]
        public string Name { get; set; }

        [Column("parent_id")]
        public Int64? ParentId { get; set; }

        [Column("create_date")]
        public DateTime? CreateDate { get; set; }

        [Column("created_by")]
        public Int64? CreatorId { get; set; }

        [Column("last_upd")]
        public DateTime? LastUpdDate { get; set; }

        [Column("last_upd_by")]
        public Int64? LastUpdateByUserId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [ForeignKey("ParentId")]
        public virtual Tag Parent { get; set; }
    }
}
