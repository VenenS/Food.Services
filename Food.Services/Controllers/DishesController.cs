using Food.Data;
using Food.Data.Entities;
using Food.Services.Extensions;
using Food.Services.Extensions.Schedule;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Caching;
using System.Security;

namespace Food.Services.Controllers
{
    [Route("api/dishes")]
    public class DishesController : ContextableApiController
    {
        [ActivatorUtilitiesConstructor]
        public DishesController()
        { }


        public DishesController(IFoodContext context, Accessor accessor)
        {
            Accessor = accessor;
            Accessor.SetTestingModeOn(context);
            Context = context;
            TestMode = true;
        }

        [Authorize(Roles = "Manager")]
        [HttpPost, Route("")]
        public IActionResult CreateDish(long cafeId, [FromBody] FoodDishModel dish)
        {
            try
            {
                if (dish == null)
                {
                    return BadRequest("Empty Dish");
                }

                var categoryIds = dish.FoodCategories.Where(d => d.IsSelected).Select(c => c.Id).ToArray();

                var currentUser =
                    User.Identity.GetUserById();

                if (Accessor.Instance.IsUserManagerOfCafe(currentUser.Id, cafeId))
                {
                    var categoryList =
                        Accessor.Instance.GetFoodCategoriesByCafeId(cafeId);

                    if (!categoryList.Any(c=>categoryIds.Contains(c.DishCategoryId)))
                    {
                        return BadRequest("Category is not exist");
                    }

                    if (!Accessor.Instance.CheckUniqueNameWithinCafe(dish.Name, cafeId))
                    {
                        return BadRequest("Блюдо не уникально в рамках кафе");
                    }

                    var versionFrom = dish.VersionFrom ?? DateTime.Now;
                    var versionTo = dish.VersionTo;

                    if (versionFrom == DateTime.MaxValue
                        || versionFrom == DateTime.MinValue)
                    {
                        versionFrom = DateTime.Now;
                    }

                    if (versionTo != null
                        && versionTo < versionFrom)
                    {
                        versionTo = null;
                    }

                    var newDish = new Dish()
                    {
                        Composition = dish.Composition,
                        VersionFrom = versionFrom,
                        VersionTo = versionTo,
                        IsActive = true,
                        Description = dish.Description,
                        ImageId = dish.Image
                    };

                    DishServiceHelper.DishValidation(dish, newDish, cafeId);

                    return Ok(
                        Accessor.Instance.AddDish(
                            cafeId,
                            categoryIds,
                            newDish,
                            currentUser.Id
                        ));
                }
                else
                {
                    throw new SecurityException("Attempt of unauthorized access");
                }
            }
            catch (Exception e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpGet, Route("ChangeDishIndex")]
        public IActionResult ChangeDishIndex(long cafeId, long categoryId, int newIndex, int oldIndex, int? dishId = null)
        {
            try
            {
                Accessor.Instance.ChangeDishIndex(cafeId, categoryId, newIndex, oldIndex, dishId);

                return Ok();
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpGet, Route("CheckUniqueNameWithinCafe")]
        public IActionResult CheckUniqueNameWithinCafe(string dishName, long cafeId, long dishId = -1)
        {
            return Ok(Accessor.Instance.CheckUniqueNameWithinCafe(dishName, cafeId, dishId));
        }

        [Authorize(Roles = "Manager")]
        [HttpPut, Route("")]
        public IActionResult UpdateDish([FromBody]FoodDishModel dish)
        {
            try
            {
                if (dish == null)
                {
                    return BadRequest("Empty Dish");
                }

                var currentUser =
                    User.Identity.GetUserById();

                var categoryIds = dish.FoodCategories.Where(d => d.IsSelected).Select(c => c.Id).ToArray();

                var oldDish =
                    Accessor.Instance.GetFoodDishesById(new[] { dish.Id });

                if (oldDish != null && oldDish.Count == 1)
                {
                    DishServiceHelper.DishValidation(dish, oldDish[0]);

                    oldDish[0].VersionTo = dish.VersionTo;

                    oldDish[0].LastUpdateByUserId = currentUser.Id;

                    var categoryList =
                        Accessor
                        .Instance
                        .GetFoodCategoriesByCafeId(
                            oldDish[0].DishCategoryLinks.FirstOrDefault().CafeCategory.CafeId
                        );

                    if (!categoryList.Any(c => categoryIds.Contains(c.DishCategoryId)))
                    {
                        throw new ValidationException("Категория не существует");
                    }

                    oldDish[0].Description = dish.Description;

                    oldDish[0].Composition = dish.Composition;
                    oldDish[0].ImageId = dish.Image;

                    if (!Accessor.Instance.CheckUniqueNameWithinCafe(
                        dish.Name, oldDish[0].DishCategoryLinks.FirstOrDefault().CafeCategory.CafeId, oldDish[0].Id))
                    {
                        throw new ValidationException("В кафе уже существует блюдо с таким названием");
                    }

                    return Ok(Accessor.Instance.EditDish(oldDish[0], categoryIds, currentUser.Id));
                }
                else
                {
                    throw new ValidationException("Блюдо не найдено");
                }
            }
            catch (Exception e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }

        }

        [HttpPost, Route("filter")]
        public IActionResult GetFoodDishesByFilter([FromBody]DishFilterModel filter)
        {
            var context = Accessor.GetContext();

            var allDishes = new List<FoodDishModel>();
            var query =
                context.Dishes.Where(c => filter.DishIds.Contains(c.Id) && !c.IsDeleted && c.IsActive == true);

            if (filter.Date.HasValue)
            {
                query = query.Where(c =>
                    c.VersionFrom != null && c.VersionFrom < filter.Date && filter.Date < c.VersionFrom &&
                    c.VersionFrom != null);
            }
            var dishes = query.ToList();
            allDishes.AddRange(dishes.Select(c => c.GetContractDish()));
            var notInDishes = filter.DishIds.Except(dishes.Select(c => c.Id));
            if (notInDishes.Any())
            {
                var queryDishVersions =
                    context.DishVersions.Where(
                        c => filter.DishIds.Contains(c.Id) && !c.IsDeleted && c.IsActive == true);
                if (filter.Date.HasValue)
                {
                    queryDishVersions = queryDishVersions.Where(c =>
                        c.VersionFrom != null && c.VersionFrom < filter.Date && filter.Date < c.VersionFrom &&
                        c.VersionFrom != null);
                }
                var dishVersions = queryDishVersions.ToList();
                allDishes.AddRange(dishVersions.Select(c => c.GetContractDish()));
            }

            return Ok(allDishes);
        }

        [HttpGet, Route("{id:long}/date/{unixDate:long}")]
        public IActionResult GetFoodDishByDate(long id, long unixDate)
        {
            var context = Accessor.GetContext();

            var date = DateTimeExtensions.FromUnixTime(unixDate);
            var dish =
                context.Dishes.FirstOrDefault(c =>
                    !c.IsDeleted && c.IsActive == true && c.Id == id);

            if (dish == null || dish.VersionFrom != null && dish.VersionFrom > date ||
                date > dish.VersionTo && dish.VersionTo != null)
            {

                var dishVersion =
                    context.DishVersions.FirstOrDefault(c =>
                        !c.IsDeleted && c.IsActive == true && c.DishId == id &&
                        (c.VersionFrom <= date || c.VersionFrom == null)
                        && (c.VersionTo >= date || c.VersionTo == null));

                return Ok(dishVersion.GetContractDish());
            }
            return Ok(dish.GetContractDish());
        }

        [HttpPost, Route("GetDishesBySearchTermAndTagListAndCafeId")]
        public IActionResult GetDishesBySearchTermAndTagListAndCafeId(string searchTerm, [FromBody] List<long> tagIds, long cafeId)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                {
                    return null;
                }

                var dishesFromBase =
                    Accessor.Instance.GetDishesBySearchTermAndTagListAndCafeId(
                        tagIds,
                        cafeId,
                        searchTerm.ToLower()
                        );

                var foodDishList = new List<FoodDishModel>();

                foreach (var dishItem in dishesFromBase)
                {
                    foodDishList.Add(dishItem.GetContractDish());
                }

                return Ok(foodDishList);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// cafeID = 1 передается в параметрах. Не используется, но все же
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("GetFirst100Dishes")]
        public IActionResult GetFirst100Dishes()
        {
            var ret = new List<OrderModel>();

            var cache = MemoryCache.Default;

            var dishes = cache.GetCacheItem("orders");

            if (dishes == null)
            {
                var dishesFromBase = Accessor.Instance.GetCurrentListOfOrdersToCafe(1, null);

                for (var i = 0; i < dishesFromBase.Count; i++)
                {
                    ret.Add(new OrderModel
                    {
                        Id = dishesFromBase[i].Id
                    });
                }

                var ci = new CacheItem("orders", ret);
                cache.Add(ci,
                    new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddDays(1) });
            }
            else
            {
                ret = (List<OrderModel>)dishes.Value;
            }

            return Ok(ret);
        }

        [HttpGet, Route("GetFoodCafeByDishId")]
        public IActionResult GetFoodCafeByDishId(long id)
        {
            var dish =
                Accessor.Instance.GetFoodDishesById(new[] { id });

            if (dish != null
                && dish.Count == 1)
            {
                var cafe =
                    dish[0].DishCategoryLinks.FirstOrDefault().CafeCategory.Cafe;

                return Ok(cafe.GetContract());
            }

            return Ok();
        }

        [HttpGet, Route("GetFoodCategoryAndFoodDishesByCafeIdAndDate")]
        public IActionResult GetFoodCategoryAndFoodDishesByCafeIdAndDate(long cafeId, long? wantedDate)
        {
            var dateTime = wantedDate != null ? DateTimeExtensions.FromUnixTime((long)wantedDate) : DateTime.Now;
            var groups = Accessor.Instance.GroupDishesByCategory(cafeId, dateTime);
            var result = groups.Select(x => new FoodCategoryWithDishes
            {
                Category = x.Item1.GetContract(),
                Dishes = x.Item2.Select(d => d.GetContractDish()).ToList()
            });
            return Ok(result);
        }

        // TODO: удалить
        [HttpGet, Route("GetFoodCategoryByDishId")]
        public IActionResult GetFoodCategoryByDishId(long id)
        {
            //var dish =
            //    Accessor.Instance.GetFoodDishesById(new[] { id });

            //if (dish != null
            //    && dish.Count == 1)
            //{
            //    var category = dish[0].CafeCategory.DishCategory;

            //    var categoryFromBase = category.GetContract();
            //    categoryFromBase.Index = dish[0].CafeCategory.Index;

            //    return Ok(categoryFromBase);
            //}
            return null;
        }

        [HttpGet, Route("GetFoodCategoriesByDishId")]
        public IActionResult GetFoodCategoriesByDishId(long id)
        {
            var dish = Accessor.Instance.GetFoodDishesById(new[] { id });

            if (dish != null && dish.Count == 1)
            {
                return Ok(dish[0].DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted)
                    .Select(l => l.CafeCategory.DishCategory.GetContract()).ToList());
            }
            return null;
        }

        [HttpGet, Route("GetFoodDishAllVersion")]
        public IActionResult GetFoodDishAllVersion(long dishId)
        {
            var foodDishVersionList = new List<FoodDishVersionModel>();

            foreach (var dishVersionItem in Accessor.Instance.GetFoodDishVersionsById(dishId))
            {
                foodDishVersionList.Add(dishVersionItem.GetContractDishVersion());
            }

            var currentDish = Accessor.Instance.GetFoodDishesById(new List<long> { dishId }.ToArray());

            if (currentDish != null
                && currentDish.Count > 0)
            {
                foodDishVersionList.Add(
                    currentDish[0].GetContractDishVersionFromDish()
                    );
            }

            return Ok(foodDishVersionList);
        }

        [HttpGet, Route("")]
        public IActionResult GetFoodDishById(long dishId)
        {
            var dish = Accessor.Instance.GetFoodDishesById(new[] { dishId }).FirstOrDefault();

            return Ok(dish.GetContractDish());
        }

        [HttpGet, Route("GetFoodDishByIdAndDate")]
        public IActionResult GetFoodDishByIdAndDate(long dishId, long date)
        {
            var dateTime = DateTimeExtensions.FromUnixTime(date);
            return Ok(GetFoodDishModelByIdAndDate(Accessor.Instance, dishId, dateTime));
        }

        [HttpGet, Route("GetFoodDishesByCafeId")]
        public IActionResult GetFoodDishesByCafeId(long cafeId, long? date)
        {
            DateTime? dateTime = null;
            if (date != null) dateTime = DateTimeExtensions.FromUnixTime((long)date);

            var foodDishList = new List<FoodDishModel>();

            foreach (var dishItem in Accessor.Instance.GetFoodDishesByCafeId(cafeId, dateTime))
            {
                foodDishList.Add(dishItem.GetContractDish());
            }

            return Ok(foodDishList);
        }

        [HttpGet, Route("GetFoodDishesByCategoryIdAndCafeId")]
        public IActionResult GetFoodDishesByCategoryIdAndCafeId(long cafeId, long categoryId, DateTime? date)
        {
            var foodDishList = new List<FoodDishModel>();

            var dateTime = date ?? DateTime.Now;

            foreach (var dishItem in Accessor.Instance.GetFoodDishesByCategoryIdAndCafeId(cafeId, categoryId, dateTime))
            {
                foodDishList.Add(dishItem.GetContractDish());
            }

            return Ok(foodDishList);
        }

        [HttpDelete, Route("")]
        public IActionResult DeleteDish(long dishId, long? categoryId = null)
        {
            try
            {
                var currentUser =
                User.Identity.GetUserById();

                if (Accessor.Instance.GetFoodDishesById(new[] { dishId }).FirstOrDefault() is Dish dish)
                {
                    if (Accessor.Instance.IsUserManagerOfCafe(
                        currentUser.Id, dish.DishCategoryLinks.FirstOrDefault().CafeCategory.CafeId))
                    {
                        //Проверяет наличие блюда в заказах
                        //if (CheckDishExistingInOrders(dishId, null, null, null))
                        //{
                        //    return -1;
                        //}

                        return Ok(Accessor.Instance.RemoveDish(dishId, currentUser.Id, categoryId));
                    }
                    else
                    {
                        throw new SecurityException("Attempt of unauthorized access");
                    }
                }
                {
                    return Ok(-1);
                }
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }

        }

        [Authorize(Roles = "Manager")]
        [HttpPut, Route("UpdateDishIndexInFirstCategory/{cafeId:long}/{newCategoryId:long}/{oldCategoryId:long}/{newIndex:long}/{oldIndex:long}/{dishId:long}")]
        public IActionResult UpdateDishIndexInFirstCategory(long cafeId, long newCategoryId, long oldCategoryId, int newIndex, int oldIndex, long dishId)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                Accessor.Instance.UpdateDishIndexInFirstCategory(cafeId, newCategoryId, oldCategoryId, newIndex, oldIndex, dishId, userId);

                return Ok();
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpPut, Route("UpdateDishIndexInSecondCategory/{cafeId:long}/{categoryId:long}/{newIndex:long}/{dishId:long}")]
        public IActionResult UpdateDishIndexInSecondCategory(long cafeId, long categoryId, int newIndex, long dishId)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                Accessor.Instance.UpdateDishIndexInSecondCategory(cafeId, categoryId, newIndex, dishId, userId);

                return Ok();
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        public static FoodDishModel GetFoodDishModelByIdAndDate(Accessor accessor, long dishId, DateTime dateTime)
        {
            var dish = accessor.GetFoodDishesById(new[] { dishId }).FirstOrDefault();

            if (dish == null
                || (
                    (dish.VersionFrom != null
                     && dish.VersionFrom > dateTime
                        )
                    || (dish.VersionTo != null
                        && dish.VersionTo < dateTime
                        )
                    )
                )
            {
                var dishVersion = accessor.GetFoodDishVersionByDishIdAndDate(dishId, dateTime).FirstOrDefault();
                if (dishVersion == null)
                {
                    return null;
                }
                return dishVersion.GetContractDish();
            }
            return dish.GetContractDish();
        }

    }

    public static class DishExtensions
    {
        public static FoodDishModel GetContractDish(this Dish dish)
        {
            if (dish == null) return null;
            long cafeId = 0;
            string cafeShortName = string.Empty;
            string cafeCleanUrl = string.Empty;
            string cafeLogoStr = string.Empty;
            DateTime? cafeTimeFrom = null;
            DateTime? cafeTimeTo = null;
            if (dish.DishCategoryLinks.Count > 0 && dish.Cafe != null)
            {
                cafeId = dish.Cafe.Id;
                cafeShortName = dish.Cafe.CafeName;
                cafeCleanUrl = dish.Cafe.CleanUrlName;
                cafeLogoStr = dish.Cafe.Logo;
                cafeTimeFrom = ITWebNet.FoodService.Food.Data.Accessor.Extensions.CafeExtensions.GetWorkingTimeToday(
                    dish.Cafe.WorkingTimeFrom);
                cafeTimeTo = ITWebNet.FoodService.Food.Data.Accessor.Extensions.CafeExtensions.GetWorkingTimeToday(
                    dish.Cafe.WorkingTimeTo);
            }
            return new FoodDishModel
            {
                Id = dish.Id,
                Name = dish.DishName,
                BasePrice = dish.BasePrice,
                Kcalories = dish.Kcalories,
                Weight = dish.Weight,
                WeightDescription = dish.WeightDescription,
                VersionFrom = dish.VersionFrom,
                VersionTo = dish.VersionTo,
                //DishIndex = dish.DishIndex,
                Uuid = dish.Uuid,
                //CategoryId = dish.CafeCategory.DishCategoryId,
                DishRatingCount = dish.DishRatingCount,
                DishRatingSumm = dish.DishRatingSumm,
                Description = dish.Description,
                Image = dish.ImageId,
                Composition = dish.Composition,
                Schedules = dish.Schedules?.Select(c => c.GetContract()).ToList() ?? new List<ScheduleModel>(),
                CafeId = cafeId,
                CafeName = cafeShortName,
                CafeUrl = cafeCleanUrl,
                CafeLogo = cafeLogoStr,
                CafeTimeFrom = cafeTimeFrom,
                CafeTimeTo = cafeTimeTo,
                DishIndexToCategory = new Dictionary<long, int?>(dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted).Select(
                    l => new KeyValuePair<long, int?>(l.CafeCategory.DishCategoryId, l.DishIndex)).Distinct()),
            };
        }
        public static FoodDishModel GetContractDishForCategory(this Dish dish, long categoryId)
        {
            if (dish == null) return null;
            long cafeId = 0;
            string cafeShortName = string.Empty;
            string cafeCleanUrl = string.Empty;
            string cafeLogoStr = string.Empty;
            DateTime? cafeTimeFrom = null;
            DateTime? cafeTimeTo = null;
            if (dish.DishCategoryLinks.Count > 0 && dish.Cafe != null)
            {
                cafeId = dish.Cafe.Id;
                cafeShortName = dish.Cafe.CafeName;
                cafeCleanUrl = dish.Cafe.CleanUrlName;
                cafeLogoStr = dish.Cafe.Logo;
                cafeTimeFrom = ITWebNet.FoodService.Food.Data.Accessor.Extensions.CafeExtensions.GetWorkingTimeToday(
                    dish.Cafe.WorkingTimeFrom);
                cafeTimeTo = ITWebNet.FoodService.Food.Data.Accessor.Extensions.CafeExtensions.GetWorkingTimeToday(
                    dish.Cafe.WorkingTimeTo);
            }
            return new FoodDishModel
            {
                Id = dish.Id,
                Name = dish.DishName,
                BasePrice = dish.BasePrice,
                Kcalories = dish.Kcalories,
                Weight = dish.Weight,
                WeightDescription = dish.WeightDescription,
                VersionFrom = dish.VersionFrom,
                VersionTo = dish.VersionTo,
                DishIndex = Accessor.Instance.GetDishIndexInCategory(dish.Id, categoryId),
                Uuid = dish.Uuid,
                CategoryId = categoryId,
                DishRatingCount = dish.DishRatingCount,
                DishRatingSumm = dish.DishRatingSumm,
                Description = dish.Description,
                Image = dish.ImageId,
                Composition = dish.Composition,
                Schedules = dish.Schedules?.Select(c => c.GetContract()).ToList() ?? new List<ScheduleModel>(),
                CafeId = cafeId,
                CafeName = cafeShortName,
                CafeUrl = cafeCleanUrl,
                CafeLogo = cafeLogoStr,
                CafeTimeFrom = cafeTimeFrom,
                CafeTimeTo = cafeTimeTo
            };
        }

        public static FoodDishVersionModel GetContractDishVersion(this DishVersion dish)
        {
            return (dish == null)
                ? null
                : new FoodDishVersionModel
                {
                    Id = dish.Id,
                    DishId = dish.DishId,
                    Name = dish.DishName,
                    BasePrice = dish.BasePrice,
                    Kcalories = dish.Kcalories,
                    Weight = dish.Weight,
                    WeightDescription = dish.WeightDescription,
                    VersionFrom = dish.VersionFrom,
                    VersionTo = dish.VersionTo
                };
        }

        public static FoodDishVersionModel GetContractDishVersionFromDish(this Dish dish)
        {
            return (dish == null)
                ? null
                : new FoodDishVersionModel
                {
                    Id = dish.Id,
                    DishId = dish.Id,
                    Name = dish.DishName,
                    BasePrice = dish.BasePrice,
                    Kcalories = dish.Kcalories,
                    Weight = dish.Weight,
                    WeightDescription = dish.WeightDescription,
                    VersionFrom = dish.VersionFrom,
                    VersionTo = dish.VersionTo
                };
        }

        public static Dish GetEntityFromDishVersion(this DishVersion dishVersionItem)
        {
            return (dishVersionItem == null)
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
                };
        }

        public static FoodDishModel GetContractDish(this DishVersion dish)
        {
            return (dish == null)
                ? null
                : new FoodDishModel
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
                    CategoryId = dish.CafeCategory.DishCategoryId,
                    Image = dish.ImageId
                };
        }
    }
}
