using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Food.Data.Entities
{
    [Table("user_password_reset_keys")]
    public class UserPasswordResetKey : EntityBaseDeletable<long>
    {
        [Column("user_id")]
        public long UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Column("key")]
        public string Key { get; set; }

        [Column("issued_to")]
        public DateTime IssuedTo { get; set; }
    }
}
