using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region Company

        /// <summary>
        /// Возвращает список всех активных компаний
        /// </summary>
        /// <returns></returns>
        public List<Company> GetCompanies()
        {
            List<Company> query;

            using (var fc = GetContext())
            {
                query = fc.Companies.AsNoTracking().Where(c => c.IsActive && !c.IsDeleted).ToList();
            }

            return query;
        }

        /// <summary>
        /// Получить список компании, привязанных к пользователю
        /// </summary>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        public List<Company> GetCompanies(long userId)
        {
            var fc = GetContext();

            var query = fc.UsersInCompanies.AsNoTracking()
                .Where(c =>
                    c.User.Id == userId
                    && c.IsActive
                    && c.Company.IsActive
                    && c.IsDeleted == false)
                .Select(c => c.Company);

            return query.ToList();
        }

        /// <summary>
        /// Добавляет компанию
        /// </summary>
        /// <param name="company">компания (сущность)</param>
        /// <returns></returns>
        public bool AddCompany(Company company)
        {
            using (var fc = GetContext())
            {
                fc.Companies.Add(company);
                fc.SaveChanges();
            }
            return true;
        }

        /// <summary>
        /// Редактирует компанию
        /// </summary>
        /// <param name="company">компания (сущность)</param>
        /// <returns></returns>
        public bool EditCompany(Company company)
        {
            using (var fc = GetContext())
            {
                Company oldCompany =
                    fc.Companies.FirstOrDefault(
                        c => c.Id == company.Id
                        && c.IsActive
                        && c.IsDeleted == false
                    );

                if (oldCompany != null)
                {
                    oldCompany.FullName = company.FullName;
                    oldCompany.JuridicalAddressId = company.JuridicalAddressId;
                    oldCompany.LastUpdateByUserId = company.LastUpdateByUserId;
                    oldCompany.LastUpdDate = DateTime.Now;
                    oldCompany.MainDeliveryAddressId = company.MainDeliveryAddressId;
                    oldCompany.Name = company.Name;
                    oldCompany.PostAddressId = company.PostAddressId;
                    oldCompany.SmsNotify = company.SmsNotify;
                    oldCompany.IsDeleted = false;
                    oldCompany.City = company.City;

                    fc.SaveChanges();
                }
                else
                {
                    return false;
                }

                fc.SaveChanges();
            }
            return true;
        }

        /// <summary>
        /// Удаляет компанию по идентификатору
        /// </summary>
        /// <param name="companyId">идентификатор компании</param>
        /// <param name="currentUserId">идентификатор пользователя</param>
        /// <returns></returns>
        public bool RemoveCompany(long companyId, long currentUserId)
        {
            using (var fc = GetContext())
            {
                Company oldCompany =
                    fc.Companies.FirstOrDefault(
                        c => c.Id == companyId
                        && c.IsActive
                        && c.IsDeleted == false
                    );

                if (oldCompany != null)
                {
                    oldCompany.IsActive = false;
                    oldCompany.IsDeleted = true;
                    oldCompany.LastUpdateByUserId = currentUserId;
                    oldCompany.LastUpdDate = DateTime.Now;

                    fc.SaveChanges();
                }
                else
                {
                    return false;
                }

                fc.SaveChanges();
            }
            return true;
        }

        /// <summary>
        /// Возвращает компанию по идентификатору
        /// </summary>
        /// <param name="companyId">идентификатор компании</param>
        /// <returns></returns>
        public Company GetCompanyById(long companyId)
        {
            var fc = GetContext();
            var company = fc.Companies.AsNoTracking().Include(a => a.Addresses).ThenInclude(a => a.Address)
                .Include(a => a.MainDeliveryAddress)
                .Include(a => a.JuridicalAddress)
                .Include(a => a.PostAddress)
                .Include(a => a.City)
                .AsNoTracking()
                .FirstOrDefault(c => c.Id == companyId && c.IsActive && c.IsDeleted == false);

            //TODO: после теста в проде удалить
            //var juridicalAddress = fc.Addresses.AsNoTracking().DeferredFirstOrDefault(a => a.Id == company.JuridicalAddressId).FutureValue();
            //var deliveryAddress = fc.Addresses.AsNoTracking().DeferredFirstOrDefault(a => a.Id == company.MainDeliveryAddressId).FutureValue();
            //var postAddress = fc.Addresses.AsNoTracking().DeferredFirstOrDefault(a => a.Id == company.PostAddressId).FutureValue();
            //company.JuridicalAddress = juridicalAddress.Value;
            //company.MainDeliveryAddress = deliveryAddress.Value;
            //company.PostAddress = postAddress.Value;

            return company;
        }

        /// <summary>
        /// Получение списка компаний.
        /// </summary>
        /// <returns>Список компаний.</returns>
        public List<Company> GetCompanys()
        {
            var fc = GetContext();

            var query = fc.Companies.Include(c => c.City).AsNoTracking().Where(c => c.IsActive && c.IsDeleted == false);

            return query.ToList();
        }

        /// <summary>
        /// Получить компании, которые привязаны к пользователю и у которых есть корп. заказы на дату
        /// </summary>
        public async Task<List<Company>> GetMyCompanyForOrder(long userId, long? cafeId, DateTime? date)
        {
            if (date == null)
            {
                date = DateTime.Now;
            }

            var fc = GetContext();

            var companyUserLinkIds = new HashSet<long> (await
                fc.UsersInCompanies
                .AsNoTracking()
                .Where(
                    e => e.IsActive 
                    && !e.IsDeleted 
                    && e.UserId == userId 
                    && e.Company != null 
                    && e.Company.IsActive 
                    && !e.Company.IsDeleted)
                .Select(e => e.CompanyId)
                .ToListAsync());

            //Проверяем есть ли корп. заказы у компаний для этих кафе
            var query = await fc.CompanyOrders
                .AsNoTracking()
                .Where(
                e => !e.IsDeleted
                && e.OpenDate <= date
                && e.AutoCloseDate >= date
                && (cafeId == null || e.CafeId == cafeId)
                && companyUserLinkIds.Contains(e.CompanyId))
                .Select(e => e.Company)
                .Distinct()
                .ToListAsync();

            return query;
        }

        /// <summary>
        /// Получение списка неактивных и удаленных компаний.
        /// </summary>
        /// <returns>Список неактивных и удаленных компаний.</returns>
        public List<Company> GetInactiveCompanies()
        {
            var fc = GetContext();

            var query = fc.Companies.AsNoTracking().Where(c => c.IsActive == false && c.IsDeleted);

            return query.ToList();
        }

        /// <summary>
        /// Восстанавливает удаленную компанию по идентификатору
        /// </summary>
        /// <param name="companyId">идентификатор компании</param>
        /// <param name="currentUserId">идентификатор пользователя</param>
        /// <returns></returns>
        public bool RestoreCompany(long companyId, long currentUserId)
        {
            using (var fc = GetContext())
            {
                Company oldCompany =
                    fc.Companies.FirstOrDefault(
                        c => c.Id == companyId
                        && c.IsActive == false
                        && c.IsDeleted
                    );

                if (oldCompany != null)
                {
                    oldCompany.IsActive = true;
                    oldCompany.IsDeleted = false;
                    oldCompany.LastUpdateByUserId = currentUserId;
                    oldCompany.LastUpdDate = DateTime.Now;

                    fc.SaveChanges();
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
