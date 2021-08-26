using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Представляет адрес.
    /// </summary>
    [Table("address")]
    public class Address: EntityBase<long>
    {
        [StringLength(10)]
        [Column("buildnum")]
        public string BuildingNumber { get; set; }

        [Column("city_id")]
        public long CityId { get; set; }

        [StringLength(120)]
        [Column("city_offname")]
        public string CityName { get; set; }

        [StringLength(512)]
        [Column("extrainfo")]
        public string ExtraInfo { get; set; }

        [StringLength(128)]
        [Column("flat")]
        public string FlatNumber { get; set; }

        [Column("house_id")]
        public long HouseId { get; set; }

        [StringLength(10)]
        [Column("housenum")]
        public string HouseNumber { get; set; }

        [StringLength(128)]
        [Column("office")]
        public string OfficeNumber { get; set; }

        [StringLength(6)]
        [Column("postalcode")]
        public string PostalCode { get; set; }

        [Column("street_id")]
        public long StreetId { get; set; }

        [StringLength(120)]
        [Column("street_offname")]
        public string StreetName { get; set; }

        [Column("entrance")]
        public string EntranceNumber { get; set; }

        [Column("storey")]
        public string StoreyNumber { get; set; }

        [Column("intercom")]
        public string IntercomNumber { get; set; }

        [StringLength(150)]
        [Column("raw_address")]
        public string RawAddress { get; set; }

        [Column("address_comment")]
        public string AddressComment { get; set; }

        [Column("create_date")]
        public DateTime? CreationDate { get; set; }

        [Column("created_by")]
        public long? CreatorId { get; set; }

        [Column("last_upd_by")]
        public long? LastUpdateByUserId { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }

        [ForeignKey("CityId")]
        public virtual City City { get; set; }

        public virtual ICollection<CompanyAddress> CompanyAddresses { get; set; }
        public virtual ICollection<UserInCompany> DefaultAddressInCompanies { get; set; }
    }
}
