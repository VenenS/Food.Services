using Food.Data.Entities;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface IRating
    {
        #region Rating

        /// <summary>
        ///     Получить все рейтинги для юзера
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="typeOfObject">Тип объекта</param>
        /// <param name="isFilter">будем ли фильтровать по типу объекта, либо будем выводить все оценки</param>
        /// <returns></returns>
        List<Rating> GetAllRatingFromUser(long userId, int typeOfObject, bool isFilter);

        /// <summary>
        ///     Получить оценки для объекта
        /// </summary>
        /// <param name="objectId">Идентификатор объекта</param>
        /// <param name="typeOfObject">Тип объекта</param>
        /// <returns></returns>
        List<Rating> GetAllRatingToObject(long objectId, int typeOfObject);

        /// <summary>
        ///     Добавить рейтинг для объекта
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="objectId">Идентификатор Объекта</param>
        /// <param name="typeOfObject">Тип объекта</param>
        /// <param name="value">Оценка</param>
        /// <returns></returns>
        long InsertNewRating(long userId, long objectId, int typeOfObject, int value);

        /// <summary>
        ///     Получить среднюю оценку для блюда
        /// </summary>
        /// <param name="dishId"></param>
        /// <returns></returns>
        double GetFinalRateToDish(long dishId);

        /// <summary>
        ///     Получить среднюю оценку для кафе
        /// </summary>
        /// <param name="cafeId"></param>
        /// <returns></returns>
        double GetFinalRateToCafe(long cafeId);

        double GetFinalRateToObjectByInfo(long ratingSumm, long ratingCount);

        /*
        /// <summary>
        /// Получить конкретную информацию об оценке для объекта
        /// </summary>
        /// <param name="objectId">Идентификатор объекта</param>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="typeOfObject">Тип объекта</param>
        /// <returns></returns>
        Rating GetRatingToObjectFromUser(long objectId, long userId, int typeOfObject);
        */

        #endregion
    }
}