using System;
using System.Collections.Generic;
using System.Linq;
using Food.Data.Entities;
using Food.Data;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region TagObject

        /// <summary>
        /// Добавить связь объект-тег
        /// </summary>
        /// <param name="tagObject"></param>
        /// <returns></returns>
        public virtual bool AddTagObject(TagObject tagObject)
        {
            try
            {
                using (var fc = GetContext())
                {
                    TagObject oldTagObject = fc.TagObject.FirstOrDefault(
                        to => to.ObjectId == tagObject.ObjectId
                        && to.ObjectTypeId == tagObject.ObjectTypeId
                        && to.TagId == tagObject.TagId
                        && to.IsDeleted == false
                    );

                    if (oldTagObject == null)
                    {
                        fc.TagObject.Add(tagObject);

                        fc.SaveChanges();
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Редактировать свзяь объект-тег
        /// </summary>
        /// <param name="tagObject"></param>
        /// <returns></returns>
        public virtual bool EditTagObject(TagObject tagObject)
        {
            try
            {
                using (var fc = GetContext())
                {
                    TagObject oldTagObject = fc.TagObject.FirstOrDefault(
                        t => t.Id == tagObject.Id
                    );

                    TagObject sameTagObject = fc.TagObject.FirstOrDefault(
                        to => to.ObjectId == tagObject.ObjectId
                        && to.ObjectTypeId == tagObject.ObjectTypeId
                        && to.TagId == tagObject.TagId
                        && to.IsDeleted == false
                    );

                    if (oldTagObject != null
                        && sameTagObject == null)
                    {
                        oldTagObject.LastUpdDate = DateTime.Now;
                        oldTagObject.LastUpdateByUserId = tagObject.LastUpdateByUserId;
                        oldTagObject.ObjectId = tagObject.ObjectId;
                        oldTagObject.ObjectTypeId = tagObject.ObjectTypeId;
                        oldTagObject.TagId = tagObject.TagId;

                        fc.SaveChanges();
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Удалить связь объект-тег
        /// </summary>
        /// <param name="tagObjectId">идентификатор связи</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        public virtual bool RemoveTagObject(long tagObjectId, long userId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    TagObject oldTagObject = fc.TagObject.FirstOrDefault(
                        t => t.Id == tagObjectId && t.IsDeleted == false
                        );

                    if (oldTagObject != null)
                    {
                        oldTagObject.IsDeleted = true;
                        fc.SaveChanges();
                    }

                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Возвращает свзяь объект-тег
        /// </summary>
        /// <param name="objectId">идентификатор объекта</param>
        /// <param name="objectType">тип объекта</param>
        /// <param name="tagId">идентификатор тега</param>
        /// <returns></returns>
        public virtual TagObject GetTagObjectById(long objectId, int objectType, long tagId)
        {
            using (var fc = GetContext())
            {
                TagObject tagObject = fc.TagObject.AsNoTracking().FirstOrDefault(
                    t => t.ObjectId == objectId
                    && t.ObjectTypeId == objectType
                    && t.TagId == tagId
                    && t.IsDeleted == false
                );

                return tagObject;
            }
        }

        /// <summary>
        /// Вернуть объект по связи тег-объект
        /// </summary>
        /// <param name="tagObject"></param>
        /// <returns></returns>
        public virtual object GetObjectFromTagObject(TagObject tagObject)
        {
            using (var fc = GetContext())
            {

                switch (tagObject.ObjectTypeId)
                {
                    case (int)ObjectTypesEnum.Cafe:
                        return
                            (
                                from c in fc.Cafes.AsNoTracking()
                                where tagObject.ObjectId == c.Id && tagObject.IsDeleted == false
                                select c
                                ).FirstOrDefault();

                    case (int)ObjectTypesEnum.Dish:
                        var dishFromBase =
                            (
                                from d in fc.Dishes.Include(i => i.DishCategoryLinks).AsNoTracking()
                                where tagObject.ObjectId == d.Id && tagObject.IsDeleted == false
                                select d
                        ).FirstOrDefault();
                        if (dishFromBase != null)
                        {
                            Dish dish = dishFromBase;
                            return dish;
                        }
                        else
                            return null;

                    default:
                        return null;

                }
            }
        }

        /// <summary>
        /// Возвращает список кафе по списку тегов
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public virtual List<Cafe> GetCafesByTagList(List<long> tags)
        {
            using (var fc = GetContext())
            {
                var cafes =
                    fc.TagObject
                    .AsNoTracking()
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
                     .Where(cafe => cafe.IsActive == true && cafe.IsDeleted == false)
                     .OrderBy(c => c.CafeName)
                     .ToList();

                return cafes;
            }
            #region предыдущий
            //using (var fc = GetContext())
            //{
            //    var queryToGetCafes =
            //        fc.TagObject
            //        .Where(
            //            to =>
            //                tags.Contains(to.TagId)
            //                && (ObjectTypesEnum) to.ObjectTypeId ==
            //                ObjectTypesEnum.CAFE
            //                && to.IsDeleted == false
            //            )
            //        .GroupBy(to => to.ObjectId)
            //        .Where(o => o.Count() >= tags.Count)
            //        .Join(
            //            fc.Cafes,
            //            to => to.Key,
            //            cafe => cafe.Id,
            //            (to, cafe) => cafe
            //        )
            //        .Where(o => o.IsActive == true && o.IsDeleted == false);

            //    List<Cafe_m> cafes = queryToGetCafes.ToList();

            //    var queryToGetCafesFromCategories =
            //        fc.TagObject
            //            .Where(
            //                to =>
            //                    tags.Contains(to.TagId)
            //                    && (ObjectTypesEnum) to.ObjectTypeId ==
            //                    ObjectTypesEnum.CATEGORY
            //                    && to.IsDeleted == false
            //            )
            //            .GroupBy(to => to.ObjectId)
            //            .Where(o => o.Count() >= tags.Count)
            //            .Join(
            //                fc.CafeCategores,
            //                to => to.Key,
            //                category =>
            //                    category.IsActive == true && category.IsDeleted == false
            //                        ? category.Id
            //                        : -1,
            //                (to, category) => category.Cafe
            //            )
            //            .Where(o => o.IsActive == true && o.IsDeleted == false);

            //    var queryToGetCafeFromDishes =
            //        fc.TagObject
            //        .Where(
            //            to =>
            //            tags.Contains(to.TagId)
            //            && (ObjectTypesEnum)to.ObjectTypeId ==
            //                ObjectTypesEnum.DISH
            //        )
            //        .GroupBy(to => to.ObjectId)
            //        .Where(o => o.Count() >= tags.Count)
            //        .Join(
            //            fc.Dishes,
            //            to => to.Key,
            //            dish =>
            //                dish.IsActive == true
            //                && dish.IsDeleted == false
            //                && dish.VersionFrom <= DateTime.Now
            //                && (
            //                    dish.VersionTo == null
            //                    || dish.VersionTo >= DateTime.Now
            //                )
            //                    ? dish.Id
            //                    : -1,
            //            (to, dish) => dish.CafeCategory.Cafe
            //        )
            //        .Where(o => o.IsActive == true && o.IsDeleted == false);

            //    cafes = cafes
            //                .Concat(queryToGetCafesFromCategories)
            //                .Concat(queryToGetCafeFromDishes)
            //                .GroupBy(c => c.Id)
            //                .Select(group => group.FirstOrDefault())
            //                .ToList();

            //    return cafes;
            //}
            #endregion
        }

        /// <summary>
        /// Возвращает список категорий по списку тегов
        /// </summary>
        /// <param name="tags">список тегов</param>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <returns></returns>
        public virtual List<DishCategoryInCafe> GetCategorysByTagListAndCafeId(List<long> tags, long cafeId)
        {
            var fc = GetContext();

            var queryToGetCategories =
                fc.TagObject
                .AsNoTracking()
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
                        category.IsActive == true && category.IsDeleted == false
                            ? category.DishCategoryId
                            : -1,
                    (to, category) => category
                )
                .Where(cc => cc.CafeId == cafeId && cc.IsActive == true && cc.IsDeleted == false)
                .GroupBy(cc => cc.DishCategoryId).Select(cc => cc.FirstOrDefault())
                .ToList();

            return queryToGetCategories;

            #region предыдущий
            //var fc = GetContext();

            //var queryToGetCategories =
            //    fc.TagObject
            //    .Where(
            //        to =>
            //        tags.Contains(to.TagId)
            //        && (ObjectTypesEnum)to.ObjectTypeId ==
            //            ObjectTypesEnum.CATEGORY
            //            && to.IsDeleted == false
            //    )
            //    .GroupBy(to => to.ObjectId)
            //    .Where(o => o.Count() >= tags.Count)
            //    .Join(
            //        fc.CafeCategores,
            //        to => to.Key,
            //        category =>
            //            category.IsActive == true && category.IsDeleted == false
            //                ? category.Id
            //                : -1,
            //        (to, category) => category
            //    );

            //var queryToGetCategoriesFromDishes =
            //    fc.TagObject
            //    .Where(
            //        to =>
            //        tags.Contains(to.TagId)
            //        && (ObjectTypesEnum)to.ObjectTypeId ==
            //            ObjectTypesEnum.DISH
            //            && to.IsDeleted == false
            //    )
            //    .GroupBy(to => to.ObjectId)
            //    .Where(o => o.Count() >= tags.Count)
            //    .Join(
            //        fc.Dishes,
            //        to => to.Key,
            //        dish =>
            //            dish.IsActive == true
            //            && dish.IsDeleted == false
            //            && dish.VersionFrom <= DateTime.Now
            //            && (
            //                dish.VersionTo == null
            //                || dish.VersionTo >= DateTime.Now
            //            )
            //                ? dish.Id
            //                : -1,
            //        (to, dish) => dish.CafeCategory
            //    )
            //    .Where(category => category.IsActive == true && category.IsDeleted == false);

            //List<CafeCategory_m> categories =
            //    queryToGetCategories
            //        .Concat(queryToGetCategoriesFromDishes)
            //        .GroupBy(c => c.Id)
            //        .Select(group => group.FirstOrDefault())
            //        .ToList();

            //return categories;
            #endregion
        }

        /// <summary>
        /// Возвращает список блюд по списку тегов
        /// </summary>
        /// <param name="tags">список тегов</param>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <returns></returns>
        public virtual List<Dish> GetDishesByTagListAndCafeId(List<long> tags, long cafeId)
        {
            var fc = GetContext();

            var queryDishes =
                fc.TagObject
                .AsNoTracking()
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
                            ? dish.Id
                            : -1,
                    (to, dish) => dish
                )
                .ToList();

            var queryCafe = fc.DishCategoriesInCafes
                .AsNoTracking()
                .Where(cc => cc.CafeId == cafeId && cc.IsActive == true && cc.IsDeleted == false);

            var query = queryDishes.Where(
                d => d.DishCategoryLinks
                .Where(
                    l => l.IsActive == true 
                    && !l.IsDeleted)
                .Select(l => l.CafeCategory.Id)
                .Any(i => queryCafe.Select(c => c.Id).Contains(i)))
                .ToList();

            return query;

            #region предыдущий
            //var fc = GetContext();

            //var query =
            //    fc.TagObject
            //    .Where(
            //        to =>
            //        tags.Contains(to.TagId)
            //        && (ObjectTypesEnum)to.ObjectTypeId ==
            //            ObjectTypesEnum.DISH
            //        && to.IsDeleted == false
            //    )
            //    .GroupBy(to => to.ObjectId)
            //    .Where(o => o.Count() >= tags.Count)
            //    .Join(
            //        fc.Dishes,
            //        to => to.Key,
            //        dish =>
            //            dish.IsActive == true
            //            && dish.IsDeleted == false
            //            && dish.VersionFrom <= DateTime.Now
            //            && (
            //                dish.VersionTo == null
            //                || dish.VersionTo >= DateTime.Now
            //            )
            //                ? dish.Id
            //                : -1,
            //        (to, dish) => dish
            //    )
            //    .GroupBy(c => c.Id)
            //    .Select(group => group.FirstOrDefault())
            //    .ToList();

            //return query;
            #endregion

        }

        /// <summary>
        /// Возвращает список тегов по списку кафе
        /// </summary>
        /// <param name="cafesList"></param>
        /// <returns></returns>
        public virtual List<Tag> GetTagsByCafesList(List<long> cafesList)
        {
            List<Tag> tags = new List<Tag>();

            using (var fc = GetContext())
            {
                if (cafesList != null
                    && cafesList.Count > 0
                    )
                {
                    tags =
                        (
                            from tagObject in fc.TagObject.AsNoTracking()
                            where cafesList.Contains(tagObject.ObjectId)
                                  && tagObject.ObjectTypeId == (int)ObjectTypesEnum.Cafe
                                  && tagObject.IsDeleted == false
                            select tagObject.Tag
                            ).GroupBy(t => t.Id).Select(group => group.FirstOrDefault()).ToList();
                }
            }

            return tags;
        }

        #endregion
    }

}