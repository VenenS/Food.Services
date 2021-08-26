using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Связь Пользователя с Компанией
    /// </summary>
    [Table("user_company_link")]
    public class UserInCompany : EntityBase<long>
    {
        /// <summary>
        /// Возвращает или задает идентификатор компании.
        /// </summary>
        [Column("company_id")]
        public long CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        /// <summary>
        /// Возвращает или задает идентификатор пользователя.
        /// </summary>
        [Column("user_id")]
        public long UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Column("role_id")]
        public Int64? UserRoleId { get; set; }
        [ForeignKey("UserRoleId")]
        public virtual Role UserRole { get; set; }

        [Column("default_address_id")]
        public long? DefaultAddressId { get; set; }
        [ForeignKey("DefaultAddressId")]
        public virtual Address DefaultAddress { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("link_start_date")]
        public DateTime StartDate { get; set; }

        [Column("link_end_date")]
        public DateTime? EndDate { get; set; }

        [Column("user_type")]
        public string UserType { get; set; }

        [Column("create_date")]
        public DateTime CreateDate { get; set; }

        [Column("created_by")]
        public Int64 CreatedBy { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdateDate { get; set; }

        [Column("last_upd_by")]
        public Int64? LastUpdateBy { get; set; }
    }
}
