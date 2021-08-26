using Food.Data.Entities;
using ITWebNet.FoodService.Food.DbAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Extensions
{
    public static class CompanyServiceHelper
    {
        public static bool IsNewUserCompanyLinkAvailable(UserInCompany userCompany)
        {
            if (userCompany == null)
                return false;

            var user =
                Accessor.Instance.GetUserById(userCompany.UserId);

            var company =
                Accessor.Instance.GetCompanyById(userCompany.CompanyId);

            if (user != null && company != null)
            {
                var userCompanyList =
                    Accessor.Instance.GetListOfCompanysByUserId(user.Id);

                var isExist =
                    !userCompanyList
                    .Any(
                        r =>
                            r.Id == company.Id
                    );

                if (!isExist)
                    throw new Exception("Уже существует данная привязка к компании для данного пользователя.");

                return true;
            }
            else
            {
                throw new Exception("Отсутствуют компания или пользователь.");
            }
        }

        public static bool IsNewCompanyNameAvailable(Company company)
        {
            if (company != null
                && !String.IsNullOrWhiteSpace(company.Name)
                && !String.IsNullOrWhiteSpace(company.FullName)
            )
            {
                var companies =
                    Accessor.Instance.GetCompanies();

                var isExist =
                    !companies
                    .Any(
                        c =>
                            (
                                String.IsNullOrWhiteSpace(c.FullName)
                                || c.FullName.ToLower().Equals(company.FullName.ToLower())
                            )
                            ||
                            (
                                String.IsNullOrWhiteSpace(c.Name)
                                || c.Name.ToLower().Equals(company.Name.ToLower())
                            )
                            && c.Id != company.Id
                    );

                if (!isExist)
                    throw new Exception("Уже существует компания с данным именем.");

                return true;
            }
            else
            {
                throw new Exception("Отсутствует компания для работы с ней.");
            }
        }
    }
}
