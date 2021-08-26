using Food.Data.Entities;
using Food.Services.Tests.Context;
using System;
using System.Collections.Generic;

namespace Food.Services.Tests.FakeFactories
{
    public class CategoryFactory
    {
        public static DishCategory Create(bool saveDB = false)
        {
            var entity = new DishCategory()
            {
                CategoryFullName = Guid.NewGuid().ToString(),
                CategoryName = Guid.NewGuid().ToString(),
                CreationDate = DateTime.Now,
                Description = Guid.NewGuid().ToString(),
                Uuid = Guid.NewGuid()
            };

            if (saveDB) ContextManager.Get().DishCategories.Add(entity);

            return entity;
        }

        public static List<DishCategory> CreateFew(int count = 3, bool saveDB = false)
        {
            var lstEntities = new List<DishCategory>();
            for (int i = 0; i < count; i++)
            {
                lstEntities.Add(new DishCategory()
                {
                    CategoryFullName = Guid.NewGuid().ToString(),
                    CategoryName = Guid.NewGuid().ToString(),
                    CreationDate = DateTime.Now,
                    Description = Guid.NewGuid().ToString(),
                    Uuid = Guid.NewGuid()
                });
            }

            if (saveDB) ContextManager.Get().DishCategories.AddRange(lstEntities);

            return lstEntities;
        }
    }
}
