using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("referral_coef")]
    public class ReferralCoefficient
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("ref_level")]
        public int Level { get; set; }

        [Column("coef")]
        public double Coefficient { get; set; }
    }
}
