using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    internal static class UserInCompanyFactory
    {
        public static UserInCompany CreateRoleForUser(User user = null, Company company = null)
        {
            if (user == null)
                user = UserFactory.CreateUser();
            if (company == null)
                company = CompanyFactory.Create();
            var uic = ContextManager.Get().UsersInCompanies.Add(new UserInCompany
            {
                User = user,
                UserId = user.Id,
                CompanyId = company.Id,
                Company = company,
                IsActive = true
            });
            return uic.Entity;
        }

        public static UserInCompany Clone(UserInCompany ancestor)
        {
            var uic = new UserInCompany
            {
                User = ancestor.User,
                UserId = ancestor.UserId,
                CompanyId = ancestor.CompanyId,
                Company = ancestor.Company,
                IsActive = true,
            };
            return uic;
        }
    }
}