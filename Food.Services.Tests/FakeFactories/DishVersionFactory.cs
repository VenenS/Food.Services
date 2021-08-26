using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    public static class DishVersionFactory
    {
        public static DishVersion Create(Dish dish = null, DishCategoryInCafe category = null)
        {
            dish = dish ?? DishFactory.Create();
            //category = category ?? dish.CafeCategory;
            var dishVersion = new DishVersion
            {
                IsDeleted = false,
                IsActive = true,
                Dish = dish,
                DishId = dish.Id,
                CafeCategory = null,
                CafeCategoryId = 0,
                DishName = Guid.NewGuid().ToString("N")
            };
            ContextManager.Get().DishVersions.Add(dishVersion);
            return dishVersion;
        }

        public static List<DishVersion> CreateFew(int count = 3, Dish dish = null, DishCategoryInCafe category = null)
        {
            dish = dish ?? DishFactory.Create();
            //category = category ?? dish.CafeCategory;
            var dishVersions = new List<DishVersion>();
            for (var i = 0; i < count; i++)
                dishVersions.Add(Create(dish, category));
            return dishVersions;
        }
    }
}