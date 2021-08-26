using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using ITWebNet.Food.Core.DataContracts.Common;

namespace Food.Services.Tests.FakeFactories
{
    public static class DishFactory
    {
        private static readonly Random Rnd = new Random();
        public static Dish Create(User creator = null, DishCategoryInCafe category = null, Cafe cafe = null)
        {
            category = category ?? DishCategoryInCafeFactory.Create(cafe: cafe);
            creator = creator ?? UserFactory.CreateUser();
            var dish = new Dish
            {
                CreationDate = DateTime.Now.AddDays(-30),
                CreatorId = creator.Id,
                IsDeleted = false,
                IsActive = true,
                DishName = Guid.NewGuid().ToString("N"),
                //CafeCategory = category,
                //CategoryId = category.Id
            };
            ContextManager.Get().Dishes.Add(dish);
            return dish;
        }

        public static List<Dish> CreateFew(int count = 3, User creator = null, DishCategoryInCafe category = null, Cafe cafe = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            var dishes = new List<Dish>();
            for (var i = 0; i < count; i++)
                dishes.Add(Create(creator, category, cafe));
            return dishes;
        }

        public static FoodDishModel CreateModel()
        {
            var dishModel = new FoodDishModel
            {
                Name = Guid.NewGuid().ToString("N"), BasePrice = Rnd.Next(), 
                Schedules = new List<ScheduleModel>(), Uuid = Guid.NewGuid()
            };
            return dishModel;
        }

        public static List<FoodDishModel> CreateFewModels(int count = 3)
        {
            var dishModels = new List<FoodDishModel>();
            for (var i = 0; i < count; i++)
                dishModels.Add(CreateModel());
            return dishModels;
        }

        public static Dish Clone(Dish parent)
        {
            var dish = new Dish
            {
                Id = parent.Id,
                CreationDate = parent.CreationDate,
                CreatorId = parent.CreatorId,
                IsDeleted = parent.IsDeleted,
                IsActive = parent.IsActive,
                DishName = parent.DishName,
                //CafeCategory = parent.CafeCategory,
                //CategoryId = parent.CategoryId
            };
            return dish;
        }
    }
}
