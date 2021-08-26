using Food.Data.Entities;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface IUserCompany
    {
        #region UserCompany

        /// <summary>
        /// Возвращает список компании, привязанных к пользователю
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<Company> GetListOfCompanysByUserId(long userId);

        /// <summary>
        /// Вернуть список юзеров по идентификатору компании
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        List<User> GetListOfUserByCompanyId(long companyId);

        /// <summary>
        /// Возвращает true, если пользователь может получать информацию о компанейских заказах
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        bool CanUserReadInfoAboutCompanyOrders(long userId, long companyId);

        /// <summary>
        /// Привязка юзера к компании
        /// </summary>
        /// <param name="userCompanyLink"></param>
        /// <returns></returns>
        bool AddUserToCompanyInRole(UserInCompany userCompanyLink);

        /// <summary>
        /// Изменение существующей привязки пользователя к компании.
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="oldCompanyId"></param>
        /// <param name="userId"></param>
        /// <param name="lastUpdate"></param>
        /// <returns>true - успешно, false - с ошибкой.</returns>
        bool EditUserCompanyLink(long companyId, long oldCompanyId, long userId, long lastUpdate);

        /// <summary>
        /// Удаление существующей привязки пользователя к компании.
        /// </summary>
        /// <param name="userCompanyLink">Привязка.</param>
        /// <returns>true - успешно, false - с ошибкой.</returns>
        bool RemoveUserCompanyLink(UserInCompany userCompanyLink);

        #endregion
    }
}
