using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Базовая сущность
    /// </summary>
    /// <typeparam name="TKey">Тип идентификатора.</typeparam>
    public class EntityBase<TKey> : EntityBaseDeletable<TKey>
    {
        [Column("is_deleted")]
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey">Тип идентификатора.</typeparam>
    public class EntityBaseDeletable<TKey>
    {
        /// <summary>
        /// Возвращает или задает идентификатор.
        /// </summary>
        [Column("id"), Key]
        public TKey Id { get; set; }
    }
}
