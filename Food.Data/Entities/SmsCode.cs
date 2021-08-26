using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Food.Data.Entities
{
    /// <summary>
    /// Таблица для хранения отправленных пользователям СМС-кодов
    /// </summary>
    [Table("sms_code")]
    public class SmsCode : EntityBase<long>
    {
        /// <summary>
        /// Идентификатор пользователя, которому отправлен код подтверждения
        /// </summary>
        [Column("user_id")]
        public long UserId { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        /// <summary>
        /// Номер телефона, на который отправлен код
        /// </summary>
        [Column("phone")]
        public string Phone { get; set; }

        /// <summary>
        /// Текст кода
        /// </summary>
        [Column("code")]
        public string Code { get; set; }

        /// <summary>
        /// Время генерации кода
        /// </summary>
        [Column("creation_time")]
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Время, когда прекращает действовать
        /// </summary>
        [Column("valid_time")]
        public DateTime ValidTime { get; set; }

        /// <summary>
        /// Текущий статус кода - действует или нет
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; }
    }
}
