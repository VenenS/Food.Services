using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("rating")]
    public class Rating : EntityBase<long>
    {
        [Column("object_type_id")]
        public int ObjectType { get; set; }

        [Column("object_id")]
        public Int64 ObjectId { get; set; }

        [Column("create_date")]
        public DateTime? CreateDate { get; set; }

        [Column("last_upd_date")]
        public DateTime LastUpdateDate { get; set; }

        [Column("last_upd_by")]
        public Int64? LastUpdatedBy { get; set; }

        [Column("created_by")]
        public Int64? CreatorId { get; set; }

        [Column("user_id")]
        public Int64 UserId { get; set; }

        [Column("rating_value")]
        public int RatingValue { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
