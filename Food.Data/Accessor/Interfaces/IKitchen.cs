using Food.Data.Entities;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface IKitchen
    {
        #region Kitchen

        /// <summary>
        /// Возвращает список всех кухонь
        /// </summary>
        /// <returns></returns>
        List<Kitchen> GetListOfKitchen();

        /// <summary>
        /// Возвращает список привязок кафе-кухня по идентификатору кафе
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <returns></returns>
        List<KitchenInCafe> GetListOfKitchenToCafe(long cafeId);
        #endregion
    }
}
