using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("cafe_managers")]
    public class CafeManager : EntityBase<long>
    {
        /// <summary>
        /// Возвращает или задает идентификатор кафе.
        /// </summary>
        [Column("cafe_id")]
        public long CafeId { get; set; }

        /// <summary>
        /// Возвращает или задает идентификатор пользователя.
        /// </summary>
        [Column("user_id")]
        public long UserId { get; set; }

        [ForeignKey(nameof(CafeId))]
        public virtual Cafe Cafe { get; set; }
        public virtual User User { get; set; }

        [StringLength(30)]
        [Column("user_role")]
        public string UserRoleId { get; set; }

        [Column("created_by")]
        public Int64? CreatedBy { get; set; }

        [Column("create_date")]
        public DateTime? CreationDate { get; set; }

        [Column("last_upd_by")]
        public Int64? LastUpdateBy { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdateDate { get; set; }
    }
}
