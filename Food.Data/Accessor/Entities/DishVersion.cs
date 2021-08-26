using System;
using System.Collections.Generic;
using System.Linq;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region DishVersion

        /// <summary>
        /// Возвращает список версионных блюд кафе по категории на указанную дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="categoryId">идентификатор категории</param>
        /// <param name="date">дата</param>
        /// <returns></returns>
        public List<DishVersion> GetFoodDishVersionByCategoryIdAndCafeId(long cafeId, long categoryId, DateTime date)
        {
            var fc = GetContext();

            var query = fc.DishVersions.Where(d =>
                                ((d.CafeCategory.CafeId == cafeId
                                && d.CafeCategory.DishCategoryId == categoryId)
                                || d.Dish.DishCategoryLinks.Any(
                                    l => l.CafeCategory.DishCategoryId == categoryId
                                    && l.CafeCategory.CafeId == cafeId
                                    && l.CreateDate <= date
                                    && l.LastUpdDate >= date))
                                && (d.VersionFrom <= date || d.VersionFrom == null)
                                && (d.VersionTo >= date || d.VersionTo == null)
                                && d.IsActive == true
                                && d.IsDeleted == false
                        );

            return query.ToList();
        }

        /// <summary>
        /// Возвращает список версионных блюд для кафе на указанную дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="date">дата</param>
        /// <returns></returns>
        public List<DishVersion> GetFoodDishVersionByCafeId(long cafeId, DateTime date)
        {
            var fc = GetContext();

            var query = fc.DishVersions.Where(d => 
                                d.CafeCategory.CafeId == cafeId
                                && (d.VersionFrom <= date || d.VersionFrom == null)
                                && (d.VersionTo >= date || d.VersionTo == null)
                                && d.IsActive == true
                                && d.IsDeleted == false
                        );

            return query.ToList();
        }

        /// <summary>
        /// Возвращает список версионных блюд, идентификаторы которых входят в переданный массив
        /// </summary>
        /// <param name="Ids">Массив идентификаторов блюд, версии которых надо получить</param>
        /// <param name="date">дата</param>
        /// <returns></returns>
        public List<Dish> GetFoodDishVersionByIds(long[] Ids, DateTime date)
        {
            var fc = GetContext();

            var query = fc.DishVersions
                .Where(d => Ids.Contains(d.DishId)
                    && (d.VersionFrom <= date || d.VersionFrom == null)
                    && (d.VersionTo >= date || d.VersionTo == null)
                    && d.IsActive == true
                    && d.IsDeleted == false)
                .Select(d => d.Dish);

            return query.ToList();
        }

        /// <summary>
        /// Получить список версионных блюд по идентификатору на указанную дату
        /// </summary>
        /// <param name="dishId">идентификатор блюда</param>
        /// <param name="date">дата</param>
        /// <returns></returns>
        public List<DishVersion> GetFoodDishVersionByDishIdAndDate(Int64 dishId, DateTime date)
        {
            var fc = GetContext();

            var query = fc.DishVersions
                .Where(d =>
                    (d.VersionFrom <= date || d.VersionFrom == null)
                    && (d.VersionTo >= date || d.VersionTo == null)
                    && d.IsActive == true
                    && d.IsDeleted == false
                    && d.DishId == dishId);

            var dishes = query.ToList();

            return dishes;
        }

        #endregion
    }
}
