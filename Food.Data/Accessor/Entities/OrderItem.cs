using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region OrderItem

        /// <summary>
        /// Изменить позицию (блюдо) в заказе
        /// </summary>
        /// <param name="orderItem"></param>
        /// <returns></returns>
        public int ChangeOrderItem(OrderItem orderItem)
        {
            try
            {
                using (var fc = GetContext())
                {
                    OrderItem oldItem =
                        fc.OrderItems.FirstOrDefault(
                            i => i.Id == orderItem.Id
                            && i.IsDeleted == false
                        );

                    if (oldItem != null)
                    {
                        oldItem.DishId = orderItem.DishId;
                        oldItem.DishName = orderItem.DishName;
                        oldItem.DishKcalories = orderItem.DishKcalories;
                        oldItem.DishWeight = orderItem.DishWeight;
                        oldItem.ImageId = orderItem.ImageId;
                        oldItem.DishCount = orderItem.DishCount;
                        oldItem.DishBasePrice = orderItem.DishBasePrice;
                        oldItem.DishDiscountPrc = orderItem.DishDiscountPrc;
                        oldItem.TotalPrice = orderItem.TotalPrice;
                        oldItem.OrderId = orderItem.OrderId;
                        oldItem.LastUpdDate = DateTime.Now;
                        oldItem.LastUpdateByUserId = orderItem.LastUpdateByUserId;
                        oldItem.Comment = orderItem.Comment;

                        fc.SaveChanges();
                        return (int)oldItem.Id;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Удалить позицию(блюдо) из заказа
        /// </summary>
        /// <param name="orderItemId">идентификатор позиции</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        public bool DeleteOrderItem(long orderItemId, long userId)
        {

            try
            {
                using (var fc = GetContext())
                {
                    OrderItem orderItem =
                        fc.OrderItems.FirstOrDefault(
                            i => i.Id == orderItemId
                            && i.IsDeleted == false
                    );

                    if (orderItem != null)
                    {
                        orderItem.IsDeleted = true;
                        orderItem.LastUpdateByUserId = userId;
                        orderItem.LastUpdDate = DateTime.Now;

                        fc.SaveChanges();

                        return true;
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }

        /// <summary>
        /// Получение OrderItem по его ID
        /// </summary>
        /// <param name="orderItemId">
        /// Идентификатор OrderItem
        /// </param>
        /// <returns>
        /// OrderItem_m, то есть строку из базы, ежели таковая существует
        /// </returns>
        public OrderItem GetOrderItemById(Int64 orderItemId)
        {
            var fc = GetContext();

            var query = fc.OrderItems.AsNoTracking().Where(ori => ori.Id == orderItemId && !ori.IsDeleted);
            return query.ToList().Count == 1 ? query.Single() : null;
        }

        /// <summary>
        /// Запись OrderItem в базу данных
        /// </summary>
        /// <param name="orderItem">OrderItem</param>
        /// <returns>int[2] {orderId, orderItemId}</returns>
        public int[] PostOrderItem(OrderItem orderItem)
        {
            try
            {
                using (var fc = GetContext())
                {
                    orderItem.CreationDate = DateTime.Now;
                    orderItem.ImageId = 1;
                    orderItem.Dish = null;
                    orderItem.Order = null;

                    fc.OrderItems.Add(orderItem);
                    fc.SaveChanges();
                    return new[] { (int)orderItem.OrderId, (int)orderItem.Id };
                }
            }
            catch (Exception)
            {
                return new[] { -1, -1 };
            }
        }

        #endregion
    }
}
