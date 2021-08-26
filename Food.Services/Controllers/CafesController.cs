using DocumentFormat.OpenXml.Office2010.PowerPoint;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using Food.Data;
using Food.Data.Entities;
using Food.Services.Contracts;
using Food.Services.Extensions;
using Food.Services.ExceptionHandling;
using Food.Services.Services;
using Food.Services.ShedulerQuartz;
using ITWebNet.Food.AuthorizationServer.Extensions;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using ShedulerQuartz = Food.Services.ShedulerQuartz;
using Microsoft.Extensions.DependencyInjection;
using Food.Services.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.Web.Administration;
using Food.Services.Config;
using ITWebNet.FoodService.Food.Data.Accessor.Extensions;
using Food.Data.Enums;

namespace Food.Services.Controllers
{
    [Route("api/cafes")]
    public class CafesController : ContextableApiController
    {
        private readonly INotificationService _notificationService;
        private readonly IConfigureSettings _configureSettings;

        public CafesController(IFoodContext context, Accessor accessor)
        {
            Accessor = accessor;
            Accessor.SetTestingModeOn(context);
            Context = context;
            TestMode = true;
            _notificationService = null;
        }

        [ActivatorUtilitiesConstructor]
        public CafesController(IConfigureSettings config, INotificationService notificationService)
        {
            _configureSettings = config;
            _notificationService = notificationService;
        }
        #region cafe

        [HttpGet, Route("")]
        public IActionResult Get()
        {
            var context = Accessor.Instance.GetContext();

            var cafes = context.Cafes.Include(c => c.City).Where(c => c.IsDeleted == false && c.IsActive == true).OrderBy(c => c.CafeName)
                .ToList();
            return Ok(cafes.Select(c => c.GetContract()));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet, Route("admin")]
        public IActionResult GetAll()
        {
            var context = Accessor.Instance.GetContext();

            var cafes = context.Cafes.Include(c => c.City).AsNoTracking().Where(c => !c.IsDeleted).OrderBy(c => c.CafeName)
                .ToList();
            return Ok(cafes.Select(c => c.GetContract()));
        }

        [HttpGet, Route("{cleanUrl}")]
        public IActionResult Get(string cleanUrl)
        {
            if (string.IsNullOrEmpty(cleanUrl))
                return Ok(null);

            var url = cleanUrl?.ToLowerInvariant();
            var context = Accessor.Instance.GetContext();
            var cafe = context.Cafes.FirstOrDefault(c => c.CleanUrlName.ToLower() == url && c.IsDeleted == false);
            return Ok(cafe.GetContract());

        }

        [HttpGet, Route("{id:long}")]
        public IActionResult Get(long id)
        {
            var context = Accessor.Instance.GetContext();
            return Ok(GetCafeById(context, id));
        }

        [HttpGet, Route("GetCafesToCurrentUser")]
        public IActionResult GetCafesToCurrentUser()
        {
            var context = Accessor.Instance.GetContext();
            var userId = User.Identity.GetUserId();
            var now = DateTime.Now;

            var query =
                from company in context.Companies
                join userCompany in context.UsersInCompanies
                    on company.Id equals userCompany.CompanyId
                join companyOrderSchedule in context.CompanyOrderSchedules
                    on company.Id equals companyOrderSchedule.CompanyId
                join cafe in context.Cafes
                    on true equals cafe.IsActive
                where
                (
                    cafe.CafeAvailableOrdersType.Equals("COMPANY_PERSON")
                    || cafe.CafeAvailableOrdersType.Equals("PERSON_ONLY")
                    ||
                    (
                        userId == userCompany.UserId && userCompany.IsActive && !userCompany.IsDeleted
                        && cafe.Id == companyOrderSchedule.CafeId && !companyOrderSchedule.IsDeleted &&
                        companyOrderSchedule.IsActive == true && (companyOrderSchedule.BeginDate == null || companyOrderSchedule.BeginDate <= now.Date) &&
                        (companyOrderSchedule.EndDate == null || now.Date <= companyOrderSchedule.EndDate)
                        && cafe.CafeAvailableOrdersType.Equals("COMPANY_ONLY")
                    )
                )
                group cafe by cafe.Id
                into groups
                select groups.FirstOrDefault();

            var queryToGetManagementCafes =
                from cafeManager in context.CafeManagers
                where
                (
                    cafeManager.UserId == userId
                )
                select cafeManager.Cafe;

            var cafes = query
                .Concat(queryToGetManagementCafes)
                .GroupBy(c => c.Id)
                .Select(group => @group.FirstOrDefault())
                .OrderBy(c => c.CafeName)
                .ToList();

            return Ok(cafes.Select(c => c.GetContract()).ToList());
        }

        [HttpGet, Route("GetCafesForSchedules")]
        public IActionResult GetCafesForSchedules()
        {
            var context = Accessor.Instance.GetContext();

            var cafes = context.Cafes.Where(
                c => c.IsDeleted == false
                && c.IsActive == true &&
                c.CafeAvailableOrdersType != "PERSON_ONLY")
                .OrderBy(c => c.Id)
                .ToList();
            return Ok(cafes.Select(c => c.GetContract()));
        }

        [HttpGet, Route("GetShippingCostToSingleCompanyAddress")]
        public async Task<IActionResult> GetShippingCostToSingleCompanyAddress(string address, long companyId,
                                                                                   long cafeId, double extraCash,
                                                                                   string date)
        {
            try
            {
                DateTime? targetDate = !String.IsNullOrEmpty(date)
                    ? DateTime.ParseExact(date, "s", System.Globalization.CultureInfo.InvariantCulture)
                    : (DateTime?)null;

                var userId = User.Identity.GetUserId();
                var companyOrder = Accessor.Instance.GetCompanyOrderForUserByCompanyId(
                    new HashSet<long> { cafeId },
                    companyId,
                    targetDate != null ? new HashSet<DateTime> { targetDate.Value } : null
                );

                if (companyOrder.Count == 0)
                {
                    return Ok(new OrderDeliveryPriceModel
                    {
                        CompanyOrder = true,
                        ErrorDescription = "Невозможно найти корп. заказ для пользователя и выбранной компании"
                    });
                }
                else if (companyOrder.Count > 1)
                {
                    return Ok(new OrderDeliveryPriceModel
                    {
                        CompanyOrder = true,
                        ErrorDescription = "Найдено слишком много корп. заказов"
                    });
                }

                var companyOrderId = companyOrder.First().Id;
                var shippingCosts = Accessor.Instance.CalculateShippingCostsToSingleCompanyAddress(
                    address, companyOrderId, extraCash
                );

                return Ok(new OrderDeliveryPriceModel
                {
                    CompanyOrder = true,
                    TotalPrice = shippingCosts.Item3,
                    CountOfOrders = shippingCosts.Item2,
                    PriceForOneOrder = shippingCosts.Item3 / (shippingCosts.Item2 + 1),
                });
            }
            catch (Exception e)
            {
                return Ok(new OrderDeliveryPriceModel
                {
                    CompanyOrder = true,
                    ErrorDescription = e.Message
                });
            }
        }

        [HttpGet, Route("GetShippingCostToSingleCompanyAddress2")]
        public async Task<IActionResult> GetShippingCostToSingleCompanyAddress2(string address, long companyOrderId, double extraCash)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var shippingCosts = Accessor.Instance.CalculateShippingCostsToSingleCompanyAddress(
                    address, companyOrderId, extraCash
                );
                return Ok(new OrderDeliveryPriceModel
                {
                    CompanyOrder = true,
                    TotalPrice = shippingCosts.Item3,
                    CountOfOrders = shippingCosts.Item2,
                    PriceForOneOrder = shippingCosts.Item3 / (shippingCosts.Item2 + 1),
                });
            }
            catch (Exception e)
            {
                return Ok(new OrderDeliveryPriceModel
                {
                    CompanyOrder = true,
                    ErrorDescription = e.Message
                });
            }
        }

        [HttpGet, Route("{cafeId:long}/cost/{price:double}/{corpOrder:bool}")]
        public async Task<IActionResult> GetCostOfDelivery(long cafeId, double price, bool corpOrder = false, string orderDate = null)
        {
            OrderDeliveryPriceModel result = new OrderDeliveryPriceModel();
            result.CompanyOrder = corpOrder;
            try
            {
                var userId = User.Identity.GetUserId();
                var context = Accessor.Instance.GetContext();

                // Для персонального заказа стоимость доставки считается отдельно для каждого заказа:
                result.CountOfOrders = 1;

                // Получение стоимости доставки заказа (корпоративного или обычного.
                // Выбираем записи со стоимостью доставки, у которых:
                var query = context.CostOfDelivery.Where(
                    // Идентификатор кафе совпадает с указанным:
                    c => c.CafeId == cafeId &&
                    // Стоимость доставки указана для нужного типа заказов:
                    c.ForCompanyOrders == corpOrder &&
                    // И которые при этом не удалены:
                    c.IsDeleted == false);
                CostOfDelivery deliveryCost = null;
                // Вычисляем стоимость доставки, если заказ персональный:
                if (!corpOrder)
                {
                    // Выбираем стоиость доставки, у которой стоимость нашего заказа попадает в указанный диапазон:
                    deliveryCost = await query.Where(c =>
                        price >= c.OrderPriceFrom && price <= c.OrderPriceTo).FirstOrDefaultAsync();
                    // Проверяем, нашлась ли стоимость доставки:
                    if (deliveryCost != null)
                    {
                        // Устанавливаем найденную стоимость доставки:
                        result.TotalPrice = deliveryCost.DeliveryPrice;
                        result.PriceForOneOrder = deliveryCost.DeliveryPrice;
                    }
                    return Ok(result);
                }

                // Для корпоративных заказов ситуация сложнее. Здесь надо посчитать, сколько всего заказов в этом кафе на этот день из данной компании и какая будет общая сумма у всех заказов.
                var companiesId = context.UsersInCompanies.Where(e => e.UserId == userId).Select(e => e.CompanyId);
                if (companiesId.Count() > 0)
                {
                    // Получение количества и общей суммы текущих заказов из компании в указанное кафе.
                    // Получаем компании пользователя:
                    var companies = Accessor.Instance.GetCompanies(userId).Select(e => e.Id);
                    DateTime time;
                    if (!string.IsNullOrWhiteSpace(orderDate))
                    {
                        if (!DateTime.TryParseExact(orderDate, "yyyy-MM-dd", Thread.CurrentThread.CurrentCulture, System.Globalization.DateTimeStyles.AssumeLocal, out time))
                            time = DateTime.Now.Date;
                    }
                    else time = DateTime.Now.Date;
                    DateTime time2 = time.AddDays(1);
                    var dailyOrders = from c in context.CompanyOrders
                                      where c.CompanyId == companies.FirstOrDefault()
                                            && c.OpenDate >= time
                                            && c.OpenDate < time2
                                            && c.IsDeleted == false
                                            && c.State != (long)EnumOrderStatus.Abort
                                      select c;
                    var dailyOrderList = dailyOrders.Select(e => e.Id).ToList();
                    // Получаем заказы, оформленные на сегодня в этом кафе из компании пользователя, у которых состояние не "Отмена":
                    var ordersForToday = context.Orders.Where(e => e.CompanyOrderId != null &&
                        dailyOrderList.Contains((long)e.CompanyOrderId) &&
                        e.State != EnumOrderState.Abort &&
                        e.TotalPrice > 0 && e.CafeId == cafeId);
                    // К общему количеству заказов добавляем оформляемый заказ:
                    result.CountOfOrders = ordersForToday.Count() + 1;
                    // Получаем общую стоимость заказов. Считать надо по стоимости позиций, так как в общей стоимости заказа включается стоимость доставки, которую мы тут как раз определяем:
                    var todayOrderIds = ordersForToday.Select(o => o.Id);
                    double priceOfItems = context.OrderItems.Where(oi => !oi.IsDeleted &&
                        todayOrderIds.Contains(oi.OrderId)).Select(oi => oi.TotalPrice)
                        .DefaultIfEmpty(0.0).Sum() + price;
                    // Выбираем стоиость доставки, для которой общая стоимость заказов попадает в указанный диапазон:
                    deliveryCost = await query.Where(c =>
                        priceOfItems >= c.OrderPriceFrom && priceOfItems <= c.OrderPriceTo)
                        .FirstOrDefaultAsync();
                    // Если стоимость доставки нашлась - надо поделить ещё на всех:
                    if (deliveryCost != null)
                    {
                        result.TotalPrice = deliveryCost.DeliveryPrice;
                        result.PriceForOneOrder = deliveryCost.DeliveryPrice / result.CountOfOrders;
                    }
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                result.ErrorDescription = ex.Message;
            }
            // Если почему-то стоимость заказа ещё не нашлась - возвращаем результат:
            return Ok(result);
        }

        [Authorize(Roles = "Manager")]
        [Route("AddNewCostOfDelivery")]
        public IActionResult AddNewCostOfDelivery([FromBody]CostOfDeliveryModel model)
        {
            try
            {
                var currentUserId = User.Identity.GetUserId();

                if (Accessor.Instance.IsUserManagerOfCafe(currentUserId, model.CafeId))
                {
                    var costOfDelivery = new CostOfDelivery()
                    {
                        CafeId = model.CafeId,
                        CreateDate = DateTime.Now,
                        CreatorId = currentUserId,
                        DeliveryPrice = model.DeliveryPrice,
                        OrderPriceFrom = model.OrderPriceFrom,
                        OrderPriceTo = model.OrderPriceTo,
                        ForCompanyOrders = model.ForCompanyOrders
                    };

                    return Ok(Accessor.Instance.AddNewCostOfDelivery(costOfDelivery));
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

        [Authorize(Roles = "Admin")]
        [HttpPut, Route("")]
        public IActionResult UpdateCafe([FromBody]CafeModel model)
        {
            try
            {
                var today = DateTime.Today;
                var context = Accessor.Instance.GetContext();

                if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.FullName))
                {
                    return BadRequest();
                }

                var isExist = context.Cafes.FirstOrDefault(c =>
                    (c.CafeFullName.ToLower().Equals(model.FullName.ToLower()) ||
                     c.CafeName.ToLower().Equals(model.Name.ToLower())) && !c.IsDeleted
                    && c.Id != model.Id);

                if (isExist != null)
                    return BadRequest("Уже существует кафе с таким именем");

                // && c.IsActive == true was deleted
                var oldCafe = context.Cafes.FirstOrDefault(c => c.Id == model.Id && !c.IsDeleted);
                if (oldCafe == null)
                    return NotFound("Кафе не найдено");

                // Нельзя изменить тип кафе на кафе только для физ. лиц, если для кафе созданы
                // компанейские расписания на данный момент или на будущее.
                if ((oldCafe.CafeAvailableOrdersType == "COMPANY_ONLY" ||
                     oldCafe.CafeAvailableOrdersType == "COMPANY_PERSON") &&
                    model.CafeType == CafeType.PersonOnly)
                {
                    var hasSchedules = oldCafe.CompanyOrderSchedules
                        .Any(x => !x.IsDeleted && x.EndDate.Value.Date >= today);

                    if (hasSchedules)
                        return BadRequest("Невозможно изменить тип кафе - для кафе существуют расписания корп. заказов");

                    // Старые, неактивные расписания просто удаляем.
                    var inactiveSchedules = oldCafe.CompanyOrderSchedules
                        .Where(x => !x.IsDeleted && x.EndDate.Value.Date < today);

                    foreach (var s in inactiveSchedules)
                        s.IsDeleted = true;
                }

                var oldCleanUrl = String.Empty;
                if (oldCafe != null)
                {
                    oldCleanUrl = oldCafe.CleanUrlName;
                    oldCafe.CafeFullName = model.FullName;
                    oldCafe.CafeName = model.Name;
                    oldCafe.LastUpdateBy = User.Identity.GetUserId();
                    oldCafe.LastUpdateDate = DateTime.Now;
                    oldCafe.CafeAvailableOrdersType = GetCafeTypeString(model.CafeType);
                    oldCafe.CleanUrlName = model.CleanUrlName;
                    oldCafe.IsActive = model.IsActive;
                    oldCafe.CityId = model.CityId;
                    
                }
                context.SaveChanges();

                if (!string.IsNullOrEmpty(model.CleanUrlName) && oldCleanUrl != model.CleanUrlName)
                {
                    AddBindingIis(model.CleanUrlName);
                }

                return Ok(oldCafe.GetContract());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new InternalServerError(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, Route("")]
        public async Task<IActionResult> CreateCafe([FromBody]CafeModel model)
        {
            try
            {
                var context = Accessor.Instance.GetContext();

                if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.FullName))
                {
                    return BadRequest();
                }

                var isExist = context.Cafes.FirstOrDefault(c =>
                    c.CafeFullName.ToLower().Equals(model.FullName.ToLower()) ||
                    c.CafeName.ToLower().Equals(model.Name.ToLower()) && !c.IsDeleted);

                if (isExist != null)
                    return BadRequest("Уже существует кафе с таким именем");

                var cafeM = model.GetEntity();

                //cafeM.AverageDeliveryTime = -1;
                cafeM.Uuid = Guid.NewGuid();
                cafeM.CreationDate = DateTime.Now;
                cafeM.DeliveryPriceRub = -1;
                cafeM.Description = string.Empty;
                cafeM.CreatedBy = User.Identity.GetUserId();
                cafeM.DeliveryComment = string.Empty;
                cafeM.IsActive = model.IsActive;
                //cafeM.MinimumSumRub = -1;
                cafeM.Id = 0;
                cafeM.SpecializationId = 1;
                cafeM.WorkingTimeFrom = null;
                cafeM.WorkingTimeTo = null;
                cafeM.WorkingWeekDays = string.Empty;
                cafeM.CafeAvailableOrdersType = GetCafeTypeString(model.CafeType);
                cafeM.CityId = model.CityId;

                context.Cafes.Add(cafeM);
                context.SaveChanges();

                if (!string.IsNullOrEmpty(cafeM.CleanUrlName))
                {
                    AddBindingIis(cafeM.CleanUrlName);
                }

                return Ok(cafeM.GetContract());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new InternalServerError(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet, Route("checkUniqueName")]
        public async Task<IActionResult> СheckUniqueName(string name, long cafeId = -1)
        {
            return Ok(Accessor.Instance.СheckUniqueName(name, cafeId));
        }

        /// <summary>
        /// Создает привязку в IIS
        /// </summary>
        private bool AddBindingIis(string cafeDomain)
        {
            try
            {
                var host = _configureSettings.ClientDomain;
                var application = _configureSettings.ClientApplication;
                if (String.IsNullOrEmpty(host) || String.IsNullOrEmpty(application)) return false;
                int errorDomain;
                if (!String.IsNullOrEmpty(cafeDomain.Trim()) && !int.TryParse(cafeDomain, out errorDomain))
                {
                    using (var iis = new ServerManager())
                    {
                        var site = iis.Sites.FirstOrDefault(s => s.Name == application);
                        site.Bindings.Add($"*:{80}:{cafeDomain}.{host}", "http");
                        site.Bindings.Add($"*:{443}:{cafeDomain}.{host}", "https");
                        site.Bindings.Add($"*:{80}:www.{cafeDomain}.{host}", "http");
                        site.Bindings.Add($"*:{443}:www.{cafeDomain}.{host}", "https");
                        iis.CommitChanges();

                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Authorize(Roles = "Admin, Manager")]
        [HttpPost, Route("CafeNotificationContact")]
        public async Task<IActionResult> AddNewCafeNotificationContact([FromBody]CafeNotificationContactModel cafeNotificationContact)
        {
            try
            {
                long result = -1;

                var userId = User.Identity.GetUserId();

                if (Accessor.Instance.IsUserManagerOfCafe(userId, cafeNotificationContact.CafeId))
                {
                    CafeNotificationContact contactToBase =
                        cafeNotificationContact.GetEntity();

                    result =
                        Accessor.Instance.AddCafeNotificationContact(contactToBase);
                }
                else
                {
                    throw new SecurityException("Attempt of unauthorized access");
                }

                return Ok(result);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete, Route("{cafeId:long}")]
        public async Task<IActionResult> DeleteCafe(long cafeId)
        {
            var context = Accessor.Instance.GetContext();
            
            var cafe = context.Cafes.FirstOrDefault(c => c.Id == cafeId && !c.IsDeleted);
            if (cafe == null)
                return Ok();

            var arrXslt = context.XsltToCafe
                .Where(e => e.IsDeleted == false && e.CafeId == cafeId)
                .ToArray();
            foreach (var item in arrXslt)
                item.IsDeleted = true;
            
            cafe.IsActive = false;
            cafe.IsDeleted = true;
            cafe.LastUpdateBy = User.Identity.GetUserId();
            cafe.LastUpdateDate = DateTime.Now;
            
            context.SaveChanges();

            return Ok();
        }

        [HttpGet, Route("GetCafesRange")]
        public IActionResult GetCafesRange(int startRange, int count)
        {
            var cafeList = new List<CafeModel>();

            foreach (var cafeItem in Accessor.Instance.GetCafes().Skip(startRange).Take(count))
            {
                cafeList.Add(cafeItem.GetContract());
            }

            return Ok(cafeList);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet, Route("cafe/user/{userId:long}")]
        public async Task<IActionResult> GetCafesToUser(long userId)
        {
            try
            {
                var currentUser =
                    User.Identity.GetUserById();

                return Ok(
                    Accessor.Instance.GetCafesByUserId(userId)
                        .Select(cafeItem => cafeItem.GetContract())
                        .ToList());
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet, Route("cafes/user/{userId:long}")]
        public async Task<IActionResult> GetListOfCafeByUserId(Int64 userId)
        {
            try
            {
                var listOfCafe = new List<CafeModel>();

                var itemFromBase =
                    Accessor.Instance.GetListOfCafeByUserId(userId);

                foreach (var item in itemFromBase)
                {
                    listOfCafe.Add(
                        item.GetContract()
                    );
                }
                return Ok(listOfCafe);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete, Route("RemoveUserCafeLink/{cafeId:long}/{userId:long}")]
        public async Task<IActionResult> RemoveUserCafeLink(Int64 cafeId, Int64 userId)
        {
            try
            {
                var currentUser =
               User.Identity.GetUserById();

                if (currentUser != null)
                {
                    var userCafeLink = new CafeManager
                    {
                        CafeId = cafeId,
                        UserId = userId,
                        LastUpdateBy = currentUser.Id,
                        LastUpdateDate = DateTime.Now
                    };

                    return Ok(Accessor.Instance.RemoveUserCafeLink(userCafeLink));
                }
                else
                {
                    return Ok(false);
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

        #region Managers

        [HttpGet, Route("{cafeId:long}/managers")]
        public async Task<IActionResult> GetCafeManagers(long cafeId)
        {
            var context = Accessor.Instance.GetContext();

            var managers = context.CafeManagers.Include(c => c.User)
                .Where(c => c.CafeId == cafeId && !c.IsDeleted)
                .ToList();
            return Ok(managers.Select(c => c.GetContract()));
        }

        [HttpGet, Route("{cafeId:long}/managers/{userId:long}")]
        public async Task<IActionResult> GetCafeManagers(long cafeId, long userId)
        {
            var context = Accessor.Instance.GetContext();

            var manager = context.CafeManagers.Include(c => c.User)
                .FirstOrDefault(c => c.CafeId == cafeId && !c.IsDeleted && c.UserId == userId);

            if (manager == null)
                return NotFound();
            return Ok(manager.GetContract());
        }

        [HttpGet, Route("GetManagedCafes"), Authorize(Roles = "Manager")]
        public List<CafeModel> GetManagedCafes()
        {
/*            try
            {*/
                var managedCafes = new List<CafeModel>();

                var currentUser = User.Identity.GetUserId();

                foreach (var cafe in Accessor.Instance.GetManagedCafes(currentUser))
                {
                    managedCafes.Add(cafe.GetContract());
                }

                return managedCafes;
/*            }
            catch (Exception e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                throw;
            }*/
        }

        [Authorize]
        [HttpGet, Route("managedcaffeebymail")]
        public async Task<IActionResult> GetManagedCaffeeByMail(string email)
        {
            var user = Accessor.Instance.GetUserByEmail(email);
            var managedCafes = Accessor.Instance.GetManagedCafes(user.Id)
                .Select(e => e.GetContract()).ToList();
            return Ok(managedCafes);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, Route("managers/add")]
        public async Task<IActionResult> PostCafeManager([FromBody]CafeManagerModel model)
        {
            try
            {
                var authorId = User.Identity.GetUserId();
                if (!Accessor.Instance.AddUserCafeLink(
                     new CafeManager()
                     {
                         CafeId = model.CafeId,
                         CreatedBy = User.Identity.GetUserId(),
                         CreationDate = DateTime.Now,
                         UserId = (int)model.UserId,
                         UserRoleId = EnumUserRole.Manager.ToUpper(),
                         IsDeleted = false
                     },
                     authorId))
                {
                    return BadRequest("Не удалось привязать пользователя к кафе");
                }

                if (!Accessor.Instance.AddUserToRole((int)model.UserId, "Manager"))
                {
                    return BadRequest("Не удалось привязать роль к пользователю");
                }

                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new InternalServerError(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete, Route("{cafeId:long}/managers/delete/{userId:long}")]
        public async Task<IActionResult> DeleteCafeManager(long cafeId, long userId)
        {
            if (!Accessor.Instance.RemoveUserCafeLink(new CafeManager() { UserId = userId, CafeId = cafeId, LastUpdateBy = User.Identity.GetUserId() }))
            {
                return BadRequest("Не удалось удалить привязку пользователя к кафе");
            }

            if (!Accessor.Instance.RemoveUserRole((int)userId, "Manager"))
            {
                return BadRequest("Не удалось удалить привязку роли у пользователя");
            }

            return Ok();
        }

        [Authorize(Roles = "Manager")]
        [HttpGet, Route("IsUserOfManagerCafe")]
        public bool IsUserOfManagerCafe(long cafeId)
        {
            var user = User.Identity.GetUserById();
            return user != null && Accessor.Instance.IsUserManagerOfCafe(user.Id, cafeId);
        }

        #endregion

        #region Шаблоны, банкеты

        [Authorize(Roles = "Manager")]
        [HttpPost, Route("menupatterns")]
        public async Task<IActionResult> AddCafeMenuPattern([FromBody]CafeMenuPatternModel pattern)
        {
            try
            {
                var context = Accessor.Instance.GetContext();

                if (!Accessor.Instance.IsUserManagerOfCafe(User.Identity.GetUserId(), pattern.CafeId))
                {
                    return BadRequest("Пользователь не является менеджером кафе");
                }

                var patternM = new CafeMenuPattern()
                {
                    Name = pattern.Name,
                    IsBanket = pattern.IsBanket,
                    CafeId = pattern.CafeId,
                    PatternDate = pattern.PatternToDate ?? DateTime.Now.Date,
                    Dishes = pattern.Dishes?.Select(c =>
                        new CafeMenuPatternDish() { DishId = c.DishId, Price = c.Price, Name = c.Name }).ToList()
                };

                context.CafeMenuPatterns.Add(patternM);

                context.SaveChanges();

                return Ok();
            }
            catch (Exception e)
            {
                return new InternalServerError(e.Message);
            }
        }

        [HttpGet, Route("{cafeId:long}/menupatterns/{patternId:long}")]
        public async Task<IActionResult> GetCafeMenuPatternById(long cafeId, long patternId)
        {
            try
            {
                var context = Accessor.Instance.GetContext();

                var pattern =
                    context.CafeMenuPatterns.FirstOrDefault(c =>
                        !c.IsDeleted && c.Id == patternId && c.CafeId == cafeId);

                return Ok(pattern.GetContract());
            }
            catch (Exception e)
            {
                return new InternalServerError(e.Message);
            }
        }

        [HttpGet, Route("{cafeId:long}/bankets")]
        public async Task<IActionResult> GetBankets(long cafeId)
        {
            var context = Accessor.Instance.GetContext();

            var banket = context.Bankets.Include(c => c.Cafe).Include(c => c.Menu)
                .Include(c => c.Company)
                .Where(c => !c.IsDeleted && c.CafeId == cafeId && c.Menu.IsBanket && !c.Cafe.IsDeleted &&
                            !c.Menu.IsDeleted && c.Company.IsActive && !c.Company.IsDeleted).ToList();

            return Ok(banket.Select(c => c.GetContract()));
        }

        [HttpGet, Route("{cafeId:long}/menupatterns")]
        public async Task<IActionResult> GetMenuPatternsByCafeId(long cafeId)
        {
            var context = Accessor.Instance.GetContext();

            var pattern = context.CafeMenuPatterns.Where(c => !c.IsDeleted && c.CafeId == cafeId).ToList();

            return Ok(pattern.Select(c => c.GetContract()));
        }

        [HttpGet, Route("{cafeId:long}/schedules")]
        public async Task<IActionResult> GetCompanyOrderSchedules(long cafeId)
        {
            try
            {
                var context = Accessor.Instance.GetContext();

                var schedules =
                    context.CompanyOrderSchedules
                        .Include(c => c.Company)
                        .Include(c => c.Cafe)
                        .Where(c => c.CafeId == cafeId && c.IsDeleted == false && c.IsActive == true);

                return Ok(schedules.ToList().Select(c => c.GetContract()));
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        [HttpGet, Route("{cafeId:long}/discounts/{unixTime:long}")]
        public async Task<IActionResult> GetDiscount(long cafeId, long unixTime, long? companyId = null)
        {
            if (!User.Identity.IsAuthenticated)
                return Ok(0);

            var date = DateTimeExtensions.FromUnixTime(unixTime);
            var userId = User.Identity?.GetUserId();
            if (companyId == null && userId == null)
                return Ok(0);

            var context = Accessor.Instance.GetContext();

            var query =
                from d in context.Discounts
                where (
                    d.IsDeleted == false
                    && d.CafeId == cafeId
                    && (
                        (
                            d.UserId == userId
                            && d.CompanyId == null
                        )
                        || (
                            d.UserId == null
                            && d.CompanyId == companyId
                        )
                        || (
                            d.UserId == userId
                            && d.CompanyId == companyId
                        )
                    )
                    && d.BeginDate <= date
                    && (
                        d.EndDate == null
                        || d.EndDate > date
                    )
                )
                select d;

            return Ok(query.Any() ? Convert.ToInt32(query.Max(d => d.Value)) : 0);
        }

        [HttpGet, Route("{cafeId:long}/menu/{id:long}")]
        public async Task<IActionResult> GetMenuByPatternId(long cafeId, long id)
        {
            try
            {
                var context = Accessor.Instance.GetContext();

                var pattern =
                    context.CafeMenuPatterns
                        .Include(c => c.Dishes)
                        .ThenInclude(d => d.Dish.DishCategoryLinks)
                        .FirstOrDefault(c => !c.IsDeleted && c.CafeId == cafeId && c.Id == id);

                if (pattern == null)
                    return BadRequest();

                List<CafeMenuModel> result = new List<CafeMenuModel>();
                
                var filteredDishes = pattern.Dishes.Where(d => d.Dish.DishCategoryLinks.Any(
                    l => !l.IsDeleted
                    && l.IsActive == true
                    && l.Dish.IsDeleted == false
                    && l.Dish.IsActive == true
                    && l.CafeCategory.IsDeleted == false
                    && l.CafeCategory.IsActive == true
                    && l.CafeCategory.DishCategory.IsDeleted == false
                    && l.CafeCategory.DishCategory.IsActive == true
                    && !SystemDishCategories.GetSystemCtegoriesIds().Contains(l.CafeCategory.DishCategoryId))
                ).ToList();
                var categories = new Dictionary<DishCategory, List<CafeMenuPatternDish>>();
                foreach (var dish in filteredDishes)
                {
                    foreach (var category in dish.Dish.DishCategoryLinks.Select(l => l.CafeCategory))
                    {
                        if (!categories.ContainsKey(category.DishCategory))
                        {
                            categories.Add(category.DishCategory, new List<CafeMenuPatternDish> { dish });
                        }
                        else if (categories[category.DishCategory].FirstOrDefault(d => d.Dish.Id == dish.Dish.Id) == null)
                        {
                            categories[category.DishCategory].Add(dish);
                        }
                    }
                }

                pattern.Dishes.ForEach(d => {
                    d.Dish.BasePrice = d.Price;
                    d.Dish.DishName = d.Name;
                });
                foreach (var category in categories)
                {
                    if (result.FirstOrDefault(c => c.Category.Id == category.Key.Id) == null)
                        result.Add(new CafeMenuModel()
                        {
                            Category = category.Key.GetContract(),
                            Dishes = new List<FoodDishModel>(
                                category.Value.Select(c => c.Dish.GetContractDish()))
                        });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return new InternalServerError(ex.Message);
            }
        }

        [HttpGet, Route("{cafeId:long}/menu/{id:long}/{search}")]
        public async Task<IActionResult> GetMenuFilteredMenuByPatternId(long cafeId, long id, string search)
        {
            try
            {
                var context = Accessor.Instance.GetContext();

                var pattern = context.CafeMenuPatterns
                    .Include(c => c.Dishes).FirstOrDefault(c => c.CafeId == cafeId && c.Id == id && !c.IsDeleted);

                if (pattern == null)
                {
                    return BadRequest();
                }

                var ids = pattern.Dishes.Where(c => !c.IsDeleted && c.Name.ToLower().Contains(search)).Select(d =>
                    d.DishId).ToList();
                var dishes = context.Dishes.Where(c => ids.Contains(c.Id)).ToList();
                dishes.ForEach(d =>
                {
                    var l = pattern.Dishes.First(c => c.DishId == d.Id);
                    d.DishName = l.Name;
                    d.BasePrice = l.Price;
                });
                var categories = new List<DishCategory>();
                foreach(var item in dishes)
                {
                    categories.AddRange(item.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted).Select(
                        l => l.CafeCategory.DishCategory).Where(c => !categories.Contains(c)));
                }
                var result = categories.Select(c => new CafeMenuModel()
                {
                    Category = c.GetContract(),
                    Dishes = dishes.Where(f => f.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted)
                    .Select(l => l.CafeCategory.DishCategoryId).Contains(c.Id))
                    .Select(d => d.GetContractDish()).ToList()
                });

                return Ok(result);
            }
            catch (Exception e)
            {
                return new InternalServerError(e.Message);
            }
        }

        [HttpDelete, Route("{cafeId:long}/menupatterns/{patternId:long}")]
        public async Task<IActionResult> RemoveCafeMenuPattern(long cafeId, long patternId)
        {
            try
            {
                var context = Accessor.Instance.GetContext();

                if (!Accessor.Instance.IsUserManagerOfCafe(User.Identity.GetUserId(), cafeId))
                {
                    return BadRequest("Пользователь не является менеджером кафе");
                }

                var pattern =
                    context.CafeMenuPatterns.Include(c => c.Bankets)
                        .FirstOrDefault(c => c.Id == patternId && c.CafeId == cafeId);

                if (pattern == null)
                    return BadRequest("Шаблон не найден");

                var today = DateTime.Now;
                if (pattern.IsBanket && pattern.Bankets.Any(c => !c.IsDeleted && c.EventDate.Date >= today))
                {
                    return BadRequest("Для выбранного меню существуют банкеты");
                }

                pattern.IsDeleted = true;
                pattern.Dishes.ForEach(c => { c.IsDeleted = true; });

                context.SaveChanges();

                return Ok();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new InternalServerError(ex.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpPut, Route("menupatterns")]
        public async Task<IActionResult> UpdateCafeMenuPattern([FromBody]CafeMenuPatternModel pattern)
        {
            try
            {
                var context = Accessor.Instance.GetContext();

                if (!Accessor.Instance.IsUserManagerOfCafe(User.Identity.GetUserId(), pattern.CafeId))
                {
                    return BadRequest("Пользователь не является менеджером кафе");
                }

                var patternM =
                    context.CafeMenuPatterns.Include(c => c.Bankets).FirstOrDefault(c =>
                        !c.IsDeleted && c.CafeId == pattern.CafeId && c.Name.ToLower() == pattern.Name.ToLower());

                if (patternM == null)
                {
                    return BadRequest();
                }

                var today = DateTime.Now.Date;
                if (!pattern.IsBanket && pattern.IsBanket != patternM.IsBanket &&
                    patternM.Bankets.Any(c => !c.IsDeleted && c.EventDate.Date >= today))
                {
                    return BadRequest("Для выбранного меню существуют банкеты");
                }
                patternM.IsBanket = pattern.IsBanket;
                patternM.Name = pattern.Name;
                patternM.PatternDate = pattern.PatternToDate ?? DateTime.Now;
                var currentDishes = patternM.Dishes.ToList();
                if (pattern.Dishes == null)
                {
                    currentDishes.ForEach(s => s.IsDeleted = true);
                }
                else
                {
                    //проверяем, есть ли записи в таблице, которые удалены, новые добавлять не будем
                    var deletedDishes = currentDishes.Where(c => !c.IsDeleted).Select(c => c.DishId)
                        .Except(pattern.Dishes.Select(c => c.DishId)).ToList();
                    foreach (var dish in currentDishes)
                    {
                        if (deletedDishes.Contains(dish.DishId))
                        {
                            dish.IsDeleted = true;
                        }
                    }
                    var addedDishes = pattern.Dishes.Select(c => c.DishId).Except(currentDishes.Select(s => s.DishId))
                        .ToList();
                    var changedIds = pattern.Dishes.Select(c => c.DishId).Intersect(currentDishes.Select(c => c.DishId))
                        .ToList();
                    var changed = context.CafeMenuPatternsDishes
                        .Where(c => changedIds.Contains(c.DishId) && c.PatternId == patternM.Id).ToList();
                    foreach (var item in changed)
                    {
                        var newDish = pattern.Dishes.FirstOrDefault(c => c.DishId == item.DishId);
                        if (newDish == null)
                            continue;

                        item.Name = newDish.Name;
                        item.Price = newDish.Price;
                        item.IsDeleted = false;
                    }

                    var newDishes = pattern.Dishes.Where(c => addedDishes.Contains(c.DishId)).Select(
                        c => new CafeMenuPatternDish()
                        {
                            DishId = c.DishId,
                            Name = c.Name,
                            Price = c.Price,
                            PatternId = patternM.Id
                        }).ToList();
                    context.CafeMenuPatternsDishes.AddRange(newDishes);

                    context.SaveChanges();
                }

                return Ok();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return new InternalServerError(e.Message);
            }
        }

        #endregion

        [Authorize(Roles = "Manager")]
        [HttpPut, Route("EditCostOfDelivery")]
        public async Task<IActionResult> EditCostOfDelivery([FromBody]CostOfDeliveryModel model)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                var costOfDelivery =
                    Accessor.Instance.GetCostOfDeliveryById(model.Id);

                if (costOfDelivery == null)
                {
                    throw new Exception("Attempt to work with unexisting object");
                }

                if (currentUser != null
                    && Accessor.Instance.IsUserManagerOfCafe(
                        currentUser.Id,
                        costOfDelivery.CafeId
                       )
                   )
                {
                    costOfDelivery.DeliveryPrice = model.DeliveryPrice;
                    costOfDelivery.LastUpdateByUserId = currentUser.Id;
                    costOfDelivery.LastUpdDate = DateTime.Now;
                    costOfDelivery.OrderPriceFrom = model.OrderPriceFrom;
                    costOfDelivery.OrderPriceTo = model.OrderPriceTo;
                    costOfDelivery.ForCompanyOrders = model.ForCompanyOrders;

                    return Ok(Accessor.Instance.EditCostOfDelivery(costOfDelivery));
                }
                else
                {
                    throw new SecurityException("Attempt of unauthorized access");
                }
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        //[CheckUserInBase(SecurityAction.Demand)]
        [HttpGet, Route("GetListOfCostOfDelivery")]
        public async Task<IActionResult> GetListOfCostOfDelivery(long cafeId, double? price)
        {
            try
            {
                var costsOfDelivery = new List<CostOfDeliveryModel>();

                var costsOfDeliveryFromBase =
                    new List<CostOfDelivery>();

                if (price != null)
                {
                    costsOfDeliveryFromBase =
                        Accessor
                            .Instance
                            .GetListOfDeliveryCosts(
                                cafeId,
                                (double)price
                            );
                }
                else
                {
                    costsOfDeliveryFromBase =
                        Accessor.Instance.GetListOfDeliveryCosts(cafeId);
                }

                foreach (var costOfDelivery in costsOfDeliveryFromBase)
                {
                    costsOfDelivery.Add(costOfDelivery.GetContract());
                }

                return Ok(costsOfDelivery);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpDelete, Route("RemoveCostOfDelivery/{costOfDeliveryId:long}")]
        public async Task<IActionResult> RemoveCostOfDelivery(Int64 costOfDeliveryId)
        {
            try
            {
                var currentUser =
                User.Identity.GetUserById();

                var costOfDelivery =
                    Accessor.Instance.GetCostOfDeliveryById(costOfDeliveryId);

                if (costOfDelivery == null)
                {
                    throw new Exception("Attempt to work with unexisting object");
                }
                if (currentUser != null
                    && Accessor.Instance.IsUserManagerOfCafe(
                        currentUser.Id,
                        costOfDelivery.CafeId
                       )
                   )
                {
                    return Ok(Accessor.Instance.RemoveCostOfDelivery(costOfDeliveryId));
                }
                else
                {
                    throw new SecurityException("Attempt of unauthorized access");
                }
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, Route("UserCafeLink/{cafeId:long}/{userId:long}")]
        public async Task<IActionResult> AddUserCafeLink(Int64 cafeId, Int64 userId)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                if (currentUser != null)
                {
                    var newUserCafeLink = new CafeManager()
                    {
                        CafeId = cafeId,
                        CreationDate = DateTime.Now,
                        CreatedBy = currentUser.Id,
                        UserId = userId,
                        UserRoleId = null
                    };

                    if (CafeServiceHelper.IsNewUserCafeLinkAvailable(newUserCafeLink))
                        return Ok(
                            Accessor
                            .Instance
                            .AddUserCafeLink(
                                newUserCafeLink,
                                User.Identity.GetUserId()
                            ));
                    else
                        return Ok(false);
                }
                else
                {
                    return Ok(false);
                }
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut, Route("EditCostOfDelivery/{cafeId:long}/{userId:long}")]
        public async Task<IActionResult> EditUserCafeLink(Int64 cafeId, Int64 userId)
        {
            try
            {
                var currentUser = User.Identity.GetUserById();

                if (currentUser != null)
                {
                    var userCafeLink = new CafeManager()
                    {
                        CafeId = cafeId,
                        UserId = userId,
                        UserRoleId = null,
                        LastUpdateBy = currentUser.Id
                    };

                    if (CafeServiceHelper.IsNewUserCafeLinkAvailable(userCafeLink))
                        return Ok(
                            Accessor
                            .Instance
                            .EditUserCafeLink(
                                userCafeLink
                            ));
                    else
                        return Ok(false);
                }
                else
                {
                    return Ok(false);
                }
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpGet, Route("GetCafeBusinessHours")]
        public async Task<IActionResult> GetCafeBusinessHours(long cafeId)
        {
            try
            {
                var user = User.Identity.GetUserById();

                if (Accessor.Instance.IsUserManagerOfCafeIgnoreActivity(user.Id, cafeId))
                {
                    var businessHours = Accessor.Instance.GetCafeBusinessHours(cafeId);

                    var cafeBusinessHours = new CafeBusinessHoursModel
                    {
                        Departures = businessHours.Departures != null ? businessHours.Departures.Select(d => new CafeBusinessHoursDepartureModel
                        {
                            Date = d.Date,
                            IsDayOff = d.IsDayOff,
                            Items = d.Items != null ? d.Items.Select(i => new CafeBusinessHoursItemModel
                            {
                                ClosingTime = i.ClosingTime,
                                OpeningTime = i.OpeningTime
                            }).ToList() : new List<CafeBusinessHoursItemModel>()
                        }).ToList() : new List<CafeBusinessHoursDepartureModel>()
                    };

                    cafeBusinessHours.Friday = businessHours.Friday != null ? businessHours.Friday.Select(i => new CafeBusinessHoursItemModel
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<CafeBusinessHoursItemModel>();

                    cafeBusinessHours.Monday = businessHours.Monday != null ? businessHours.Monday.Select(i => new CafeBusinessHoursItemModel
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<CafeBusinessHoursItemModel>();

                    cafeBusinessHours.Saturday = businessHours.Saturday != null ? businessHours.Saturday.Select(i => new CafeBusinessHoursItemModel
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<CafeBusinessHoursItemModel>();

                    cafeBusinessHours.Sunday = businessHours.Sunday != null ? businessHours.Sunday.Select(i => new CafeBusinessHoursItemModel
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<CafeBusinessHoursItemModel>();

                    cafeBusinessHours.Thursday = businessHours.Thursday != null ? businessHours.Thursday.Select(i => new CafeBusinessHoursItemModel
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<CafeBusinessHoursItemModel>();

                    cafeBusinessHours.Tuesday = businessHours.Tuesday != null ? businessHours.Tuesday.Select(i => new CafeBusinessHoursItemModel
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<CafeBusinessHoursItemModel>();

                    cafeBusinessHours.Wednesday = businessHours.Wednesday != null ? businessHours.Wednesday.Select(i => new CafeBusinessHoursItemModel
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<CafeBusinessHoursItemModel>();

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

        [HttpPost, Route("SetCafeBusinessHours")]
        public async Task<IActionResult> SetCafeBusinessHours([FromBody]CafeBusinessHoursModel cafeBusinessHours)
        {
            try
            {
                var user = User.Identity.GetUserById();

                if (Accessor.Instance.IsUserManagerOfCafeIgnoreActivity(user.Id, cafeBusinessHours.CafeId))
                {
                    var businessHours = new BusinessHours
                    {
                        Departures = cafeBusinessHours.Departures != null ? cafeBusinessHours.Departures.Select(d => new BusinessHoursDeparture
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

                    businessHours.Friday = cafeBusinessHours.Friday != null ? cafeBusinessHours.Friday.Select(i => new BusinessHoursItem
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<BusinessHoursItem>();

                    businessHours.Monday = cafeBusinessHours.Monday != null ? cafeBusinessHours.Monday.Select(i => new BusinessHoursItem
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<BusinessHoursItem>();

                    businessHours.Saturday = cafeBusinessHours.Saturday != null ? cafeBusinessHours.Saturday.Select(i => new BusinessHoursItem
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<BusinessHoursItem>();

                    businessHours.Sunday = cafeBusinessHours.Sunday != null ? cafeBusinessHours.Sunday.Select(i => new BusinessHoursItem
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<BusinessHoursItem>();

                    businessHours.Thursday = cafeBusinessHours.Thursday != null ? cafeBusinessHours.Thursday.Select(i => new BusinessHoursItem
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<BusinessHoursItem>();

                    businessHours.Tuesday = cafeBusinessHours.Tuesday != null ? cafeBusinessHours.Tuesday.Select(i => new BusinessHoursItem
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<BusinessHoursItem>();

                    businessHours.Wednesday = cafeBusinessHours.Wednesday != null ? cafeBusinessHours.Wednesday.Select(i => new BusinessHoursItem
                    {
                        ClosingTime = i.ClosingTime,
                        OpeningTime = i.OpeningTime
                    }).ToList() : new List<BusinessHoursItem>();

                    Accessor.Instance.SetCafeBusinessHours(businessHours, cafeBusinessHours.CafeId);

                    return Ok();
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

        [HttpGet, Route("GetCafeByIdIgnoreActivity")]
        public async Task<IActionResult> GetCafeByIdIgnoreActivity(long cafeId)
        {
            var cafe = Accessor.Instance.GetCafeByIdIgnoreActivity(cafeId);
            var model = cafe.GetContract();
            model.Kitchens = Accessor.Instance.GetListOfKitchenToCafe(cafeId).Select(c => c.Kitchen.GetContract()).ToList();
            return Ok(model);
        }

        [Authorize]
        [HttpGet, Route("GetCafesAvailableToEmployee")]
        public IActionResult GetCafesAvailableToEmployee(List<long> dates, long? cityId = null)
        {
            try
            {
                if (dates == null)
                    return Ok(new List<CompanyServersModel> { });

                var actualDates = dates.Select(x => new DateTime(x)).ToList();
                var userId = User.Identity.GetUserById().Id;
                var result = Accessor.Instance.GetCafesAvailableToEmployee(userId, actualDates, cityId);
                return Ok(result.Select(
                    x => new CompanyServersModel
                    {
                        Date = x.Item1,
                        CompanyId = x.Item2.Id,
                        CompanyName = x.Item2.Name,
                        ServingCafes = x.Item3.Select(y => y.Id).ToList()
                    }
                ));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpGet, Route("GetCafeInfo")]
        public IActionResult GetCafeInfo(long cafeId)
        {
            try
            {
                var user = User.Identity.GetUserById();

                if (Accessor.Instance.IsUserManagerOfCafe(user.Id, cafeId))
                {
                    var cafe = Accessor.Instance.GetCafeById(cafeId);
                    if (cafe != null)
                    {
                        return Ok(new CafeInfoModel
                        {
                            CafeId = cafe.Id,
                            Description = cafe.Description ?? "",
                            ShortDescription = cafe.CafeShortDescription ?? "",
                            WeekMenuIsActive = cafe.WeekMenuIsActive,
                            HeadPicture = cafe.SmallImage,
                            Logotype = cafe.BigImage,
                            Address = cafe.Address,
                            Phone = cafe.Phone,
                            DeferredOrder = cafe.DeferredOrder,
                            PaymentMethod = (PaymentTypeEnum)cafe.PaymentMethod
                        });
                    }
                    else
                    {
                        throw new Exception("Cafe not found.");
                    }
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

        /// <summary>
        /// Изменение настроек кафе
        /// </summary>
        /// <param name="cafeInfo">Данные кафе</param>
        /// <returns></returns>
        [HttpPost, Route("SetCafeInfo")]
        public async Task SetCafeInfo([FromBody]CafeInfoModel cafeInfo)
        {
            try
            {
                var fc = Accessor.Instance.GetContext();
                var user = User.Identity.GetUserById();

                if (Accessor.Instance.IsUserManagerOfCafeIgnoreActivity(user.Id, cafeInfo.CafeId))
                {
                    var cafe = Accessor.Instance.GetCafeByIdIgnoreActivity(cafeInfo.CafeId);
                    if (cafe != null)
                    {
                        var isDiffer = cafe.OrderAbortTime != cafeInfo.OrderAbortTime;
                        cafe.Description = cafeInfo.Description;
                        cafe.CafeShortDescription = cafeInfo.ShortDescription;
                        cafe.WeekMenuIsActive = cafeInfo.WeekMenuIsActive;
                        cafe.Logo = cafeInfo.CafeIcon;
                        cafe.BigImage = cafeInfo.Logotype;
                        cafe.SmallImage = cafeInfo.HeadPicture;
                        cafe.Address = cafeInfo.Address;
                        cafe.Phone = cafeInfo.Phone;
                        cafe.DeferredOrder = cafeInfo.DeferredOrder;
                        cafe.DailyCorpOrderSum = cafeInfo.DailyCorpOrderSum;
                        cafe.OrderAbortTime = cafeInfo.OrderAbortTime;
                        cafe.PaymentMethod = (EnumPaymentType)cafeInfo.PaymentMethod;
                        cafe.MinimumSumRub = cafeInfo.MinimumOrderSum;
                        cafe.KitchenInCafes = fc.KitchensInCafes.Where(k => cafeInfo.Kitchens
                        .Select(c => c.Id).Contains(k.Id) && k.CafeId == cafeInfo.CafeId).ToList();
                        cafe.DeliveryRegions = cafeInfo.Regions;
                        cafe.DeliveryComment = cafeInfo.DeliveryComment;
                        cafe.AverageDelivertyTime = cafeInfo.AverageDeliveryTime;
                        var (id, cancelledOrders) = Accessor.Instance.EditCafe(cafe);

                        var kitchensIds = new HashSet<long>(cafeInfo.Kitchens.Select(k => k.Id));
                        var kitchensInCafe = fc.KitchensInCafes.Where(k => k.CafeId == cafeInfo.CafeId).ToList();
                        foreach(var item in kitchensInCafe)
                        {
                            if (!kitchensIds.Contains(item.KitchenId))
                            {
                                item.IsDeleted = true;
                            }
                        }

                        foreach(var item in kitchensIds)
                        {
                            if (!fc.KitchensInCafes.AsNoTracking().Any(k=>!k.IsDeleted && k.CafeId==cafeInfo.CafeId && k.KitchenId == item))
                            {
                                fc.KitchensInCafes.Add(new KitchenInCafe
                                {
                                    CafeId = cafeInfo.CafeId,
                                    CreateDate = DateTime.Now,
                                    IsDeleted = false,
                                    CreatorId = user.Id,
                                    KitchenId = item
                                });
                            }
                        }

                        fc.SaveChanges();

                        //Если время отмены заказов поменялось - меняем его для всех задач в schedulere
                        if (isDiffer && cafeInfo.OrderAbortTime.HasValue)
                        {
                            //Получаем все корпоративные заказы для данного кафе
                            var companyOrders = Accessor.Instance.GetAllCompanyOrdersGreaterDate(DateTime.Now).
                                Where(co => co.CafeId == cafe.Id);

                            foreach (var co in companyOrders)
                            {
                                //Если есть задача по отмене заказов - редактируем ее в соответствии с новой информацией
                                var isExist = await ShedulerQuartz.Scheduler.Instance.IsAbortOrdersByAddressScheduled(co.Id);
                                if (isExist && co.DeliveryDate.HasValue)
                                {
                                    await ShedulerQuartz.Scheduler.Instance.CancelAbortOrdersByAddress(co.Id);
                                    DateTime newDate = co.DeliveryDate.Value.Date.Add(cafeInfo.OrderAbortTime.Value).AddMinutes(-30);
                                    await ShedulerQuartz.Scheduler.Instance.AbortOrdersByAddressAt(newDate, co.Id);
                                }
                            }
                        }
                        

                        var notifications =
                            Accessor.Instance.GetCafeNotificationContactByCafeId(cafeInfo.CafeId,
                                NotificationChannelEnum.Email);
                        if (notifications.Count == 0)
                        {
                            Accessor.Instance.AddCafeNotificationContact(new CafeNotificationContact()
                            {
                                CafeId = cafeInfo.CafeId,
                                NotificationContact = cafeInfo.NotificationContact,
                                NotificationChannelId = (short)NotificationChannelEnum.Email
                            });
                        }
                        else
                        {
                            var notification = notifications.OrderBy(c => c.Id).First();
                            notification.NotificationContact = cafeInfo.NotificationContact;
                            Accessor.Instance.UpdateCafeNotificationContact(notification);
                        }

                        if (_notificationService != null)
                        {
                            var message = "В связи с изменениями условий работы кафе, ваши заказы ({0}) были отменены";
                            var ok = !await CommonSmsNotifications.InformAboutCancelledOrders(
                                _notificationService,
                                cancelledOrders,
                                (u, os) => String.Format(message, os.Count()));

                            if (!ok)
                                Log.Logger.Error($"Error sending SMS notifications (CAFE_ID={id})");
                        }
                    }
                    else
                    {
                        throw new Exception("Cafe not found.");
                    }
                }
                else
                {
                    throw new SecurityException("Unauthorized user.");
                }
            }
            catch (Exception e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
            }
        }

        [Authorize(Roles = "Admin, Manager")]
        [HttpGet, Route("CafeNotificationContact/{id:long}")]
        public async Task<IActionResult> GetCafeNotificationContactById(long id)
        {
            try
            {
                CafeNotificationContactModel cafeNotificationContact = null;

                var contactFromBase =
                    Accessor
                        .Instance
                        .GetCafeNotificationContactById(id);

                if (contactFromBase != null)
                {
                    cafeNotificationContact =
                        contactFromBase.GetContract();
                }

                return Ok(cafeNotificationContact);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin, Manager")]
        [HttpGet, Route("GetNotificationContactsToCafe")]
        public async Task<IActionResult> GetNotificationContactsToCafe(long cafeId, long? notificationChannel)
        {
            try
            {
                var cafeNotificationContacts =
                    new List<CafeNotificationContactModel>();

                var channelToBase =
                    notificationChannel == null
                        ? null
                        : notificationChannel;

                var contactsFromBase =
                    Accessor
                        .Instance
                        .GetCafeNotificationContactByCafeId(
                            cafeId,
                            (NotificationChannelEnum)channelToBase
                        );

                foreach (var contactFromBase in contactsFromBase)
                {
                    cafeNotificationContacts.Add(
                        contactFromBase.GetContract()
                        );
                }

                return Ok(cafeNotificationContacts);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin, Manager")]
        [HttpDelete, Route("RemoveCafeNotificationContact/{id:long}")]
        public async Task<IActionResult> RemoveCafeNotificationContact(long id)
        {
            try
            {
                var result = false;

                var currentUser =
                    User.Identity.GetUserById();

                var contactFromBase =
                    Accessor.Instance.GetCafeNotificationContactById(id);

                if (contactFromBase == null)
                {
                    throw new Exception("Attempt to work with unexisting object");
                }

                if (currentUser != null
                    &&
                    (
                        User.IsInRole(EnumUserRole.Admin)
                        || Accessor.Instance.IsUserManagerOfCafe(
                            currentUser.Id,
                            contactFromBase.CafeId
                            )
                        )
                    )
                {
                    result =
                        Accessor.Instance.RemoveCafeNotificationContact(id);
                }
                else
                {
                    throw new SecurityException("Attempt of unauthorized access");
                }

                return Ok(result);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin, Manager")]
        [HttpPut, Route("UpdateCafeNotificationContact")]
        public async Task<IActionResult> UpdateCafeNotificationContact([FromBody]CafeNotificationContactModel cafeNotificationContact)
        {
            try
            {
                var result = false;

                var currentUser =
                    User.Identity.GetUserById();

                var contactFromBase =
                    Accessor
                        .Instance
                        .GetCafeNotificationContactById(
                            cafeNotificationContact.Id
                        );

                if (contactFromBase == null)
                {
                    throw new Exception("Attempt to work with unexisting object");
                }

                if (currentUser != null
                    &&
                    (
                        User.IsInRole(EnumUserRole.Admin)
                        || Accessor.Instance.IsUserManagerOfCafe(
                            currentUser.Id,
                            cafeNotificationContact.CafeId
                            )
                        )
                    )
                {
                    CafeNotificationContact contactToBase =
                        cafeNotificationContact.GetEntity();

                    result =
                        Accessor
                            .Instance
                            .UpdateCafeNotificationContact(
                                contactToBase
                            );
                }
                else
                {
                    throw new SecurityException("Attempt of unauthorized access");
                }

                return Ok(result);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        public static CafeModel GetCafeById(IFoodContext context, long id)
        {
            Cafe cafe = context.Cafes.FirstOrDefault(c => c.Id == id && c.IsDeleted == false);
            return cafe.GetContract();
        }

        private string GetCafeTypeString(CafeType type)
        {
            switch (type) {
                case CafeType.CompanyOnly:
                    return "COMPANY_ONLY";
                case CafeType.CompanyPerson:
                    return "COMPANY_PERSON";
                case CafeType.PersonOnly:
                    return "PERSON_ONLY";
                default:
                    throw new ArgumentException("Bad cafe type", nameof(type));
            }
        }
    }

    public static class CafeMenuPatternExtensions
    {
        public static CafeMenuPatternModel GetContract(this CafeMenuPattern patternM)
        {
            return (patternM == null)
                ? new CafeMenuPatternModel()
                : new CafeMenuPatternModel
                {
                    Id = patternM.Id,
                    CafeId = patternM.CafeId,
                    Name = patternM.Name,
                    PatternToDate = patternM.PatternDate,
                    IsBanket = patternM.IsBanket,
                    Dishes = new List<CafeMenuPatternDishModel>()
                };
        }
    }

    public static class CafeNotificationContactExtensions
    {
        public static CafeNotificationContactModel GetContract(this CafeNotificationContact notification)
        {
            return (notification == null)
                ? null
                : new CafeNotificationContactModel
                {
                    CafeId = notification.CafeId,
                    Id = notification.Id,
                    NotificationChannel =
                        (NotificationChannelModel)notification
                            .NotificationChannelId,
                    NotificationContact =
                        notification.NotificationContact
                };
        }

        public static CafeNotificationContact GetEntity(this CafeNotificationContactModel notification)
        {
            return (notification == null)
                ? new CafeNotificationContact()
                : new CafeNotificationContact
                {
                    Id = notification.Id,
                    CafeId = notification.CafeId,
                    NotificationChannelId =
                        (short)notification.NotificationChannel,
                    NotificationContact =
                        notification.NotificationContact
                };
        }
    }

    public static class CostOfDeliveryExtensions
    {
        public static CostOfDeliveryModel GetContract(this CostOfDelivery costOfDelivery)
        {
            return (costOfDelivery == null)
                ? null
                : new CostOfDeliveryModel
                {
                    CafeId = costOfDelivery.CafeId,
                    CreateDate = costOfDelivery.CreateDate,
                    CreatorId = costOfDelivery.CreatorId,
                    DeliveryPrice = costOfDelivery.DeliveryPrice,
                    Id = costOfDelivery.Id,
                    LastUpdateByUserId = costOfDelivery.LastUpdateByUserId,
                    LastUpdDate = costOfDelivery.LastUpdDate,
                    OrderPriceFrom = costOfDelivery.OrderPriceFrom,
                    OrderPriceTo = costOfDelivery.OrderPriceTo,
                    ForCompanyOrders = costOfDelivery.ForCompanyOrders
                };
        }

        public static CostOfDelivery GetEntity(this CostOfDeliveryModel costOfDelivery)
        {
            return (costOfDelivery == null)
                ? new CostOfDelivery()
                : new CostOfDelivery
                {
                    CafeId = costOfDelivery.CafeId,
                    CreateDate = costOfDelivery.CreateDate,
                    CreatorId = costOfDelivery.CreatorId,
                    DeliveryPrice = costOfDelivery.DeliveryPrice,
                    Id = costOfDelivery.Id,
                    LastUpdateByUserId = costOfDelivery.LastUpdateByUserId,
                    LastUpdDate = costOfDelivery.LastUpdDate,
                    OrderPriceFrom = costOfDelivery.OrderPriceFrom,
                    OrderPriceTo = costOfDelivery.OrderPriceTo,
                    ForCompanyOrders = costOfDelivery.ForCompanyOrders
                };
        }
    }
}
