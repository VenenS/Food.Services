using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("images")]
    public class Image : EntityBase<long>
    {
        [Column("object_id")]
        public Int64 ObjectId { get; set; }

        [Column("hash")]
        public string Hash { get; set; }

        [Column("object_type_id")]
        public int ObjectType { get; set; }

        [Column("create_date")]
        public DateTime? CreateDate { get; set; }

        [Column("created_by")]
        public Int64? CreatorId { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }

        [Column("last_upd_by")]
        public Int64? LastUpdateByUserId { get; set; }
    }
}
