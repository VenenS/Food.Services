using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("xslt_transform")]
    public class ReportStylesheet : EntityBase<long>
    {
        /// <summary>
        /// Возвращает или задает идентификатор кафе.
        /// </summary>
        [Column("cafe_id")]
        public long? CafeId { get; set; }

        public virtual Cafe Cafe { get; set; }

        [Column("ext_id")]
        public Int64 ExtId { get; set; }

        /// <summary>
        /// Возвращает значение того, является ли шаблон общим для всех кафе или нет
        /// </summary>
        [Column("is_common")]
        public bool IsCommon { get; set; }

        [StringLength(128)]
        [Column("name_template")]
        public string Name { get; set; }

        [StringLength(1024)]
        [Column("description")]
        public string Description { get; set; }

        [Column("xslt")]
        public string Transformation { get; set; }

        [Column("create_date")]
        public DateTime? CreateDate { get; set; }

        [Column("created_by")]
        public Int64? CreatorId { get; set; }

        [Column("last_upd_date")]
        public DateTime? LastUpdDate { get; set; }

        [Column("last_upd_by")]
        public Int64? LastUpdateByUserId { get; set; }

        [ForeignKey("ExtId")]
        public virtual Ext Ext { get; set; }
    }
}
