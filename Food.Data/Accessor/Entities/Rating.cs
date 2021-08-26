using System;
using System.Collections.Generic;
using System.Linq;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region Rating

        /// <summary>
        ///     Получить все рейтинги для юзера
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="typeOfObject">Тип объекта</param>
        /// <param name="isFilter">будем ли фильтровать по типу объекта, либо будем выводить все оценки</param>
        /// <returns></returns>
        public virtual List<Rating> GetAllRatingFromUser(long userId, int typeOfObject, bool isFilter)
        {
            using (var fc = GetContext())
            {
                var query = fc.Rating.AsNoTracking().Where(r => r.UserId == userId && r.IsDeleted == false);
                if (isFilter)
                    query = query.Where(r => r.ObjectType == typeOfObject);

                return query.ToList();
            }
        }

        /// <summary>
        ///     Получить оценки для объекта
        /// </summary>
        /// <param name="objectId">Идентификатор объекта</param>
        /// <param name="typeOfObject">Тип объекта</param>
        /// <returns></returns>
        public virtual List<Rating> GetAllRatingToObject(long objectId, int typeOfObject)
        {
            using (var fc = GetContext())
            {
               return fc.Rating.AsNoTracking().Where(r => r.ObjectId == objectId
                             && r.ObjectType == typeOfObject
                             && r.IsDeleted == false).ToList();
            }
        }

        /// <summary>
        ///     Добавить рейтинг для объекта
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="objectId">Идентификатор Объекта</param>
        /// <param name="typeOfObject">Тип объекта</param>
        /// <param name="value">Оценка</param>
        /// <returns></returns>
        public virtual long InsertNewRating(long userId, long objectId, int typeOfObject, int value)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var newRating = new Rating
                    {
                        CreateDate = DateTime.Now,
                        UserId = userId,
                        RatingValue = value,
                        CreatorId = userId,
                        ObjectType = typeOfObject,
                        ObjectId = objectId
                    };

                    var oldRating = fc.Rating.FirstOrDefault(r => r.ObjectId == objectId
                                    && r.UserId == userId && r.ObjectType == typeOfObject
                                    && r.IsDeleted == false);

                    var oldRatingValue = 0;

                    if (oldRating != null)
                    {
                        oldRatingValue = oldRating.RatingValue;
                        oldRating.RatingValue = value;
                        oldRating.LastUpdateDate = DateTime.Now;
                        oldRating.LastUpdatedBy = userId;
                    }

                    if (typeOfObject == (int) ObjectTypesEnum.Cafe)
                    {
                        var cafe = fc.Cafes.FirstOrDefault(
                            c => c.Id == objectId && c.IsActive == true
                                                  && c.IsDeleted == false);

                        if (cafe != null)
                        {
                            if (oldRating != null)
                                cafe.CafeRatingSumm = cafe.CafeRatingSumm + value - oldRatingValue;
                            else
                            {
                                fc.Rating.Add(newRating);
                                cafe.CafeRatingSumm = cafe.CafeRatingSumm + value;
                                cafe.CafeRatingCount++;
                            }
                        }
                    }
                    else if (typeOfObject == (int) ObjectTypesEnum.Dish)
                    {
                        var dish = fc.Dishes.FirstOrDefault(d => d.Id == objectId
                                 && d.IsActive == true && d.IsDeleted == false);

                        if (dish != null)
                            if (oldRating != null)
                                dish.DishRatingSumm = dish.DishRatingSumm + value - oldRatingValue;
                            else
                            {
                                fc.Rating.Add(newRating);
                                dish.DishRatingSumm = dish.DishRatingSumm + value;
                                dish.DishRatingCount++;
                            }
                    }

                    fc.SaveChanges();
                    if (oldRating != null) return oldRating.Id;
                    if (newRating.Id != 0) return newRating.Id;
                    return -1;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        ///     Получить среднюю оценку для блюда
        /// </summary>
        /// <param name="dishId"></param>
        /// <returns></returns>
        public virtual double GetFinalRateToDish(long dishId)
        {
            double rate = 0;

            using (var fc = GetContext())
            {
                var dish =
                    fc.Dishes.AsNoTracking().FirstOrDefault(c => c.Id == dishId
                     && c.IsActive == true && c.IsDeleted == false);

                if (dish != null)
                    rate = GetFinalRateToObjectByInfo(dish.DishRatingSumm, dish.DishRatingCount);
            }

            return rate;
        }

        /// <summary>
        ///     Получить среднюю оценку для кафе
        /// </summary>
        /// <param name="cafeId"></param>
        /// <returns></returns>
        public virtual double GetFinalRateToCafe(long cafeId)
        {
            double rate = 0;

            using (var fc = GetContext())
            {
                var cafe =
                    fc.Cafes.AsNoTracking().FirstOrDefault(c => c.Id == cafeId
                                                           && c.IsActive == true && c.IsDeleted == false);

                if (cafe != null)
                    rate =
                        GetFinalRateToObjectByInfo(
                            cafe.CafeRatingSumm,
                            cafe.CafeRatingCount
                        );
            }

            return rate;
        }

        public virtual double GetFinalRateToObjectByInfo(long ratingSumm, long ratingCount)
        {
            double rate = 0;
            if (ratingCount > 0)
                rate = Math.Round(ratingSumm / (double) ratingCount, 2);
            rate = rate - 5.0 > 1e-5 || rate - 1.0 < 1e-5 ? 0 : rate;

            return rate;
        }

        /*
        /// <summary>
        /// Получить конкретную информацию об оценке для объекта
        /// </summary>
        /// <param name="objectId">Идентификатор объекта</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="typeOfObject">Тип объекта</param>
        /// <returns></returns>
        public virtual Rating GetRatingToObjectFromUser(long objectId, long userId, int typeOfObject)
        {
            Rating rating;

            using (var fc = GetContext())
            {
                rating =
                    fc.Rating.FirstOrDefault(r => r.ObjectId == objectId
                        && r.UserId == userId
                        && r.ObjectType == typeOfObject
                        && r.IsDeleted == false);
            }

            return rating;
        }
        */

        #endregion
    }
}