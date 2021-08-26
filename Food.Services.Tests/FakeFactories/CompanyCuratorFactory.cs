using Food.Data.Entities;
using Food.Services.Tests.Context;
using ITWebNet.Food.Core.DataContracts.Common;
using System.Collections.Generic;

namespace Food.Services.Tests.FakeFactories
{
    class CompanyCuratorFactory
    {
        public static CompanyCurator Create(Company company = null, User user = null, bool saveDB = false)
        {
            var entityCompany = company ?? CompanyFactory.Create();
            var entityUser = user ?? UserFactory.CreateUser();
            var entity = new CompanyCurator()
            {
                 Company = entityCompany,
                 CompanyId = entityCompany.Id,
                 IsDeleted = false,
                 User = entityUser,
                 UserId = entityUser.Id,
            };

            if (saveDB) ContextManager.Get().CompanyCurators.Add(entity);

            return entity;
        }

        public static List<CompanyCurator> CreateFew(int count = 3, bool saveDB = false)
        {
            var lstEntities = new List<CompanyCurator>();
            for (int i = 0; i < count; i++)
            {
                lstEntities.Add(Create(saveDB: saveDB));
            }

            return lstEntities;
        }

        public static List<CompanyCurator> CreateFew(User[] users, Company company = null, bool saveDB = false)
        {
            var lstEntities = new List<CompanyCurator>();
            foreach (User user in users)
            {
                lstEntities.Add(Create(company, user, saveDB));
            }
            return lstEntities;
        }

        public static CompanyCuratorModel CreateModel(CompanyCurator entity)
        {
            var modelUser = UserFactory.CreateAdminModel();
            var model = new CompanyCuratorModel()
            {
                CompanyId = entity.CompanyId,
                Id = entity.Id,
                User = modelUser,
                UserId = modelUser.Id,
            };

            return model;
        }

        public static ITWebNet.FoodService.Food.Data.Accessor.Models.CompanyCuratorModel CreateAccessorModel(CompanyCurator entity)
        {
            var model = new ITWebNet.FoodService.Food.Data.Accessor.Models.CompanyCuratorModel()
            {
                CompanyId = entity.CompanyId,
                Id = entity.Id,
                UserId = UserFactory.CreateAdminModel().Id,
            };

            return model;
        }
    }
}
