using Food.Data.Entities;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Food.Services.Extensions
{
    public static class DishServiceHelper
    {
        public static void DishValidation(FoodDishModel dish, Data.Entities.Dish oldDish, long? cafeId = null)
        {
            #region BasePrice
            if (
                    dish.BasePrice > 0
                    && dish.BasePrice < 1000000
               )
            {
                oldDish.BasePrice = dish.BasePrice;
            }
            else
            {
                throw new ValidationException("Wrong Dish Price, it can be only in range between 0 and 1000000");
            }
            #endregion

            #region Kcalories
            if (
                    dish.Kcalories == null
                    ||
                    (
                        dish.Kcalories > 0
                        && dish.Kcalories < 1000000
                    )
                )
            {
                oldDish.Kcalories = dish.Kcalories;
            }
            else
            {
                throw new ValidationException("Wrong Dish Kcalories, it can be only in range between 0 and 1000000");
            }
            #endregion

            #region Name
            if (!string.IsNullOrWhiteSpace(dish.Name) && dish.Name.Length > 1)
            {
                if (string.IsNullOrEmpty(oldDish.DishName) || oldDish.DishName != dish.Name)
                {
                    var dishesInCategory = Accessor.Instance.GetFoodDishesByCafeId(
                        oldDish?.DishCategoryLinks?.FirstOrDefault()?.CafeCategory?.CafeId ?? cafeId.GetValueOrDefault());
                    if (dishesInCategory.Find(
                        d => d.DishName.ToLower() == dish.Name.ToLower() 
                        && d.Id != dish.Id) != null)
                    {
                        throw new ValidationException("Wrong Dish Name, now exist dish with this name");
                    }
                }
                oldDish.DishName = dish.Name;
            }
            else
            {
                throw new ValidationException("Wrong Dish Name, it can`t be empty and minimum length must be more then 1 symbol");
            }
            #endregion

            #region Weight
            if (
                    dish.Weight == null
                    ||
                    (
                        dish.Weight > 0
                        && dish.Weight < 1000000
                    )
                )
            {
                oldDish.Weight = dish.Weight;
            }
            else
            {
                throw new ValidationException("Wrong Dish Weight, it can be only in range between 0 and 1000000");
            }
            #endregion

            oldDish.WeightDescription = dish.WeightDescription;
        }
    }
}
