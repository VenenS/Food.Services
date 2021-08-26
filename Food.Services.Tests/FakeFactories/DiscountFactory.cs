using System;
using System.Collections.Generic;
using ITWebNet.Food.Core.DataContracts.Manager;
using ITWebNet.Food.Core.DataContracts.Common;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    public static class DiscountFactory
    {
        private static readonly Random Rnd = new Random();

        public static Discount Create(User user = null, Cafe cafe = null, Company company = null)
        {
            user = user ?? UserFactory.CreateUser();
            cafe = cafe ?? CafeFactory.Create(user);
            company = company ?? CompanyFactory.Create(user);
            var discount = new Discount
            {
                CreationDate = DateTime.Now.AddDays(-30),
                CreatorId = user.Id,
                IsDeleted = false,
                BeginDate = DateTime.MinValue,
                EndDate = DateTime.MaxValue,
                CafeId = cafe.Id,
                Cafe = cafe,
                Company = company,
                CompanyId = company.Id,
                UserId = user.Id,
                User = user,
                Value = Rnd.Next(1, 100)
            };
            ContextManager.Get().Discounts.Add(discount);
            return discount;
        }

        public static List<Discount> CreateFew(int count = 3, User user = null, Cafe cafe = null,
            Company company = null)
        {
            user = user ?? UserFactory.CreateUser();
            cafe = cafe ?? CafeFactory.Create(user);
            company = company ?? CompanyFactory.Create(user);
            var cafes = new List<Discount>();
            for (var i = 0; i < count; i++)
                cafes.Add(Create(user, cafe, company));
            return cafes;
        }
        public static DiscountModel CreateModel(Discount discount = null)
        {
            discount = discount ?? DiscountFactory.Create();
            DiscountModel model = new DiscountModel()
            {
                BeginDate = discount.BeginDate,
                Cafe = CafeFactory.CreateModel(discount.Cafe),
                CafeId = discount.CafeId,
                CompanyId = discount.CompanyId,
                CreateDate = (DateTime)discount.CreationDate,
                CreatorId = discount.CreatorId,
                EndDate = discount.EndDate,
                Value = (int)discount.Value,
                UserId = discount.UserId
            };
            return model;
            
        }
    }
}