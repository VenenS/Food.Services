using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    public static class DishCategoryInCafeFactory
    {
        private static Random _rnd = new Random();
        public static DishCategoryInCafe Create(User creator = null, Cafe cafe = null, DishCategory category = null)
        {
            cafe = cafe ?? CafeFactory.Create(creator);
            creator = creator ?? UserFactory.CreateUser();
            category = category ?? DishCategoryFactory.Create(creator);
            var categoryInCafe = new DishCategoryInCafe
            {
                CreateDate = DateTime.Now.AddDays(-30),
                CreatorId = creator.Id,
                IsDeleted = false,
                IsActive = true, Index = _rnd.Next(1, 10000), Cafe = cafe, CafeId = cafe.Id, 
                DishCategory = category, DishCategoryId = category.Id
            };
            ContextManager.Get().DishCategoriesInCafes.Add(categoryInCafe);
            return categoryInCafe;
        }

        public static List<DishCategoryInCafe> CreateFew(int count = 3, User creator = null, Cafe cafe = null)
        {
            cafe = cafe ?? CafeFactory.Create(creator);
            creator = creator ?? UserFactory.CreateUser();
            var categoryInCafes = new List<DishCategoryInCafe>();
            for (var i = 0; i < count; i++)
                categoryInCafes.Add(Create(creator, cafe));
            return categoryInCafes;
        }
        /*
        public static Categor CreateModel()
        {
            var categoryInCafeModel = new FoodDishCategoryInCafeModel
            {
                Name = Guid.NewGuid().ToString("N"),
                BasePrice = (double)_rnd.Next(),
                Schedules = new List<ScheduleModel>(),
                Uuid = Guid.NewGuid()
            };
            return categoryInCafeModel;
        }

        public static List<FoodDishCategoryInCafeModel> CreateFewModels(int count = 3)
        {
            var categoryInCafeModels = new List<FoodDishCategoryInCafeModel>();
            Parallel.For(0, count, (i) =>
            {
                categoryInCafeModels.Add(CreateModel());
            });
            return categoryInCafeModels;
        }*/
    }
}
