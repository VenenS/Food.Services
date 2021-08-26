using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    public static class BanketFactory
    {
        public static Banket Create(User creator = null, Company company = null, Cafe cafe = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            company = company ?? CompanyFactory.Create(creator);
            cafe = cafe ?? CafeFactory.Create(creator);
            var banket = new Banket
            {
                IsDeleted = false,
                Orders = new List<Order>(),
                Cafe = cafe,
                CafeId = cafe.Id,
                Company = company,
                CompanyId = company.Id,
                EventDate = System.DateTime.Now,
                Name = "",
                Status = EnumBanketStatus.Preparing,
                Url = ""
            };
            ContextManager.Get().Bankets.Add(banket);
            return banket;
        }
        public static Banket CreateWithMenu(User creator = null, Company company = null, Cafe cafe = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            company = company ?? CompanyFactory.Create(creator);
            cafe = cafe ?? CafeFactory.Create(creator);
            var banket = new Banket
            {
                Menu = CafeMenuPatternFactory.CreateWithBankets(cafe),
                IsDeleted = false,
                Orders = new List<Order>(),
                Cafe = cafe,
                CafeId = cafe.Id,
                Company = company,
                CompanyId = company.Id,
                Name = "",
                Status = EnumBanketStatus.Preparing,
                Url = ""
            };
            ContextManager.Get().Bankets.Add(banket);
            return banket;
        }

        public static List<Banket> CreateFew(int count = 3, User creator = null, Company company = null,
            Cafe cafe = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            company = company ?? CompanyFactory.Create(creator);
            cafe = cafe ?? CafeFactory.Create(creator);
            var bankets = new List<Banket>();
            for (var i = 0; i < count; i++)
                bankets.Add(Create(creator, company, cafe)); 
            return bankets;
        }
    }
}