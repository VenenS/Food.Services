using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Food.Data.Entities;
using Food.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Xml.Serialization;
using ITWebNet.FoodService.Food.Data.Accessor.Extensions;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region Category

        /// <summary>
        /// Возвращает категорию по идентификатору
        /// </summary>
        /// <param name="id">идентификатор категории</param>
        /// <returns></returns>
        public virtual DishCategory GetFoodCategoryById(long id)
        {
            if (!_categoryCache.TryGetValue("FullListOfCategories", out var cached))
            {
                return GetFoodCategories().FirstOrDefault(c => c.Id == id);
            }
            else
            {
                return ((List<DishCategory>)cached).FirstOrDefault(c => c.Id == id);
            }
        }

        /// <summary>
        /// Возвращает список всех активных категорий
        /// </summary>
        /// <returns></returns>
        public virtual List<DishCategory> GetFoodCategories()
        {
            List<DishCategory> categories;

            if (!_categoryCache.TryGetValue("FullListOfCategories", out var cached))
            {
                var fc = GetContext();

                categories = fc.DishCategories.AsNoTracking().Where(c => c.IsActive.Value).ToList();

                SetObjectInCache(
                    _categoryCache,
                    categories,
                    "FullListOfCategories"
                    );
            }
            else
            {
                categories = (List<DishCategory>)cached;
            }

            return categories;
        }

        //TODO: добавить в фейковую БД возможность вызова транзакции
        /// <summary>
        /// Получение категории с блюдами
        /// </summary>
        /// <param name="wantedDate"></param>
        /// <param name="cafeId"></param>
        /// <returns></returns>
        public virtual Dictionary<DishCategory, List<Dish>> GetFoodCategoriesForManager(DateTime? wantedDate, long cafeId)
        {
            var result = new Dictionary<DishCategory, List<Dish>>();
            using (var fc = GetContext())
            {
                //using (var transaction = fc.Database.BeginTransaction())
                {
                    var allCategories = fc.DishCategories.AsNoTracking().Where(e => e.IsActive == true);

                    var queryDishes = fc.Dishes
                        .Include(d => d.DishCategoryLinks)
                        .ThenInclude(l => l.CafeCategory)
                        .ThenInclude(c => c.Cafe)
                        .Where(d => d.DishCategoryLinks.Any(
                            l => l.CafeCategory.Cafe.Id == cafeId
                            && !l.IsDeleted
                            && l.IsActive == true
                            && !l.CafeCategory.IsDeleted
                            && l.CafeCategory.IsActive == true
                            )
                        && (d.VersionFrom <= DateTime.Now || d.VersionFrom == null)
                        && (d.VersionTo >= DateTime.Now || d.VersionTo == null)
                        && d.IsActive == true
                        && !d.IsDeleted);

                    if (wantedDate != null)
                    {
                        queryDishes = queryDishes.Where(d =>
                                d.VersionFrom <= wantedDate
                                &&
                                (
                                    d.VersionTo == null
                                    || d.VersionTo >= wantedDate
                                )
                            );
                    }

                    var dishes = queryDishes.ToList();

                    var cafeCategories = fc.DishCategoriesInCafes.AsNoTracking().Where(
                        c => c.CafeId == cafeId
                        && c.IsDeleted == false
                        && c.IsActive == true
                        && c.DishCategory.IsActive == true)
                        .ToList();

                    //transaction.Commit();

                    foreach (var item in allCategories)
                    {
                        var cafeCategory = cafeCategories.FirstOrDefault(c => c.DishCategoryId == item.Id);

                        if (cafeCategory == null)
                        {
                            item.Index = -1;
                            result.Add(item, null);
                        }
                        else
                        {
                            item.Index = cafeCategory.Index;
                            result.Add(item, dishes.Where(c => c.DishCategoryLinks.Any(
                                l => l.CafeCategory.DishCategoryId == item.Id
                                && l.IsActive == true
                                && !l.IsDeleted))
                                .OrderBy(d => d.DishCategoryLinks.FirstOrDefault(
                                    l => l.IsActive == true
                                    && !l.IsDeleted
                                    && l.CafeCategory.DishCategoryId == item.Id)
                                ?.DishIndex).ThenBy(d => d.Id).ToList());
                        }
                    }

                    return result;
                }
            }
        }

        //TODO: добавить в фейковую БД возможность вызова транзакции
        /// <summary>
        /// Получение катгории с блюдами из таблицы dish_version
        /// </summary>
        /// <param name="wantedDate"></param>
        /// <param name="cafeId"></param>
        /// <returns></returns>
        public virtual Dictionary<DishCategory, List<DishVersion>> GetFoodCategoriesVersionForManager(DateTime? wantedDate,
            long cafeId)
        {
            var result = new Dictionary<DishCategory, List<DishVersion>>();
            using (var fc = GetContext())
            {
                //using (var transaction = fc.Database.BeginTransaction())
                {
                    var allCategories = fc.DishCategories.AsNoTracking().Where(c => c.IsActive == true && !c.IsDeleted);

                    var dishes = fc.DishVersions.AsNoTracking().Where(d =>
                            d.CafeCategory.CafeId == cafeId
                            && (d.VersionFrom <= wantedDate || d.VersionFrom == null)
                            && (d.VersionTo >= wantedDate || d.VersionTo == null)
                            && d.IsActive == true
                            && d.IsDeleted == false
                            && !d.Dish.IsDeleted
                            && d.Dish.IsActive == true
                        ).ToList();

                    var cafeCategories = fc.DishCategoriesInCafes.AsNoTracking().Where(
                        c => c.CafeId == cafeId
                        && c.IsDeleted == false
                        && c.IsActive == true
                        && c.DishCategory.IsActive == true).ToList();

                    //transaction.Commit();

                    foreach (var item in allCategories)
                    {
                        var cafeCategory = cafeCategories.FirstOrDefault(c => c.DishCategoryId == item.Id);

                        if (cafeCategory == null)
                        {
                            item.Index = -1;
                            result.Add(item, null);
                        }
                        else
                        {
                            item.Index = cafeCategory.Index;
                            result.Add(item, dishes.Where(c => c.CafeCategoryId == cafeCategory.Id).ToList());
                        }
                    }

                    return result;
                }
            }
        }

        /// <summary>
        /// Возвращает список всех активных категорий кафе
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <returns></returns>
        public virtual List<DishCategoryInCafe> GetFoodCategoriesByCafeId(long cafeId)
        {
            List<DishCategoryInCafe> cafeCategories;

            if (!_categoryCache.TryGetValue("ListOfCafeCategories" + cafeId.ToString(), out var cached))
            {
                var fc = GetContext();

                var query = fc.DishCategoriesInCafes.AsNoTracking()
                    .Include(c => c.DishCategory)
                    .Where(c => c.CafeId == cafeId
                    && c.IsDeleted == false
                    && c.IsActive.Value
                    && c.DishCategory.IsActive.Value);

                cafeCategories = query.OrderBy(o => o.Index).GroupBy(e => e.DishCategoryId).Select(e => e.FirstOrDefault()).ToList();

                SetObjectInCache(
                    _categoryCache,
                    cafeCategories,
                    "ListOfCafeCategories"
                    + cafeId.ToString()
                    );
            }
            else
            {
                cafeCategories = (List<DishCategoryInCafe>)cached;
            }

            return cafeCategories;
        }

        /// <summary>
        /// Создает связь кафе-категория
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="categoryId">идентификатор категории</param>
        /// <param name="categoryIndex">приоритет категории</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        public virtual Int64 AddCafeFoodCategory(long cafeId, long categoryId, int categoryIndex, long userId)
        {
            long result;

            try
            {
                using (var fc = GetContext())
                {
                    var category =
                        fc.DishCategoriesInCafes.FirstOrDefault(
                            c => c.CafeId == cafeId
                            && c.DishCategoryId == categoryId
                            && c.IsDeleted == false);

                    if (category != null)
                    {
                        if (category.IsActive != true)
                        {
                            //Если новый индекс больше старого
                            if (categoryIndex > category.Index)
                            {
                                //Сдвигаем вверх на 10 все категории между старым индексом и новым индексом включительно
                                ShiftCategories(cafeId, categoryId,
                                    categoryIndex, (int)category.Index, true);
                            }
                            else
                            {
                                ShiftCategories(cafeId, categoryId, categoryIndex, -1, false);
                            }

                            category.IsActive = true;
                            category.LastUpdDate = DateTime.Now;
                            category.LastUpdateByUserId = userId;
                            category.Index = categoryIndex;

                            fc.SaveChanges();
                        }

                        result = category.Id;
                    }
                    else
                    {
                        ShiftCategories(cafeId, categoryId, categoryIndex, -1, false);

                        DishCategoryInCafe cafeCategory = new DishCategoryInCafe()
                        {
                            CafeId = cafeId,
                            DishCategoryId = categoryId,
                            IsActive = true,
                            Index = categoryIndex,
                            CreateDate = DateTime.Now,
                            CreatorId = userId,
                            IsDeleted = false
                        };

                        fc.DishCategoriesInCafes.Add(cafeCategory);
                        fc.SaveChanges();

                        var allCafeCategory =
                            fc.DishCategoriesInCafes.Where(
                                c => c.CafeId == cafeId
                                && c.IsDeleted == false)
                            .OrderBy(c => c.Index)
                            .ToList();

                        for (int i = 0; i < allCafeCategory.Count(); i++)
                        {
                            allCafeCategory[i].Index = (i + 1) * 10;
                        }

                        fc.SaveChanges();

                        result = cafeCategory.Id;
                    }
                }
            }
            catch (Exception)
            {
                return -1;
            }

            return result;
        }

        /// <summary>
        /// Меняем приоритет у категории для кафе
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="categoryId">идентификатор категории</param>
        /// <param name="categoryIndex">назначаемый приоритет</param>
        /// <param name="currentIndex">текущий приоритет</param>
        /// <param name="shiftUp">сдвиг вверх</param>
        public virtual void ShiftCategories(
            long cafeId,
            long categoryId,
            int categoryIndex,
            int currentIndex,
            bool shiftUp
        )
        {
            try
            {
                using (var fc = GetContext())
                {
                    var allCafeCategory =
                        from c in fc.DishCategoriesInCafes
                        where c.CafeId == cafeId
                            && c.IsDeleted == false
                        orderby c.Index ascending
                        select c;

                    if (shiftUp && currentIndex != -1)
                    {
                        var activeCategories =
                            allCafeCategory.Where(
                                c => c.DishCategoryId != categoryId
                                && c.Index > currentIndex
                                && c.Index <= categoryIndex)
                            .ToList();

                        for (int i = 0; i < activeCategories.Count(); i++)
                        {
                            activeCategories[i].Index =
                                (currentIndex - 10) + (i + 1) * 10;
                        }
                    }
                    else
                    {
                        var activeCategories =
                            allCafeCategory.Where(
                                c => c.DishCategoryId != categoryId
                                && c.Index >= categoryIndex)
                            .ToList();

                        for (int i = 0; i < activeCategories.Count(); i++)
                        {
                            activeCategories[i].Index =
                                categoryIndex + (i + 1) * 10;
                        }
                    }

                    fc.SaveChanges();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// Удаляем привязку категории к кафе
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="categoryId">идентификатор категории</param>
        /// <param name="userId"></param>
        public virtual bool RemoveCafeFoodCategory(long cafeId, long categoryId, long userId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    // Список категорий с DishCategoryId == categoryId в кафе с cafeId
                    var categories = fc.DishCategoriesInCafes.Where(
                            c => c.CafeId == cafeId
                            && c.DishCategoryId == categoryId
                            && c.IsActive == true
                            && c.IsDeleted == false
                            ).ToList();
                    // Блюда с активной привязкой к categoryId в cafeId
                    var dishes = fc.Dishes.Where(
                        d => !d.IsDeleted
                        && d.IsActive == true
                        && d.DishCategoryLinks.Any(
                            l => l.IsActive == true
                            && !l.IsDeleted
                            && l.CafeCategory.DishCategoryId == categoryId
                            && l.CafeCategory.CafeId == cafeId
                            )
                        ).ToList();
                    foreach (var dish in dishes)
                    {
                        // Деактивация привязки блюда к категории
                        dish.DishCategoryLinks.Where(
                            l => l.IsActive == true
                            && !l.IsDeleted
                            && l.CafeCategory.DishCategoryId == categoryId)
                            .ToList().ForEach(
                            l =>
                            {
                                l.IsActive = false;
                                l.LastUpdateByUserId = userId;
                                l.LastUpdDate = DateTime.Now;
                            });
                        // Если у блюда нет активных привязок к другим категориям
                        if (!dish.DishCategoryLinks.Any(l => l.IsActive == true && !l.IsDeleted && l.CafeCategory.DishCategoryId != categoryId))
                        {
                            var deleteCategory = GetDeletedCategoryIdByCafeId(cafeId, userId);
                            // Создает привязку к категории "Блюда удаленных категорий"
                            fc.DishCategoryLinks.Add(new DishCategoryLink
                            {
                                CafeCategoryId = deleteCategory,
                                CreateDate = DateTime.Now,
                                CreatorId = userId,
                                DishId = dish.Id,
                                IsActive = true,
                            });
                        }
                    }
                    // Деактивирует привязки к категориям кафе с categoryId
                    foreach (var category in categories)
                    {
                        category.IsActive = false;
                        category.LastUpdDate = DateTime.Now;
                        category.LastUpdateByUserId = userId;
                    }

                    fc.SaveChanges();

                    //обновляем индексы категорий
                    UpdateIndexCategories(cafeId);

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Меняем приоритет категории
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="categoryId">идентификтаор категории</param>
        /// <param name="categoryOrder">приоритет категории</param>
        /// <param name="userId">идентификатор пользователя</param>
        public virtual void ChangeFoodCategoryOrder(long cafeId, long categoryId, long categoryOrder, long userId)
        {
            try
            {
                var fc = GetContext();

                var link = fc.DishCategoriesInCafes.FirstOrDefault(c =>
                    c.CafeId == cafeId && c.DishCategoryId == categoryId && c.IsActive == true &&
                    !c.IsDeleted);

                if (link == null)
                    return;

                var maxIndex =
                    fc.DishCategoriesInCafes.Count(c => c.CafeId == cafeId) * 10;

                var newIndex = link.Index + categoryOrder * 10;

                if (newIndex < 10)
                {
                    newIndex = 10;
                }
                else
                {
                    if (newIndex > maxIndex)
                        newIndex = maxIndex;
                }

                var allCategories = fc.DishCategories.AsNoTracking().Where(e => e.IsActive == true);

                var cafeCategories = fc.DishCategoriesInCafes.Where(
                    c => c.CafeId == cafeId
                    && c.IsDeleted == false
                    && c.IsActive == true
                    && c.DishCategory.IsActive == true)
                    .ToList();

                var catList = new List<long>();
                foreach (var item in allCategories)
                {
                    var cafeCategory = cafeCategories.FirstOrDefault(c => c.DishCategoryId == item.Id);
                    if (cafeCategory != null)
                        catList.Add(cafeCategory.Id);
                }
                var categories = fc.DishCategoriesInCafes.AsNoTracking().Where(
                    c => c.CafeId == cafeId
                    && !c.IsDeleted
                    && c.IsActive == true
                    && c.DishCategory.IsActive == true
                    && catList.Contains(c.Id)
                    && c.DishCategoryId != -1)
                    .OrderBy(c => c.Index).ThenBy(c => c.DishCategoryId).ToList();

                var linkIndexOf = categories.IndexOf(categories.First(c => c.Id == link.Id));

                if (categoryOrder != 0)
                {
                    if (categoryOrder > 0)
                    {
                        for (int i = 0; i < categoryOrder; i++)
                        {
                            var temp = categories[linkIndexOf + i + 1];
                            categories[linkIndexOf + i + 1] = categories[linkIndexOf + i];
                            categories[linkIndexOf + i] = temp;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < -categoryOrder; i++)
                        {
                            var temp = categories[linkIndexOf - i - 1];
                            categories[linkIndexOf - i - 1] = categories[linkIndexOf - i];
                            categories[linkIndexOf - i] = temp;
                        }
                    }
                    for (var i = 0; i < categories.Count; i++)
                    {
                        var c = categories[i];
                        cafeCategories.Where(d => d.DishCategoryId == c.DishCategoryId)
                            .ToList()
                            .ForEach(d =>
                            {
                                d.Index = (i + 1) * 10;
                                d.LastUpdateByUserId = userId;
                                d.LastUpdDate = DateTime.Now;
                            });
                    }
                }

                fc.SaveChanges();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Обновить индексацию категорий у кафе
        /// </summary>
        /// <param name="cafeId">Идентификатор кафе</param>
        public virtual void UpdateIndexCategories(long cafeId)
        {
            using (var fc = GetContext())
            {
                var allCafeCategory = fc.DishCategoriesInCafes.Where(
                    c => c.CafeId == cafeId
                    && c.IsDeleted == false
                    ).OrderByDescending(c => c.IsActive)
                    .ThenBy(c => c.Index)
                    .ToList();

                for (int i = 0; i < allCafeCategory.Count; i++)
                    allCafeCategory[i].Index = 10 + i * 10;

                fc.SaveChanges();
            }
        }

        /// <summary>
        /// Перемещение блюд из категории в категорию "Блюда удаленных категорий" c id=-1 для всех кафе если нет других привязок
        /// </summary>
        public virtual void MoveDishesFromCategory(long categoryId, long userId)
        {
            const long foodcategoryDeleted = -1;//Блюда удаленных категорий
            using (var fc = GetContext())
            {
                var cafesLinks = fc.DishCategoriesInCafes.AsNoTracking().Where(
                    o => (o.DishCategoryId == -1 || o.DishCategoryId == categoryId)
                    && !o.IsDeleted).GroupBy(o => o.CafeId).ToList();

                foreach (var itemGroup in cafesLinks)
                {
                    var cafeCategorySource = itemGroup.FirstOrDefault(o => o.DishCategoryId == categoryId); //категория, из которой переносим в -1
                    var cafeCategoryDeleted = itemGroup.FirstOrDefault(o => o.DishCategoryId == foodcategoryDeleted);
                    if (cafeCategorySource != null && cafeCategoryDeleted != null)
                    {
                        var dishes = fc.Dishes.Where(o => o.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted)
                        .Select(d => d.CafeCategory).Any(c => c.Id == cafeCategorySource.Id)).ToList();
                        foreach (var dish in dishes)
                        {
                            var links = fc.DishCategoryLinks.Where(
                                l => l.IsActive == true && !l.IsDeleted && l.DishId == dish.Id);
                            // Если нет активных привязок к категориям кроме categoryId и удаленных
                            if (!links.Any(l => l.CafeCategory.DishCategoryId != categoryId
                            && l.CafeCategory.DishCategoryId != foodcategoryDeleted))
                            {
                                // Связываем блюдо с категорией удаленных блюд текущего кафе
                                fc.DishCategoryLinks.Add(new DishCategoryLink
                                {
                                    CafeCategoryId = cafeCategoryDeleted.Id,
                                    CreateDate = DateTime.Now,
                                    CreatorId = userId,
                                    DishId = dish.Id,
                                    IsDeleted = false,
                                    IsActive = true
                                });
                            }

                            var dishCategoryLinks = fc.DishCategoryLinks.Where(
                                l => l.DishId == dish.Id
                                && l.CafeCategoryId == cafeCategorySource.Id
                                && l.CafeCategoryId != cafeCategoryDeleted.Id
                                && !l.IsDeleted
                                && l.IsActive == true);

                            // Удаляем привязки к категории
                            foreach (var link in dishCategoryLinks)
                            {
                                link.IsActive = false;
                                link.IsDeleted = true;
                                link.LastUpdateByUserId = userId;
                                link.LastUpdDate = DateTime.Now;
                            }
                        }
                    }
                }
                fc.SaveChanges();
            }
        }

        /// <summary>
        /// Получает рабочее время категории.
        /// </summary>
        /// <param name="cafeId">Идентификатор кафе.</param>
        /// <param name="categoryId">Идентификатор категории.</param>
        /// <returns>Рабочее время категории.</returns>
        public virtual BusinessHours GetCategoryBusinessHours(long cafeId, long categoryId)
        {
            using (var fc = GetContext())
            {
                var dishCategoriesInCafes = fc.DishCategoriesInCafes.AsNoTracking().FirstOrDefault(
                    e => e.CafeId == cafeId
                    && e.DishCategoryId == categoryId
                    && e.IsDeleted == false);

                return dishCategoriesInCafes?.WorkingHours ?? new BusinessHours();
            }
        }

        /// <summary>
        /// Задает рабочее время категории.
        /// </summary>
        /// <param name="businessHours">Рабочее время категории.</param>
        /// <param name="cafeId">Идентификатор кафе.</param>
        /// <param name="categoryId">Идентификатор категории.</param>
        public virtual void SetCategoryBusinessHours(BusinessHours businessHours, long cafeId, long categoryId)
        {
            using (var fc = GetContext())
            {
                var dishCategoriesInCafes = fc.DishCategoriesInCafes.FirstOrDefault(
                    e => e.CafeId == cafeId
                    && e.DishCategoryId == categoryId
                    && e.IsDeleted == false);

                if (dishCategoriesInCafes == null) return;

                using (StringWriter writer = new StringWriter())
                {
                    XmlSerializer serialiser = new XmlSerializer(typeof(BusinessHours));
                    serialiser.Serialize(writer, businessHours);

                    dishCategoriesInCafes.OrderHours = writer.ToString();
                }

                fc.SaveChanges();
            }
        }

        public virtual long GetDeletedCategoryIdByCafeId(long cafeId, long userId)
        {
            var fc = GetContext();
            if (!fc.DishCategoriesInCafes.Any(
                c => !c.IsDeleted
                && c.IsActive == true
                && c.DishCategory.CategoryName.Trim().ToLower() == SystemDishCategories.CategoryForDeletedDishes
                && c.CafeId == cafeId))
            {
                var deleted = fc.DishCategories.FirstOrDefault(
                    c => c.CategoryName.Trim().ToLower() == SystemDishCategories.CategoryForDeletedDishes).Id;
                fc.DishCategoriesInCafes.Add(new DishCategoryInCafe
                {
                    CafeId = cafeId,
                    DishCategoryId = deleted,
                    CreateDate = DateTime.Now,
                    CreatorId = userId,
                    IsActive = true,
                });
                fc.SaveChanges();
            }
            return fc.DishCategoriesInCafes.FirstOrDefault(
                c => !c.IsDeleted
                && c.IsActive == true
                && c.DishCategory.CategoryName.Trim().ToLower() == SystemDishCategories.CategoryForDeletedDishes
                && c.CafeId == cafeId).Id;
        }
        #endregion
    }
}
