using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("client")]
    public class Client
    {
        [Key, Column("id"), MaxLength(100)]
        public string Id { get; set; }

        [Required, Column("secret")]
        public string Secret { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("application_type")]
        public EnumApplicationTypes ApplicationType { get; set; }

        [Column("active")]
        public bool Active { get; set; }

        [Column("refresh_token_lifetime")]
        public int RefreshTokenLifeTime { get; set; }

        [MaxLength(100), Column("allowed_origin")]
        public string AllowedOrigin { get; set; }
    }
}