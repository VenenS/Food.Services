using System;
using System.Collections.Generic;
using System.Linq;
using Food.Data.Entities;
using Food.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using ITWebNet.FoodService.Food.Data.Accessor.Extensions;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region Schedule


        /// <summary>
        /// Возвращает определенное количество первых блюд в каждой категории, выбранных из запланированных меню
        /// для всех кафе на указанную дату (или на текущую дату, если date == null)
        /// </summary>
        /// <param name="cafeIds">Идентификаторы кафе, из которых можно заказывать блюда</param>
        /// <param name="date">дата</param>
        /// <param name="name">URL кафе, для которого нужно показать блюда</param>
        /// <param name="countDishes">Количество блюд, которое нужно взять</param>
        /// <param name="dishCatId">Категория, из которой надо отобразить блюда. Применяется в случае выбора фильтра по категориям.</param>
        /// <returns></returns>
        public async Task<List<First6DishesFromCat>> GetFirstDishesFromAllSchedules(long[] cafeIds, DateTime? date, string name = null, int? countDishes = 6, string filter = null, long[] cafeCategoryIds = null)
        {
            try
            {
                List<First6DishesFromCat> schedules;

                DateTime dateTime = date ?? DateTime.Today;

                dateTime = dateTime.Date;

                var fc = GetContext();

                var systemCategoriesIds = SystemDishCategories.GetSystemCtegoriesIds();

                string dayOfTheWeek = ((int)dateTime.DayOfWeek).ToString();
                string dayOfTheMonth = dateTime.Day.ToString();

                // Выбор запланированных блюд:
                var query = fc.DishesInMenus.Select(s => s);

                if (!string.IsNullOrEmpty(name))
                {
                    query = query.Where(s => s.Dish.Cafe.CleanUrlName == name);
                }

                if (!string.IsNullOrEmpty(filter))
                {
                    filter = filter.Trim().ToLower();
                    query = query.Where(s => s.Dish.DishName.ToLower().Contains(filter)
                    || s.Dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted).Any(
                        d => d.CafeCategory.DishCategory.CategoryName.ToLower().Contains(filter)));
                }

                // Конекретная категория не указана, надо выбирать все категории.
                // Исключаем системные категории, а также удалённые и неактивные категории:
                else
                {
                    // Выбирает блюда, у которых есть активные не системные категории
                    query = query.Where(s => s.Dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted)
                    .Select(d => d.CafeCategory).Any(
                        c => !systemCategoriesIds.Contains(c.DishCategoryId)
                        && !c.DishCategory.IsDeleted
                        && c.DishCategory.IsActive == true));
                }
                query = query
                    .Include(d => d.Dish)
                    .ThenInclude(d => d.DishCategoryLinks)
                    .ThenInclude(c => c.CafeCategory)
                    .ThenInclude(c => c.DishCategory)
                    .Include(d => d.Dish.DishCategoryLinks)
                    .ThenInclude(c => c.CafeCategory)
                    .ThenInclude(c => c.Cafe)
                    .Include(d => d.Dish.Schedules)
                    .ThenInclude(s => s.Creator)
                            .Where(
                               s => s.IsActive == true
                                   && s.IsDeleted == false
                                   && s.Type != "E"
                                   // Выбираем блюдо, если оно состоит в категории кафе из списка cafeIds
                                   && s.Dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted).Select(d => d.CafeCategory).Any(cc => cafeIds.Contains(cc.CafeId))
                                   //Эта строка не ломает логику.
                                   //У Dish приоритет выше.
                                   && s.Dish.IsActive == true
                                   && s.Dish.IsDeleted == false
                                   && (cafeCategoryIds == null || s.Dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted).Any(x => cafeCategoryIds.Contains(x.CafeCategoryId)))
                                   && s.Dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted).Select(d => d.CafeCategory).Any(cc => !cc.IsDeleted && cc.IsActive == true)
                                   && s.Dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted).Select(d => d.CafeCategory).Any(cc => !cc.DishCategory.IsDeleted && cc.DishCategory.IsActive == true)
                                   && (
                                        (
                                            s.BeginDate == null
                                            || s.BeginDate <= dateTime
                                        )
                                        && (
                                            s.EndDate == null
                                            || s.EndDate >= dateTime
                                        )
                                   )
                                   && (
                                        s.Type == "D"
                                        || (
                                            s.Type == "S"
                                            && (
                                                s.BeginDate == dateTime
                                                || s.OneDate == dateTime
                                            )
                                        )
                                   )
                            );

                var allDishes = query.AsNoTracking().ToList();
                var sTypeDishes = allDishes.Where(s =>
                            s.Type == "S"
                            && (
                                s.BeginDate == dateTime
                                || s.OneDate == dateTime
                            )
                        ).ToList();
                var dTypeDishes = allDishes
                            .Where(
                               s => s.Type == "D"
                            ).ToList();
                //Если у нас есть расписание с типом Simple, то Daily с этим же блюдом не берем
                foreach (var item in dTypeDishes)
                {
                    if (sTypeDishes.FirstOrDefault(d => d.DishId == item.DishId) == null)
                        sTypeDishes.Add(item);
                }
                sTypeDishes.ForEach(e => e.Dish.BasePrice = e.Price ?? 0);

                #region old
                //// Группировка
                //var top6cat = sTypeDishes.GroupBy(s => s.Dish.CafeCategories.Select(c => c.DishCategoryId))
                //// Выбор
                //.Select(g => new First6DishesFromCat()
                //{
                //    CategoryId = g.Key,
                //    CountOfDishes = g.Count(),
                //    Dishes = g.OrderBy(d => Guid.NewGuid()).Take(countDishes.HasValue? countDishes.Value: g.Count()).Select(d => d.Dish)
                //});

                //schedules = top6cat.ToList();
                #endregion

                // Список категорий в которых состоят блюда
                var categories = new List<DishCategoryInCafe>();
                foreach (var item in sTypeDishes)
                {
                    // Выбираем ИД категорий кафе из списка
                    categories.AddRange(item.Dish.DishCategoryLinks
                        .Where(l => l.IsActive == true && !l.IsDeleted && cafeIds.Contains(l.CafeCategory.CafeId))
                        .Select(c => c.CafeCategory));
                }
                // Исключаем системные категории и повторения. Сортируем
                var categoryIds = categories
                    .Where(i => !systemCategoriesIds.Contains(i.DishCategoryId))
                    .Select(i => i.DishCategoryId)
                    .Distinct().OrderBy(i => i).ToList();

                // Формируем список категорий
                schedules = categoryIds.Select(i =>
                {
                    var count = sTypeDishes.Count(s => s.Dish.DishCategoryLinks.Any(l => l.IsActive == true && !l.IsDeleted && l.CafeCategory.DishCategoryId == i));
                    return new First6DishesFromCat
                    {
                        CategoryId = i,
                        CountOfDishes = count,
                        // Выбираем блюда входящие в текущую категорию
                        Dishes = sTypeDishes.Where(s => s.Dish.DishCategoryLinks.Any(l => l.IsActive == true && !l.IsDeleted && l.CafeCategory.DishCategoryId == i))
                        .OrderBy(d => Guid.NewGuid()).Take(countDishes ?? count)
                    };
                }).ToList();

                return schedules;
            }
            catch (Exception)
            {
                return null;
            }
        }

        //TODO: Сделать 1н, т.к. GetFirstDishesFromAllSchedules точно такой же
        /// <summary>
        /// Возвращает список расписаний на дату для всех кафе, за
        /// исключением блюд, указанных в списке
        /// </summary>
        /// <param name="date">дата</param>
        /// <returns></returns>
        public async Task<List<Dish>> GetSchedulesRestOfCat(long[] cafeIds, DateTime? date, long[] cafeCategoryIds, IEnumerable<long> dishIds, string name = null)
        {

            DateTime dateTime = date ?? DateTime.Today;

            dateTime = dateTime.Date;

            var fc = GetContext();

            string dayOfTheWeek = ((int)dateTime.DayOfWeek).ToString();
            string dayOfTheMonth = dateTime.Day.ToString();

            // Получение запланированных блюд.
            var query = fc.DishesInMenus.Select(s => s);
            if (!string.IsNullOrEmpty(name))
            {
                var cafeId = GetCafeByUrl(name).Id;
                query = query.Where(s => s.Dish.Cafe.CleanUrlName == name);
            }

            HashSet<long> hsetDishIds = new HashSet<long>(dishIds);

            query = query
                            .Where(
                               s => !hsetDishIds.Contains(s.DishId)
                                   && s.IsActive == true
                                   && s.IsDeleted == false
                                   && s.Type != "E"
                                   // Выбираем блюдо, если оно состоит в категории кафе из списка cafeIds
                                   && s.Dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted).Select(l => l.CafeCategory).Any(cc => cafeIds.Contains(cc.CafeId))
                                   && s.Dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted).Select(l => l.CafeCategory).Any(cc => cafeCategoryIds.Contains(cc.Id))
                                   //&& cafeIds.Contains(s.Dish.CafeCategory.CafeId)
                                   //&& s.Dish.CafeCategory.DishCategoryId == categoryId
                                   && s.Dish.IsActive == true
                                   && s.Dish.IsDeleted == false
                                   && s.Dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted).Select(l => l.CafeCategory).Any(cc => !cc.IsDeleted && cc.IsActive == true)
                                   && s.Dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted).Select(l => l.CafeCategory).Any(cc => !cc.DishCategory.IsDeleted && cc.DishCategory.IsActive == true)
                                   && (
                                        (
                                            s.BeginDate <= dateTime
                                            || s.BeginDate == null
                                        )
                                        && (
                                            s.EndDate == null
                                            || s.EndDate >= dateTime
                                        )
                                   )
                                   && (
                                        s.Type == "D"
                                        || (
                                            s.Type == "S"
                                            && (
                                                s.BeginDate == dateTime
                                                || s.OneDate == dateTime
                                            )
                                        )
                                   )
                            );
            var allDishes = query.ToList();
            var sTypeDishes = allDishes.Where(s =>
                        s.Type == "S"
                        && (
                            s.BeginDate == dateTime
                            || s.OneDate == dateTime
                        )
                    ).ToList();
            var dTypeDishes = allDishes
                        .Where(
                           s => s.Type == "D"
                        ).ToList();
            //Если у нас есть расписание с типом Simple, то Daily с этим же блюдом не берем
            foreach (var item in dTypeDishes)
            {
                if (sTypeDishes.FirstOrDefault(d => d.DishId == item.DishId) == null)
                    sTypeDishes.Add(item);
            }
            sTypeDishes.ForEach(e => e.Dish.BasePrice = e.Price.Value);
            var dishes = sTypeDishes.Select(d => d.Dish).ToList();

            return dishes;

        }

        /// <summary>
        /// Возвращает расписание по идентификатору
        /// </summary>
        /// <param name="id">идентификатор</param>
        /// <returns></returns>
        public DishInMenu GetScheduleById(long id)
        {
            DishInMenu schedule;

            if (!_scheduleCache.TryGetValue($"ScheduleByID-{id}", out var cached))
            {
                var fc = GetContext();

                var query = from s in fc.DishesInMenus.AsNoTracking()
                            where (
                                s.Id == id
                                && s.IsActive == true
                                && s.IsDeleted == false
                            )
                            select s;

                schedule = query.FirstOrDefault();

                SetObjectInCache(
                    _scheduleCache,
                    schedule,
                    $"ScheduleByID-{id}");
            }
            else
            {
                schedule = (DishInMenu)cached;
            }

            return schedule;
        }

        /// <summary>
        /// Возвращает список расписаний блюда на дату
        /// </summary>
        /// <param name="date">дата</param>
        /// <param name="dishId">идентификатор блюда</param>
        /// <returns></returns>
        public List<DishInMenu> GetScheduleActiveByDate(DateTime date, long? dishId)
        {
            List<DishInMenu> schedules;

            if (!_scheduleCache.TryGetValue($"ScheduleByDate-{date}-{dishId}", out var cached))
            {
                var fc = GetContext();

                schedules = fc.DishesInMenus
                    .AsNoTracking()
                    .Where(x => x.IsActive == true && !x.IsDeleted
                             && (dishId == null || x.DishId == dishId)
                             && (x.OneDate == date.Date || (x.BeginDate <= date && (x.EndDate == null || x.EndDate >= date))))
                    .ToList();

                SetObjectInCache(
                    _scheduleCache,
                    schedules,
                    $"ScheduleByDate-{date}-{dishId}");
            }
            else
            {
                schedules = (List<DishInMenu>)cached;
            }
            return schedules;
        }

        /// <summary>
        /// Возващает список распианий блюда на период
        /// </summary>
        /// <param name="startPeriod">начало периода</param>
        /// <param name="endPeriod">окончание периода</param>
        /// <param name="dishId">идентификатор блюда</param>
        /// <returns></returns>
        public List<DishInMenu> GetScheduleActiveByDateRange(DateTime startPeriod, DateTime endPeriod, Int64? dishId)
        {
            List<DishInMenu> schedules;

            if (!_scheduleCache.TryGetValue($"ScheduleByDateRange-{startPeriod}-{endPeriod}-{dishId}", out var cached))
            {
                var fc = GetContext();

                IQueryable<DishInMenu> query;

                if (dishId == null)
                {
                    query = from s in fc.DishesInMenus.AsNoTracking()
                            where (
                                    s.IsActive == true
                                    &&
                                    (
                                        (
                                            s.OneDate == null
                                            ||
                                            (
                                                s.OneDate >= startPeriod
                                                && s.OneDate <= endPeriod
                                            )
                                        )
                                        ||
                                        (
                                            (
                                                s.EndDate == null
                                                &&
                                                s.BeginDate <= endPeriod
                                            )
                                            ||
                                            (
                                                s.BeginDate <= startPeriod
                                                &&
                                                s.EndDate >= startPeriod
                                            )
                                            ||
                                            (
                                                s.BeginDate <= endPeriod
                                                &&
                                                s.EndDate >= endPeriod
                                            )
                                        )
                                    )
                                    && s.IsDeleted == false
                                   )
                            select s;
                }
                else
                {
                    query = from s in fc.DishesInMenus.AsNoTracking()
                            where (
                                    s.IsActive == true
                                    && s.DishId == dishId
                                    &&
                                    (
                                        (
                                            s.OneDate == null
                                            ||
                                            (
                                                s.OneDate >= startPeriod
                                                && s.OneDate <= endPeriod
                                            )
                                        )
                                        &&
                                        (
                                            (
                                                s.EndDate == null
                                                &&
                                                s.BeginDate == null
                                            )
                                            ||
                                            (
                                                s.EndDate == null
                                                &&
                                                s.BeginDate <= endPeriod
                                            )
                                            ||
                                            (
                                                s.BeginDate <= startPeriod
                                                &&
                                                s.EndDate >= startPeriod
                                            )
                                            ||
                                            (
                                                s.BeginDate <= endPeriod
                                                &&
                                                s.EndDate >= endPeriod
                                            )
                                            ||
                                            (
                                                s.BeginDate >= startPeriod
                                                &&
                                                s.EndDate <= endPeriod
                                            )
                                        )
                                    )
                                    && s.IsDeleted == false
                                   )
                            select s;
                }

                schedules = query.ToList();

                SetObjectInCache(
                    _scheduleCache,
                    schedules,
                    $"ScheduleByDateRange-{startPeriod}-{endPeriod}-{dishId}");
            }
            else
            {
                schedules = (List<DishInMenu>)cached;
            }


            return schedules;
        }

        /// <summary>
        /// Возвращает список расписаний для блюда на дату
        /// </summary>
        /// <param name="dishId">идентификатор блюда</param>
        /// <param name="date">дата</param>
        /// <returns></returns>
        public List<DishInMenu> GetScheduleActiveByDishId(long dishId, DateTime date)
        {
            List<DishInMenu> schedules;

            if (!_scheduleCache.TryGetValue($"ScheduleByDishId-{dishId}-{date}", out var cached))
            {
                var fc = GetContext();

                var query = from s in fc.DishesInMenus.AsNoTracking()
                            where (
                                s.DishId == dishId
                                && s.IsActive == true
                                && (
                                    s.OneDate == date
                                    || (
                                        s.BeginDate <= date
                                        && s.EndDate == null
                                    ) || (
                                        s.BeginDate <= date
                                        && s.EndDate >= date
                                    )
                                )
                                && s.IsDeleted == false
                            )
                            select s;

                schedules = query.ToList();

                SetObjectInCache(
                    _scheduleCache,
                    schedules,
                    $"ScheduleByDishId-{dishId}-{date}");
            }
            else
            {
                schedules = (List<DishInMenu>)cached;
            }


            return schedules;
        }

        /// <summary>
        /// Возвращает список расписаний для кафе на дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="date">дата</param>
        /// <returns></returns>
        public List<DishInMenu> GetSchedulesForCafe(long cafeId, DateTime? date)
        {
            List<DishInMenu> schedules;

            DateTime dateTime = date ?? DateTime.Now;

            dateTime = dateTime.Date;

            if (!_scheduleCache.TryGetValue($"ScheduleForCafe-{cafeId}-{dateTime}", out var cached))
            {

                var fc = GetContext();

                string dayOfTheWeek = ((int)dateTime.DayOfWeek).ToString();
                string dayOfTheMonth = dateTime.Day.ToString();

                var query = fc.DishesInMenus.AsNoTracking()
                    .Where(s => s.IsActive == true
                               && s.Dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted)
                               .Select(l => l.CafeCategory).Any(
                                   cc => cc.CafeId == cafeId
                                   && !cc.IsDeleted
                                   && cc.IsActive == true
                                   && !cc.DishCategory.IsDeleted
                                   && cc.DishCategory.IsActive == true
                                   && !SystemDishCategories.GetSystemCtegoriesIds().Contains(cc.DishCategoryId))
                               && s.Dish.IsActive == true
                               && s.Dish.IsDeleted == false
                               && (
                                        (
                                            s.BeginDate <= dateTime
                                            || s.BeginDate == null
                                        )
                                        && (
                                            s.EndDate == null
                                            || s.EndDate >= dateTime
                                        )
                                   )
                                   && (
                                        s.Type == "D"
                                        || (
                                            s.Type == "W"
                                            && s.WeekDays
                                                .Contains(
                                                    dayOfTheWeek
                                                )
                                        )
                                        || (
                                            s.Type == "M"
                                            && s.MonthDays
                                                .Contains(
                                                    dayOfTheMonth
                                                )
                                        )
                                        || (
                                            s.Type == "S"
                                            && (
                                                s.BeginDate == dateTime
                                                || s.OneDate == dateTime
                                            )
                                        )
                                   )
                                   && s.IsDeleted == false
                            ).ToList()
                            .Except(
                                fc.DishesInMenus.Where(
                                    fe => fe.IsActive == true
                                    && fe.Dish.DishCategoryLinks.Select(l => l.CafeCategory).Any(
                                        cc => cc.CafeId == cafeId
                                        && !cc.IsDeleted
                                        && cc.IsActive == true)
                                    && fe.Dish.IsDeleted == false
                                    && fe.Dish.IsActive == true
                                    && fe.Type == "E"
                                    && (
                                        fe.BeginDate <= dateTime
                                        && (
                                            fe.EndDate == null
                                            || fe.EndDate >= dateTime
                                        )
                                        || fe.OneDate == dateTime
                                    )
                                    && fe.IsDeleted == false
                                ).ToList(), new ScheduleExcludeComparer()
                            );

                schedules = query.ToList();

                SetObjectInCache(
                    _scheduleCache,
                    schedules,
                    $"ScheduleForCafe-{cafeId}-{dateTime}");
            }
            else
            {
                schedules = (List<DishInMenu>)cached;
            }
            return schedules;
        }

        /// <summary>
        /// Возвращает список расписаний для кафе на дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="dishList"></param>
        /// <param name="onlyActive"></param>
        /// <returns></returns>
        public List<DishInMenu> GetSchedulesForCafeByDishList(long cafeId, long[] dishList, bool onlyActive)
        {
            var fc = GetContext();
            var query = fc.DishesInMenus
                .Where(s => dishList.Contains(s.Dish.Id)
                && s.Dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted)
                .Select(l => l.CafeCategory).Any(
                    cc => cc.CafeId == cafeId
                    && !cc.IsDeleted
                    && cc.IsActive == true)
                && s.Dish.IsActive == true
                && s.Dish.IsDeleted == false
                && s.IsDeleted == false
                ).ToList();

            var schedules = onlyActive
                ? query.Where(s => s.IsActive == true).ToList()
                : query.ToList();
            return schedules;
        }
        /// <summary>
        /// Возвращает список расписаний для кафе на дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="dishList"></param>
        /// <returns></returns>
        public async Task<List<DishInMenuHistory>> GetDishesInMenuHistoryForCafe(long cafeId, string dishName)
        {
            var fc = GetContext();
            //Достаем все блюда из меню на сегодня по выбранному кафе
            var dishList = await GetFirstDishesFromAllSchedules(new long[] { cafeId }, DateTime.Today,
                null, null, dishName);
            List<long> dishIds = dishList.SelectMany(d => d.Dishes.Select(g => g.DishId).ToList()).Distinct().ToList();
            var query = fc.DishesInMenuHistory.AsNoTracking()
                .Include(d => d.Dish)
                .Where(s => dishIds.Contains(s.Dish.Id)
                && s.Dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted)
                .Select(l => l.CafeCategory).Any(
                    cc => cc.CafeId == cafeId
                    && !cc.IsDeleted
                    && cc.IsActive == true)
                && !s.Dish.IsDeleted
                && s.Dish.IsActive == true
                ).ToList();
            return query;
        }
        /// <summary>
        /// Возвращает список расписаний для кафе на дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="date">дата</param>
        /// <returns></returns>
        public Dictionary<DishCategory, List<Dish>> GetShedulesForManager(long cafeId, DateTime? date)
        {
            var scheduledDishList =
                new Dictionary<DishCategory, List<Dish>>();
            var dateTime = date ?? DateTime.Now;

            dateTime = dateTime.Date;

            using (var fc = GetContext())
            {
                // using (var transaction = fc.Database.BeginTransaction())
                //  {
                var dayOfTheWeek = ((int)dateTime.DayOfWeek).ToString();
                var dayOfTheMonth = dateTime.Day.ToString();

                var query = fc.DishesInMenus
                    .Where(s => s.IsActive == true
                    && s.Dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted)
                    .Select(l => l.CafeCategory).Any(
                        cc => cc.CafeId == cafeId
                        && !cc.IsDeleted
                        && cc.IsActive == true
                        && !cc.DishCategory.IsDeleted
                        && cc.DishCategory.IsActive == true
                        && !SystemDishCategories.GetSystemCtegoriesIds().Contains(cc.DishCategoryId))
                    && s.Dish.IsActive == true
                    && s.Dish.IsDeleted == false && (
                        s.BeginDate <= dateTime
                        || s.BeginDate == null
                    )
                    && (
                        s.EndDate == null
                        || s.EndDate >= dateTime
                    )
                    && (
                        s.Type == "D"
                        || s.Type == "W"
                        && s.WeekDays.Contains(dayOfTheWeek)
                        || s.Type == "M"
                        && s.MonthDays.Contains(dayOfTheMonth)
                        || s.Type == "S"
                        && (
                            s.BeginDate == dateTime
                            || s.OneDate == dateTime
                        )
                    )
                    && s.IsDeleted == false
                    ).ToList()
                    .Except(
                        fc.DishesInMenus.Where(
                            fe => fe.IsActive == true
                            && fe.Dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted)
                            .Select(l => l.CafeCategory).Any(
                                cc => cc.CafeId == cafeId
                                && !cc.IsDeleted
                                && cc.IsActive == true)
                            && fe.Dish.IsDeleted == false
                            && fe.Dish.IsActive == true
                            && fe.Type == "E"
                            && (
                                fe.BeginDate <= dateTime
                                && (
                                    fe.EndDate == null
                                    || fe.EndDate >= dateTime
                                )
                                || fe.OneDate == dateTime
                            )
                            && fe.IsDeleted == false
                            ).ToList(), new ScheduleExcludeComparer()
                    );

                var currentSchedule = query.ToList();
                var dishId = currentSchedule.Select(c => c.DishId);
                var queryDishes = from d in fc.Dishes
                                  where
                                      dishId.Contains(d.Id)
                                      && d.IsActive == true
                                      && d.IsDeleted == false
                                  select d;

                var dishes = queryDishes.Where(d =>
                (
                    d.VersionFrom <= date
                    || d.VersionFrom == null
                )
                && (
                    d.VersionTo >= date ||
                    d.VersionTo == null
                )).ToList();

                var queryDishVersions = from d in fc.DishVersions
                                        where d.CafeCategory.CafeId == cafeId
                                              && (d.VersionFrom <= date || d.VersionFrom == null)
                                              && (d.VersionTo >= date || d.VersionTo == null)
                                              && d.IsActive == true
                                              && d.IsDeleted == false
                                        select d;

                var dishesVersion = queryDishVersions.ToList();

                foreach (var dish in dishes)
                {
                    var existingDishes =
                        dishesVersion.Where(dv => dv.DishId == dish.Id).ToList();
                    for (var i = 0; i < existingDishes.Count; i++)
                    {
                        dishesVersion.Remove(existingDishes[i]);
                    }
                }
                foreach (var dishVersionItem in dishesVersion)
                {
                    if (currentSchedule.FirstOrDefault(
                            s => s.DishId == dishVersionItem.DishId
                        ) != null)
                    {
                        dishes.Add(dishVersionItem == null
                            ? new Dish()
                            : new Dish
                            {
                                Id = dishVersionItem.DishId,
                                DishName = dishVersionItem.DishName,
                                BasePrice = dishVersionItem.BasePrice,
                                Kcalories = dishVersionItem.Kcalories,
                                Weight = dishVersionItem.Weight,
                                WeightDescription = dishVersionItem.WeightDescription,
                                VersionFrom = dishVersionItem.VersionFrom,
                                VersionTo = dishVersionItem.VersionTo,
                                Composition = string.Empty
                            });
                    }
                }

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
                        {
                            dish.BasePrice = (double)existingSchedule.Price;
                        }
                    }
                }

                var queryCafeCategories = from c in fc.DishCategoriesInCafes
                                          where c.CafeId == cafeId && c.IsDeleted == false
                                                && c.IsActive == true
                                                && c.DishCategory.IsActive == true
                                          select new { cafeCategories = c, category = c.DishCategory };

                var cafeCategories = queryCafeCategories.ToList();
                // transaction.Commit();

                foreach (var cafeCategoryItem in cafeCategories)
                {
                    var foodCategoryItem = cafeCategoryItem.category;
                    foodCategoryItem.Index = cafeCategoryItem.cafeCategories.Index;

                    var dishByScheduleAndCategory = dishes.Where(d =>
                        d.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted)
                        .Select(l => l.CafeCategory.DishCategoryId)
                        .Contains(cafeCategoryItem.cafeCategories.DishCategoryId)
                        ).ToList();

                    if (dishByScheduleAndCategory.Count > 0)
                    {
                        if (scheduledDishList.ContainsKey(foodCategoryItem)) continue;
                        scheduledDishList.Add(foodCategoryItem, dishByScheduleAndCategory);
                    }
                }
                return scheduledDishList;
                //   }
            }
        }

        /// <summary>
        /// Редактировать существующее расписание
        /// </summary>
        /// <param name="schedule">расписание</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        public long EditSchedule(DishInMenu schedule, long userId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    DishInMenu oldSchedule =
                        fc.DishesInMenus.FirstOrDefault(
                            s => s.OneDate == schedule.OneDate &&
                                 s.DishId == schedule.DishId
                            );

                    if (oldSchedule != null)
                    {
                        schedule.Id = oldSchedule.Id;

                        oldSchedule.BeginDate = schedule.BeginDate;
                        oldSchedule.EndDate = schedule.EndDate;
                        oldSchedule.MonthDays = schedule.MonthDays;
                        oldSchedule.WeekDays = schedule.WeekDays;
                        oldSchedule.OneDate = schedule.OneDate;
                        oldSchedule.DishId = schedule.DishId;
                        oldSchedule.Price = schedule.Price;
                        oldSchedule.Type = schedule.Type;
                        oldSchedule.LastUpdateByUserId = userId;
                        oldSchedule.IsActive = schedule.IsActive;
                        oldSchedule.IsDeleted = schedule.IsDeleted;
                        oldSchedule.LastUpdDate = DateTime.Now;

                        fc.SaveChanges();
                    }
                    else
                    {
                        return -1;
                    }

                    fc.SaveChanges();
                }
            }
            catch (Exception)
            {
                return -1;
            }

            return schedule.Id;
        }


        /// <summary>
        /// Возвращает расписание с указанными параметрами (блюдо, тип, даты, дни месяца).
        /// Нужно для удаления старого расписания при добавлении нового с такими же параметрами, чтобы не было задвоений.
        /// </summary>
        /// <param name="dishId"></param>
        /// <param name="type"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <param name="oneDay"></param>
        /// <param name="monthDays"></param>
        /// <returns></returns>
        public List<DishInMenu> GetSheduleByParams(long dishId, string type, DateTime? beginDate, DateTime? endDate,
            DateTime? oneDay)
        {
            var fc = GetContext();
            var query = fc.DishesInMenus.Where(dim => dim.DishId == dishId && dim.Type == type
                && dim.IsActive == true && !dim.IsDeleted);

            if (beginDate.HasValue)
            {
                DateTime dt = beginDate.Value.Date;
                query = query.Where(dim => dim.BeginDate.Value.Date == dt);
            }
            else
                query = query.Where(dim => dim.BeginDate == null);
            if (endDate.HasValue)
            {
                DateTime dt = endDate.Value.Date;
                query = query.Where(dim => dim.EndDate.Value.Date == dt);
            }
            else
                query = query.Where(dim => dim.EndDate == null);
            if (oneDay.HasValue)
            {
                DateTime dt = oneDay.Value.Date;
                query = query.Where(dim => dim.OneDate.Value.Date == dt);
            }
            else
                query = query.Where(dim => dim.OneDate == null);

            return query.ToList();
        }

        /// <summary>
        /// Обновить расписание
        /// </summary>
        /// <param name="schedule">расписание</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        public Int64 UpdateSchedule(DishInMenu schedule, long userId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    DishInMenu oldSchedule =
                        fc.DishesInMenus.FirstOrDefault(
                            s => s.Id == schedule.Id
                                 && s.IsActive == true
                                 && s.IsDeleted == false
                            );

                    if (oldSchedule != null)
                    {
                        oldSchedule.BeginDate = schedule.BeginDate;
                        oldSchedule.EndDate = schedule.EndDate;
                        oldSchedule.MonthDays = schedule.MonthDays;
                        oldSchedule.WeekDays = schedule.WeekDays;
                        oldSchedule.OneDate = schedule.OneDate;
                        oldSchedule.DishId = schedule.DishId;
                        oldSchedule.Price = schedule.Price;
                        oldSchedule.Type = schedule.Type;
                        oldSchedule.LastUpdateByUserId = userId;
                        oldSchedule.IsActive = schedule.IsActive;
                        oldSchedule.IsDeleted = schedule.IsDeleted;
                        oldSchedule.LastUpdDate = DateTime.Now;

                        fc.SaveChanges();
                    }
                    else
                    {
                        return -1;
                    }

                    fc.SaveChanges();
                }
            }
            catch (Exception)
            {
                return -1;
            }

            return schedule.Id;
        }


        /// <summary>
        /// Добавить расписание для блюда
        /// </summary>
        /// <param name="dishId">идентификатор блюда</param>
        /// <param name="scheduleType">тип расписания</param>
        /// <param name="beginDate">начало периода</param>
        /// <param name="endDate">окончание периода</param>
        /// <param name="oneDate">дата</param>
        /// <param name="monthDays">дни месяца</param>
        /// <param name="weekDays">дни недели</param>
        /// <param name="price">цена</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        public long AddSchedule(
            long dishId,
            char scheduleType,
            DateTime? beginDate,
            DateTime? endDate,
            DateTime? oneDate,
            string monthDays,
            string weekDays,
            double? price,
            long userId
        )
        {

            try
            {
                using (var fc = GetContext())
                {
                    DishInMenu schedule = new DishInMenu()
                    {
                        BeginDate = beginDate,
                        EndDate = endDate,
                        MonthDays = monthDays,
                        WeekDays = weekDays,
                        OneDate = oneDate,
                        DishId = dishId,
                        Price = price,
                        Type = scheduleType.ToString(),
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        CreatorId = userId
                    };

                    var sameDishRows = fc.DishesInMenus
                        .Where(dim =>
                        dim.DishId == schedule.DishId &&
                        dim.OneDate == schedule.OneDate
                        );
                    var lastUpdDate = sameDishRows.Max(dim => dim.LastUpdDate ?? dim.CreateDate);
                    var exSchedule = sameDishRows.FirstOrDefault(dim => dim.LastUpdDate == lastUpdDate || dim.CreateDate == lastUpdDate);

                    if (exSchedule != null)
                    {
                        if (scheduleType == 'E') //если удаляемое блюдо из расп.
                        {
                            schedule.IsActive = false;
                            schedule.IsDeleted = true;
                            schedule.Price = schedule.Dish.BasePrice;
                        }

                        schedule.Id = EditSchedule(schedule, userId);

                        if (schedule.Id < 0)
                            throw new Exception();
                    }
                    else
                    {
                        if (scheduleType == 'E')
                        {
                            // если удаляемое блюдо из расп.
                            // и оно не найдено в расписании
                            throw new Exception();
                        }
                        else
                        {
                            fc.DishesInMenus.Add(schedule);
                            fc.SaveChanges();
                        }
                    }

                    if (schedule.Id >= 0)
                    {
                        var dishInMenuHostoryId = AddDishInMenuHistory(dishId, scheduleType, price, userId);
                        if (dishInMenuHostoryId < 0)
                            return -1;
                    }
                    return schedule.Id;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }
        /// <summary>
        /// Добавить историю
        /// </summary>
        /// <param name="dishId">идентификатор блюда</param>
        /// <param name="scheduleType">тип расписания</param>
        /// <param name="price">цена</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        public long AddDishInMenuHistory(
            long dishId,
            char scheduleType,
            double? price,
            long userId
        )
        {

            try
            {
                using (var fc = GetContext())
                {
                    DishInMenuHistory dishInMenuHistory = new DishInMenuHistory()
                    {
                        DishId = dishId,
                        Price = price ?? 0,
                        Type = scheduleType.ToString(),
                        LastUpdDate = DateTime.Now,
                        UserId = userId
                    };

                    fc.DishesInMenuHistory.Add(dishInMenuHistory);

                    fc.SaveChanges();

                    return dishInMenuHistory.Id;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }
        /// <summary>
        /// Удалить расписание
        /// </summary>
        /// <param name="scheduleId">идентификатор расписания</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        public long RemoveSchedule(long scheduleId, long userId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    DishInMenu schedule =
                        fc.DishesInMenus.FirstOrDefault(
                            s => s.Id == scheduleId
                            && s.IsActive == true
                            && s.IsDeleted == false
                        );

                    if (schedule != null)
                    {
                        schedule.IsActive = false;
                        schedule.LastUpdDate = DateTime.Now;
                        schedule.LastUpdateByUserId = userId;

                        fc.SaveChanges();
                    }
                    else
                    {
                        return -1;
                    }

                    fc.SaveChanges();

                    return scheduleId;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Пометить блюда определенного типа в расписании как удаленные
        /// </summary>
        /// <param name="type">S, D, W, M, E</param>
        /// <returns></returns>
        public bool RemoveSchedules(HashSet<string> hstType, DateTime date, long cafeId)
        {
            var fc = GetContext();

            var query = fc.DishesInMenus
                .Where(e => e.OneDate.HasValue
                && e.OneDate.Value == date
                && e.Dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted).Select(l => l.CafeCategory).Any(c => c.CafeId == cafeId)
                && hstType.Contains(e.Type)
                && e.IsActive.Value
                && !e.IsDeleted)
                .ToList();

            if (query.Count > 0)
            {
                foreach (var item in query)
                {
                    item.IsActive = false;
                    item.IsDeleted = false;
                }

                fc.SaveChanges();
            }

            return true;
        }

        public long DeleteSchedule(long scheduleId, long userId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    DishInMenu schedule =
                        fc.DishesInMenus.FirstOrDefault(
                            s => s.Id == scheduleId
                            && s.IsActive == true
                            && s.IsDeleted == false
                        );

                    if (schedule != null)
                    {
                        fc.DishesInMenus.Remove(schedule);
                        fc.SaveChanges();
                    }
                    else
                    {
                        return -1;
                    }

                    fc.SaveChanges();

                    return scheduleId;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Деактивировать блюда в расписании, которые были исключены
        /// </summary>
        public bool SetActiveScheduleByDish(long dishId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var dishMenus = fc.DishesInMenus.Where(
                        o => !o.IsDeleted
                        && o.DishId == dishId
                        && o.Type == "E"
                        && o.IsActive == true).ToList();

                    foreach (var item in dishMenus)
                        item.IsActive = false;

                    fc.SaveChanges();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }

    public class First6DishesFromCat
    {
        public long CategoryId { get; set; }
        public int CountOfDishes { get; set; }
        public IEnumerable<DishInMenu> Dishes { get; set; }
    }
}
