using Food.Data;
using Food.Data.Entities;
using Food.Services.Extensions;
using Food.Services.Extensions.Dish;
using Food.Services.Extensions.DishInMenu;
using Food.Services.Extensions.Schedule;
using Food.Services.Models.Schedule;
using Food.Services.Services;

using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.Data.Accessor.Extensions;
using ITWebNet.FoodService.Food.DbAccessor;
using ITWebNet.FoodServiceManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Food.Services.Controllers
{
    [Route("api/menu")]
    public class MenuController : ContextableApiController
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IMenuService _menuService;

        [ActivatorUtilitiesConstructor]
        public MenuController(ICurrentUserService currentUserService, IMenuService menuService)
        {
            _currentUserService = currentUserService;
            _menuService = menuService;
        }

        public MenuController(IFoodContext context, Accessor accessor)
        {
            // Конструктор для обеспечения юнит-тестов
            Accessor = accessor;
            Accessor.SetTestingModeOn(context);
            Context = context;
            TestMode = true;
        }

        [HttpGet]
        [Authorize(Roles = "Manager")]
        [Route("schedule/add")]
        public IActionResult AddSchedule(long dishId, long? beginDate, long? endDate,
            long? oneDay, string monthDays, string weekDays, double? price, int scheduleType)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var startDateTime = beginDate != null ? DateTimeExtensions.FromUnixTime(beginDate.Value) : (DateTime?)null;
                var endDateTime = endDate != null ? DateTimeExtensions.FromUnixTime(endDate.Value) : (DateTime?)null;
                var oneDatyDate = oneDay != null ? DateTimeExtensions.FromUnixTime(oneDay.Value) : (DateTime?)null;
                var currentUser = User.Identity.GetUserById();

                var scheduleProcessing =
                    new ScheduleProcessing(
                        dishId,
                        (ScheduleTypeEnum)scheduleType,
                        startDateTime,
                        endDateTime,
                        oneDatyDate,
                        monthDays,
                        weekDays,
                        price,
                        currentUser
                    );

                if (scheduleProcessing.CheckScheduleOnStartInformation())
                {
                    // Отключение старого расписания с такими же параметрами, если оно есть:
                    scheduleProcessing.DisableOldSheduleWithSameParams();
                    // Добавление нового:
                    return
                        Ok(accessor.AddSchedule(
                            scheduleProcessing.FinalSchedule.DishID,
                            scheduleProcessing.FinalSchedule.Type[0],
                            scheduleProcessing.FinalSchedule.BeginDate,
                            scheduleProcessing.FinalSchedule.EndDate,
                            scheduleProcessing.FinalSchedule.OneDate,
                            scheduleProcessing.FinalSchedule.MonthDays,
                            scheduleProcessing.FinalSchedule.WeekDays,
                            scheduleProcessing.FinalSchedule.Price,
                            currentUser.Id
                        ));
                }
                throw new Exception("Schedule can`t been added");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("dishesbyschedule/cafeid/{cafeId:long}/date/{date:long}")]
        [Route("dishesbyschedule/cafeid/{cafeId:long}")]
        public IActionResult GetDishesByScheduleByDate(long cafeId, long? date = null)
        {
            try
            {
                var result = GetDishesByScheduleByDateHelper(cafeId, date);
                var model = CategoryExtensions.ConvertDictionaryToListFoodCategory(result);
                return Ok(model);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Получает первые N блюд в каждой категории, выбранных в случайном порядке.
        /// Применяется для вывода блюд на главной странице, а также на странице кафе (при указании параметра name).
        /// Также применяется при выборе фильтра по строке поиска (параметр filter) и при выборе фильтра по категории.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="name">URL кафе для вывода блюд конкретного кафе или null для вывода блюд из всех кафе</param>
        /// <param name="countDishes">Количество блюд, которые надо показать в каждой категории.</param>
        /// <param name="filter">Значение строки поиска фильтра</param>
        /// <param name="dishCatId">Категория, из которой надо отобразить блюда. Применяется в случае выбора фильтра по категориям.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("fordate")] // /api/menu/fordate
        public async Task<IActionResult> GetDishesForDate(
            long? date = null, string name = null, int? countDishes = 6, string filter = null, long? dishCatId = null, long? companyId = null, long? cityId = null)
        {
            Accessor accessor = GetAccessor();
            DateTime? dateTime = date != null ? DateTimeExtensions.FromUnixTime(date.Value) : (DateTime?)null;
            dateTime =
               dateTime == null
               || dateTime == DateTime.MinValue
               || dateTime == DateTime.MaxValue
                   ? DateTime.Now
                   : dateTime.Value.Date;
            //DateTime? dateTime = (DateTime?)DateTime.Today;

            //

            companyId = companyId ?? _currentUserService.GetUserCompany()?.Id;

            // Получение запланированных блюд на дату:
            var cafeIds = await GetCafeIdsForCurrentUser(companyId: companyId, date: dateTime, cityId: cityId);
            var categories = cafeIds.SelectMany(x => accessor.GetFoodCategoriesByCafeId(x))
                .Where(x => (dishCatId == null || x.DishCategoryId == dishCatId)
                         && _menuService.IsCategoryVisible(x, dateTime.Value));
            var categoryIds = categories.Select(x => x.Id).ToArray();

            var currentSchedule = await accessor.GetFirstDishesFromAllSchedules(
                cafeIds, dateTime, name, countDishes, filter, cafeCategoryIds: categoryIds);

            // Преобразуем выбранные группы БД в список FoodCategoryWithDishes для передачи на сайт:
            List<FoodCategoryWithDishes> resultDto = new List<FoodCategoryWithDishes>(currentSchedule.Count);
            foreach (First6DishesFromCat NextCat in currentSchedule)
            {
                FoodCategoryWithDishes CatDto = new FoodCategoryWithDishes();

                CatDto.Category = accessor.GetFoodCategoryById(NextCat.CategoryId).GetContract();
                CatDto.CountOfDishes = NextCat.CountOfDishes;
                try
                {
                    CatDto.Dishes = NextCat.Dishes?.Select(d => d.Dish.GetContractDish()).ToList() ?? new List<FoodDishModel>();
                }
                catch (Exception ex) { }

                GetDishesDiscount(CatDto.Dishes);

                resultDto.Add(CatDto);
            }

            return Ok(resultDto);
        }

        [HttpPost]
        [Route("other6")] // /api/menu/other6
        public async Task<IActionResult> GetRestOfCat([FromBody] RestOfCategory rest)
        {
            Accessor accessor = GetAccessor();
            DateTime? dateTime = (DateTime?)DateTime.Today;

            var cafeIds = await GetCafeIdsForCurrentUser(
                companyId: rest.CompanyId,
                cityId: null /* FIXME: вернутся блюда кафе всех городов? */
            );

            var categories = cafeIds.SelectMany(x => accessor.GetFoodCategoriesByCafeId(x))
                .Where(x => rest.CategoryId == x.DishCategoryId
                         && _menuService.IsCategoryVisible(x, dateTime.Value));
            var categoryIds = categories.Select(x => x.Id).ToArray();

            // Получение запланированных блюд на дату:
            List<Dish> dishes = await accessor.GetSchedulesRestOfCat(
                cafeIds, dateTime, categoryIds, rest.DishIds, rest.RestName);
            List<FoodDishModel> dishesDto = dishes.Select(d => d.GetContractDish()).ToList();

            GetDishesDiscount(dishesDto);

            return Ok(dishesDto);
        }

        public Dictionary<FoodCategoryModel, List<FoodDishModel>> GetDishesByScheduleByDateHelper(long cafeId,
            long? date)
        {
            var dateTime = date != null ? DateTimeExtensions.FromUnixTime(date.Value) : (DateTime?)null;
            var scheduledDishList = new Dictionary<FoodCategoryModel, List<FoodDishModel>>();

            dateTime =
                dateTime == null
                || dateTime == DateTime.MinValue
                || dateTime == DateTime.MaxValue
                    ? DateTime.Today
                    : dateTime.Value.Date;

            // Получение запланированных блюд на дату
            // Получение списка Schedule:
            var currentSchedule = GetScheduleForCafeByCafeIdHelper(cafeId, date);
            // Получение списка id блюд из списка Schedule:
            var dishesId = new List<long>();
            foreach (var schedule in currentSchedule)
                dishesId.Add(schedule.DishID);
            // Получение объектов блюд по массиву их id:
            List<Dish> dishes = new List<Dish>();
            if (dishesId.Any())
                dishes = Accessor.Instance.GetFoodDishesById(dishesId.ToArray());
            dishes = dishes.Where(
                d => (
                         d.VersionFrom <= dateTime
                         || d.VersionFrom == null
                     )
                     && (
                         d.VersionTo >= dateTime ||
                         d.VersionTo == null
                     )
            ).ToList();

            // Добавление версий блюд
            // Получение версий блюд для кафе на дату:
            List<DishVersion> dishesVersion =
                Accessor.Instance.GetFoodDishVersionByCafeId(cafeId, (DateTime)dateTime);

            // Удаление тех версий блюд, идентификаторы которых для которых уже есть:
            foreach (Dish dish in dishes)
            {
                var existingDishes =
                    dishesVersion.Where(dv => dv.DishId == dish.Id).ToList();
                for (var i = 0; i < existingDishes.Count; i++)
                    dishesVersion.Remove(existingDishes[i]);
            }

            // Добавление версий блюд в список:
            foreach (var dishVersionItem in dishesVersion)
            {
                if (currentSchedule.FirstOrDefault(s => s.DishID == dishVersionItem.DishId) != null)
                    dishes.Add(dishVersionItem.GetEntityFromDishVersion());
            }

            // Установка цен на блюда
            foreach (var dish in dishes)
            {
                var existingScheduleForDishes =
                    currentSchedule.Where(s => s.DishID == dish.Id).ToList();

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
            var categoriesInCafe = Accessor.Instance.GetFoodCategoriesByCafeId(cafeId);
            if (categoriesInCafe != null)
            {
                foreach (var cafeCategoryItem in categoriesInCafe)
                {
                    if (!_menuService.IsCategoryVisible(cafeCategoryItem, dateTime.Value))
                        continue;

                    var foodCategoryItem = cafeCategoryItem.DishCategory.GetContract();
                    foodCategoryItem.Index = cafeCategoryItem.Index;

                    var dishByScheduleAndCategory = new List<FoodDishModel>();
                    var dishList = dishes.Where(d => d.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted)
                    .Select(l => l.CafeCategory.DishCategoryId).Contains(cafeCategoryItem.DishCategoryId))
                        .GroupBy(e => e.Id).Select(e => e.FirstOrDefault());

                    foreach (var dishItem in dishList)
                    {
                        dishByScheduleAndCategory.Add(dishItem.GetContractDish());
                    }

                    GetDishesDiscount(dishByScheduleAndCategory);

                    if (dishByScheduleAndCategory.Count > 0 && !scheduledDishList.ContainsKey(foodCategoryItem))
                    {
                        scheduledDishList.Add(foodCategoryItem, dishByScheduleAndCategory);
                    }
                }
            }

            return scheduledDishList;
        }

        private void GetDishesDiscount(List<FoodDishModel> lstFoodDishModel)
        {
            if (!User.Identity.IsAuthenticated)
                return;

            var currentUser = User.Identity.GetUserById();

            var lstDiscounts = GetAccessor().GetDiscounts(currentUser.Id, DateTime.Now);

            foreach (var dish in lstFoodDishModel)
            {
                try
                {
                    dish.Discount = Convert.ToInt32(
                    GetAccessor()
                        .GetDiscountValue(lstDiscounts, dish.CafeId, DateTime.Now, currentUser.Id)//User.Identity.GetUserId<long>())
                    );
                }
                catch (Exception ex)
                {

                }
            }
        }

        [HttpGet]
        [Route("dishesformanager/cafeid/{cafeId:long}/date/{date:long}")]
        public IActionResult GetDishesForManager(long cafeId, long? date = null)
        {
            var result = GetDishesForManagerHelper(cafeId, date) ?? new Dictionary<FoodCategoryModel, List<FoodDishModel>>();
            var model = CategoryExtensions.ConvertDictionaryToListFoodCategory(result);
            return Ok(model);
        }

        public Dictionary<FoodCategoryModel, List<FoodDishModel>> GetDishesForManagerHelper(long cafeId, long? date)
        {
            var dateTime = date != null ? DateTimeExtensions.FromUnixTime(date.Value) : (DateTime?)null;
            dateTime =
                dateTime == null
                || dateTime == DateTime.MinValue
                || dateTime == DateTime.MaxValue
                    ? DateTime.Now
                    : dateTime;

            dateTime = dateTime.Value.Date;

            var allCategories = Accessor.Instance.GetFoodCategoriesForManager(dateTime, cafeId);
            var result = new Dictionary<FoodCategoryModel, List<FoodDishModel>>();
            foreach (var item in allCategories)
            {
                var category = item.Key.GetContract();

                if (item.Value == null)
                {
                    category.Index = -1;
                    result.Add(category, null);
                }
                else
                {
                    category.Index = item.Key.Index;
                    result.Add(category, item.Value.Select(dish => new FoodDishModel
                    {
                        Id = dish.Id,
                        Name = dish.DishName,
                        BasePrice = dish.BasePrice,
                        Kcalories = dish.Kcalories,
                        Weight = dish.Weight,
                        WeightDescription = dish.WeightDescription,
                        VersionFrom = dish.VersionFrom,
                        VersionTo = dish.VersionTo,
                        DishIndex = Accessor.Instance.GetDishIndexInCategory(dish.Id, item.Key.Id),
                        Uuid = dish.Uuid,
                        CategoryId = category.Id,
                        DishRatingCount = dish.DishRatingCount,
                        DishRatingSumm = dish.DishRatingSumm,
                        Description = dish.Description,
                        Image = dish.ImageId,
                        Composition = dish.Composition
                    }).OrderBy(d => d.DishIndex).ThenBy(d => d.Id).ToList());
                }
            }

            return result;
        }

        [HttpGet]
        [Route("menuformanager/cafeid/{cafeId:long}/date/{date:long}")]
        public IActionResult GetMenuForManager(long cafeId, long? date = null)
        {
            var dateTime = date != null ? DateTimeExtensions.FromUnixTime(date.Value) : (DateTime?)null;
            dateTime =
                dateTime == null
                || dateTime == DateTime.MinValue
                || dateTime == DateTime.MaxValue
                    ? DateTime.Now
                    : dateTime;

            dateTime = dateTime.Value.Date;

            var currentDishList = GetDishesForManagerHelper(cafeId, date);
            currentDishList = currentDishList.Where(c => c.Value != null).ToDictionary(c => c.Key, c => c.Value);

            var dishVersions =
                Accessor.Instance.GetFoodCategoriesVersionForManager(dateTime, cafeId);

            foreach (var dishVersion in dishVersions)
            {
                var existingCategory = currentDishList.Keys.FirstOrDefault(fc => fc.Id == dishVersion.Key.Id);
                if (existingCategory == null)
                {
                    var category = dishVersion.Key.GetContract();
                    category.Index = dishVersion.Key.Index;
                    currentDishList.Add(category,
                        dishVersion.Value?.Select(dish => new FoodDishModel
                        {
                            Id = dish.DishId,
                            Name = dish.DishName,
                            BasePrice = dish.BasePrice,
                            Kcalories = dish.Kcalories,
                            Weight = dish.Weight,
                            WeightDescription = dish.WeightDescription,
                            VersionFrom = dish.VersionFrom,
                            VersionTo = dish.VersionTo,
                            DishIndex = dish.DishIndex,
                            CategoryId = category.Id,
                            Image = dish.ImageId
                        }).ToList() ?? new List<FoodDishModel>());
                }
                else
                {
                    var dishes = dishVersion.Value?.Where(c => c.CafeCategoryId == existingCategory.Id) ??
                                 new List<DishVersion>();
                    currentDishList.FirstOrDefault(c => c.Key.Id == dishVersion.Key.Id).Value
                        .AddRange(dishes.Select(dish => new FoodDishModel
                        {
                            Id = dish.DishId,
                            Name = dish.DishName,
                            BasePrice = dish.BasePrice,
                            Kcalories = dish.Kcalories,
                            Weight = dish.Weight,
                            WeightDescription = dish.WeightDescription,
                            VersionFrom = dish.VersionFrom,
                            VersionTo = dish.VersionTo,
                            DishIndex = dish.DishIndex,
                            CategoryId = dishVersion.Key.Id,
                            Image = dish.ImageId
                        }).ToList());
                }
            }

            for (var i = 0; i < currentDishList.Count; i++)
                if (currentDishList.ElementAt(i).Value == null || currentDishList.ElementAt(i).Value.Count == 0)
                {
                    currentDishList.Remove(currentDishList.ElementAt(i).Key);
                    i--;
                }

            var result = currentDishList.OrderBy(c => c.Key.Index).ToDictionary(c => c.Key, c => c.Value.OrderBy(d => d.DishIndex).ToList());
            var model = CategoryExtensions.ConvertDictionaryToListFoodCategory(result);

            return Ok(model);
        }

        [HttpGet]
        [Route("menuschedule/cafeid/{cafeId:long}/date/{date:long}")]
        [Route("menuschedule/cafeid/{cafeId:long}")]
        public IActionResult GetMenuSchedules(long cafeId, long? date = null)
        {
            try
            {
                var dateTime = date != null ? DateTimeExtensions.FromUnixTime(date.Value) : (DateTime?)null;
                var currentSchedule = new Dictionary<FoodCategoryModel, List<FoodDishModel>>();
                dateTime =
                    dateTime == null
                    || dateTime == DateTime.MinValue
                    || dateTime == DateTime.MaxValue
                        ? DateTime.Now
                        : dateTime;

                dateTime = dateTime.Value.Date;

                var schedules = Accessor.Instance.GetShedulesForManager(cafeId, dateTime);
                foreach (var schedule in schedules)
                    currentSchedule.Add(schedule.Key.GetContract(), schedule.Value.Select(dish => new FoodDishModel
                    {
                        Id = dish.Id,
                        Name = dish.DishName,
                        BasePrice = dish.BasePrice,
                        Kcalories = dish.Kcalories,
                        Weight = dish.Weight,
                        WeightDescription = dish.WeightDescription,
                        VersionFrom = dish.VersionFrom,
                        VersionTo = dish.VersionTo,
                        DishIndex = Accessor.Instance.GetDishIndexInCategory(dish.Id, schedule.Key.Id),
                        CategoryId = schedule.Key.Id,
                        Image = dish.ImageId
                    }).ToList());

                var result = currentSchedule.OrderBy(c => c.Key.Index).ToDictionary(c => c.Key, c => c.Value.OrderBy(d => d.DishIndex).ToList());
                var model = CategoryExtensions.ConvertDictionaryToListFoodCategory(result);

                return Ok(model);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("getDishesByScheduleByDateByRange/cafe/{cafeId:long}/category/{categoryId:long}/date/{date:long}/start/{startRange:int}/count/{count:int}")]
        [Route("getDishesByScheduleByDateByRange/cafe/{cafeId:long}/category/{categoryId:long}/start/{startRange:int}/count/{count:int}")]
        public IActionResult GetDishesByScheduleByDateByRange(long cafeId,
            long categoryId, int startRange, int count, long? date = null)
        {
            try
            {
                var listOfDishes =
                    GetDishesByScheduleByDateHelper(cafeId, date)
                        .Where(i => i.Key.Id == categoryId)
                        .ToDictionary(
                            i => i.Key,
                            i => i.Value
                                .Skip(startRange)
                                .Take(count)
                                .ToList()
                        );

                var model = new List<FoodCategoryWithDishes>();
                var result = listOfDishes;
                foreach (var el in result)
                {
                    model.Add(
                        new FoodCategoryWithDishes()
                        {
                            Category = el.Key,
                            Dishes = el.Value.ToList()
                        });
                }

                return Ok(model);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        [Route("getDishesByScheduleByDateByTagListAndSearchTerm")]
        public IActionResult GetDishesByScheduleByDateByTagListAndSearchTerm(
            long cafeId, long? date, string searchTerm, [FromBody] List<long> tagIds)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var cafes = Accessor.Instance.GetCafesByUserId(userId);

                if (!cafes.Select(c => c.Id).Contains(cafeId))
                    return Ok(new Dictionary<FoodCategoryModel, List<FoodDishModel>>());

                var scheduledDishList =
                    GetDishesByScheduleByDateHelper(cafeId, date);

                List<FoodDishModel> dishesBySearch;

                if (!string.IsNullOrEmpty(searchTerm))
                    dishesBySearch = Accessor.Instance
                        .GetDishesBySearchTermAndTagListAndCafeId(tagIds, cafeId, searchTerm).ToList()
                        .Select(e => e.GetContractDish()).ToList();
                else
                    dishesBySearch = Accessor.Instance.GetDishesByTagListAndCafeId(tagIds, cafeId).ToList()
                        .Select(e => e.GetContractDish()).ToList();

                foreach (var elem in scheduledDishList)
                {
                    var dishesToDelete =
                        elem
                            .Value
                            .Where(d => !dishesBySearch.Select(dish => dish.Id).Contains(d.Id))
                            .Select(d => d)
                            .ToList();

                    foreach (var dish in dishesToDelete)
                        elem.Value.Remove(dish);
                }

                var categoryToDelete = new List<FoodCategoryModel>();

                foreach (var elem in scheduledDishList)
                    if (elem.Value.Count == 0)
                        categoryToDelete.Add(elem.Key);

                foreach (var category in categoryToDelete)
                    scheduledDishList.Remove(category);

                var result = scheduledDishList;
                var model = CategoryExtensions.ConvertDictionaryToListFoodCategory(result);

                return Ok(model);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Manager")]
        [Route("menubypattern/cafe/{cafeId:long}/id/{id:long}")]
        public IActionResult GetMenuByPatternId(long cafeId, long id)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var currentUser = User.Identity.GetUserById();

                if (!accessor.IsUserManagerOfCafe(currentUser.Id, cafeId))
                    return null;

                var patternM = Accessor.Instance.GetMenuByPatternId(cafeId, id);
                if (patternM == null)
                    return Ok(new Dictionary<FoodCategoryModel, List<FoodDishModel>>());

                var result = patternM.ToDictionary(c => c.Key.GetContract(),
                    v => v.Value.Select(d => d.GetContractDish()).ToList());

                var model = CategoryExtensions.ConvertDictionaryToListFoodCategory(result);

                return Ok(model);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("schedule/{scheduleId:long}")]
        public IActionResult GetScheduleById(long scheduleId)
        {
            var schedule = Accessor.Instance.GetScheduleById(scheduleId);
            return Ok(schedule.GetContract());
        }

        [HttpGet]
        [Route("schedullebycafe/{cafeId:long}/date/{date:long}")]
        [Route("schedullebycafe/{cafeId:long}")]
        public IActionResult GetScheduleForCafeByCafeId(long cafeId, long? date = null)
        {
            try
            {
                return Ok(GetScheduleForCafeByCafeIdHelper(cafeId, date));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        private List<ScheduleModel> GetScheduleForCafeByCafeIdHelper(long cafeId, long? date)
        {
            var dateTime = date != null ? DateTimeExtensions.FromUnixTime(date.Value) : (DateTime?)null;
            var scheduleList = new List<ScheduleModel>();
            var time = dateTime ?? DateTime.Now;

            foreach (var scheduleItem in Accessor.Instance.GetSchedulesForCafe(cafeId, time))
                scheduleList.Add(scheduleItem.GetContract());

            return scheduleList;
        }

        [HttpGet]
        [Route("schedullefordish/{dishId:long}/date/{date:long}")]
        [Route("schedullefordish/{dishId:long}")]
        public IActionResult GetScheduleForDish(long dishId, long? date)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var dateTime = date != null ? DateTimeExtensions.FromUnixTime(date.Value) : (DateTime?)null;
                var time = dateTime ?? DateTime.Now;

                var scheduleList = new List<ScheduleModel>();

                foreach (var scheduleItem in accessor.GetScheduleActiveByDishId(dishId, time))
                    scheduleList.Add(scheduleItem.GetContract());

                return Ok(scheduleList);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [Route("getSchedulesForCafeByDishList")]
        public IActionResult GetSchedulesForCafeByDishList(long cafeId, [FromBody] long[] dishList, bool onlyActive = true)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var scheduleList = new List<ScheduleModel>();

                foreach (
                    var scheduleItem in accessor.GetSchedulesForCafeByDishList(cafeId, dishList, onlyActive))
                    scheduleList.Add(scheduleItem.GetContract());

                return Ok(scheduleList);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Manager")]
        [Route("getDishesInMenuHistoryForCafe")]
        public IActionResult GetDishesInMenuHistoryForCafe(long cafeId, string dishName)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var historyItems = accessor.GetDishesInMenuHistoryForCafe(cafeId, dishName).
                    GetAwaiter().GetResult().OrderByDescending(d => d.LastUpdDate);
                var groupItems = historyItems.GroupBy(s => s.Dish);
                var pageDishInMenu = new List<PageDishInMenuHistoryModel>();
                foreach (var item in groupItems)
                {
                    pageDishInMenu.Add(new PageDishInMenuHistoryModel
                    {
                        Description = item.Key.Description,
                        Name = item.Key.DishName,
                        Price = historyItems.FirstOrDefault(s => s.Dish.Id == item.Key.Id).Price,
                        Weight = item.Key.Weight,
                        DishCategoryIdNames = new Dictionary<long, string>(item.Key.DishCategoryLinks.Select(l => new KeyValuePair<long, string>(l.CafeCategory.DishCategoryId, l.CafeCategory.DishCategory.CategoryName))),
                        DishesHistory = historyItems.
                            Where(d => d.Dish.Id == item.Key.Id).Select(d => d.GetContract()).
                            OrderByDescending(d => d.LastUpdDate).ToList()
                    });
                }

                var pageDisthInMenuHistoryCategory = new List<PageDishInMenuHistoryCategoryModel>();
                var categoryNames = new List<string>();
                foreach (var item in groupItems)
                {
                    categoryNames.AddRange(item.Key.DishCategoryLinks.Select(l => l.CafeCategory.DishCategory.CategoryName));
                }
                categoryNames = categoryNames.Distinct().ToList();
                foreach (var categoryName in categoryNames)
                {
                    pageDisthInMenuHistoryCategory.Add(new PageDishInMenuHistoryCategoryModel
                    {
                        CategoryName = categoryName,
                        PageDisthes = pageDishInMenu
                        .Where(s => s.DishCategoryIdNames.ContainsValue(categoryName))
                        .OrderBy(s => s.Name).ToList()
                    });
                }
                return Ok(pageDisthInMenuHistoryCategory);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Manager")]
        [Route("RemoveCafeMenuPattern/{id:long}/cafe/{cafeId:long}")]
        public IActionResult RemoveCafeMenuPattern(long cafeId, long id)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var currentUser = User.Identity.GetUserById();

                if (!accessor.IsUserManagerOfCafe(currentUser.Id, cafeId))
                    return Ok(false);

                return Ok(Accessor.Instance.RemoveCafeMenuPattern(cafeId, id));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Manager")]
        [Route("removeschedule/{scheduleId:long}")]
        public IActionResult RemoveSchedule(long scheduleId)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var currentUser = User.Identity.GetUserById();
                var schedule = Accessor.Instance.GetScheduleById(scheduleId);

                if (schedule != null)
                    if (
                        schedule.OneDate != null
                        && !ScheduleProcessing.CheckDishExistingInOrders(schedule.DishId, schedule.OneDate, null, null)
                        || !ScheduleProcessing.CheckDishExistingInOrders(schedule.DishId, null, schedule.BeginDate,
                            schedule.EndDate)
                    )
                        if (accessor.IsUserManagerOfCafe(currentUser.Id,
                            schedule.Dish.DishCategoryLinks.FirstOrDefault().CafeCategory.CafeId))
                            return Ok(Accessor.Instance.RemoveSchedule(scheduleId, currentUser.Id));
                        else
                            throw new SecurityException("Attempt of unauthorized access");
                return Ok(-1);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [Route("updateschedule")]
        public IActionResult UpdateSchedule([FromBody] ScheduleModel schedule)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                var scheduleProcessing =
                    new ScheduleProcessing(schedule, currentUser);

                if (scheduleProcessing.CheckScheduleOnStartInformation())
                {
                    var oldSchedule = scheduleProcessing.FinalSchedule.GetEntity();
                    oldSchedule.IsActive = true;
                    oldSchedule.LastUpdateByUserId = currentUser.Id;
                    oldSchedule.LastUpdDate = DateTime.Now;

                    return Ok(Accessor.Instance.UpdateSchedule(oldSchedule, currentUser.Id));
                }
                throw new Exception("Schedule can`t been updated");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        //TODO:тут что-то очень много всего делается чтобы применить шаблон
        //Задача:
        //Нужно удалить все S на текущую дату
        //Взять из Шаблона блюда и добавить их на текущую дату с S
        [HttpGet]
        [Authorize(Roles = "Manager")]
        [Route("updateSchedulesByPatternId/cafe/{cafeId:long}/pattern/{patternId:long}/date/{requestDate:long}")]
        [Route("updateSchedulesByPatternId/cafe/{cafeId:long}/pattern/{patternId:long}")]
        public IActionResult UpdateSchedulesByPatternId(long cafeId, long patternId, long? requestDate = null)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var currentUser = User.Identity.GetUserById();
                var pattern = accessor.GetCafeMenuPatternById(cafeId, patternId);

                if (pattern == null || !accessor.IsUserManagerOfCafe(currentUser.Id, cafeId))
                    return Ok(false);

                pattern.Dishes = pattern.Dishes.Where(
                    o => !o.IsDeleted
                    && o.Dish.DishCategoryLinks.Any(
                        l => l.IsActive == true
                         && !l.IsDeleted
                         && !SystemDishCategories.GetSystemCtegoriesIds().Contains(l.CafeCategory.DishCategoryId)
                         && l.CafeCategory.DishCategory.IsDeleted == false
                         && l.CafeCategory.DishCategory.IsActive == true)
                    ).ToList();

                if (requestDate.HasValue)
                {
                    var date = DateTimeExtensions.FromUnixTime(requestDate.Value);
                    Accessor.Instance.RemoveSchedules(new HashSet<string>() { "S" }, date.Date, cafeId);
                }

                var currentDishes = GetDishesByScheduleByDateHelper(cafeId, requestDate).SelectMany(c => c.Value).ToList();
                foreach (var item in pattern.Dishes)
                {

                    var currItem = currentDishes.FirstOrDefault(d => d.Id == item.DishId);
                    if (currItem == null ||
                      (currItem != null &&
                        Math.Abs(currItem.BasePrice - item.Price) > 0.001 && !item.IsDeleted))
                    {
                        AddSchedule(item.DishId, null, null, requestDate, null, null, item.Price, (int)ScheduleTypeEnum.Simply);
                        accessor.SetActiveScheduleByDish(item.DishId);
                    }
                }

                var deletedDishes = currentDishes.Select(c => c.Id)
                    .Except(pattern.Dishes.Where(c => !c.IsDeleted).Select(c => c.DishId)).ToList();
                foreach (var deleted in deletedDishes)
                {
                    var dish = currentDishes.FirstOrDefault(c => c.Id == deleted);
                    if (dish == null)
                        continue;
                    AddSchedule(deleted, null, null, requestDate, null, null, dish.BasePrice, (int)ScheduleTypeEnum.Exclude);
                }

                return Ok(true);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Функция для определения, есть ли кафе, привязанные к организации текущего пользователя.
        /// Нужна, чтобы показывать блюда только из тех кафе, которые обслуживают организацию пользователя.
        /// </summary>
        /// <returns>Если пользователь не авторизован или пользователь не относится к компании - возвращает null.
        /// В другом случае возвращает массив с идентификаторами кафе, которые обслуживают
        /// организацию пользователя. </returns>
        public async Task<long[]> GetCafeIdsForCurrentUser(long? companyId = null, DateTime? date = null, long? cityId = null)
        {
            var accessor = GetAccessor();
            var context = accessor.GetContext();
            var when = date ?? DateTime.Now;
            var cafes = new List<long>();

            if (User.Identity.IsAuthenticated && User.IsInRole(EnumUserRole.Manager))
            {
                // Для менеджера возвращаем список только тех кафе, которыми он управляет.
                long userId = User.Identity.GetUserId();
                var managedCafes = await context.CafeManagers
                    .Where(x => x.UserId == userId && !x.IsDeleted && !x.Cafe.IsDeleted && x.Cafe.IsActive
                    && ((cityId != null && x.Cafe.CityId == cityId) || cityId == null))
                    .Select(x => x.Cafe.Id)
                    .ToListAsync().ConfigureAwait(false);
                cafes.AddRange(managedCafes);
            }
            else if (User.Identity.IsAuthenticated && User.IsInRole(EnumUserRole.Admin))
            {
                // Админы не могут пользоваться основным функционалом сайта, поэтому
                // кафе для них нет.
            }
            else
            {
                if (companyId != null)
                {
                    // Если у пользователя указаны компании - тогда надо показать блюда только из тех кафе,
                    // для которых на данный момент оформлен корп. заказ.
                    cafes = await context.CompanyOrders
                        .Where(x => x.CompanyId == companyId
                                 && x.OpenDate.Value <= when && when <= x.AutoCloseDate
                                 && x.State == (long)EnumOrderStatus.Created
                                 && !x.IsDeleted && !x.Cafe.IsDeleted && x.Cafe.IsActive
                                 && ((cityId != null && x.Cafe.CityId == cityId) || cityId == null))
                        .Select(x => x.Cafe.Id)
                        .ToListAsync().ConfigureAwait(false);
                }
                else
                {
                    // Если нет - тогда надо показывать блюда только из активных кафе, для которых доступно
                    // персональное обслуживание.
                    cafes = await context.Cafes
                        .Where(c => !c.IsDeleted && c.IsActive
                                 && (c.CafeAvailableOrdersType == "COMPANY_PERSON" || c.CafeAvailableOrdersType == "PERSON_ONLY")
                                 && ((cityId != null && c.CityId == cityId) || cityId == null))
                        .Select(c => c.Id)
                        .ToListAsync().ConfigureAwait(false);
                }
            }

            // Если возвращаем список кафе на сегодня - выборка только работающих в данный момент кафе.
            // Если на будущее - выборка только тех кафе где предзаказы включены.
            if (when.Date == DateTime.Today)
            {
                TimeSpan time = DateTime.Now.TimeOfDay;

                return context.Cafes
                    .Where(c => cafes.Contains(c.Id)
                             && ((c.WorkingTimeFrom < time && c.WorkingTimeTo > time) || c.WorkingTimeFrom == null))
                    .Select(c => c.Id)
                    .ToArray();
            }
            else if (when.Date > DateTime.Today)
            {
                return context.Cafes
                    .Where(c => cafes.Contains(c.Id) && c.WeekMenuIsActive)
                    .Select(c => c.Id)
                    .ToArray();
            }

            return new long[] { };
        }
    }
}
