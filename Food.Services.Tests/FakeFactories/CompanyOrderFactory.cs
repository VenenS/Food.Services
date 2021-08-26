using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using ITWebNet.Food.Core.DataContracts.Common;

namespace Food.Services.Tests.FakeFactories
{
    public static class CompanyOrderFactory
    {
        public static CompanyOrder Create(User creator = null, Company company = null, Cafe cafe = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            company = company ?? CompanyFactory.Create(creator);
            cafe = cafe ?? CafeFactory.Create(creator);
            var order = new CompanyOrder
            {
                CreationDate = DateTime.Now.AddDays(-30),
                CreatorId = creator.Id,
                IsDeleted = false,
                Company = company,
                CompanyId = company.Id,
                ContactEmail = Guid.NewGuid().ToString("N"),
                OpenDate = DateTime.Today,
                AutoCloseDate = DateTime.Today.AddDays(1),
                CafeId = cafe.Id,
                Cafe = cafe,
                DeliveryDate = DateTime.Now,
                State = (long)OrderStatusEnum.Created,
                Orders = new List<Order>()
            };
            ContextManager.Get().CompanyOrders.Add(order);
            return order;
        }

        public static List<CompanyOrder> CreateFew(int count = 3, User creator = null,
            Company company = null, Cafe cafe = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            company = company ?? CompanyFactory.Create(creator);
            var orders = new List<CompanyOrder>();
            for (var i = 0; i < count; i++)
                orders.Add(Create(creator, company, cafe));
            return orders;
        }
    }
}