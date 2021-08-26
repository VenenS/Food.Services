using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using ITWebNet.Food.Core.DataContracts.Manager;

namespace Food.Services.Tests.FakeFactories
{
    public class CafeMenuPatternFactory
    {
        public static CafeMenuPatternModel CreateModel(Cafe cafe)
        {
            var pattern = new CafeMenuPatternModel()
            {
                CafeId = cafe.Id,
                IsBanket = true,
                Name = Guid.NewGuid().ToString("N"),
            };
            return pattern;
        }
        public static CafeMenuPatternModel CreateModel(Cafe cafe, CafeMenuPattern pattern)
        {
            var patternModel = new CafeMenuPatternModel()
            {
                CafeId = cafe.Id,
                IsBanket = true,
                Name = Guid.NewGuid().ToString("N"),
            };
            return patternModel;
        }
        public static CafeMenuPatternModel CreateModelWithDishes(Cafe cafe, List<CafeMenuPatternDish> dishes)
        {
            var tmp = new List<Contracts.CafeMenuPatternDishModel>();
            foreach(var i in dishes)
            {
                tmp.Add(new Contracts.CafeMenuPatternDishModel()
                {
                    DishId = i.DishId,
                    Name = i.Name
                });
            }
            var patternModel = new CafeMenuPatternModel()
            {
                Dishes = tmp,
                CafeId = cafe.Id,
                IsBanket = true,
                Name = Guid.NewGuid().ToString("N"),
            };
            return patternModel;
        }
        public static CafeMenuPattern Create(Cafe cafe)
        {
            var pattern = new CafeMenuPattern()
            {
                IsDeleted = false,
                CafeId = cafe.Id,
                Name = Guid.NewGuid().ToString("N"),
            };
            ContextManager.Get().CafeMenuPatterns.Add(pattern);
            return pattern;
        }
        public static CafeMenuPattern CreateWithBankets(Cafe cafe, DateTime eventBanket = new DateTime())
        {
            var bankets = BanketFactory.CreateFew();
            foreach(var i in bankets)
            {
                i.EventDate = eventBanket;
            }
            var pattern = new CafeMenuPattern()
            {
                Bankets = bankets,
                IsDeleted = false,
                CafeId = cafe.Id,
                Name = Guid.NewGuid().ToString("N"),
                IsBanket = true
            };
            ContextManager.Get().CafeMenuPatterns.Add(pattern);
            return pattern;
        }
        public static CafeMenuPattern CreateWithDishes(Cafe cafe, int countDishes = 3)
        {
            var dishesList = new List<CafeMenuPatternDish>();
            for(var i = 0; i < countDishes; i++)
            {
                var dish = DishFactory.Create();
                dishesList.Add(new CafeMenuPatternDish()
                {
                    Dish = dish,
                    DishId = dish.Id,
                    IsDeleted = false,
                    Name = Guid.NewGuid().ToString("N"),
                    Price = 5
                });
            }
            var pattern = new CafeMenuPattern()
            {
                Dishes = dishesList,
                IsDeleted = false,
                CafeId = cafe.Id,
                Name = Guid.NewGuid().ToString("N")
            };
            ContextManager.Get().CafeMenuPatterns.Add(pattern);
            return pattern;
        }

        public static List<CafeMenuPattern> CreateFew(Cafe cafe, int count = 3)
        {
            var rand = new Random();
            var lstEntities = new List<CafeMenuPattern>();
            for (; count >= 0; count--)
            {
                lstEntities.Add(new CafeMenuPattern()
                {
                    CafeId = cafe.Id,
                    Id = rand.Next(1, 1000),
                    IsBanket = Convert.ToBoolean(rand.Next(0, 2)),
                    IsDeleted = Convert.ToBoolean(rand.Next(0, 2)),
                    Name = Guid.NewGuid().ToString(),
                    PatternDate = DateTime.Now.AddDays(rand.Next(1, 20))
                });
            }

            return lstEntities;
        }
    }
}
