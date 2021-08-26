using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    public static class NotificationChannelFactory
    {
        public static NotificationChannel Create()
        {
            var channel = new NotificationChannel
            {
                NotificationTypeCode = Guid.NewGuid().ToString("N"),
                NotificationTypeName = Guid.NewGuid().ToString("N")
            };
            ContextManager.Get().NotificationChannel.Add(channel);
            return channel;
        }

        public static List<NotificationChannel> CreateFew(int count = 3)
        {
            var channels = new List<NotificationChannel>();
            for (var i = 0; i < count; i++)
                channels.Add(Create());
            return channels;
        }
    }
}