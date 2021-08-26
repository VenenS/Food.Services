using Food.Data.Entities;
using System;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface ICafeManager
    {
        #region CafeManager
        /// <summary>
        /// Возвращает true, если пользователь является управляющим кафе
        /// </summary>
        /// <param name="userId">идентификтаор пользователя</param>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <returns></returns>
        bool IsUserManagerOfCafe(long userId, long cafeId);

        /// <summary>
        /// Возвращает true, если пользователь является управляющим кафе
        /// #ref too many references to main method at this moment
        /// </summary>
        /// <param name="userId">идентификтаор пользователя</param>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <returns></returns>
        bool IsUserManagerOfCafeIgnoreActivity(long userId, long cafeId);
        #endregion

        #region UserCafeLink

        /// <summary>
        /// Получает список кафе, к которым привязан пользователь.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Список кафе.</returns>
        List<Cafe> GetListOfCafeByUserId(Int64 userId);

        /// <summary>
        /// Добавляет новую привязку пользователя к кафе.
        /// </summary>
        /// <param name="userCafeLink">Новая привязка.</param>
        /// <param name="authorId"></param>
        /// <returns>true - успешно, false - с ошибкой.</returns>
        bool AddUserCafeLink(CafeManager userCafeLink, long authorId);

        /// <summary>
        /// Изменение существуещей привязки пользорвателя к кафе.
        /// </summary>
        /// <param name="userCafeLink">Обновленная привязка.</param>
        /// <returns>true - успешно, false - с ошибкой.</returns>
        bool EditUserCafeLink(CafeManager userCafeLink);

        /// <summary>
        /// Удаление существующей привязки пользователя к кафе.
        /// </summary>
        /// <param name="userCafeLink">Привязка.</param>
        /// <returns>true - успешно, false - с ошибкой.</returns>
        bool RemoveUserCafeLink(CafeManager userCafeLink);
        #endregion
    }
}
