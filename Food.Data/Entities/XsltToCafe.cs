using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    [Table("xslt_to_cafe")]
    public class XsltToCafe : EntityBase<long>
    {
        /// <summary>
        /// Возвращает или задает идентификатор кафе.
        /// </summary>
        [Column("id_cafe")]
        public long CafeId { get; set; }
        [ForeignKey("CafeId")]
        public virtual Cafe Cafe { get; set; }
        /// <summary>
        /// Возвращает или задает идентификатор шаблона.
        /// </summary>
        [Column("xslt_id")]
        public long XsltId { get; set; }

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