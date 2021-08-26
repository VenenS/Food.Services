using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("bankets")]
    public class Banket : EntityBase<long>
    {
        [Column("menu_id")]
        public long MenuId { get; set; }

        [Column("cafe_id")]
        public long CafeId { get; set; }

        [Column("company_id")]
        public long CompanyId { get; set; }

        [Column("event_date")]
        public DateTime EventDate { get; set; }

        [Column("order_start_date")]
        public DateTime OrderStartDate { get; set; }

        [Column("order_end_date")]
        public DateTime OrderEndDate { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("status")]
        public EnumBanketStatus Status { get; set; }

        [Column("url")]
        public string Url { get; set; }

        [Column("total_sum")]
        public double TotalSum { get; set; }

        [ForeignKey("MenuId")]
        public virtual CafeMenuPattern Menu { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        [ForeignKey("CafeId")]
        public virtual Cafe Cafe { get; set; }

        public virtual List<Order> Orders { get; set; }
    }
}