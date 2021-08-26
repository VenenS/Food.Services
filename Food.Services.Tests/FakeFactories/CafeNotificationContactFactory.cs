using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    public static class CafeNotificationContactFactory
    {
        public static CafeNotificationContact Create(
            NotificationChannel notificationChannel = null, Cafe cafe = null)
        {
            notificationChannel = notificationChannel ?? NotificationChannelFactory.Create();
            cafe = cafe ?? CafeFactory.Create();
            var contact = new CafeNotificationContact
            {
                IsDeleted = false,
                Cafe = cafe,
                CafeId = cafe.Id,
                NotificationContact = Guid.NewGuid().ToString("N"),
                NotificationChannel = notificationChannel,
                NotificationChannelId = notificationChannel.Id
            };
            ContextManager.Get().CafeNotificationContact.Add(contact);
            return contact;
        }

        public static List<CafeNotificationContact> CreateFew(int count = 3,
            NotificationChannel notificationChannel = null,
            Cafe cafe = null)
        {
            notificationChannel = notificationChannel ?? NotificationChannelFactory.Create();
            cafe = cafe ?? CafeFactory.Create();
            var contacts = new List<CafeNotificationContact>();
            for (var i = 0; i < count; i++)
                contacts.Add(Create(notificationChannel, cafe));
            return contacts;
        }
    }
}