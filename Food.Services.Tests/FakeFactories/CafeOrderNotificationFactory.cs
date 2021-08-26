using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    public static class CafeOrderNotificationFactory
    {
        private static readonly Random _rnd = new Random();

        public static CafeOrderNotification Create(User user = null, Cafe cafe = null)
        {
            user = user ?? UserFactory.CreateUser();
            cafe = cafe ?? CafeFactory.Create(user);
            var discount = new CafeOrderNotification
            {
                CafeId = cafe.Id,
                UserId = user.Id, DeliverDate = DateTime.MinValue
            };
            ContextManager.Get().CafeOrderNotifications.Add(discount);
            return discount;
        }

        public static List<CafeOrderNotification> CreateFew(int count = 3, User user = null, Cafe cafe = null)
        {
            user = user ?? UserFactory.CreateUser();
            cafe = cafe ?? CafeFactory.Create(user);
            var cafes = new List<CafeOrderNotification>();
            for (var i = 0; i < count; i++)
                cafes.Add(Create(user, cafe));
            return cafes;
        }
    }
}
