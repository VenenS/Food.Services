using Food.Data.Entities;
using System.Threading.Tasks;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface ISmsCode
    {
        #region SmsCode

        /// <summary>
        /// Получение кода по Id
        /// </summary>
        /// <param name="Id">идентификатор смс-кода</param>
        /// <returns></returns>
        Task<SmsCode> GetSmsCodeById(long Id);

        /// <summary>
        /// Получение сущности Смс кода по телефону и коду
        /// </summary>
        /// <param name="phone">Телефон</param>
        /// <param name="code">Смс код</param>
        /// <returns></returns>
        Task<SmsCode> GetSmsCode(string phone, string code);

        /// <summary>
        /// Добавление смс-кода в БД
        /// </summary>
        /// <param name="code">смс-код (сущность);</param>
        /// <returns>Возвращает идентификатор кода, или -1, если код добавить не удалось</returns>
        Task<long> AddSmsCode(SmsCode code);

        /// <summary>
        /// Изменение кода
        /// </summary>
        /// <param name="cafe">смс-код (сущность););</param>
        /// <returns></returns>
        Task<long> EditSmsCode(SmsCode code);

        /// <summary>
        /// Удаление кода
        /// </summary>
        /// <param name="cafeId">Идентификатор кода</param>
        /// <returns></returns>
        Task<bool> RemoveSmsCode(long Id);

        /// <summary>
        /// Блокировка СМС-кода (прекращение действия);
        /// </summary>
        /// <param name="cafeId">Идентификатор кода</param>
        /// <returns></returns>
        Task<bool> BlockSmsCode(long Id);

        /// <summary>
        /// Получение самого последнего сгенерированного кода для указанного пользователя
        /// </summary>
        /// <param name="UserId">Идентификатор ползователя, для которого надо получить код</param>
        /// <returns></returns>
        Task<SmsCode> GetLastUserCode(long UserId);

        #endregion
    }
}
