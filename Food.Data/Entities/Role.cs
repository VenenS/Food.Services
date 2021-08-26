using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Представляет роль.
    /// </summary>
    [Table("role")]
    public class Role : EntityBase<long>
    {
        [Column("role_name")]
        [StringLength(50)]
        public string RoleName { get; set; }

        public virtual ICollection<UserInRole> UserInRoles {get;set;}
        public virtual ICollection<UserInCompany> UserInCompanies { get; set; }
    }
}
