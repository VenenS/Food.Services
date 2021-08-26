using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("reports_extensions")]
    public class Ext : EntityBase<long>
    {
        [StringLength(2048)]
        [Column("ext")]
        public string Name { get; set; }

        [Column("create_date")]
        public DateTime? CreateDate { get; set; }

        [Column("created_by")]
        public Int64? CreatorId { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }

        [Column("last_upd_by")]
        public Int64? LastUpdateByUserId { get; set; }
    }
}
