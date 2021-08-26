using Food.Data.Entities;

using System;

namespace Food.Services.Services
{
    public interface IMenuService
    {
        /// <summary>
        /// Возвращает разрешен ли заказ из указанной категории <b>для
        /// текущего пользователя</b>.
        /// </summary>
        /// <param name="category">Категория в кафе</param>
        /// <param name="orderDay">Дата на которую оформлен заказ, без времени</param>
        /// <param name="deliveryTime">Время доставки. Если null, то тип доставки - "как можно быстрее".
        /// Игнорируется для корп. заказов.</param>
        bool CanOrderFromCategory(DishCategoryInCafe category, DateTime orderDay, TimeSpan? deliveryTime = null);

        /// <summary>
        /// Возвращает видна ли категория <b>для текущего пользователя</b>.
        /// </summary>
        /// <param name="category">Категория в кафе</param>
        /// <param name="when">Дата на которую просматривается категория</param>
        bool IsCategoryVisible(DishCategoryInCafe category, DateTime when);
    }
}