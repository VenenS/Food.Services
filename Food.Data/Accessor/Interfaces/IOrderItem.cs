using Food.Data.Entities;
using System;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface IOrderItem
    {
        #region OrderItem

        /// <summary>
        /// Изменить позицию (блюдо) в заказе
        /// </summary>
        /// <param name="orderItem"></param>
        /// <returns></returns>
        int ChangeOrderItem(OrderItem orderItem);

        /// <summary>
        /// Удалить позицию(блюдо) из заказа
        /// </summary>
        /// <param name="orderItemId">идентификатор позиции</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        bool DeleteOrderItem(long orderItemId, long userId);

        /// <summary>
        /// Получение OrderItem по его ID
        /// </summary>
        /// <param name="orderItemId">
        /// Идентификатор OrderItem
        /// </param>
        /// <returns>
        /// OrderItem_m, то есть строку из базы, ежели таковая существует
        /// </returns>
        OrderItem GetOrderItemById(Int64 orderItemId);

        /// <summary>
        /// Запись OrderItem в базу данных
        /// </summary>
        /// <param name="orderItem">OrderItem</param>
        /// <returns>int[2] {orderId, orderItemId}</returns>
        int[] PostOrderItem(OrderItem orderItem);

        #endregion
    }
}
