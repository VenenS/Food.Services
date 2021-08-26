using Food.Data.Entities;
using Food.Data;
using Food.Services.Extensions;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ITWebNet.Food.Core.DataContracts.Manager;
using ITWebNet.FoodService.Food.Data.Accessor.Extensions;

namespace Food.Services.Controllers
{
    [Authorize(Roles = "Admin, Manager")]
    [Route("api/categories")]
    public class CategoriesController : ContextableApiController
    {
        public CategoriesController(IFoodContext context, Accessor accessor) 
        {
            Accessor = accessor;
            Accessor.SetTestingModeOn(context);
            Context = context;
            TestMode = true;
        }

        [ActivatorUtilitiesConstructor]
        public CategoriesController()
        { }

        [HttpGet, Route("AddCafeFoodCategory")]
        public IActionResult AddCafeFoodCategory(long cafeId, long categoryId, int categoryIndex)
        {
            try
            {
                var currentUser = User.Identity.GetUserId();

                if (GetAccessor().IsUserManagerOfCafe(currentUser, cafeId))
                {
                    return Ok(GetAccessor().AddCafeFoodCategory(cafeId, categoryId, categoryIndex, currentUser));
                }
                throw new SecurityException("Attempt of unauthorized access");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2}{3}{4}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, Environment.NewLine, e.StackTrace);
                return BadRequest(e.Message);
            }
        }

        [HttpGet, Route("ChangeFoodCategoryOrder")]
        public IActionResult ChangeFoodCategoryOrder(long cafeId, long categoryId, long categoryOrder)
        {
            try
            {
                var currentUser = User.Identity.GetUserId();

                GetAccessor().ChangeFoodCategoryOrder(cafeId, categoryId, categoryOrder, currentUser);

                return Ok();
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2}{3}{4}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, Environment.NewLine, e.StackTrace);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("")]
        public IActionResult Get()
        {
            return Ok(GetAccessor().GetFoodCategories().Where(c => !c.IsDeleted).ToList()
                .Select(c => c.GetContract()).ToList());
        }

        [HttpGet]
        [Route("{id:long}")]
        public IActionResult Get(long id)
        {
            return Ok(GetAccessor().GetFoodCategoryById(id).GetContract());
        }

        [HttpPost]
        [Route("")]
        public IActionResult Post([FromBody]FoodCategoryModel category)
        {
            var context = Accessor.Instance.GetContext();

            var dishCategory = GetAccessor().GetFoodCategories().FirstOrDefault(
                c => !c.IsDeleted && c.CategoryName == category.Name.Trim()
                && c.CategoryFullName == category.FullName.Trim());

            if (dishCategory != null)
                return BadRequest("Категория с таким названием уже существует");

            var newDishCategory = new DishCategory()
            {
                CategoryFullName = category.FullName,
                Description = category.Description,
                CategoryName = category.Name,
                CreationDate = DateTime.Now,
                CreatorId = User.Identity.GetUserId(),
                IsActive = true,
                IsDeleted = false,
                Uuid = Guid.NewGuid()
            };

            context.DishCategories.Add(newDishCategory);

            context.SaveChanges();

            return Ok(newDishCategory.GetContract());
        }

        [HttpPut]
        [Route("")]
        public IActionResult Put([FromBody]FoodCategoryModel category)
        {
            var context = Accessor.Instance.GetContext();

            var dishCategory =
                context.DishCategories.FirstOrDefault(c => !c.IsDeleted && c.IsActive == true && c.Id == category.Id);
            if (dishCategory == null)
                return NotFound();

            dishCategory.CategoryFullName = category.FullName;
            dishCategory.CategoryName = category.Name;
            dishCategory.LastUpdateByUserId = User.Identity.GetUserId();
            dishCategory.LastUpdDate = DateTime.Now;
            dishCategory.Description = category.Description;

            context.SaveChanges();

            return Ok(dishCategory.GetContract());
        }

        [HttpDelete]
        [Route("{id:long}")]
        public IActionResult Delete(long id)
        {
            var context = Accessor.Instance.GetContext();

            var dishCategory = context.DishCategories.FirstOrDefault(c => c.Id == id);
            if (dishCategory == null)
                return NotFound();

            var userId = User.Identity.GetUserId();

            dishCategory.IsActive = false;
            dishCategory.IsDeleted = true;
            dishCategory.LastUpdateByUserId = userId;
            dishCategory.LastUpdDate = DateTime.Now;

            context.SaveChanges();

            GetAccessor().MoveDishesFromCategory(id, userId);

            return Ok();
        }



        [HttpGet, Route("RemoveCafeFoodCategory")]
        public IActionResult RemoveCafeFoodCategory(long cafeId, long categoryId)
        {
            try
            {
                var currentUser = User.Identity.GetUserId();

                if (GetAccessor().IsUserManagerOfCafe(currentUser, cafeId))
                {
                    return Ok(GetAccessor().RemoveCafeFoodCategory(cafeId, categoryId, currentUser));
                }
                else
                {
                    throw new SecurityException("Attempt of unauthorized access");
                }
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2}{3}{4}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, Environment.NewLine, e.StackTrace);
                return BadRequest(e.Message);
            }
        }

        #region CafeCategory

        [HttpGet, Route(nameof(GetCategoryBusinessHours))]
        public async Task<IActionResult> GetCategoryBusinessHours(long cafeId, long categoryId)
        {
            try
            {
                var user = User.Identity.GetUserById();

                if (Accessor.Instance.IsUserManagerOfCafeIgnoreActivity(user.Id, cafeId))
                {
                    var businessHours = Accessor.Instance.GetCategoryBusinessHours(cafeId, categoryId);

                    var cafeBusinessHours = new CategoryBusinessHoursModel
                    {
                        Departures = businessHours.Departures != null ? businessHours.Departures.Select(d => new CafeBusinessHoursDepartureModel
                        {
                            Date = d.Date,
                            IsDayOff = d.IsDayOff,
                            Items = d.Items != null ? d.Items.Select(i => new CafeBusinessHoursItemModel
                            {
                                ClosingTime = i.ClosingTime,
                                OpeningTime = i.OpeningTime
                            }).ToList() : null
                        }).ToList() : null
                    };

                    cafeBusinessHours.Friday = businessHours.Friday != null ? businessHours.Friday.Select(i => new CafeBusinessHoursItemModel
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : null;

                    cafeBusinessHours.Monday = businessHours.Monday != null ? businessHours.Monday.Select(i => new CafeBusinessHoursItemModel
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : null;

                    cafeBusinessHours.Saturday = businessHours.Saturday != null ? businessHours.Saturday.Select(i => new CafeBusinessHoursItemModel
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : null;

                    cafeBusinessHours.Sunday = businessHours.Sunday != null ? businessHours.Sunday.Select(i => new CafeBusinessHoursItemModel
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : null;

                    cafeBusinessHours.Thursday = businessHours.Thursday != null ? businessHours.Thursday.Select(i => new CafeBusinessHoursItemModel
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : null;

                    cafeBusinessHours.Tuesday = businessHours.Tuesday != null ? businessHours.Tuesday.Select(i => new CafeBusinessHoursItemModel
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : null;

                    cafeBusinessHours.Wednesday = businessHours.Wednesday != null ? businessHours.Wednesday.Select(i => new CafeBusinessHoursItemModel
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : null;

                    return Ok(cafeBusinessHours);
                }
                else
                {
                    throw new SecurityException("Unauthorized user.");
                }
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpPost, Route(nameof(SetCategoryBusinessHours))]
        public async Task<IActionResult> SetCategoryBusinessHours([FromBody] CategoryBusinessHoursModel categoryBusinessHours)
        {
            try
            {
                var user = User.Identity.GetUserById();

                if (Accessor.Instance.IsUserManagerOfCafeIgnoreActivity(user.Id, categoryBusinessHours.CafeId))
                {
                    var businessHours = new BusinessHours
                    {
                        Departures = categoryBusinessHours.Departures != null ? categoryBusinessHours.Departures.Select(d => new BusinessHoursDeparture
                        {
                            Date = d.Date,
                            IsDayOff = d.IsDayOff,
                            Items = d.Items != null ? d.Items.Select(i => new BusinessHoursItem
                            {
                                ClosingTime = i.ClosingTime,
                                OpeningTime = i.OpeningTime
                            }).ToList() : new List<BusinessHoursItem>()
                        }).ToList() : new List<BusinessHoursDeparture>()
                    };

                    businessHours.Friday = categoryBusinessHours.Friday != null ? categoryBusinessHours.Friday.Select(i => new BusinessHoursItem
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<BusinessHoursItem>();

                    businessHours.Monday = categoryBusinessHours.Monday != null ? categoryBusinessHours.Monday.Select(i => new BusinessHoursItem
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<BusinessHoursItem>();

                    businessHours.Saturday = categoryBusinessHours.Saturday != null ? categoryBusinessHours.Saturday.Select(i => new BusinessHoursItem
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<BusinessHoursItem>();

                    businessHours.Sunday = categoryBusinessHours.Sunday != null ? categoryBusinessHours.Sunday.Select(i => new BusinessHoursItem
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<BusinessHoursItem>();

                    businessHours.Thursday = categoryBusinessHours.Thursday != null ? categoryBusinessHours.Thursday.Select(i => new BusinessHoursItem
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<BusinessHoursItem>();

                    businessHours.Tuesday = categoryBusinessHours.Tuesday != null ? categoryBusinessHours.Tuesday.Select(i => new BusinessHoursItem
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<BusinessHoursItem>();

                    businessHours.Wednesday = categoryBusinessHours.Wednesday != null ? categoryBusinessHours.Wednesday.Select(i => new BusinessHoursItem
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<BusinessHoursItem>();

                    Accessor.Instance.SetCategoryBusinessHours(businessHours, categoryBusinessHours.CafeId, categoryBusinessHours.CategoryId);

                    return Ok(true);
                }
                else
                {
                    throw new SecurityException("Unauthorized user.");
                }
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        #endregion

        [Route("GetSelectableCategoriesByDishId/{dishId:long}")]
        public IActionResult GetSelectableCategoriesByDishId(long dishId)
        {
            using(var fc = Accessor.Instance.GetContext())
            {
                var dish = fc.Dishes.FirstOrDefault(d => d.Id == dishId && !d.IsDeleted);
                var systemCategories = SystemDishCategories.GetSystemCtegoriesIds();
                var categories = fc.DishCategoriesInCafes.Where(
                    c => c.CafeId == dish.DishCategoryLinks.FirstOrDefault().CafeCategory.CafeId
                    && c.IsActive == true
                    && !c.IsDeleted
                    && c.DishCategory.IsActive == true
                    && !c.DishCategory.IsDeleted
                    && !systemCategories.Contains(c.DishCategoryId))
                    .Select(c => c.DishCategory).ToList();
                return Ok(categories.Distinct(new EntityComparer<DishCategory>())
                    .Select(c => new SelectableFoodCategoryModel
                {
                    Id = c.Id,
                    Name = c.CategoryName,
                    IsSelected = dish.DishCategoryLinks.Any(
                        l => l.CafeCategory.DishCategoryId == c.Id
                        && l.IsActive == true
                        && !l.IsDeleted)
                }).OrderByDescending(c => c.IsSelected).ThenBy(c => c.Name).ToList());
            }
        }

        [Route("GetSelectableCategoriesByCafeId/{cafeId:long}")]
        public IActionResult GetSelectableCategoriesByCafeId(long cafeId)
        {
            using (var fc = Accessor.Instance.GetContext())
            {
                var systemCategories = SystemDishCategories.GetSystemCtegoriesIds();
                var categories = fc.DishCategoriesInCafes.Where(
                    c => c.CafeId == cafeId
                    && c.IsActive == true
                    && !c.IsDeleted
                    && c.DishCategory.IsActive == true
                    && !c.DishCategory.IsDeleted
                    && !systemCategories.Contains(c.DishCategoryId))
                    .Select(c => c.DishCategory);
                return Ok(categories.Select(c => new SelectableFoodCategoryModel
                {
                    Id = c.Id,
                    Name = c.CategoryName,
                    IsSelected = false
                }).OrderByDescending(c => c.IsSelected).ThenBy(c => c.Name).ToList());
            }
        }
    }

    public static class DishCategoryExtensions
    {
        public static FoodCategoryModel GetContract(this DishCategory category)
        {
            return (category == null)
                ? null
                : new FoodCategoryModel
                {
                    Id = category.Id,
                    Name = category.CategoryName,
                    FullName = category.CategoryFullName,
                    Description = category.Description,
                    Uuid = category.Uuid,
                };
        }
    }
}
