using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    internal class OrderInfoFactory
    {
        private static readonly Random Random = new Random();

        public static OrderInfo Create()
        {
            var orderItem = CreateVirtual();
            ContextManager.Get().OrderInfo.Add(orderItem);
            return orderItem;
        }

        public static List<OrderInfo> CreateFew(int count = 3, User creator = null, Dish dish = null,
            Order order = null)
        {
            var orderItems = new List<OrderInfo>();
            for (var i = 0; i < count; i++)
                orderItems.Add(Create());
            return orderItems;
        }

        public static OrderInfo Clone(OrderInfo ancestor)
        {
            var orderItem = new OrderInfo
            {
                Id = ancestor.Id,
                IsDeleted = ancestor.IsDeleted,
                CreateDate = ancestor.CreateDate,
                DeliverySumm = ancestor.DeliverySumm,
                DiscountSumm = ancestor.DiscountSumm,
                OrderAddress = ancestor.OrderAddress,
                OrderEmail = ancestor.OrderEmail,
                OrderPhone = ancestor.OrderPhone
            };
            return orderItem;
        }

        public static OrderInfo CreateVirtual()
        {
            var orderItem = new OrderInfo
            {
                Id = -1,
                IsDeleted = false,
                CreateDate = DateTime.Now.AddDays(-30),
                DeliverySumm = Random.Next(),
                DiscountSumm = Random.Next(0, 50),
                OrderAddress = Guid.NewGuid().ToString("N"),
                OrderEmail = Guid.NewGuid().ToString("N"),
                OrderPhone = Guid.NewGuid().ToString("N")
            };
            return orderItem;
        }
    }
}