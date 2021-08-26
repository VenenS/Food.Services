using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("tag_object_link")]
    public class TagObject : EntityBase<long>
    {
        [Column("tag_id")]
        public Int64 TagId { get; set; }

        [Column("object_id")]
        public Int64 ObjectId { get; set; }

        [Column("object_type_id")]
        public Int64 ObjectTypeId { get; set; }

        [Column("create_date")]
        public DateTime? CreateDate { get; set; }

        [Column("created_by")]
        public Int64? CreatorId { get; set; }

        [Column("last_upd")]
        public DateTime? LastUpdDate { get; set; }

        [Column("last_upd_by")]
        public Int64? LastUpdateByUserId { get; set; }

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; }

        [ForeignKey("ObjectTypeId")]
        public virtual ObjectType ObjectType { get; set; }
    }
}
