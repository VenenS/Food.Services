using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    //TODO: Авторизация через соцсети. Удалить
    /// <summary>
    /// Предствляет внешнего пользователя. Vk, Facebook
    /// </summary>
    [Table("user_external_login")]
    public class UserExternalLogin
    {
        [Column("login_provider")]
        public string LoginProvider { get; set; }

        [Column("provider_key")]
        public string ProviderKey { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        public virtual User User { get; set; }
    }
}
