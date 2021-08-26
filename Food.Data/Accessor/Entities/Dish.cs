using Food.Data.Accessor.Extensions;
using Food.Data.Entities;
using ITWebNet.FoodService.Food.Data.Accessor.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region Dish

        /// <summary>
        /// Проверяет уникальное ли имя блюда в рамках кафе
        /// </summary>
        public virtual bool CheckUniqueNameWithinCafe(string dishName, long cafeId, long dishId = -1)
        {
            using (var fc = GetContext())
            {
                bool dishExists;
                dishName = dishName.Trim().ToLower();
                if (dishId <= 0)
                    dishExists = fc.Dishes.AsNoTracking().Any(
                        o => o.DishName.Trim().ToLower() == dishName
                        && !o.IsDeleted
                        && o.DishCategoryLinks.Any(c => c.CafeCategory.CafeId == cafeId && !c.IsDeleted && c.IsActive == true));
                else
                    dishExists = fc.Dishes.AsNoTracking().Any(
                        o => o.Id != dishId && o.DishName.Trim().ToLower() == dishName
                        && !o.IsDeleted
                        && o.DishCategoryLinks.Any(c => c.CafeCategory.CafeId == cafeId && !c.IsDeleted && c.IsActive == true));

                return !dishExists;
            }
        }

        /// <summary>
        /// Возвращает список блюд указанной категории
        /// </summary>
        /// <param name="foodCategoryId">идентификатор категории</param>
        /// <returns></returns>
        public virtual List<Dish> GetFoodDishesByCategoryId(long foodCategoryId)
        {
            List<Dish> dishes;

            if (!_dishCache.TryGetValue($"FoodDishByCategoryId-{foodCategoryId}", out var cached))
            {
                var fc = GetContext();

                dishes = fc.Dishes.AsNoTracking().Where(fd =>
                fd.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted)
                .Select(d => d.CafeCategory).Any(
                    c => c.Id == foodCategoryId
                    && !c.IsDeleted
                    && c.IsActive == true)
                && fd.IsActive == true
                && fd.IsDeleted == false
                ).ToList();

                SetObjectInCache(
                    _dishCache,
                    dishes,
                    $"FoodDishByCategoryId-{foodCategoryId}"
                );
            }
            else
            {
                dishes = (List<Dish>)cached;
            }
            return dishes;
        }

        /// <summary>
        /// Возвращает список блюд кафе на указанную дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="wantedDate">дата (опционально)</param>
        /// <returns></returns>
        public virtual List<Dish> GetFoodDishesByCafeId(long cafeId, DateTime? wantedDate = null)
        {
            List<Dish> dishes;

            if (!_dishCache.TryGetValue($"FoodDishByCafeId-{cafeId}-{wantedDate}", out var cached))
            {
                var fc = GetContext();

                var query = fc.Dishes.AsNoTracking().Where(d =>
                d.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted)
                .Select(l => l.CafeCategory).Any(
                    c => c.CafeId == cafeId
                    && c.IsDeleted == false
                    && c.IsActive == true
                    && c.DishCategory.IsDeleted == false
                    && c.DishCategory.IsActive == true)
                && d.IsActive == true
                && d.IsDeleted == false);

                if (wantedDate != null)
                    query = query.Where(d => d.VersionFrom <= wantedDate && (d.VersionTo == null || d.VersionTo >= wantedDate));
                dishes = query.ToList();

                SetObjectInCache(
                    _dishCache,
                    dishes,
                    $"FoodDishByCafeId-{cafeId}-{wantedDate}"
                );
            }
            else
            {
                dishes = (List<Dish>)cached;
            }
            return dishes;
        }

        /// <summary>
        /// Возвращает снапшот всех блюд кафе, которые были активны на указанную дату,
        /// сгруппированые по категориям блюд.
        /// </summary>
        /// <param name="cafeId">Идентификатор кафе</param>
        /// <param name="snapshotDate"></param>
        /// <remarks>Внимание: возвращает блюда только существующих категорий. Если на указанную дату
        /// существовали блюда, но связанная категория была удалена - эта категория и связанные с ней
        /// блюда не будут включены в результат.</remarks>
        public virtual IEnumerable<Tuple<DishCategory, IEnumerable<Dish>>> GroupDishesByCategory(long cafeId, DateTime snapshotDate)
        {
            var fc = GetContext();
            var dishes =
                from d in fc.Dishes
                where d.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted)
                .Select(l => l.CafeCategory).Any(c => c.CafeId == cafeId
                        && c.IsDeleted == false
                        && c.IsActive == true
                        && c.DishCategory.IsDeleted == false
                        && c.DishCategory.IsActive == true)
                        && (
                            (d.VersionFrom == null || d.VersionFrom <= snapshotDate) &&
                            (d.VersionTo == null || snapshotDate <= d.VersionTo)
                        )
                        && d.IsDeleted == false
                        && d.IsActive == true
                select d;

            var categoryIds = new HashSet<long>();
            foreach (var item in dishes)
            {
                // Выбираем ИД категорий кафе из списка
                categoryIds.Union(item.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted)
                    .Select(d => d.CafeCategory).Where(c => c.CafeId == cafeId).Select(c => c.DishCategoryId));
            }

            var categories = fc.DishCategories.AsNoTracking().Where(c => categoryIds.Contains(c.Id)).OrderBy(c => c.Id).ToList();

            var res = new List<Tuple<DishCategory, IEnumerable<Dish>>>();
            for(var i = 0; i < categories.Count; i++)
            {
                var category = categories[0];
                // Выбирает блюда, у которых есть активная привязка к текущей категории
                var dishList = dishes.Where(
                    d => d.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted)
                    .Select(l => l.CafeCategory.DishCategoryId).Contains(category.Id));
                // Сортирует блюда по индексу 
                dishList = dishList.OrderBy(d => (d.DishCategoryLinks.FirstOrDefault(
                    l => l.IsActive == true
                    && !l.IsDeleted
                    && l.CafeCategory.DishCategoryId == category.Id)
                ?? new DishCategoryLink())
                .DishIndex);
                res.Add(Tuple.Create(category, dishList.AsEnumerable()));
            }

            return res;
        }

        /// <summary>
        /// Возвращает список блюд для кафе в указанной категории на дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="categoryId">идентификатор категории</param>
        /// <param name="date">дата</param>
        /// <returns></returns>
        public virtual List<Dish> GetFoodDishesByCategoryIdAndCafeId(Int64 cafeId, Int64 categoryId, DateTime date)
        {
            List<Dish> dishes;

            if (!_dishCache.TryGetValue($"FoodDishByCategoryIdAndCafeId-{cafeId}-{categoryId}-{date}", out var cached))
            {
                var fc = GetContext();

                var query = fc.Dishes
                    .Include(d=>d.DishCategoryLinks)
                    .ThenInclude(l=>l.CafeCategory)
                    .AsNoTracking()
                    .Where(d => d.DishCategoryLinks
                    .Where(l => l.IsActive == true && !l.IsDeleted)
                    .Select(l => l.CafeCategory).Any(
                        c => c.CafeId == cafeId
                        && c.IsDeleted == false
                        && c.IsActive == true
                        && c.DishCategoryId == categoryId
                        && c.DishCategory.IsDeleted == false
                        && c.DishCategory.IsActive == true)
                    && (d.VersionFrom <= date || d.VersionFrom == null)
                    && (d.VersionTo >= date || d.VersionTo == null)
                    && d.IsActive == true
                    && d.IsDeleted == false)
                    .OrderBy(d => d.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted)
                    .FirstOrDefault(c => c.CafeCategory.DishCategoryId == categoryId).DishIndex);

                dishes = query.ToList();

                SetObjectInCache(
                    _dishCache,
                    dishes,
                    $"FoodDishByCategoryIdAndCafeId-{cafeId}-{categoryId}-{date}"
                );
            }
            else
            {
                dishes = (List<Dish>)cached;
            }
            return dishes;
        }

        /// <summary>
        /// Возвращает список блюд по идентификтаорам
        /// </summary>
        /// <param name="dishId">список идентификаторов</param>
        /// <returns></returns>
        public virtual List<Dish> GetFoodDishesById(long[] dishId)
        {
            var fc = GetContext();

            var query = fc.Dishes.Where(
                            d => dishId.Contains(d.Id)
                            && d.IsActive == true
                            && d.IsDeleted == false
                        );

            return query.ToList();
        }

        /// <summary>
        /// Возвращает блюдо по идентификтаору
        /// </summary>
        /// <param name="dishId"></param>
        /// <returns></returns>
        public virtual Dish GetFoodDishById(long dishId)
        {
            var fc = GetContext();
            var query = fc.Dishes.FirstOrDefault(d => d.Id == dishId);
            return query;
        }

        /// <summary>
        /// Возвращает версионное блюдо
        /// </summary>
        /// <param name="dishId">идентификатор блюда</param>
        /// <returns></returns>
        public virtual List<DishVersion> GetFoodDishVersionsById(long dishId)
        {
            var fc = GetContext();

            var query = fc.DishVersions.AsNoTracking().Where(
                            d => d.DishId == dishId
                            && d.IsActive == true
                            && d.IsDeleted == false
                        );

            return query.ToList();
        }

        /// <summary>
        /// Добавляет блюдо
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="categoryIds">идентификаторы категорий</param>
        /// <param name="dish">блюдо (сущность)</param>
        /// <param name="userId">идентификтаор пользователя</param>
        /// <returns></returns>
        public virtual long AddDish(long cafeId, long[] categoryIds, Dish dish, long userId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    // Если указана хотя бы одна категория
                    if (categoryIds != null && categoryIds.Length > 0
                        // И если хотя бы одна из этих категорий привязана к кафе
                        && fc.DishCategoriesInCafes.Any(d => categoryIds.Contains(d.DishCategoryId)))
                    {
                        // Добавляем блюда
                        dish.ImageId = dish.ImageId ?? string.Empty;
                        dish.IsDeleted = false;
                        dish.CreationDate = DateTime.Now;
                        dish.CreatorId = userId;
                        fc.Dishes.Add(dish);
                        fc.SaveChanges();

                        for (var i = 0; i < categoryIds.Length; i++)
                        {
                            var cafeCategory =
                                fc.DishCategoriesInCafes.FirstOrDefault(
                                    c => c.CafeId == cafeId
                                    && c.DishCategoryId == categoryIds[i]
                                    && c.IsActive == true
                                    && c.IsDeleted == false);
                            if (cafeCategory != null)
                            {
                                // Максимальный индекс в пределах категории
                                var maxDishIndex = fc.DishCategoryLinks.Where(
                                    d => !d.IsDeleted
                                    && d.IsActive == true
                                    && d.CafeCategoryId == cafeCategory.Id
                                    ).Select(d => d.DishIndex).Max();

                                // Создаем привязку к каждой категории из categoryIds
                                fc.DishCategoryLinks.Add(new DishCategoryLink
                                {
                                    CafeCategoryId = cafeCategory.Id,
                                    CreateDate = DateTime.Now,
                                    CreatorId = userId,
                                    DishId = dish.Id,
                                    DishIndex = maxDishIndex == null ? 0 : maxDishIndex + 1,
                                    IsDeleted = false,
                                    IsActive = true
                                });
                                fc.SaveChanges();
                            }
                        }
                        return dish.Id;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Редактирует блюдо
        /// </summary>
        /// <param name="dish">блюдо</param>
        /// <param name="categoryIds">Обновленный список категорий</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <returns></returns>
        public virtual long EditDish(Dish dish, long[] categoryIds, long userId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    // Проверка на активность не нужна
                    Dish oldDish = fc.Dishes.FirstOrDefault(d => d.Id == dish.Id && d.IsDeleted == false);

                    if (oldDish != null)
                    {
                        DishVersion dishVersion = new DishVersion()
                        {
                            DishId = dish.Id,
                            CafeCategoryId = 0,
                            DishName = dish.DishName,
                            DishIndex = 0,
                            Kcalories = dish.Kcalories,
                            Weight = dish.Weight,
                            ImageId = dish.ImageId,
                            BasePrice = dish.BasePrice,
                            LastUpdateByUserId = dish.LastUpdateByUserId,
                            LastUpdDate = DateTime.Now,
                            IsActive = true
                        };

                        DishVersion oldDishVersion =
                            fc.DishVersions
                                .Where(
                                    d => d.IsActive == true
                                         && d.IsDeleted == false
                                         && d.Id == dish.Id)
                                .OrderByDescending(
                                    d => d.VersionTo)
                                .FirstOrDefault();

                        if (oldDishVersion != null)
                        {
                            dishVersion.VersionFrom =
                                oldDishVersion.VersionTo.HasValue
                                    ? oldDishVersion.VersionTo
                                    : dish.VersionFrom.HasValue
                                        ? dish.VersionFrom.Value.AddSeconds(1)
                                        : new DateTime(2000, 1, 1).AddSeconds(1);
                            dishVersion.VersionTo = DateTime.Now.AddSeconds(-1);
                        }
                        else
                        {
                            dishVersion.VersionFrom = oldDish.VersionFrom;
                            dishVersion.VersionTo = DateTime.Now.AddSeconds(-1);
                        }

                        fc.DishVersions.Add(dishVersion);

                        oldDish.DishName = dish.DishName;
                        oldDish.Kcalories = dish.Kcalories;
                        oldDish.Weight = dish.Weight;
                        oldDish.WeightDescription = dish.WeightDescription;
                        oldDish.ImageId = dish.ImageId;
                        oldDish.VersionFrom = DateTime.Now;
                        oldDish.VersionTo = DateTime.Now.AddYears(100);
                        oldDish.BasePrice = dish.BasePrice;
                        oldDish.LastUpdDate = DateTime.Now;
                        oldDish.LastUpdateByUserId = userId;
                        oldDish.Description = dish.Description;
                        oldDish.Composition = dish.Composition;

                        // Деавктивируем связи с категориями не указаными в categoryIds
                        fc.DishCategoryLinks.Where(
                            l => l.IsActive == true
                            && !l.IsDeleted
                            && !categoryIds.Contains(l.CafeCategory.DishCategoryId)
                            && l.DishId == oldDish.Id)
                            .ToList().ForEach(l =>
                            {
                                l.IsActive = false;
                                l.LastUpdDate = DateTime.Now;
                                l.LastUpdateByUserId = userId;
                            });

                        for (var i = 0; i < categoryIds.Length; i++)
                        {
                            var cafeCategory = fc.DishCategoriesInCafes.FirstOrDefault(
                                c => c.DishCategoryId == categoryIds[i]
                                && c.CafeId == dish.DishCategoryLinks.FirstOrDefault().CafeCategory.CafeId
                                && !c.IsDeleted
                                && c.IsActive == true);

                            // Если нет активной привязки блюда к категории добавляем привязку
                            if (cafeCategory != null
                                && !fc.DishCategoryLinks.Any(
                                    l => l.DishId == oldDish.Id
                                    && l.CafeCategory.DishCategoryId == categoryIds[i]
                                    && l.CafeCategory.CafeId == dish.DishCategoryLinks.FirstOrDefault().CafeCategory.CafeId
                                    && l.IsActive == true
                                    && !l.IsDeleted))
                            {
                                // Максимальный индекс в пределах категории
                                var maxDishIndex = fc.DishCategoryLinks.Where(
                                    d => !d.IsDeleted
                                    && d.IsActive == true
                                    && d.CafeCategoryId == cafeCategory.Id
                                    ).Select(d => d.DishIndex).Max();

                                fc.DishCategoryLinks.Add(new DishCategoryLink
                                {
                                    CafeCategoryId = cafeCategory.Id,
                                    CreateDate = DateTime.Now,
                                    CreatorId = userId,
                                    DishId = oldDish.Id,
                                    DishIndex = maxDishIndex == null ? 0 : maxDishIndex + 1,
                                    IsDeleted = false,
                                    IsActive = true
                                });
                            }
                        }

                        fc.SaveChanges();

                        return oldDish.Id;
                    }

                    // Если блюда нет, довабляем
                    return AddDish(dish.DishCategoryLinks.FirstOrDefault().CafeCategory.CafeId, categoryIds, dish, userId);
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Удаляет блюдо
        /// </summary>
        /// <param name="dishId">идентификатор блюда</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        public virtual long RemoveDish(long dishId, long userId, long? oldCategoryId = null)
        {
            try
            {
                using (var fc = GetContext())
                {
                    Dish dish =
                        fc.Dishes
                        .Include(d => d.DishCategoryLinks)
                        .FirstOrDefault(
                            d => d.Id == dishId
                                 && d.IsDeleted == false
                        );

                    if (dish != null)
                    {
                        // Не понимаю в чем смысл данной строки, если зараннее известен идентификатор категории
                        //long categoryId = fc.DishCategories.FirstOrDefault(c => c.Id == -2 && c.CategoryName.Trim().ToLower() == SystemDishCategories.DeletedDishesCategory).Id;
                        long categoryId = -2;

                        //dish.IsDeleted = true;
                        dish.LastUpdateByUserId = userId;
                        dish.LastUpdDate = DateTime.Now;
                        if (dish.DishCategoryLinks.Count(l => !l.IsDeleted && l.IsActive == true) == 1 || oldCategoryId is null)
                        {
                            //убираем блюдо из расписания
                            var queryShedule = (from s in fc.DishesInMenus
                                                where s.DishId == dish.Id
                                                      && s.IsActive == true
                                                      && s.IsDeleted == false
                                                      && s.Type != "E"
                                                select s).ToList();

                            foreach (DishInMenu s in queryShedule)
                            {
                                s.Type = "E";
                            }

                            #region ПеремещаетВКатегориюУдаленныеБлюда

                            long cafeId = dish.DishCategoryLinks.FirstOrDefault().CafeCategory.CafeId;
                            if (categoryId != 0 && cafeId != 0)
                            {
                                var resCc = fc.DishCategoriesInCafes.FirstOrDefault(
                                    cc => cc.CafeId == cafeId
                                          && cc.DishCategoryId == categoryId
                                          && cc.IsActive == true
                                          && cc.IsDeleted == false);
                                long categoryDeletedId;
                                if (resCc == null)
                                {
                                    categoryDeletedId = AddCafeFoodCategory(cafeId, categoryId, 0, userId);
                                }
                                else
                                {
                                    categoryDeletedId = resCc.Id;
                                }
                                fc.DishCategoryLinks.Add(new DishCategoryLink
                                {
                                    CafeCategoryId = categoryDeletedId,
                                    CreateDate = DateTime.Now,
                                    CreatorId = userId,
                                    DishId = dishId,
                                    IsActive = true,
                                    IsDeleted = false
                                });
                            }

                            #endregion
                        }

                        if (oldCategoryId != null)
                        {
                            // Удаляет активные привязки к категории
                            foreach (var link in fc.DishCategoryLinks.Where(
                                l => l.CafeCategory.DishCategoryId == oldCategoryId
                                && l.IsActive == true
                                && !l.IsDeleted
                                && l.CafeCategory.IsActive == true
                                && !l.CafeCategory.IsDeleted
                                && l.DishId == dish.Id))
                            {
                                link.IsActive = false;
                                link.LastUpdateByUserId = userId;
                                link.LastUpdDate = DateTime.Now;
                            }
                        }
                        else
                        {
                            // Деактивирует привязки кроме категории удаленных блюд
                            var links = fc.DishCategoryLinks.Where(
                                l => l.IsActive == true
                                && l.DishId == dish.Id
                                && l.CafeCategory.DishCategoryId != categoryId).ToList();
                            foreach (var link in links)
                            {
                                link.IsActive = false;
                                //link.IsDeleted = true;
                                link.LastUpdateByUserId = userId;
                                link.LastUpdDate = DateTime.Now;
                            }
                        }

                        fc.SaveChanges();

                        return dish.Id;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Изменяет порядок блюд в категории от newIndex до oldIndex
        /// </summary>
        /// <param name="cafeId">Идентификатор кафе</param>
        /// <param name="categoryId">Идентификатор категории</param>
        /// <param name="newIndex">Новый индекс блюда</param>
        /// <param name="oldIndex">Старый индекс блюда</param>
        public virtual void ChangeDishIndex(long cafeId, long categoryId, int newIndex, int oldIndex, int? dishId = null)
        {
            using (var fc = GetContext())
            {
                var lstDishes = fc.DishCategoryLinks.AsNoTracking()
                    .Include(d => d.CafeCategory)
                    .Include(l => l.Dish)
                    .Where(
                    l => l.CafeCategory.DishCategoryId == categoryId
                    && l.CafeCategory.CafeId == cafeId
                    && l.IsActive == true
                    && !l.IsDeleted
                    && l.Dish.IsActive == true
                    && !l.Dish.IsDeleted)
                    .OrderBy(l => l.DishIndex)
                    .ThenBy(l => l.Dish.Id)
                    .ToList();

                var linkIndexOf = lstDishes.IndexOf(lstDishes.FirstOrDefault(d => d.Dish.Id == dishId));

                var dishOrder = newIndex - oldIndex;

                if (dishOrder != 0 && oldIndex >= 0 && linkIndexOf != -1)
                {
                    if (dishOrder > 0)
                    {
                        for (int i = 0; i < dishOrder; i++)
                        {
                            var temp = lstDishes[linkIndexOf + i + 1];
                            lstDishes[linkIndexOf + i + 1] = lstDishes[linkIndexOf + i];
                            lstDishes[linkIndexOf + i] = temp;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < -dishOrder; i++)
                        {
                            var temp = lstDishes[linkIndexOf - i - 1];
                            lstDishes[linkIndexOf - i - 1] = lstDishes[linkIndexOf - i];
                            lstDishes[linkIndexOf - i] = temp;
                        }
                    }
                }
                for (var i = 0; i < lstDishes.Count; i++)
                {
                    var item = lstDishes[i];
                    if (fc.DishCategoryLinks.FirstOrDefault(d => !d.IsDeleted && d.IsActive == true && d.DishId == item.DishId
                    && d.CafeCategory.DishCategoryId == categoryId) is DishCategoryLink link)
                    {
                        link.DishIndex = i;
                    }
                }

                #region old
                //List<Dish> lstDishes = GetFoodCategoriesForManager(null, cafeId)
                //    .FirstOrDefault(pair => pair.Key.Id == categoryId).Value
                //    .OrderBy(c => GetDishIndexInCategory(c.Id, categoryId))
                //    .ThenBy(c => c.Id).ToList();

                //var linkIndexOf = lstDishes.IndexOf(lstDishes.First(d => d.Id == dishId));

                //var dishOrder = newIndex - oldIndex;

                //if (dishOrder != 0 && oldIndex >= 0)
                //{
                //    if (dishOrder > 0)
                //    {
                //        for (int i = 0; i < dishOrder; i++)
                //        {
                //            var temp = lstDishes[linkIndexOf + i + 1];
                //            lstDishes[linkIndexOf + i + 1] = lstDishes[linkIndexOf + i];
                //            lstDishes[linkIndexOf + i] = temp;
                //        }
                //    }
                //    else
                //    {
                //        for (int i = 0; i < -dishOrder; i++)
                //        {
                //            var temp = lstDishes[linkIndexOf - i - 1];
                //            lstDishes[linkIndexOf - i - 1] = lstDishes[linkIndexOf - i];
                //            lstDishes[linkIndexOf - i] = temp;
                //        }
                //    }
                //}
                //for (var i = 0; i < lstDishes.Count; i++)
                //{
                //    var item = lstDishes[i];
                //    if (fc.DishCategoryLinks.FirstOrDefault(d => d.DishId == item.Id) is DishCategoryLink link)
                //    {
                //        link.DishIndex = i;
                //    }
                //    //if (fc.Dishes.FirstOrDefault(d => d.Id == item.Id) is Dish dish)
                //    //    dish.DishIndex = i;
                //}
                #endregion

                fc.SaveChanges();
            }
        }

        /// <summary>
        /// Обновляет индексы в категории, в которую перенесли блюдо
        /// </summary>
        /// <param name="cafeId">Идентификатор кафе</param>
        /// <param name="categoryId">Идентификатор категории</param>
        /// <param name="newIndex">Новый индекс блюда</param>
        /// <param name="dishId">Идентификатор блюда</param>
        public virtual void UpdateDishIndexInSecondCategory(long cafeId, long categoryId, int newIndex, long dishId, long userId)
        {
            using (var fc = GetContext())
            {
                var linksCount = fc.DishCategoryLinks.Count(
                    l => l.DishId == dishId
                    && l.CafeCategory.IsDeleted == false
                    && l.CafeCategory.IsActive == true
                    && (l.Dish.VersionFrom <= DateTime.Now || l.Dish.VersionFrom == null)
                    && (l.Dish.VersionTo >= DateTime.Now || l.Dish.VersionTo == null)
                    && !l.IsDeleted
                    && l.IsActive == true);
                if (!fc.DishCategoryLinks.Any(
                    l => l.DishId == dishId
                    && l.CafeCategory.DishCategoryId == categoryId
                    && l.CafeCategory.IsDeleted == false
                    && l.CafeCategory.IsActive == true
                    && (l.Dish.VersionFrom <= DateTime.Now || l.Dish.VersionFrom == null)
                    && (l.Dish.VersionTo >= DateTime.Now || l.Dish.VersionTo == null)
                    && !l.IsDeleted
                    && l.IsActive == true)
                    && ((!SystemDishCategories.GetSystemCtegoriesIds().Contains(categoryId)
                    && linksCount >= 1)
                    || linksCount == 0)) 
                {
                    var cafecategoryId = fc.DishCategoriesInCafes.FirstOrDefault(
                        c => !c.IsDeleted && c.IsActive == true && c.CafeId == cafeId && c.DishCategoryId == categoryId).Id;
                    fc.DishCategoryLinks.Add(new DishCategoryLink
                    {
                        CafeCategoryId = cafecategoryId,
                        CreateDate = DateTime.Now,
                        CreatorId = userId,
                        DishId = dishId,
                        IsActive = true,
                        IsDeleted = false,
                    });
                    fc.SaveChanges();

                    var lstDishes = fc.DishCategoryLinks
                        .Where(d => d.CafeCategory.CafeId == cafeId
                        && d.CafeCategory.DishCategoryId == categoryId
                        && !d.CafeCategory.IsDeleted
                        && d.CafeCategory.IsActive == true
                        && (d.Dish.VersionFrom <= DateTime.Now || d.Dish.VersionFrom == null)
                        && (d.Dish.VersionTo >= DateTime.Now || d.Dish.VersionTo == null)
                        && d.Dish.IsActive == true
                        && !d.Dish.IsDeleted
                        && !d.IsDeleted
                        && d.IsActive == true)
                        .OrderBy(d => d.DishIndex).ToList();
                    int iBegin;

                    if (lstDishes[newIndex].Id == dishId)
                        iBegin = newIndex + 1;
                    else
                    {
                        lstDishes[newIndex].DishIndex = newIndex + 1;
                        iBegin = newIndex + 2;
                        if (newIndex + 1 < lstDishes.Count)
                            lstDishes[newIndex + 1].DishIndex = newIndex;
                    }

                    for (; iBegin < lstDishes.Count; iBegin++)
                        lstDishes[iBegin].DishIndex = iBegin;
                    fc.SaveChanges();
                }

                #region old
                //List<Dish> lstDishes = (from d in fc.Dishes
                //                        where (
                //                            d.CafeCategory.CafeId == cafeId
                //                            && d.CafeCategory.DishCategoryId == categoryId
                //                            && d.CafeCategory.IsDeleted == false
                //                            && d.CafeCategory.IsActive == true
                //                            && (d.VersionFrom <= DateTime.Now || d.VersionFrom == null)
                //                            && (d.VersionTo >= DateTime.Now || d.VersionTo == null)
                //                            && d.IsActive == true
                //                            && d.IsDeleted == false
                //                        )
                //                        orderby d.DishIndex
                //                        select d).ToList();

                //int iBegin;
                //if (lstDishes[newIndex].Id == dishId)
                //    iBegin = newIndex + 1;
                //else
                //{
                //    lstDishes[newIndex].DishIndex = newIndex + 1;
                //    iBegin = newIndex + 2;
                //    lstDishes[newIndex + 1].DishIndex = newIndex;
                //}

                //for (; iBegin < lstDishes.Count; iBegin++)
                //    lstDishes[iBegin].DishIndex = iBegin;

                //fc.SaveChanges();
                #endregion
            }
        }

        /// <summary>
        /// Обновляет индексы в категории, из которой перенесли блюдо. И меняет категорию у блюда.
        /// </summary>
        /// <param name="cafeId">Идентификатор кафе</param>
        /// <param name="newCategoryId">Идентификатор категории, в которую перенесли</param>
        /// <param name="oldCategoryId">Идентификатор категории, из которой взяли блюдо</param>
        /// <param name="newIndex">Новый индекс блюда</param>
        /// <param name="oldIndex">Старый индекс блюда</param>
        /// <param name="dishId">Идентификатор блюда</param>
        public virtual void UpdateDishIndexInFirstCategory(long cafeId, long newCategoryId, long oldCategoryId, int newIndex,
            int oldIndex, long dishId, long userId)
        {
            using (var fc = GetContext())
            {
                var oldLink = fc.DishCategoryLinks.FirstOrDefault(
                    l => l.IsActive == true
                    && !l.IsDeleted
                    && l.DishId == dishId
                    && l.CafeCategory.DishCategoryId == oldCategoryId
                    && !l.CafeCategory.IsDeleted
                    && l.CafeCategory.IsActive == true);

                // Деактивируеn старую привязку
                if (oldLink != null)
                {
                    oldLink.IsActive = false;
                    oldLink.LastUpdateByUserId = userId;
                    oldLink.LastUpdDate = DateTime.Now;
                }
                fc.SaveChanges();

                var lstDishes = fc.DishCategoryLinks
                        .Where(d => d.CafeCategory.CafeId == cafeId
                        && d.CafeCategory.DishCategoryId == oldCategoryId
                        && !d.CafeCategory.IsDeleted
                        && d.CafeCategory.IsActive == true
                        && (d.Dish.VersionFrom <= DateTime.Now || d.Dish.VersionFrom == null)
                        && (d.Dish.VersionTo >= DateTime.Now || d.Dish.VersionTo == null)
                        && d.Dish.IsActive == true
                        && !d.Dish.IsDeleted
                        && !d.IsDeleted
                        && d.IsActive == true)
                        .OrderBy(d => d.DishIndex).ToList();
                for (int i = 0; i < lstDishes.Count; i++)
                    lstDishes[i].DishIndex = i;
                fc.SaveChanges();

                #region old
                ////ищем блюдо и меняем ему категорию
                //var dish = fc.Dishes.FirstOrDefault(d =>
                //    d.Id == dishId
                //    && d.IsActive == true
                //    && d.IsDeleted == false
                //);
                //if (dish != null)
                //{
                //    //ищем категорию, в которую перемещают
                //    var dishCategory = fc.DishCategoriesInCafes.FirstOrDefault(cc =>
                //        cc.CafeId == cafeId
                //        && cc.IsActive == true
                //        && cc.IsDeleted == false
                //        && cc.DishCategoryId == newCategoryId
                //    );
                //    if (dishCategory != null)
                //    {
                //        dish.CategoryId = dishCategory.Id;
                //        dish.DishIndex = newIndex;
                //        fc.SaveChanges();
                //        if (oldIndex < 0) return;
                //    }
                //    else return;
                //}
                //else return;

                //List<Dish> lstDishes = (from d in fc.Dishes
                //                        where (
                //                            d.CafeCategory.CafeId == cafeId
                //                            && d.CafeCategory.DishCategoryId == oldCategoryId
                //                            && d.CafeCategory.IsDeleted == false
                //                            && d.CafeCategory.IsActive == true
                //                            && (d.VersionFrom <= DateTime.Now || d.VersionFrom == null)
                //                            && (d.VersionTo >= DateTime.Now || d.VersionTo == null)
                //                            && d.IsActive == true
                //                            && d.IsDeleted == false
                //                        )
                //                        orderby d.DishIndex
                //                        select d).ToList();

                //for (int i = 0; i < lstDishes.Count; i++)
                //    lstDishes[i].DishIndex = i;
                //fc.SaveChanges();
                #endregion
            }

        }

        public virtual Dictionary<DishCategory, List<Dish>> GetDishesByScheduleByDate(long cafeId, DateTime? date)
        {
            var scheduledDishList =
                new Dictionary<DishCategory, List<Dish>>();

            date =
                date == null
                || date == DateTime.MinValue
                || date == DateTime.MaxValue
                    ? DateTime.Now
                    : date;

            date = date.Value.Date;

            var currentSchedule = Instance.GetSchedulesForCafe(cafeId, date);

            var dishesId = new List<long>();
            foreach (var schedule in currentSchedule)
                dishesId.Add(schedule.DishId);

            var dishes = Instance.GetFoodDishesById(dishesId.ToArray());
            dishes = dishes.Where(
                d => (
                         d.VersionFrom <= date
                         || d.VersionFrom == null
                     )
                     && (
                         d.VersionTo >= date ||
                         d.VersionTo == null
                     )
            ).ToList();

            var dishesVersion =
                Instance.GetFoodDishVersionByCafeId(cafeId, (DateTime)date);

            foreach (var dish in dishes)
            {
                var existingDishes =
                    dishesVersion.Where(dv => dv.DishId == dish.Id).ToList();
                for (var i = 0; i < existingDishes.Count; i++)
                    dishesVersion.Remove(existingDishes[i]);
            }
            foreach (var dishVersionItem in dishesVersion)
                if (currentSchedule.FirstOrDefault(
                        s => s.DishId == dishVersionItem.DishId
                    ) != null)
                    dishes.Add(dishVersionItem.GetEntityFromDishVersion());

            foreach (var dish in dishes)
            {
                var existingScheduleForDishes =
                    currentSchedule.Where(s => s.DishId == dish.Id).ToList();

                var simpleSchedule =
                    existingScheduleForDishes.Find(s => s.Type == "S");

                if (simpleSchedule != null && simpleSchedule.Price != null)
                {
                    dish.BasePrice = (double)simpleSchedule.Price;
                }
                else
                {
                    var existingSchedule = existingScheduleForDishes.Find(s => s.Price != null);

                    if (existingSchedule != null)
                        dish.BasePrice = (double)existingSchedule.Price;
                }
            }

            foreach (var cafeCategoryItem in Instance.GetFoodCategoriesByCafeId(cafeId))
            {
                var foodCategoryItem = cafeCategoryItem.DishCategory;
                foodCategoryItem.Index = cafeCategoryItem.Index;

                var dishByScheduleAndCategory = new List<Dish>();
                foreach (
                    var dishItem
                    in dishes
                        .Where(
                        d => d.DishCategoryLinks
                        .Select(l => l.CafeCategory.DishCategoryId)
                        .Contains(cafeCategoryItem.DishCategoryId)
                        )
                )
                    dishByScheduleAndCategory.Add(dishItem);

                if (dishByScheduleAndCategory.Count > 0 && !scheduledDishList.ContainsKey(foodCategoryItem))
                    scheduledDishList.Add(
                        foodCategoryItem,
                        dishByScheduleAndCategory
                    );
            }
            return scheduledDishList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dishId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public virtual int? GetDishIndexInCategory(long dishId, long categoryId)
        {
            using (var fc = GetContext())
            {
                var dish = fc.Dishes
                    .Include(d => d.DishCategoryLinks)
                    .ThenInclude(l => l.CafeCategory)
                    .FirstOrDefault(d => d.Id == dishId);
                var index = dish.DishCategoryLinks.FirstOrDefault(
                    l => l.IsActive == true
                    && !l.IsDeleted
                    && l.CafeCategory.DishCategoryId == categoryId)
                    ?.DishIndex;
                return index;
            }
        }

        #endregion
    }
}