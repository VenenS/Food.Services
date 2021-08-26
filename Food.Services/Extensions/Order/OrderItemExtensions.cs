using Food.Data.Entities;
using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Extensions.OrderExtensions
{
    public static class OrderItemExtensions
    {
        public static OrderItemModel GetContract(this OrderItem orderItem)
        {
            return orderItem == null
                ? null
                : new OrderItemModel
                {
                    Id = orderItem.Id,
                    Comment = orderItem.Comment,
                    Discount = orderItem.DishDiscountPrc,
                    DishCount = orderItem.DishCount,
                    FoodDishId = orderItem.DishId,
                    IsDeleted = orderItem.IsDeleted,
                    DishKcalories = orderItem.DishKcalories,
                    DishName = orderItem.DishName,
                    DishWeight = orderItem.DishWeight,
                    DishBasePrice = orderItem.DishBasePrice,
                    TotalPrice = orderItem.TotalPrice
                };
        }

        public static OrderItem GetEntity(this OrderItemModel orderItem)
        {
            return orderItem == null
                ? new OrderItem()
                : new OrderItem
                {
                    Id = orderItem.Id,
                    DishName = orderItem.DishName,
                    DishId = orderItem.FoodDishId,
                    DishCount = orderItem.DishCount,
                    DishWeight = orderItem.DishWeight,
                    Comment = orderItem.Comment,
                    DishKcalories = orderItem.DishKcalories,
                    DishDiscountPrc = orderItem.Discount,
                    DishBasePrice = orderItem.DishBasePrice,
                    TotalPrice = orderItem.TotalPrice
                };
        }
    }
}
