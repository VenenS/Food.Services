using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey">Тип идентификатора.</typeparam>
    public class TrackableEntityBase<TKey> : EntityBase<TKey>
    {
        /// <summary>
        /// Возвращает или задает дату создания.
        /// </summary>
        [Column("date_created")]
        public DateTime DateCreated { get; set; }

        [Column("date_updated")]
        public DateTime? DateUpdated { get; set; }
    }
}
