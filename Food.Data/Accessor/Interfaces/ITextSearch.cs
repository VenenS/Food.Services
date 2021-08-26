using Food.Data.Entities;
using System;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface ITextSearch
    {
        #region TextSearch
        /// <summary>
        /// Получить список кафе по входной строке и списку тегов
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        List<Cafe> GetCafesBySearchTermAndTagList(List<long> tags, string searchTerm);

        /// <summary>
        ///  Получить список кафе (также содержащих блюда и категории блюд); по входной строке и списку тегов.
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        List<Cafe> GetCafesWithDishesBySearchTermAndTagList(List<long> tags, string searchTerm);

        /// <summary>
        /// Возвращает категории по списку тегов и входной строке
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="cafeId"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        List<DishCategoryInCafe> GetCategorysBySearchTermAndTagListAndCafeId(List<Int64> tags, Int64 cafeId, string searchTerm);

        List<Dish> GetDishesBySearchTermAndTagListAndCafeId(List<Int64> tags, Int64 cafeId, string searchTerm);

        #endregion
    }
}
