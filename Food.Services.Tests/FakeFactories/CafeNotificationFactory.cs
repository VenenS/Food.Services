using System;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using ITWebNet.Food.Core.DataContracts.Common;

namespace Food.Services.Tests.FakeFactories
{
    class CafeNotificationFactory
    {
        public static CafeNotificationContact CreateNotificationContact(Cafe cafe)
        {
            var notify = new CafeNotificationContact()
            {
                Cafe = cafe,
                CafeId = cafe.Id,
                IsDeleted = false,
                NotificationContact = Guid.NewGuid().ToString("n") + "@" + Guid.NewGuid().ToString("n") + ".cru",
            };
            ContextManager.Get().CafeNotificationContact.Add(notify);
            return notify;
        }
        public static CafeNotificationContactModel CreateModel(Cafe cafe)
        {
            var notify = new CafeNotificationContactModel()
            {
                CafeId = cafe.Id,
                NotificationContact = Guid.NewGuid().ToString("n") + "@" + Guid.NewGuid().ToString("n") + ".cru"
            };
            return notify;
        }
        public static CafeNotificationContactModel CreateModel(CafeNotificationContact notificationContact)
        {
            var notify = new CafeNotificationContactModel()
            {
                CafeId = notificationContact.CafeId,
                NotificationContact = notificationContact.NotificationContact
            };
            return notify;
        }
    }
}
