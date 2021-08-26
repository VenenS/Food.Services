using System;
using System.Collections.Generic;
using System.Linq;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region UserCompany

        /// <summary>
        /// Возвращает список компании, привязанных к пользователю
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<Company> GetListOfCompanysByUserId(long userId)
        {
            var fc = GetContext();

            var query = from uc in fc.UsersInCompanies.AsNoTracking()
                        where uc.UserId == userId
                        && uc.IsActive
                        && uc.IsDeleted == false
                        && uc.StartDate <= DateTime.Now
                        &&
                        (
                            uc.EndDate == null
                            || uc.EndDate >= DateTime.Now
                        )
                        select uc.Company;

            return query.ToList();
        }

        /// <summary>
        /// Вернуть список юзеров по идентификатору компании
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<User> GetListOfUserByCompanyId(long companyId)
        {
            var fc = GetContext();

            var query = from uc in fc.UsersInCompanies.AsNoTracking()
                        where uc.CompanyId == companyId
                        && uc.IsActive
                        && uc.IsDeleted == false
                        && uc.StartDate <= DateTime.Now
                        &&
                        (
                            uc.EndDate == null
                            || uc.EndDate >= DateTime.Now
                        )
                        select uc.User;

            return query.ToList();
        }

        /// <summary>
        /// Возвращает true, если пользователь может получать информацию о компанейских заказах
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public bool CanUserReadInfoAboutCompanyOrders(long userId, long companyId)
        {
            var fc = GetContext();

            var query = fc.UsersInCompanies.AsNoTracking().Any(uc => uc.CompanyId == companyId
                        && uc.UserId == userId
                        &&
                        (
                            uc.UserType == EnumUserType.Manager
                            || uc.UserType == EnumUserType.Consolidator
                        )
                        && uc.IsActive
                        && uc.IsDeleted == false
                        && uc.StartDate <= DateTime.Now
                        &&
                        (
                            uc.EndDate == null
                            || uc.EndDate >= DateTime.Now
                        ));

            return query;
        }

        /// <summary>
        /// Привязка юзера к компании
        /// </summary>
        /// <param name="userCompanyLink"></param>
        /// <returns></returns>
        public bool AddUserToCompanyInRole(UserInCompany userCompanyLink)
        {
            try
            {
                using (var fc = GetContext())
                {
                    UserInCompany oldUserCompanyLink = fc.UsersInCompanies.FirstOrDefault(
                        uc => uc.CompanyId == userCompanyLink.CompanyId && uc.UserId == userCompanyLink.UserId
                    );

                    if (oldUserCompanyLink != null)
                    {
                        oldUserCompanyLink.IsDeleted = false;
                        oldUserCompanyLink.IsActive = true;
                    }
                    else
                        fc.UsersInCompanies.Add(userCompanyLink);

                    fc.SaveChanges();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Возвращает привязку пользователя к компании по идентификатору привязки.
        /// </summary>
        /// <param name="linkId">Идентификатор привязки</param>
        public UserInCompany GetUserCompanyLinkById(long linkId)
        {
            try {
                using (var fc = GetContext()) {
                    return fc.UsersInCompanies.FirstOrDefault(x => x.Id == linkId);
                }
            } catch (Exception) {
                return null;
            }
        }

        /// <summary>
        /// Изменение существующей привязки пользователя к компании.
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="oldCompanyId"></param>
        /// <param name="userId"></param>
        /// <param name="lastUpdate"></param>
        /// <returns>true - успешно, false - с ошибкой.</returns>
        public bool EditUserCompanyLink(long companyId, long oldCompanyId, long userId, long lastUpdate)
        {
            try
            {
                using (var fc = GetContext())
                {
                    UserInCompany oldUserCompanyLink =
                        fc.UsersInCompanies.FirstOrDefault(
                            uc => uc.CompanyId == oldCompanyId
                            && uc.UserId == userId
                            && uc.IsActive
                            && uc.IsDeleted == false
                        );

                    if (oldUserCompanyLink != null)
                    {
                        oldUserCompanyLink.LastUpdateBy =
                            lastUpdate;
                        oldUserCompanyLink.LastUpdateDate = 
                            DateTime.Now;
                        oldUserCompanyLink.UserId = userId;
                        oldUserCompanyLink.CompanyId =
                            companyId;
                        oldUserCompanyLink.IsActive = true;
                        oldUserCompanyLink.IsDeleted = false;

                        fc.SaveChanges();
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Удаление существующей привязки пользователя к компании.
        /// </summary>
        /// <param name="userCompanyLink">Привязка.</param>
        /// <returns>true - успешно, false - с ошибкой.</returns>
        public bool RemoveUserCompanyLink(UserInCompany userCompanyLink)
        {
            try
            {
                using (var fc = GetContext())
                {
                    UserInCompany oldUserCompanyLink =
                        fc.UsersInCompanies.FirstOrDefault(
                            uc => uc.CompanyId == userCompanyLink.CompanyId
                            && uc.UserId == userCompanyLink.UserId
                            && uc.IsActive
                            && uc.IsDeleted == false
                        );

                    if (oldUserCompanyLink != null)
                    {
                        oldUserCompanyLink.IsActive = false;
                        oldUserCompanyLink.IsDeleted = true;
                        oldUserCompanyLink.LastUpdateBy = 
                            userCompanyLink.LastUpdateBy;
                        oldUserCompanyLink.LastUpdateDate = DateTime.Now;

                        fc.SaveChanges();
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool SetUserCompany(long userId, long companyId)
        {
            try
            {
                var fc = GetContext();
                fc.UsersInCompanies.Where(u => u.UserId == userId && !u.IsDeleted && u.IsActive).ToList().ForEach(
                    u =>
                    {
                        u.IsActive = false;
                        u.IsDeleted = true;
                    });
                var userInCompany = fc.UsersInCompanies.FirstOrDefault(u => u.UserId == userId && u.CompanyId == companyId);
                if (userInCompany != null)
                {
                    userInCompany.IsDeleted = false;
                    userInCompany.IsActive = true;
                }
                else
                {
                    fc.UsersInCompanies.Add(new UserInCompany
                    {
                        CompanyId = companyId,
                        CreateDate = DateTime.Now,
                        CreatedBy = userId,
                        IsActive = true,
                        UserId = userId,
                        UserRoleId = 1,
                        StartDate = DateTime.Now,
                        EndDate = null,
                    });
                }
                fc.SaveChanges();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public long? GetUserCompanyId(long userId)
        {
            var fc = GetContext();
            return fc.UsersInCompanies
                .AsNoTracking()
                .Include(u => u.Company)
                .FirstOrDefault(
                u => u.UserId == userId
                && u.IsActive
                && !u.IsDeleted
                && u.Company.IsActive
                && !u.Company.IsDeleted)?.CompanyId;
        }
        #endregion
    }
}
