using Food.Data.Entities;
using ITWebNet.FoodService.Food.Data.Accessor.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        /// <summary>
        /// Добавление куратора в компаниюы
        /// </summary>
        public bool AddCompanyCurator(CompanyCuratorModel model, long authorId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var curator = fc.CompanyCurators.FirstOrDefault(
                        o => o.UserId == model.UserId
                        && o.CompanyId == model.CompanyId);

                    if (curator != null)
                    {
                        if (!curator.IsDeleted)
                        {
                            return true;
                        }
                        curator.IsDeleted = false;
                        curator.LastUpdateByUserId = authorId;
                        curator.LastUpdDate = DateTime.Now;
                    }
                    else
                    {
                        var userInCompany = fc.UsersInCompanies.FirstOrDefault(c => c.UserId == model.UserId && c.CompanyId == model.CompanyId);
                        if (userInCompany == null)
                        {
                            fc.UsersInCompanies.Add(new UserInCompany()
                            {
                                CompanyId = model.CompanyId,
                                CreateDate = DateTime.Now,
                                IsActive = true,
                                UserId = model.UserId,
                                CreatedBy = authorId
                            });
                        }
                        else
                        {
                            userInCompany.IsDeleted = false;
                            userInCompany.IsActive = true;
                        }
                        var newCompanyCurator = new CompanyCurator()
                        {
                            CompanyId = model.CompanyId,
                            CreatorId = authorId,
                            CreationDate = DateTime.Now,
                            UserId = model.UserId,
                            IsDeleted = false
                        };
                        fc.CompanyCurators.Add(newCompanyCurator);
                    }

                    fc.SaveChanges();

                    Instance.AddUserToRole(model.UserId, EnumUserRole.Consolidator);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Удаление куратора из компании
        /// </summary>
        public bool DeleteCompanyCurator(long companyId, long userId, long authorId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var curator = fc.CompanyCurators.FirstOrDefault(
                        o => o.UserId == userId
                        && o.CompanyId == companyId);

                    if (curator != null)
                    {
                        curator.IsDeleted = true;
                        curator.LastUpdateByUserId = authorId;
                        curator.LastUpdDate = DateTime.Now;
                        fc.SaveChanges();

                        Instance.RemoveUserRole(userId, EnumUserRole.Consolidator);
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверка пользователя на куратора компании
        /// </summary>
        public bool IsUserCuratorOfCafe(long userId, long companyId)
        {
            var fc = GetContext();
            var query = fc.CompanyCurators.AsNoTracking()
                .Any(cc =>
                    cc.CompanyId == companyId
                    && cc.UserId == userId
                    && cc.Company.IsActive
                    && cc.Company.IsDeleted == false);

            return query;
        }

        /// <summary>
        /// Получение курирующей компании
        /// </summary>
        public Company GetCurationCompany(long userId)
        {
            var fc = GetContext();

            var curator = fc.CompanyCurators.AsNoTracking()
                .Include(c => c.Company).FirstOrDefault(c => c.UserId == userId && c.IsDeleted == false);

            return curator?.Company;
        }

        /// <summary>
        /// Получение привязки куратора и компании
        /// </summary>
        public CompanyCurator GetCompanyCurator(long userId, long companyId)
        {
            CompanyCurator entity;

            using (var fc = GetContext())
            {
                entity = fc.CompanyCurators.AsNoTracking().FirstOrDefault(
                    o => !o.IsDeleted
                    && o.UserId == userId
                    && o.CompanyId == companyId);
            }

            return entity;
        }


        /// <summary>
        /// Получение кураторов комп. заказа по id заказа
        /// </summary>
        /// <param name="companyOrderId">Id заказа</param>
        /// <returns>Возвращает список кураторов и название кафе</returns>
        public (Dictionary<string, string>,string) GetEmailsCuratorByCompanyOrderId(long companyOrderId)
        {
            string cafeName = String.Empty;
            Dictionary<string, string> emailAndAddressCurator = new Dictionary<string, string>();
            using (var fc = GetContext())
            {
                var companyOrder = fc.CompanyOrders.FirstOrDefault(c => c.Id == companyOrderId);
                var lstCompanyCurator = fc.CompanyCurators
                    .Include(e => e.User)
                    .Where(c => c.CompanyId == companyOrder.CompanyId&&c.IsDeleted==false)
                    .ToList();
                foreach (var curator in lstCompanyCurator)
                {
                    emailAndAddressCurator.Add(curator.User.Email, curator.User.FullName);
                }

                var cafe = fc.Cafes.FirstOrDefault(n => n.Id == companyOrder.CafeId);
                cafeName = cafe.CafeName;

                return (emailAndAddressCurator, cafeName);
            }
        }
    }
}
