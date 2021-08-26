using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Связь Пользователя с Ролью
    /// </summary>
    [Table("user_role_link")]
    public class UserInRole : EntityBase<long>
    {
        /// <summary>
        /// Возвращает или задает идентификатор роли.
        /// </summary>
        [Column("role_id")]
        public long RoleId { get; set; }

        /// <summary>
        /// Возвращает или задает идентификатор пользователя.
        /// </summary>
        [Column("user_id")]
        public long UserId { get; set; }

        public virtual Role Role { get; set; }
        public virtual User User { get; set; }
    }
}
