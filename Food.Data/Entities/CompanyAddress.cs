using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Представляет адрес компании.
    /// </summary>
    [Table("address_company_link")]
    public class CompanyAddress : EntityBase<long>
    {
        /// <summary>
        /// Возвращает или задает идентификатор адреса.
        /// </summary>
        [Column("address_id")]
        public long AddressId { get; set; }

        /// <summary>
        /// Возвращает или задает идентификатор компании.
        /// </summary>
        [Column("company_id")]
        public long CompanyId { get; set; }

        public virtual Address Address { get; set; }
        public virtual Company Company { get; set; }

        [Column("create_date")]
        public DateTime? CreateDate { get; set; }

        [Column("created_by")]
        public Int64? CreatorId { get; set; }

        [Column("display_type")]
        public EnumDisplayAddressType DisplayType { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }

        [Column("last_upd_by")]
        public long? LastUpdateByUserId { get; set; }
    }
}
