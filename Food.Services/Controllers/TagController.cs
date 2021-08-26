using Food.Data;
using Food.Data.Entities;
using Food.Services.Extensions;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Food.Services.Controllers
{
    [Route("api/tags")]
    public class TagController : ContextableApiController
    {
        [ActivatorUtilitiesConstructor]
        public TagController()
        {
            Logger = Log.Logger;
        }

        public TagController(IFoodContext context, Accessor accessor, Serilog.ILogger logger)
        {
            Accessor = accessor;
            Accessor.SetTestingModeOn(context);
            Context = context;
            TestMode = true;
            Logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        [Route("add")]
        public IActionResult AddNewTag([FromBody]TagModel model)
        {
            try
            {
                var currentUser = User.Identity.GetUserId();

                var tag = model.GetEntity();
                tag.CreateDate = DateTime.Now;
                tag.CreatorId = currentUser;
                tag.IsActive = true;

                return Ok(GetAccessor().AddTag(tag));

            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2}{3}{4}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, Environment.NewLine, e.StackTrace);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("addtocafe/cafe/{cafeId:long}/tag/{tagId:long}")]
        public IActionResult AddTagToCafe(long cafeId, long tagId)
        {
            try
            {
                var currentUser = User.Identity.GetUserId();

                var cafe =
                    GetAccessor().GetCafeById(cafeId);

                if (cafe == null) throw new Exception("Attempt to work with unexisting object");

                var tagObject = new TagObject
                {
                    CreateDate = DateTime.Now,
                    CreatorId = currentUser,
                    ObjectId = cafeId,
                    ObjectTypeId = (int)ObjectTypesEnum.CAFE,
                    TagId = tagId
                };

                return Ok(GetAccessor().AddTagObject(tagObject));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2}{3}{4}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, Environment.NewLine, e.StackTrace);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpGet]
        [Route("addtodish/dish/{dishId:long}/tag/{tagId:long}")]
        public IActionResult AddTagToDish(long dishId, long tagId)
        {
            try
            {
                var currentUser = User.Identity.GetUserId();

                var dish = GetAccessor().GetFoodDishesById(new[] { dishId }).FirstOrDefault();
                if (dish == null ||
                    !GetAccessor().IsUserManagerOfCafe(currentUser, 
                    dish.DishCategoryLinks.FirstOrDefault().CafeCategory.CafeId) 
                    || GetAccessor().GetListOfTagsConnectedWithObjectAndHisChild(
                        dish.DishCategoryLinks.FirstOrDefault().CafeCategory.CafeId,
                        (long)ObjectTypesEnum.CAFE).All(t => t.Id != tagId)) return Ok(false);
                var tagObject = new TagObject
                {
                    CreateDate = DateTime.Now,
                    CreatorId = currentUser,
                    ObjectId = dishId,
                    ObjectTypeId = (long)ObjectTypesEnum.DISH,
                    TagId = tagId
                };
                return Ok(GetAccessor().AddTagObject(tagObject));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2}{3}{4}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, Environment.NewLine, e.StackTrace);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("edit")]
        public IActionResult EditTag([FromBody]TagModel model)
        {
            try
            {
                var currentUser = User.Identity.GetUserId();
                var tag = model.GetEntity();
                tag.IsActive = true;
                tag.LastUpdateByUserId = currentUser;
                tag.LastUpdDate = DateTime.Now;

                return Ok(GetAccessor().EditTag(tag));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2}{3}{4}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, Environment.NewLine, e.StackTrace);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        [Route("")]
        public IActionResult GetAllTags()
        {
            try
            {
                var tags = new List<TagModel>();
                var currentUserId = User.Identity.GetUserId();
                var cafesId = GetAccessor().GetCafesByUserId(currentUserId)
                    .Select(c => c.Id)
                    .ToList();

                var tagsFromBase =
                    GetAccessor().GetTagsByCafesList(cafesId);

                foreach (var tag in tagsFromBase)
                    tags.Add(tag.GetContract());

                return Ok(tags);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2}{3}{4}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, Environment.NewLine, e.StackTrace);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        [Route("childrenbyparentid/{tagId:long}")]
        public IActionResult GetChildTagsByTagId(long tagId)
        {
            try
            {
                var tags = new List<TagModel>();

                var tagsFromBase =
                    GetAccessor().GetChildListOfTagsByTagId(tagId);

                foreach (var tag in tagsFromBase)
                    tags.Add(tag.GetContract());

                return Ok(tags);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2}{3}{4}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, Environment.NewLine, e.StackTrace);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("getDishesByTagListAndCafeId/{cafeId:long}")]
        public IActionResult GetDishesByTagListAndCafeId([FromBody]List<long> tagIds, long cafeId)
        {
            var dishesFromBase = GetAccessor().GetDishesByTagListAndCafeId(tagIds, cafeId);

            var foodDishList = new List<FoodDishModel>();

            foreach (var dishItem in dishesFromBase)
                foodDishList.Add(dishItem.GetContractDish());

            return Ok(foodDishList);
        }

        [HttpPost]
        [Route("getFoodCategoriesSearchTermAndTagsAndCafeId")]
        public IActionResult GetFoodCategoriesSearchTermAndTagsAndCafeId(string searchTerm, [FromBody]List<long> tagIds,
            long cafeId)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                    return null;

                var foodCategoryList = new List<FoodCategoryModel>();

                foreach (
                    var cafeCategoryItem
                    in GetAccessor().GetCategorysBySearchTermAndTagListAndCafeId(
                        tagIds,
                        cafeId,
                        searchTerm.ToLower()
                    )
                )
                {
                    var category = cafeCategoryItem.DishCategory.GetContract();
                    category.Index = cafeCategoryItem.Index;
                    foodCategoryList.Add(category);
                }

                return Ok(foodCategoryList);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2}{3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("getFoodCategoriesTagsAndCafeId/{cafeId:long}")]
        public IActionResult GetFoodCategoriesTagsAndCafeId([FromBody]List<long> tagIds, long cafeId)
        {
            var foodCategoryList = new List<FoodCategoryModel>();

            foreach (
                var cafeCategoryItem
                in GetAccessor().GetCategorysByTagListAndCafeId(
                    tagIds,
                    cafeId
                )
            )
            {
                var category = cafeCategoryItem.DishCategory.GetContract();
                category.Index = cafeCategoryItem.Index;
                foodCategoryList.Add(category);
            }

            return Ok(foodCategoryList);
        }

        /// <summary>
        /// Получение списка кафе по строке поиска
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="tagIds"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getListOfCafesBySearchTermAndTagsList")]
        public IActionResult GetListOfCafesBySearchTermAndTagsList(string searchTerm, [FromBody]List<long> tagIds)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm)) return Ok(null);
                DateTime? date = DateTime.Now.Date;
                // Создаем пустой список кафе cafes. Его наполним и вернем в итоге.
                var cafes = new List<CafeModel>();

                // Получаем список Id кафе, доступных пользователю.
                var cafesId = tagIds == null
                    ? GetAccessor().GetCafes().Select(c => c.Id).ToList()
                    : GetListOfCafesByTagsListHelper(tagIds).Select(c => c.Id).ToList();
                // Создаем новый пустой список подходящих по поисковому запросу кафе.
                var rightCafes = new List<Cafe>();
                // Переменная isSuitable определяет, подходит ли нам кафе.

                foreach (var cafeId in cafesId)
                {
                    var isSuitable = false;
                    // Получаем доступные категории и блюда на текущую дату.
                    var baseDishList = GetAccessor().GetDishesByScheduleByDate(cafeId, date);
                    var scheduledDishList = new Dictionary<FoodCategoryModel, List<FoodDishModel>>();
                    foreach (var dish in baseDishList)
                        scheduledDishList.Add(dish.Key.GetContract(),
                            dish.Value.Select(e => e.GetContractDish()).ToList());

                    // Проверяем по названию кафе.
                    var currentCafe = GetAccessor().GetCafeById(cafeId);
                    if (currentCafe != null)
                    {
                        var cafeName = currentCafe.CafeName.ToLower();
                        if (cafeName.Contains(searchTerm.ToLower()))
                            isSuitable = true;
                    }

                    foreach (var category in scheduledDishList)
                    {
                        var categoryName = category.Key.Name.ToLower();
                        // Проверяем по категориям блюд.
                        if (categoryName.Contains(searchTerm.ToLower()))
                        {
                            isSuitable = true;
                            break;
                        }

                        foreach (var dish in category.Value.ToList())
                        {
                            var dishName = dish.Name.ToLower();
                            //Проверяем по названиям блюд.
                            if (dishName.Contains(searchTerm.ToLower()))
                            {
                                isSuitable = true;
                                break;
                            }
                        }
                    }

                    // Если кафе подходящее, добавляем его в список rightCafes.
                    if (isSuitable)
                        rightCafes.Add(currentCafe);
                }

                // Переносим подходящие кафе в возвращаемый список.
                foreach (var cafeItem in rightCafes) cafes.Add(cafeItem.GetContract());

                return Ok(cafes);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("getListOfCafesByTagsList")]
        public IActionResult GetListOfCafesByTagsList([FromBody]List<long> tagIds)
        {
            return Ok(GetListOfCafesByTagsListHelper(tagIds));
        }

        private List<CafeModel> GetListOfCafesByTagsListHelper(List<long> tagIds)
        {
            var cafesFromBase =
                GetAccessor().GetCafesByTagList(tagIds);

            var cafes = new List<CafeModel>();

            foreach (var cafeItem in cafesFromBase)
                cafes.Add(cafeItem.GetContract());

            return cafes;
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        [Route("tagsByString")]
        public IActionResult GetListOfTagsByString(string textToFind)
        {
            try
            {
                var tags = new List<TagModel>();

                var tagsFromBase =
                    GetAccessor().GetListOfTagsByString(textToFind);

                foreach (var tag in tagsFromBase) tags.Add(tag.GetContract());

                return Ok(tags);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        [Route("getListOfTagsConnectedWithObjectAndHisChild/{objectId:long}/{typeOfObject:long}")]
        public IActionResult GetListOfTagsConnectedWithObjectAndHisChild(long objectId, long typeOfObject)
        {
            try
            {
                var tags = new List<TagModel>();

                var tagsFromBase =
                    GetAccessor()
                        .GetListOfTagsConnectedWithObjectAndHisChild(
                            objectId, typeOfObject
                        );

                foreach (var tag in tagsFromBase)
                    tags.Add(tag.GetContract());

                return Ok(tags);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        [Route("roottags")]
        public IActionResult GetRootTags()
        {
            try
            {
                var tags = new List<TagModel>();

                var tagsFromBase = GetAccessor().GetRootTags();

                foreach (var tag in tagsFromBase)
                    tags.Add(tag.GetContract());

                return Ok(tags);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        [Route("tagbyid/{tagId:long}")]
        public IActionResult GetTagByTagId(long tagId)
        {
            try
            {
                TagModel tag = null;
                var tagFromBase = GetAccessor().GetTagById(tagId);
                if (tagFromBase != null)
                    tag = tagFromBase.GetContract();
                return Ok(tag);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public IActionResult RemoveTag(long tagId)
        {
            try
            {
                var currentUser = User.Identity.GetUserId();
                return Ok(GetAccessor().RemoveTag(tagId, currentUser));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Manager")]
        [Route("removeTagObjectLink/{objectId:long}/{objectType:int}/{tagId:long}")]
        public IActionResult RemoveTagObjectLink(long objectId, int objectType, long tagId)
        {
            try
            {
                var currentUser = User.Identity.GetUserId();

                var tagObject = GetAccessor().GetTagObjectById(objectId, objectType, tagId);

                if (tagObject == null) return Ok(false);
                var objectFromTag = GetAccessor().GetObjectFromTagObject(tagObject);

                var hasUserRightsToObject = false;

                if (objectFromTag == null) return Ok(false);

                if (objectFromTag is Dish)
                {
                    var dish = (Dish)objectFromTag;
                    hasUserRightsToObject = GetAccessor().IsUserManagerOfCafe(
                        currentUser, dish.DishCategoryLinks.FirstOrDefault().CafeCategory.CafeId);
                }

                if (hasUserRightsToObject)
                    return Ok(GetAccessor().RemoveTagObject(tagObject.Id, currentUser));
                throw new SecurityException("Attempt of unauthorized access");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }
    }
}
