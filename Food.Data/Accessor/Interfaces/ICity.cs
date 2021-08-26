using Food.Data.Entities;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface ICity
    {
        #region City
        /// <summary>
        /// Возвращает список городов
        /// </summary>
        /// <returns></returns>
        List<City> GetCities();

        /// <summary>
        /// Возвращает город по имени
        /// </summary>
        /// <param name="cityName">имя роли</param>
        /// <returns></returns>
        City GetCityByName(string cityName);
        #endregion
    }
}
