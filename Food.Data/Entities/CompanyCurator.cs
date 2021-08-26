using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("company_curators")]
    public class CompanyCurator: EntityBase<long>
    {
        /// <summary>
        /// Возвращает или задает идентификатор пользователя.
        /// </summary>
        [Column("user_id")]
        public long UserId { get; set; }

        /// <summary>
        /// Возвращает или задает идентификатор компании.
        /// </summary>
        [Column("company_id")]
        public long CompanyId { get; set; }

        [Column("create_date")]
        public DateTime? CreationDate { get; set; }

        [Column("created_by")]
        public Int64? CreatorId { get; set; }

        [Column("last_upd_by")]
        public Int64? LastUpdateByUserId { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }

        public virtual Company Company { get; set; }
        public virtual User User { get; set; }
    }
}
