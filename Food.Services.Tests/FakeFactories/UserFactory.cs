using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using ITWebNet.Food.Core.DataContracts.Admin;
using ITWebNet.Food.Core.DataContracts.Common;

namespace Food.Services.Tests.FakeFactories
{
    public static class UserFactory
    {
        private static readonly Random Rnd;

        static UserFactory()
        {
            Rnd = new Random();
        }

        public static User CreateUser()
        {
            var user = new User
            {
                Email = Guid.NewGuid().ToString("n") + "@" +
                        Guid.NewGuid().ToString("n") + ".cru",
                EmailConfirmed = true,
                Name = Guid.NewGuid().ToString("n"),
                CreationDate = DateTime.Now.AddMonths(-2),
                PhoneNumber = Rnd.Next(7000000, 9000000).ToString(),
                PhoneNumberConfirmed = true,
                Password = Guid.NewGuid().ToString("n"), DeviceUuid = Guid.NewGuid().ToString("n"),
                DisplayName = Guid.NewGuid().ToString("n"), 
                FirstName = Guid.NewGuid().ToString("n"), LastName = Guid.NewGuid().ToString("n"), UserReferralLink = Guid.NewGuid().ToString("n"),
                AccessFailedCount = 0
            };
            ContextManager.Get().Users.Add(user);
            return user;
        }

        public static List<User> CreateFew(int count = 3)
        {
            List<User> users = new List<User>();
            for (int i = 0; i < count; i++)
                users.Add(CreateUser());
            return users;
        }

        public static UserModel CreateCommonModel()
        {
            var model = new UserModel
            {
                Email = Guid.NewGuid().ToString("n") + "@" +
                        Guid.NewGuid().ToString("n") + ".cru",
                EmailConfirmed = true,
                PhoneNumber = Rnd.Next(7000000, 9000000).ToString(),
                PhoneNumberConfirmed = true,
                IsDeleted = false,
                LockoutEnabled = false,
                SmsNotify = false,
                TwoFactorEnabled = false,
                UserFullName = Guid.NewGuid().ToString("n"),
            };

            return model;
        }

        public static UserAdminModel CreateAdminModel()
        {
            var model = new UserAdminModel
            {
                Email = Guid.NewGuid().ToString("n") + "@" +
                        Guid.NewGuid().ToString("n") + ".cru",
                EmailConfirmed = true,
                PhoneNumber = Rnd.Next(7000000, 9000000).ToString(),
                PhoneNumberConfirmed = true,
                IsDeleted = false,
                LockoutEnabled = false,
                SmsNotify = false,
                TwoFactorEnabled = false,
                UserFullName = Guid.NewGuid().ToString("n"),
            };

            return model;
        }
    }
}