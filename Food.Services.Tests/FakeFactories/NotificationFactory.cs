using Food.Data.Entities;
using Food.Services.Tests.Context;
using System.Collections.Generic;

namespace Food.Services.Tests.FakeFactories
{
    public static class NotificationFactory
    {
        public static Notification Create(Cafe cafe = null, User user = null)
        {
            if (cafe == null) cafe = CafeFactory.Create();
            if (user == null) user = UserFactory.CreateUser();
            var notification = new Notification
            {
                CafeId = cafe.Id,
                UserId = user.Id
            };
            ContextManager.Get().Notifications.Add(notification);
            return notification;
        }

        public static List<Notification> CreateFew(int count = 3, Cafe cafe = null, User user = null)
        {
            var notifications = new List<Notification>();
            for (var i = 0; i < count; i++)
                notifications.Add(Create(cafe: cafe, user: user));
            return notifications;
        }
    }
}
