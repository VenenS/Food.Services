using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using System.Collections.Generic;

namespace Food.Services.Tests.Accessor.Entities
{
    public static class DishInMenuFactory
    {
        public static DishInMenu Create(Dish dish = null)
        {
            if (dish == null) dish = DishFactory.Create();
            var entity = new DishInMenu()
            {
                DishId = dish.Id,
                Dish = dish,
                IsActive = true,
                Type = "D"
            };

            ContextManager.Get().DishesInMenus.Add(entity);
            return entity;
        }

        public static List<DishInMenu> CreateFew(int count = 3)
        {
            var lstEntities = new List<DishInMenu>();
            for (int i = 0; i < count; i++)
                lstEntities.Add(Create());
            return lstEntities;
        }
    }
}