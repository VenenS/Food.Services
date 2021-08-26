using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Представляет компанию.
    /// </summary>
    [Table("company")]
    public class Company : EntityBase<long>
    {
        [StringLength(1024)]
        [Column("company_full_name")]
        public string FullName { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("company_jur_address_id")]
        public long? JuridicalAddressId { get; set; }

        [Column("main_delivery_address_id")]
        public long? MainDeliveryAddressId { get; set; }

        [StringLength(256)]
        [Column("company_name")]
        public string Name { get; set; }

        [Column("company_post_address_id")]
        public long? PostAddressId { get; set; }

        public virtual ICollection<CompanyAddress> Addresses { get; set; }
        public virtual ICollection<UserInCompany> UserInCompanies { get; set; }

        [ForeignKey("JuridicalAddressId")]
        public virtual Address JuridicalAddress { get; set; }

        [ForeignKey("MainDeliveryAddressId")]
        public virtual Address MainDeliveryAddress { get; set; }

        [ForeignKey("PostAddressId")]
        public virtual Address PostAddress { get; set; }

        [Column("create_date")]
        public DateTime? CreationDate { get; set; }

        [Column("created_by")]
        public long? CreatorId { get; set; }

        [Column("last_upd_by")]
        public long? LastUpdateByUserId { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }
        /// <summary>
        /// Возвращает или задает идентификатор города.
        /// </summary>
        [Column("city_id")]
        public long CityId { get; set; }
        [ForeignKey("CityId")]
        public virtual City City { get; set; }

        /// <summary>
        ///  Включает/отключает оповещение пользователей компании о новых заказах по СМС
        /// </summary>
        [Column("sms_notify")]
        public bool SmsNotify { get; set; }

    }
}
