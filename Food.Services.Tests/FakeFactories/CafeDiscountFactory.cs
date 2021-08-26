using Food.Data.Entities;
using Food.Services.Tests.Context;
using System;

namespace Food.Services.Tests.FakeFactories
{
    public static class CafeDiscountFactory
    {
        public static CafeDiscount Create(User creator = null, Cafe cafe = null, Company company = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            var cafeDiscount = new CafeDiscount
            {
                BeginDate = DateTime.Now.AddDays(-1),
                CafeId = cafe == null ? CafeFactory.Create(creator).Id : cafe.Id,
                CompanyId = company == null ? CompanyFactory.Create(creator).Id : company.Id,
                CreateDate = DateTime.Now,
                CreatorId = creator.Id,
                IsDeleted = false,
                Summ = 20,
                SummFrom = 100,
                SummTo = 200,
                EndDate = DateTime.Now.AddDays(1)

            };
            ContextManager.Get().CafeDiscounts.Add(cafeDiscount);
            return cafeDiscount;
        }
    }
}
