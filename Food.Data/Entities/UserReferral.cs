using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Связь Пользователя с его Рефералами
    /// </summary>
    [Table("user_referral_link")]
    public class UserReferral
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("parent_id")]
        public long? ParentId { get; set; }

        [Column("referral_id")]
        public long RefId { get; set; }

        [Column("root_id")]
        public long RootId { get; set; }

        [Column("ref_level")]
        public int Level { get; set; }

        [Column("path_index")]
        public int PathIndex { get; set; }

        [Column("num_mapping")]
        public string NumMapping { get; set; }

        [Column("create_date")]
        public DateTime CreateDate { get; set; }

        [Column("earned_points")]
        public double EarnedPoints { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }

        [ForeignKey("ParentId")]
        public virtual User Parent { get; set; }

        [ForeignKey("RefId")]
        public virtual User Referral { get; set; }
    }
}
