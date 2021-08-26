using System;
using System.Collections.Generic;
using System.Linq;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region TextSearch
        /// <summary>
        /// Получить список кафе по входной строке и списку тегов
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public virtual List<Cafe> GetCafesBySearchTermAndTagList(List<long> tags, string searchTerm)
        {
            using (var fc = GetContext())
            {
                IQueryable<Cafe> queryToGetCafes;

                if (tags != null && tags.Count > 0)
                {
                    #region SearchConsiderWithTagsAndSearchTerm

                    queryToGetCafes =
                    fc.TagObject
                    .Where(
                        to =>
                        tags.Contains(to.TagId)
                        && (ObjectTypesEnum)to.ObjectTypeId ==
                            ObjectTypesEnum.Cafe
                        && to.IsDeleted == false
                    )
                    .GroupBy(to => to.ObjectId)
                    .Select(to => to.FirstOrDefault())
                    .Join(
                         fc.Cafes,
                         to => to.ObjectId,
                         cafe => cafe.Id,
                         (to, cafe) => cafe
                     )
                        .Where(cafe => cafe.IsActive == true
                            && cafe.IsDeleted == false
                         &&
                            (
                                cafe
                                    .CafeFullName
                                    .ToLower()
                                    .Contains(searchTerm)
                                || cafe
                                    .CafeName
                                    .ToLower()
                                    .Contains(searchTerm)
                                || cafe
                                    .CafeShortDescription
                                    .ToLower()
                                    .Contains(searchTerm)
                            )
                    );
                    #endregion
                }
                else
                {
                    #region SearchConsiderOnlyWithSearchTerm
                    queryToGetCafes =
                        from cafe in fc.Cafes
                        where cafe.IsActive == true
                        && cafe.IsDeleted == false
                            && (
                                cafe
                                    .CafeFullName
                                    .ToLower()
                                    .Contains(searchTerm)
                                || cafe
                                    .CafeName
                                    .ToLower()
                                    .Contains(searchTerm)
                                || cafe
                                    .CafeShortDescription
                                    .ToLower()
                                    .Contains(searchTerm)
                        )
                        select cafe;

                    #endregion
                }

                List<Cafe> cafes = queryToGetCafes.OrderBy(c => c.CafeName).ToList();

                return cafes;
            }

        }
        /// <summary>
        ///  Получить список кафе (также содержащих блюда и категории блюд) по входной строке и списку тегов.
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public virtual List<Cafe> GetCafesWithDishesBySearchTermAndTagList(List<long> tags, string searchTerm)
        {
            using (var fc = GetContext())
            {
                IQueryable<Cafe> queryToGetCafes,
                    queryToGetCafesFromCategories,
                    queryToGetCafeFromDishes;

                if (tags != null && tags.Count > 0)
                {
                    #region SearchConsiderWithTagsAndSearchTerm
                    queryToGetCafes =
                        fc.TagObject
                        .Where(
                            to =>
                            tags.Contains(to.TagId)
                            && (ObjectTypesEnum)to.ObjectTypeId ==
                                ObjectTypesEnum.Cafe
                                && to.IsDeleted == false
                        )
                        .GroupBy(to => to.ObjectId)
                        .Where(o => o.Count() >= tags.Count)
                        .Join(
                            fc.Cafes,
                            to => to.Key,
                            cafe => cafe.Id,
                            (to, cafe) => cafe
                        )
                        .Where(
                            cafe => cafe.IsActive == true
                            && cafe.IsDeleted == false
                            &&
                            (
                                cafe
                                    .CafeFullName
                                    .ToLower()
                                    .Contains(searchTerm)
                                || cafe
                                    .CafeName
                                    .ToLower()
                                    .Contains(searchTerm)
                                || cafe
                                    .CafeShortDescription
                                    .ToLower()
                                    .Contains(searchTerm)
                            )
                    );

                    queryToGetCafesFromCategories =
                        fc.TagObject
                        .Where(
                            to =>
                            tags.Contains(to.TagId)
                            && (ObjectTypesEnum)to.ObjectTypeId ==
                                ObjectTypesEnum.Category
                                && to.IsDeleted == false
                        )
                        .GroupBy(to => to.ObjectId)
                        .Where(o => o.Count() >= tags.Count)
                        .Join(
                            fc.DishCategoriesInCafes,
                            to => to.Key,
                            category =>
                                category.IsActive == true
                                && category.IsDeleted == false
                                &&
                                (
                                    category
                                        .DishCategory
                                        .Description
                                        .ToLower()
                                        .Contains(searchTerm)
                                    || category
                                        .DishCategory
                                        .CategoryFullName
                                        .ToLower()
                                        .Contains(searchTerm)
                                    || category
                                        .DishCategory
                                        .CategoryName
                                        .ToLower()
                                        .Contains(searchTerm)
                                )
                                    ? category.Id
                                    : -1,
                            (to, category) => category.Cafe
                        )
                        .Where(o => o.IsActive == true && o.IsDeleted == false);

                    queryToGetCafeFromDishes =
                        fc.TagObject
                        .Where(
                            to =>
                            tags.Contains(to.TagId)
                            && (ObjectTypesEnum)to.ObjectTypeId ==
                                ObjectTypesEnum.Dish
                                && to.IsDeleted == false
                        )
                        .GroupBy(to => to.ObjectId)
                        .Where(o => o.Count() >= tags.Count)
                        .Join(
                            fc.Dishes,
                            to => to.Key,
                            dish =>
                                dish.IsActive == true
                                && dish.IsDeleted == false
                                && dish.VersionFrom <= DateTime.Now
                                && (
                                    dish.VersionTo == null
                                    || dish.VersionTo >= DateTime.Now
                                )
                                && (
                                    dish
                                        .DishName
                                        .ToLower()
                                        .Contains(searchTerm)
                                )
                                    ? dish.Id
                                    : -1,
                            (to, dish) => dish.DishCategoryLinks.Count > 0 ? dish.DishCategoryLinks.FirstOrDefault().CafeCategory.Cafe : null
                        )
                        .Where(o => o.IsActive == true && o.IsDeleted == false);
                    #endregion
                }
                else
                {
                    #region SearchConsiderOnlyWithSearchTerm
                    queryToGetCafes =
                        from cafe in fc.Cafes
                        where cafe.IsActive == true
                            && (
                                cafe
                                    .CafeFullName
                                    .ToLower()
                                    .Contains(searchTerm)
                                || cafe
                                    .CafeName
                                    .ToLower()
                                    .Contains(searchTerm)
                                || cafe
                                    .CafeShortDescription
                                    .ToLower()
                                    .Contains(searchTerm)
                        )
                        select cafe;

                    queryToGetCafesFromCategories =
                        from category in fc.DishCategoriesInCafes
                        where (
                            category.IsActive == true
                            && (
                                category
                                    .DishCategory
                                    .Description
                                    .ToLower()
                                    .Contains(searchTerm)
                                || category
                                    .DishCategory
                                    .CategoryFullName
                                    .ToLower()
                                    .Contains(searchTerm)
                                || category
                                    .DishCategory
                                    .CategoryName
                                    .ToLower()
                                    .Contains(searchTerm)
                            )
                        )
                        select category.Cafe;

                    queryToGetCafeFromDishes =
                        from dish in fc.Dishes
                        where (
                            dish.IsActive == true
                            && dish.VersionFrom <= DateTime.Now
                            && (
                                dish.VersionTo == null
                                || dish.VersionTo >= DateTime.Now
                            )
                            && (
                                dish
                                    .DishName
                                    .ToLower()
                                    .Contains(searchTerm)
                            )
                        )
                        select dish.DishCategoryLinks.FirstOrDefault().CafeCategory.Cafe;
                    #endregion
                }

                List<Cafe> cafes = queryToGetCafes.ToList();

                cafes = cafes
                            .Concat(queryToGetCafesFromCategories)
                            .Concat(queryToGetCafeFromDishes)
                            .GroupBy(c => c.Id)
                            .Select(group => group.FirstOrDefault())
                            .OrderBy(c => c.CafeName)
                            .ToList();

                return cafes;
            }

        }


        /// <summary>
        /// Возвращает категории по списку тегов и входной строке
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="cafeId"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public virtual List<DishCategoryInCafe> GetCategorysBySearchTermAndTagListAndCafeId(List<Int64> tags, Int64 cafeId, string searchTerm)
        {
            var fc = GetContext();

            IQueryable<DishCategoryInCafe> queryToGetCategories;

            if (tags != null && tags.Count > 0)
            {
                #region SearchConsiderWithTagsAndSearchTerm
                queryToGetCategories =
                fc.TagObject
                .Where(
                    to =>
                    tags.Contains(to.TagId)
                    && (ObjectTypesEnum)to.ObjectTypeId ==
                        ObjectTypesEnum.Category
                    && to.IsDeleted == false
                )
                .GroupBy(to => to.ObjectId)
                .Select(to => to.FirstOrDefault())
                .Join(
                    fc.DishCategoriesInCafes,
                    to => to.ObjectId,
                    category =>
                        category.IsActive == true
                        && category.IsDeleted == false
                        && (
                            category
                                .DishCategory
                                .Description
                                .ToLower()
                                .Contains(searchTerm)
                            || category
                                .DishCategory
                                .CategoryFullName
                                .ToLower()
                                .Contains(searchTerm)
                            || category
                                .DishCategory
                                .CategoryName
                                .ToLower()
                                .Contains(searchTerm)
                        )
                            ? category.DishCategoryId
                            : -1,
                    (to, category) => category
                )
                .Where(cc => cc.CafeId == cafeId && cc.IsActive == true && cc.IsDeleted == false);

                #endregion
            }
            else
            {
                #region SearchConsiderOnlyWithSearchTerm
                queryToGetCategories =
                    from cafeCategory in fc.DishCategoriesInCafes
                    where (
                        cafeCategory.IsActive == true
                        && cafeCategory.IsDeleted == false
                        && cafeCategory.CafeId == cafeId
                        && (
                                cafeCategory
                                    .DishCategory
                                    .Description
                                    .ToLower()
                                    .Contains(searchTerm)
                                || cafeCategory
                                    .DishCategory
                                    .CategoryFullName
                                    .ToLower()
                                    .Contains(searchTerm)
                                || cafeCategory
                                    .DishCategory
                                    .CategoryName
                                    .ToLower()
                                    .Contains(searchTerm)
                        )
                    )
                    select cafeCategory;
                #endregion
            }

            queryToGetCategories = queryToGetCategories.GroupBy(cc => cc.DishCategoryId).Select(cc => cc.FirstOrDefault());

            return queryToGetCategories.ToList();
        }

        public virtual List<Dish> GetDishesBySearchTermAndTagListAndCafeId(List<Int64> tags, Int64 cafeId, string searchTerm)
        {
            var fc = GetContext();

            IQueryable<Dish> queryToGetDishes;

            if (tags != null && tags.Count > 0)
            {
                #region SearchConsiderWithTagsAndSearchTerm
                var queryDishes =
                    fc.TagObject
                    .Where(
                        to =>
                        tags.Contains(to.TagId)
                        && (ObjectTypesEnum)to.ObjectTypeId ==
                            ObjectTypesEnum.Dish
                        && to.IsDeleted == false
                    )
                    .GroupBy(to => to.ObjectId)
                    .Select(to => to.FirstOrDefault())
                    .Join(
                        fc.Dishes,
                        to => to.ObjectId,
                        dish =>
                            dish.IsActive == true
                            && dish.IsDeleted == false
                            && dish.VersionFrom <= DateTime.Now
                            && (
                                dish.VersionTo == null
                                || dish.VersionTo >= DateTime.Now
                            )
                            && (
                                dish
                                .DishName
                                .ToLower()
                                .Contains(searchTerm)
                            )
                                ? dish.Id
                                : -1,
                        (to, dish) => dish
                    )
                    .GroupBy(c => c.Id)
                    .Select(group => group.FirstOrDefault());

                var queryCafe = fc.DishCategoriesInCafes
                    .Where(cc => cc.CafeId == cafeId && cc.IsActive == true && cc.IsDeleted == false);

                queryToGetDishes = queryDishes.Where(
                    d => d.DishCategoryLinks
                    .Where(l => l.IsActive == true && !l.IsDeleted)
                    .Select(l => l.CafeCategory.Id)
                    .Any(i => queryCafe.Select(c => c.Id).Contains(i)));

                #endregion
            }
            else
            {
                #region SearchConsiderOnlyWithSearchTerm
                queryToGetDishes =
                    from d in fc.Dishes
                    where (
                        d.DishCategoryLinks.FirstOrDefault().CafeCategory.CafeId == cafeId
                        && d.IsActive == true
                        && d.IsDeleted == false
                        && d.VersionFrom <= DateTime.Now
                        && (
                            d.VersionTo == null
                            || d.VersionTo >= DateTime.Now
                        )
                        && (
                            d
                                .DishName
                                .ToLower()
                                .Contains(searchTerm)
                        )
                    )
                    select d;
                #endregion
            }

            return queryToGetDishes.ToList();
        }
        #endregion
    }
}
