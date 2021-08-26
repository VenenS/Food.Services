using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Определяет состояние заказа компании.
    /// </summary>
    [Table("order_status")]
    public class CompanyOrderStatus : EntityBase<long>
    {
        [Column("status_name")]
        public string Name { get; set; }

        [Column("status_code")]
        public string Code { get; set; }

        [Column("guid")]
        public string Guid { get; set; }
    }
}
