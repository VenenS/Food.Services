using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Представляет пользователя.
    /// </summary>
    [Table("user")]
    public class User : EntityBase<long>
    {
        [StringLength(256)]
        [Column("user_email")]
        public string Email { get; set; }

        [Column("user_email_confirmed")]
        public bool EmailConfirmed { get; set; }

        [StringLength(50)]
        [Column("user_first_name")]
        public string FirstName { get; set; }

        [StringLength(50)]
        [Column("user_surname")]
        public string LastName { get; set; }

        [StringLength(256)]
        [Column("user_name")]
        public string Name { get; set; }

        /// <summary>
        /// Телефон пользователя
        /// </summary>
        [Column("user_phone")]
        public string PhoneNumber { get; set; }

        [Column("user_phone_confirmed")]
        public bool PhoneNumberConfirmed { get; set; }

        [Column("user_access_failed_count")]
        public int? AccessFailedCount { get; set; }

        [Column("created_by")]
        public long? CreatorId { get; set; }

        [Column("create_date")]
        public DateTime? CreationDate { get; set; }

        [Column("default_address_id")]
        public long? DefaultAddressId { get; set; }

        [StringLength(256)]
        [Column("user_device_uuid")]
        public string DeviceUuid { get; set; }

        [StringLength(50)]
        [Column("user_display_name")]
        public string DisplayName { get; set; }

        [Column("user_fullname")]
        public string FullName { get; set; }

        [Column("last_upd_by")]
        public long? LastUpdateByUserId { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }

        [Column("user_lockout_enabled")]
        public bool Lockout { get; set; }

        [Column("user_lockout_enddate_utc")]
        public DateTime? LockoutEnddate { get; set; }

        [Column("user_password")]
        public string Password { get; set; }

        [Column("user_security_stamp")]
        public string SecurityStamp { get; set; }

        [Column("user_twofactor_enabled")]
        public bool TwoFactor { get; set; }

        [Column("personal_points")]
        public double PersonalPoints { get; set; }

        [Column("referral_points")]
        public double ReferralPoints { get; set; }

        [Column("percent_of_order")]
        public double PercentOfOrder { get; set; }

        [Column("user_referral_link")]
        public string UserReferralLink { get; set; }

        /// <summary>
        /// Включает/отключает оповещение пользователя о заказах по СМС
        /// </summary>
        [Column("sms_notify")]
        public bool SmsNotify { get; set; }

        /// <summary>
        /// Код подтверждения E-mail
        /// </summary>
        [Column("user_email_confirmation_code")]
        public string EmailConfirmationCode { get; set; }

        [ForeignKey("DefaultAddressId")]
        public virtual Address Address { get; set; }

        public virtual UserPasswordResetKey UserPasswordResetKey { get; set; }

        public virtual ICollection<UserInCompany> UserInCompanies { get; set; }
        public virtual ICollection<UserInRole> UserInRoles { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
