using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("refresh_token")]
    public class RefreshToken
    {
        [Key, Column("id")]
        public int Id { get; set; }

        [Required, Column("token")]
        public string Token { get; set; }

        [Required, MaxLength(50), Column("subject")]
        public string Subject { get; set; }

        [Required, Column("client_id"), ForeignKey("Client")]
        public string ClientId { get; set; }

        [Column("issued_utc")]
        public DateTime IssuedUtc { get; set; }

        [Column("expired_utc")]
        public DateTime ExpiresUtc { get; set; }

        [Required, Column("protected_ticket")]
        public string ProtectedTicket { get; set; }

        public virtual Client Client { get; set; }
    }
}