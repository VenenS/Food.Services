using Food.Data.Entities;
using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Extensions.Dish
{
    public static class CategoryExtensions
    {
        // Определен в CategoriesController.cs
        //public static FoodCategoryModel GetContract(this DishCategory category)
        //{
        //    return category == null
        //        ? null
        //        : new FoodCategoryModel
        //        {
        //            Id = category.Id,
        //            Name = category.CategoryName,
        //            FullName = category.CategoryFullName,
        //            Description = category.Description,
        //            Uuid = category.Uuid
        //        };
        //}

        public static List<FoodCategoryWithDishes> ConvertDictionaryToListFoodCategory(Dictionary<FoodCategoryModel, List<FoodDishModel>> dictionary)
        {
            var model = new List<FoodCategoryWithDishes>();
            foreach (var el in dictionary)
            {
                model.Add(
                    new FoodCategoryWithDishes()
                    {
                        Category = el.Key,
                        Dishes = el.Value?.ToList().OrderBy(o => o.DishIndex).ToList()
                    });
            }

            return model;
        }
    }
}
