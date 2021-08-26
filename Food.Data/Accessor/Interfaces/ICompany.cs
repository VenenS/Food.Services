using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface ICompany
    {
        #region Company

        /// <summary>
        /// Возвращает список всех активных компаний
        /// </summary>
        /// <returns></returns>
        List<Company> GetCompanies();

        /// <summary>
        /// Получить список компании, привязанных к пользователю
        /// </summary>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        List<Company> GetCompanies(long userId);

        /// <summary>
        /// Добавляет компанию
        /// </summary>
        /// <param name="company">компания (сущность)</param>
        /// <returns></returns>
        bool AddCompany(Company company);

        /// <summary>
        /// Редактирует компанию
        /// </summary>
        /// <param name="company">компания (сущность)</param>
        /// <returns></returns>
        bool EditCompany(Company company);

        /// <summary>
        /// Удаляет компанию по идентификатору
        /// </summary>
        /// <param name="companyId">идентификатор компании</param>
        /// <param name="currentUserId">идентификатор пользователя</param>
        /// <returns></returns>
        bool RemoveCompany(long companyId, long currentUserId);

        /// <summary>
        /// Возвращает компанию по идентификатору
        /// </summary>
        /// <param name="companyId">идентификатор компании</param>
        /// <returns></returns>
        Company GetCompanyById(long companyId);

        /// <summary>
        /// Получение списка компаний.
        /// </summary>
        /// <returns>Список компаний.</returns>
        List<Company> GetCompanys();

        /// <summary>
        /// Получить компании, которые привязаны к пользователю и у которых есть корп. заказы на дату
        /// </summary>
        Task<List<Company>> GetMyCompanyForOrder(long userId, long? cafeId, DateTime? date);

        /// <summary>
        /// Получение списка неактивных и удаленных компаний.
        /// </summary>
        /// <returns>Список неактивных и удаленных компаний.</returns>
        List<Company> GetInactiveCompanies();

        /// <summary>
        /// Восстанавливает удаленную компанию по идентификатору
        /// </summary>
        /// <param name="companyId">идентификатор компании</param>
        /// <param name="currentUserId">идентификатор пользователя</param>
        /// <returns></returns>
        bool RestoreCompany(long companyId, long currentUserId);

        #endregion
    }
}
