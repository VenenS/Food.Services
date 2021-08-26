using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    public static class OrderItemFactory
    {
        private static readonly Random Random = new Random();

        public static OrderItem Create(User creator = null, Dish dish = null, Order order = null)
        {
            var orderItem = CreateVirtual(creator, dish, order);
            ContextManager.Get().OrderItems.Add(orderItem);
            return orderItem;
        }

        public static List<OrderItem> CreateFew(int count = 3, User creator = null, Dish dish = null, Order order = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            order = order ?? OrderFactory.Create(creator);
            dish = dish ?? DishFactory.Create(creator);
            var orderItems = new List<OrderItem>();
            for (var i = 0; i < count; i++)
                orderItems.Add(Create(creator, dish, order));
            return orderItems;
        }

        public static OrderItem Clone(OrderItem ancestor)
        {
            var orderItem = new OrderItem
            {
                Id = ancestor.Id,
                CreationDate = ancestor.CreationDate,
                CreatorId = ancestor.CreatorId,
                IsDeleted = ancestor.IsDeleted,
                Order = ancestor.Order,
                OrderId = ancestor.OrderId,
                Dish = ancestor.Dish,
                DishId = ancestor.DishId,
                DishBasePrice = ancestor.DishBasePrice,
                DishName = ancestor.DishName,
                DishKcalories = ancestor.DishKcalories,
                DishWeight = ancestor.DishWeight,
                TotalPrice = ancestor.TotalPrice,
                Comment = ancestor.Comment
            };
            return orderItem;
        }

        public static OrderItem CreateVirtual(User creator = null, Dish dish = null, Order order = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            order = order ?? OrderFactory.Create(creator);
            dish = dish ?? DishFactory.Create(creator);
            var orderItem = new OrderItem
            {
                Id = -1,
                CreationDate = DateTime.Now.AddDays(-30),
                CreatorId = creator.Id,
                IsDeleted = false,
                Order = order,
                OrderId = order.Id,
                Dish = dish,
                DishId = dish.Id,
                DishBasePrice = Random.Next(),
                DishName = dish.DishName,
                DishKcalories = dish.Kcalories,
                DishWeight = dish.Weight,
                TotalPrice = Random.Next(),
                Comment = Guid.NewGuid().ToString("N")
            };
            return orderItem;
        }
    }
}
