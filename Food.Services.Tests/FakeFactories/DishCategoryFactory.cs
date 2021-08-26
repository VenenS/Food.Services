using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using ITWebNet.Food.Core.DataContracts.Common;

namespace Food.Services.Tests.FakeFactories
{
    class DishCategoryFactory
    {
        private static Random _rnd = new Random();
        public static DishCategory Create(User creator = null, Cafe cafe = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            var category = new DishCategory
            {
                CreationDate = DateTime.Now.AddDays(-30),
                CreatorId = creator.Id,
                IsDeleted = false,
                IsActive = true,
                CategoryFullName = Guid.NewGuid().ToString("N"),
                CategoryName = Guid.NewGuid().ToString("N"),
                Index = _rnd.Next(1, 10000),
                Uuid = Guid.NewGuid()
            };
            ContextManager.Get().DishCategories.Add(category);
            return category;
        }

        public static List<DishCategory> CreateFew(int count = 3, Cafe cafe = null, User creator = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            var categories = new List<DishCategory>();
            for (var i = 0; i < count; i++)
                categories.Add(Create(creator, cafe));
            return categories;
        }

        public static FoodCategoryModel CreateModel()
        {
            var dishModel = new FoodCategoryModel
            {
                Name = Guid.NewGuid().ToString("N"),
                Uuid = Guid.NewGuid()
            };
            return dishModel;
        }

        public static List<FoodCategoryModel> CreateFewModels(int count = 3)
        {
            var dishModels = new List<FoodCategoryModel>();
            for (var i = 0; i < count; i++)
                dishModels.Add(CreateModel());
            return dishModels;
        }
    }
}
