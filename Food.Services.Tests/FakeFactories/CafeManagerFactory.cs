using System;
using System.Collections.Generic;
using Food.Data.Entities;
using ITWebNet.Food.Core.DataContracts.Common;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    public static class CafeManagerFactory
    {
        public static CafeManager Create(User user = null, Cafe cafe = null)
        {
            cafe = cafe ?? CafeFactory.Create();
            user = user ?? UserFactory.CreateUser();
            var cafeManager = new CafeManager
            {
                CreationDate = DateTime.Now.AddYears(-1),
                Cafe = cafe,
                CafeId = cafe.Id,
                User = user,
                IsDeleted = false,
                UserId = user.Id
            };
            ContextManager.Get().CafeManagers.Add(cafeManager);
            return cafeManager;
        }

        public static List<CafeManager> CreateFew(int count = 3, User user = null, Cafe cafe = null)
        {
            var cafeManagers = new List<CafeManager>();
            for (var i = 0; i < count; i++)
                cafeManagers.Add(Create(user, cafe));
            return cafeManagers;
        }
        public static CafeManagerModel CreateModel(Cafe cafe, long userId)
        {
            var model = new CafeManagerModel()
            {
                CafeId = cafe.Id,
                UserId = userId
            };
            return model;
        }
    }
}