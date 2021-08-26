using Food.Data.Entities;
using Food.Services.Extensions.Schedule;
using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Extensions.Dish
{
    public static class DishExtensions
    {
        public static FoodDishModel GetContractDish(this Data.Entities.Dish dish)
        {
            return dish == null
                ? null
                : new FoodDishModel
                {
                    Id = dish.Id,
                    Name = dish.DishName,
                    BasePrice = dish.BasePrice,
                    Kcalories = dish.Kcalories,
                    Weight = dish.Weight,
                    WeightDescription = dish.WeightDescription,
                    VersionFrom = dish.VersionFrom,
                    VersionTo = dish.VersionTo,
                    Uuid = dish.Uuid,
                    DishRatingCount = dish.DishRatingCount,
                    DishRatingSumm = dish.DishRatingSumm,
                    Description = dish.Description,
                    Image = dish.ImageId,
                    Composition = dish.Composition,
                    Schedules = dish.Schedules?.Select(c => c.GetContract()).ToList() ?? new List<ScheduleModel>(),
                    DishIndexToCategory = new Dictionary<long, int?>(dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted).Select(
                        l => new KeyValuePair<long, int?>(l.CafeCategory.DishCategoryId, l.DishIndex)))
                };
        }

        public static FoodDishVersionModel GetContractDishVersion(this DishVersion dish)
        {
            return dish == null
                ? null
                : new FoodDishVersionModel
                {
                    Id = dish.Id,
                    DishId = dish.DishId,
                    Name = dish.DishName,
                    BasePrice = dish.BasePrice,
                    Kcalories = dish.Kcalories,
                    Weight = dish.Weight,
                    WeightDescription = dish.WeightDescription,
                    VersionFrom = dish.VersionFrom,
                    VersionTo = dish.VersionTo
                };
        }

        public static FoodDishVersionModel GetContractDishVersionFromDish(this Data.Entities.Dish dish)
        {
            return dish == null
                ? null
                : new FoodDishVersionModel
                {
                    Id = dish.Id,
                    DishId = dish.Id,
                    Name = dish.DishName,
                    BasePrice = dish.BasePrice,
                    Kcalories = dish.Kcalories,
                    Weight = dish.Weight,
                    WeightDescription = dish.WeightDescription,
                    VersionFrom = dish.VersionFrom,
                    VersionTo = dish.VersionTo
                };
        }

        public static FoodDishModel GetContractDish(this DishVersion dish)
        {
            return dish == null
                ? null
                : new FoodDishModel
                {
                    Id = dish.DishId,
                    Name = dish.DishName,
                    BasePrice = dish.BasePrice,
                    Kcalories = dish.Kcalories,
                    Weight = dish.Weight,
                    WeightDescription = dish.WeightDescription,
                    VersionFrom = dish.VersionFrom,
                    VersionTo = dish.VersionTo,
                    DishIndex = dish.DishIndex,
                    CategoryId = dish.CafeCategory.DishCategoryId,
                    Image = dish.ImageId
                };
        }
    }
}
