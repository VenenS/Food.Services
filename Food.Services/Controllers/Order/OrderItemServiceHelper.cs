using ITWebNet.FoodService.Food.DbAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Food.Data.Entities;
using ITWebNet.Food.Core.DataContracts.Common;

namespace Food.Services.Controllers
{
    public static class OrderItemServiceHelper
    {
        /// <summary>
        /// Проверка наличия заказа компании
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static bool CheckCompanyOrderAvailability(Order order)
        {
            if (order != null && order.CompanyOrderId != null)
            {
                var companyOrder = Accessor.Instance.GetCompanyOrderById((long)order.CompanyOrderId);
                if (companyOrder != null)
                {
                    if (
                        (
                            companyOrder.AutoCloseDate == null
                            || companyOrder.AutoCloseDate > DateTime.Now
                        )
                        &&
                        (
                            companyOrder.OpenDate == null
                            || companyOrder.OpenDate < DateTime.Now
                        )
                    )
                        return true;
                    return false;
                }

                return true;
            }

            return true;
        }

        public static double GetCurrentActualPriceForDish(long dishId, DateTime scheduleDate)
        {
            var dishPrice =
                Accessor.Instance.GetFoodDishesById
                (
                    new List<long> { dishId }.ToArray()
                )[0].BasePrice;
            var scheduleForDish =
                Accessor.Instance.GetScheduleActiveByDishId
                (
                    dishId,
                    scheduleDate
                );

            if (scheduleForDish != null && scheduleForDish.Count > 0)
            {
                var simpleSchedule =
                    scheduleForDish.Find(s => s.Type.Equals("S"));

                if (simpleSchedule != null)
                    dishPrice =
                        simpleSchedule.Price ?? dishPrice;
                else
                    dishPrice =
                        scheduleForDish[0].Price ?? dishPrice;
            }
            else
            {
                return double.NaN;
            }

            return dishPrice;
        }
    }

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
