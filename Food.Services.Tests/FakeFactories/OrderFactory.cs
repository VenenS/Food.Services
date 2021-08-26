using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    public static class OrderFactory
    {
        public static Order Create(User creator = null, Banket banket = null, Cafe cafe = null, User user = null, CompanyOrder companyOrder = null, OrderInfo orderInfo = null)
        {
            var order = CreateVirtual(creator, banket, cafe, user, companyOrder, orderInfo);
            ContextManager.Get().Orders.Add(order);
            return order;
        }

        public static List<Order> CreateFew(int count = 3, User creator = null, Banket banket = null, Cafe cafe = null, User user = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            cafe = cafe ?? CafeFactory.Create(creator);
            // Банкет создавать не нужно, если только он не указан явно, так как большинство заказов идёт без банкета
            var orders = new List<Order>();
            for (var i = 0; i < count; i++)
                orders.Add(Create(creator, banket, cafe, user));
            return orders;
        }

        public static Order CreateVirtual(User creator = null, Banket banket = null, Cafe cafe = null, User user = null, CompanyOrder companyOrder = null, OrderInfo orderInfo = null)
        {
            cafe = cafe ?? CafeFactory.Create(creator);
            creator = creator ?? UserFactory.CreateUser();
            companyOrder = companyOrder ?? CompanyOrderFactory.Create(user, cafe:cafe);
            user = user ?? UserFactory.CreateUser();
            orderInfo = orderInfo ?? OrderInfoFactory.Create();
            // Банкет создавать не нужно, если только он не указан явно, так как большинство заказов идёт без банкета
            var order = new Order
            {
                Id = -1,
                CreationDate = DateTime.Now.AddDays(-30),
                CreatorId = creator.Id,
                DeliverDate = DateTime.Now,
                IsDeleted = false,
                Cafe = cafe,
                CafeId = cafe.Id,
                Banket = banket,
                BanketId = banket?.Id,
                OrderItems = new List<OrderItem>(),
                State = EnumOrderState.Accepted,
                Comment = Guid.NewGuid().ToString("N"), UserId = user.Id, User = user,
                CompanyOrder = companyOrder, CompanyOrderId = companyOrder.Id, OrderInfo = orderInfo, OrderInfoId = orderInfo.Id
            };
            return order;
        }

        public static Order Clone(Order ancestor)
        {
            var order = new Order
            {
                CreationDate = ancestor.CreationDate,
                CreatorId = ancestor.CreatorId,
                IsDeleted = ancestor.IsDeleted,
                Cafe = ancestor.Cafe,
                CafeId = ancestor.CafeId,
                Banket = ancestor.Banket,
                BanketId = ancestor.BanketId,
                OrderItems = ancestor.OrderItems,
                State = ancestor.State,
                Comment = ancestor.Comment,
                UserId = ancestor.UserId,
                User = ancestor.User,
                Id = ancestor.Id,
                CompanyOrder = ancestor.CompanyOrder,
                CompanyOrderId = ancestor.CompanyOrderId,
                OrderInfo = ancestor.OrderInfo,
                OrderInfoId = ancestor.OrderInfoId,
                ManagerComment = ancestor.ManagerComment,
                PayType = ancestor.PayType
            };
            return order;
        }
    }
}
