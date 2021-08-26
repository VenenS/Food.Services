using System;
using System.Collections.Generic;
using System.Linq;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface IDishVersion
    {
        #region DishVersion

        /// <summary>
        /// Возвращает список версионных блюд кафе по категории на указанную дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="categoryId">идентификатор категории</param>
        /// <param name="date">дата</param>
        /// <returns></returns>
        List<DishVersion> GetFoodDishVersionByCategoryIdAndCafeId(long cafeId, long categoryId, DateTime date);

        /// <summary>
        /// Возвращает список версионных блюд для кафе на указанную дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="date">дата</param>
        /// <returns></returns>
        List<DishVersion> GetFoodDishVersionByCafeId(long cafeId, DateTime date);

        /// <summary>
        /// Возвращает список версионных блюд, идентификаторы которых входят в переданный массив
        /// </summary>
        /// <param name="Ids">Массив идентификаторов блюд, версии которых надо получить</param>
        /// <param name="date">дата</param>
        /// <returns></returns>
        List<Dish> GetFoodDishVersionByIds(long[] Ids, DateTime date);

        /// <summary>
        /// Получить список версионных блюд по идентификатору на указанную дату
        /// </summary>
        /// <param name="dishId">идентификатор блюда</param>
        /// <param name="date">дата</param>
        /// <returns></returns>
        List<DishVersion> GetFoodDishVersionByDishIdAndDate(Int64 dishId, DateTime date);

        #endregion
    }
}
