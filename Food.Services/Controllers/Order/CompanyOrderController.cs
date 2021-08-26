using Food.Data;
using Food.Data.Entities;
using Food.Services.Models;
using Food.Services.Extensions;
using Food.Services.ShedulerQuartz;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.ServiceModel;
using System.Threading.Tasks;
using QuartzScheduler = Food.Services.ShedulerQuartz;
using System.Threading;
using Food.Services.Extensions.OrderExtensions;
using Food.Services.Services;

namespace Food.Services.Controllers
{
    [Route("api/companyorder")]
    public class CompanyOrderController : ContextableApiController
    {
        private readonly INotificationService _notificationService;
        private readonly OrderProcessingHelper _orderProcessingHelper;

        [ActivatorUtilitiesConstructor]
        public CompanyOrderController(INotificationService notificationService,
                                      IMenuService menuService,
                                      ICurrentUserService currentUserService)
        {
            _notificationService = notificationService;
            _orderProcessingHelper = new OrderProcessingHelper(menuService, currentUserService);
        }

        public CompanyOrderController(IFoodContext context, Accessor accessor)
        {
            Accessor = accessor;
            Accessor.SetTestingModeOn(context);
            Context = context;
            TestMode = true;
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        [Route("")]
        public IActionResult AddNewCompanyOrder([FromBody]CompanyOrderModel companyOrder)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var currentUser = User.Identity.GetUserId();
                var companyOrderByBase = companyOrder.GetEntity();
                companyOrderByBase.CreationDate = DateTime.Now;
                companyOrderByBase.IsDeleted = false;
                companyOrderByBase.LastUpdate = DateTime.Now;
                companyOrderByBase.LastUpdateByUserId = currentUser;
                return Ok(accessor.AddCompanyOrder(companyOrderByBase));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        [Route("schedule")]
        public IActionResult AddNewCompanyOrderSchedule([FromBody]CompanyOrderScheduleModel companyOrderSchedule)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var companyOrderScheduleByBase =
                    companyOrderSchedule.GetEntity();
                companyOrderScheduleByBase.CreateDate = DateTime.Now;
                companyOrderScheduleByBase.IsDeleted = false;
                companyOrderScheduleByBase.LastUpdDate = DateTime.Now;
                companyOrderScheduleByBase.IsActive = true;

                return Ok(accessor.AddCompanyOrderSchedule(companyOrderScheduleByBase));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin, Consolidator")]
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> EditCompanyOrder([FromBody]CompanyOrderModel companyOrder)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var currentUser = 1;//long.Parse(User.Identity.GetUserId()); в проекте авторизации

                var companyOrderByBase = companyOrder.GetEntity();
                companyOrderByBase.LastUpdateByUserId = currentUser;
                companyOrderByBase.LastUpdate = DateTime.Now;
                await ShedulerQuartz.Scheduler.Instance.CancelCompanyOrderNotification(companyOrder.Id);
                await ShedulerQuartz.Scheduler.Instance.DispatchCompanyOrderNotificationAt(companyOrder.OrderAutoCloseDate.Value, companyOrder.Id);

                return Ok(accessor.EditCompanyOrder(companyOrderByBase));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("schedule")]
        public async Task<IActionResult> EditCompanyOrderSchedule([FromBody]CompanyOrderScheduleModel companyOrderSchedule)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var companyOrderScheduleByBase = Accessor.Instance.GetCompanyOrderScheduleById(companyOrderSchedule.Id);


                if (companyOrderScheduleByBase == null) throw new Exception("Attempt to work with unexisting object");

                companyOrderScheduleByBase.BeginDate = companyOrderSchedule.BeginDate;
                companyOrderScheduleByBase.OrderStartTime = companyOrderSchedule.OrderStartTime.TimeOfDay;
                companyOrderScheduleByBase.CafeId = companyOrderSchedule.CafeId;
                companyOrderScheduleByBase.CompanyDeliveryAdress = companyOrderSchedule.CompanyDeliveryAdress;
                companyOrderScheduleByBase.CompanyId = companyOrderSchedule.CompanyId;
                companyOrderScheduleByBase.EndDate = companyOrderSchedule.EndDate;
                companyOrderScheduleByBase.IsActive = companyOrderSchedule.IsActive ?? false;
                companyOrderScheduleByBase.LastUpdateByUserId = companyOrderSchedule.LastUpdateByUserId;
                companyOrderScheduleByBase.LastUpdDate = DateTime.Now;
                companyOrderScheduleByBase.OrderSendTime = companyOrderSchedule.OrderSendTime.TimeOfDay;
                companyOrderScheduleByBase.OrderStopTime = companyOrderSchedule.OrderStopTime.TimeOfDay;

                var (id, cancelledOrders) = accessor.EditCompanyOrderSchedule(companyOrderScheduleByBase);
                if (_notificationService != null)
                {
                    var message = "В связи с изменениями расписания куратором, ваши заказы ({0}) были отменены";
                    var ok = !await CommonSmsNotifications.InformAboutCancelledOrders(
                        _notificationService,
                        cancelledOrders,
                        (u, os) => String.Format(message, os.Count()));

                    if (!ok)
                        Logger.Error($"Error sending SMS notifications (COS_ID={id})");
                }
                return Ok(id);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        [Route("availableorders")]
        public IActionResult GetAvailableCompanyOrders()
        {
            try
            {
                var companyOrdersList = new List<CompanyOrderModel>();
                var userId = User.Identity.GetUserId();
                var companies = Accessor.Instance.GetCompanies(userId);
                foreach (var company in companies)
                    foreach (
                        var companyOrderItem in Accessor.Instance.GetAvailableCompanyOrdersForTime(company.Id, null))
                        companyOrdersList.Add(companyOrderItem.GetContract());

                return Ok(companyOrdersList);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        //[Authorize(Roles = "User")]
        [HttpGet]
        [Route("availableorderstouser")]
        [Route("availableorderstouser/user/{userId:long}/date/{date:long}")]
        [Route("availableorderstouser/date/{date:long}")]
        [Route("availableorderstouser/user/{userId:long}")]
        public IActionResult GetAvailableCompanyOrdersToUser(long? userId = null, long? date = null)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var orders =
                    new List<CompanyOrderModel>();
                if (userId == null)
                {
                    userId = User.Identity.GetUserId();
                }
                else
                {
                    if (!User.IsInRole(EnumUserRole.Manager))
                        throw new SecurityException("Attempt of unauthorized access");
                }

                var userCompanies =
                    accessor.GetCompanies((long)userId);

                if (userCompanies != null
                    && userCompanies.Count > 0)
                    foreach (var company in userCompanies)
                    {
                        var currentOrders = accessor.GetCompanyOrdersAvailableToEmployees(company.Id);

                        if (date.HasValue)
                            currentOrders = currentOrders.Where(co =>
                                _orderProcessingHelper
                                    .CheckAvailabilityOfCompanyOrder(co)
                                    .ExceptionList == null).ToList();
                        foreach (var companyOrder in currentOrders)
                            orders.Add(companyOrder.GetContract());
                    }

                return Ok(orders);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Проверить наличие компанейского заказа для пользователя на дату
        /// </summary>
        [HttpGet]
        [Route("CheckUserIsEmployee/cafe/{cafeId:long}")]
        [Route("CheckUserIsEmployee/cafe/{cafeId:long}/user/{userId:long}/date/{date:long}")]
        [Route("CheckUserIsEmployee/cafe/{cafeId:long}/date/{date:long}")]
        [Route("CheckUserIsEmployee/cafe/{cafeId:long}/user/{userId:long}")]
        public IActionResult CheckUserIsEmployee(long cafeId, long? userId = null, long? date = null)
        {
            try
            {
                Accessor accessor = GetAccessor();
                if (userId == null)
                {
                    userId = User.Identity.GetUserId();
                }
                var userCompanies =
                    accessor.GetCompanies((long)userId);

                if (userCompanies != null
                    && userCompanies.Count > 0)
                    foreach (var company in userCompanies)
                    {
                        var companyId = company.Id;

                        var currentOrders =
                            accessor.GetCompanyOrders(companyId);

                        if (date.HasValue)
                        {
                            currentOrders = currentOrders.Where(co =>
                                _orderProcessingHelper
                                    .CheckAvailabilityOfCompanyOrder(co, cafeId)
                                    .ExceptionList == null
                                    ).ToList();

                        }

                        if (currentOrders.Count > 0)
                        {
                            return Ok(true);
                        }
                    }

                return Ok(false);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Consolidator")]
        [HttpGet]
        [Route("userorderfromcompanyorder/{companyOrderId:long}")]
        public IActionResult GetAvailableUserOrderFromCompanyOrder(long companyOrderId)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var userOrdersList = new List<OrderModel>();

                var currentUser = User.Identity.GetUserById();

                foreach (var userOrder in accessor.GetUserOrderFromCompanyOrder(companyOrderId))
                {
                    var userOrderItemsList = new List<OrderItemModel>();

                    foreach (var userOrderItem in accessor.GetOrderItemsByOrderId(null, userOrder.Id))
                        userOrderItemsList.Add(userOrderItem.GetContract());

                    var newOrder = userOrder.GetContract();
                    newOrder.OrderItems = userOrderItemsList;
                    newOrder.CreatorLogin = currentUser.Name;

                    userOrdersList.Add(newOrder);
                }

                return Ok(userOrdersList);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }


        [Authorize(Roles = "Manager,User, Consolidator, Admin")]
        [HttpGet]
        [Route("companyorder/{companyOrderId:long}")]
        public IActionResult GetCompanyOrder(long companyOrderId)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var currentUser = User.Identity.GetUserId();
                var companyOrder = accessor.GetCompanyOrderById(companyOrderId);
                if (companyOrder != null)
                {
                    if (User.IsInRole(EnumUserRole.Admin))
                        return Ok(companyOrder.GetContract());

                    if (User.IsInRole(EnumUserRole.Manager))
                    {
                        if (!accessor.IsUserManagerOfCafe(currentUser, companyOrder.CafeId))
                            throw new SecurityException("Attempt of unauthorized access");
                    }
                    else if (!accessor.IsUserCuratorOfCafe(currentUser, companyOrder.CompanyId))
                    {
                        var userCompanies = accessor.GetListOfCompanysByUserId(currentUser);

                        if (userCompanies.FirstOrDefault(c => c.Id == companyOrder.CompanyId) == null)
                            throw new SecurityException("Attempt of unauthorized access");
                    }
                    else
                    {
                        var result = companyOrder.GetContract();
                        result.UserOrders = accessor.GetUserOrderFromCompanyOrder(result.Id)
                            .Select(o => o.GetContract())
                            .ToList();
                        return Ok(result);
                    }
                }
                return Ok(companyOrder.GetContract());
            }
            catch (Exception e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.TargetSite);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin,Consolidator")]
        [HttpGet]
        [Route("companyorderschedulebyid/{id:long}")]
        public IActionResult GetCompanyOrderScheduleById(long id)
        {
            Accessor accessor = GetAccessor();
            var scheduleFromBase = accessor.GetCompanyOrderScheduleById(id);
            var companyOrderSchedule = scheduleFromBase.GetContract();
            return Ok(companyOrderSchedule);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("companyordersbydate/{companyId:long}/from/{startDate:datetime}/to/{endDate:long}")]
        [Route("companyordersbydate/{companyId:long}/cafe/{cafeId:long}/from/{startDate:long}/to/{endDate:long}")]
        public IActionResult GetListOfCompanyOrderByDate(long companyId, long startDate, long endDate, long? cafeId = null)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var companyOrders = new List<CompanyOrderModel>();

                var companyOrderFromBase =
                    accessor.GetCompanyOrdersByDate(
                        companyId,
                        cafeId,
                        DateTimeExtensions.FromUnixTime(startDate),
                        DateTimeExtensions.FromUnixTime(endDate)
                    );
                companyOrderFromBase.ForEach(item => companyOrders.Add(item.GetContract()));

                return Ok(companyOrders);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }
        [Authorize(Roles = "Admin, Consolidator")]
        [HttpPost]
        [Route("companyordersbydate/bydate/{date:long}")]
        public IActionResult GetCompanyOrderByDate(long date)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var currentUser =
                    User.Identity.GetUserById();
                var companyOrders = new List<CompanyOrderModel>();

                var companyOrderFromBase =
                    accessor.GetCompanyOrdersByDate(
                        DateTimeExtensions.FromUnixTime(date)
                    );
                if (currentUser != null)
                {
                    if (User.IsInRole("Admin"))
                        return Ok(companyOrderFromBase.Select(o => o.GetContract()));

                    foreach (var item in companyOrderFromBase)
                    {
                        if (Accessor.Instance.IsUserCuratorOfCafe(currentUser.Id, item.CompanyId))
                        {
                            companyOrders.Add(item.GetContract());
                        }
                    }
                }

                return Ok(companyOrders);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("companyordersschedule/{companyId:long}/from/{startDate:long}/to/{endDate:long}")]
        [Route("companyordersschedule/{companyId:long}/cafe/{cafeId:long}/from/{startDate:long}/to/{endDate:long}")]
        public IActionResult GetListOfCompanyOrderSchedule(long companyId, long startDate, long endDate, long? cafeId = null)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var companyOrderSchedule =
                    new List<CompanyOrderScheduleModel>();

                var companyOrderScheduleFromBase =
                    accessor.GetCompanyOrderScheduleByRangeDate(
                        companyId,
                        cafeId,
                        DateTimeExtensions.FromUnixTime(startDate),
                        DateTimeExtensions.FromUnixTime(endDate)
                    );
                companyOrderScheduleFromBase.ForEach(item => companyOrderSchedule.Add(item.GetContract()));

                return Ok(companyOrderSchedule);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("companyordersschedulebycafe/{cafeId:long}")]
        public IActionResult GetListOfCompanyOrderScheduleByCafeId(long cafeId)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var companyOrderSchedules =
                    new List<CompanyOrderScheduleModel>();

                var companyOrderScheduleByBase =
                    accessor.GetCompanyOrderScheduleByCafeId(cafeId);

                companyOrderScheduleByBase.ForEach(item => companyOrderSchedules.Add(item.GetContract()));

                return Ok(companyOrderSchedules);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("companyordersschedulebycompany/{companyId:long}")]
        public IActionResult GetListOfCompanyOrderScheduleByCompanyId(long companyId)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var companyOrderSchedules =
                    new List<CompanyOrderScheduleModel>();

                var companyOrderScheduleByBase =
                    accessor.GetCompanyOrderScheduleByCompanyId(companyId);
                companyOrderScheduleByBase.ForEach(item => companyOrderSchedules.Add(item.GetContract()));

                return Ok(companyOrderSchedules);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Получить корпоративные заказы для пользователя по списку кафе на дату
        /// Используется в при оформлении заказа
        /// </summary>
        [HttpPost]
        [Route("GetCompanyOrderForUserByCompanyId")]
        public async Task<IActionResult> GetCompanyOrderForUserByCompanyId([FromBody]RequestCartCompanyOrderModel model)
        {
            var response = GetAccessor().GetCompanyOrderForUserByCompanyId(model.CafeIds, model.CompanyId, model.Dates);

            return Ok(new ResponseModel() { Result = response.Select(e => e.GetContract()) });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete]
        [Route("companyorder/{companyOrderId:long}")]
        public IActionResult RemoveCompanyOrder(long companyOrderId)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var currentUser =
                    User.Identity.GetUserById();

                return
                    Ok(accessor.RemoveCompanyOrder(companyOrderId, currentUser.Id));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpDelete]
        [Route("companyorderschedule/{companyOrderScheduleId:long}")]
        public async Task<IActionResult> RemoveCompanyOrderSchedule(long companyOrderScheduleId)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var currentUser = User.Identity.GetUserById();
                var (ok, cancelledOrders) = accessor.RemoveCompanyOrderSchedule(companyOrderScheduleId, currentUser.Id);
                if (_notificationService != null)
                {
                    var message = "В связи с изменениями расписания куратором, ваши заказы ({0}) были отменены";
                    var sendOk = !await CommonSmsNotifications.InformAboutCancelledOrders(
                        _notificationService,
                        cancelledOrders,
                        (u, os) => String.Format(message, os.Count()));

                    if (!sendOk)
                        Logger.Error($"Error sending SMS notifications (COS_ID={companyOrderScheduleId})");
                }
                return Ok(ok);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }
    }
    public static class CompanyOrderExtensions
    {
        public static CompanyOrderModel GetContract(this CompanyOrder order)
        {
            return order == null
                ? null
                : new CompanyOrderModel
                {
                    CafeId = order.CafeId,
                    CompanyId = order.CompanyId,
                    CreateDate = order.CreationDate,
                    DeliveryAddressId = order.DeliveryAddress,
                    DeliveryDate = order.DeliveryDate,
                    OrderAutoCloseDate = order.AutoCloseDate,
                    Id = order.Id,
                    OrderOpenDate = order.OpenDate,
                    OrderStatus = order.State,
                    ContactEmail = order.ContactEmail,
                    ContactPhone = order.ContactPhone,
                    Company = order.Company?.GetContract(),
                    Cafe = order.Cafe?.GetContract(),
                    UserOrders = new List<OrderModel>(),
                    TotalPrice = order.TotalPrice ?? 0,
                    TotalDeliveryCost = order.TotalDeliveryCost,
                    CafeFullName = order.Cafe.CafeFullName
                };
        }

        public static CompanyOrder GetEntity(this CompanyOrderModel order)
        {
            return order == null
                ? new CompanyOrder()
                : new CompanyOrder
                {
                    CafeId = order.CafeId,
                    CompanyId = order.CompanyId,
                    CreationDate = order.CreateDate,
                    DeliveryAddress = order.DeliveryAddressId,
                    DeliveryDate = order.DeliveryDate,
                    AutoCloseDate = order.OrderAutoCloseDate,
                    Id = order.Id,
                    ContactEmail = order.ContactEmail,
                    ContactPhone = order.ContactPhone,
                    OpenDate = order.OrderOpenDate,
                    State = order.OrderStatus,
                    TotalDeliveryCost = order.TotalDeliveryCost
                };
        }
    }

    public static class CompanyOrderScheduleExtensions
    {
        public static CompanyOrderScheduleModel GetContract(this CompanyOrderSchedule companyOrderSchedule)
        {
            if (companyOrderSchedule == null)
                return null;
            else
            {
                var model = new CompanyOrderScheduleModel
                {
                    BeginDate = companyOrderSchedule.BeginDate,
                    CafeId = companyOrderSchedule.CafeId,
                    CompanyDeliveryAdress = companyOrderSchedule.CompanyDeliveryAdress,
                    CompanyId = companyOrderSchedule.CompanyId,
                    CreateDate = companyOrderSchedule.CreateDate,
                    CreatorId = companyOrderSchedule.CreatorId,
                    EndDate = companyOrderSchedule.EndDate,
                    Id = companyOrderSchedule.Id,
                    IsActive = companyOrderSchedule.IsActive,
                    LastUpdateByUserId = companyOrderSchedule.LastUpdateByUserId,
                    LastUpdDate = companyOrderSchedule.LastUpdDate,
                };

                if (companyOrderSchedule.EndDate != null && companyOrderSchedule.EndDate.Value.Date <= DateTime.Now.Date)
                {
                    model.OrderSendTime = companyOrderSchedule.EndDate.Value.Date.Add(companyOrderSchedule.OrderSendTime);
                    model.OrderStartTime = companyOrderSchedule.EndDate.Value.Date.Add(companyOrderSchedule.OrderStartTime);
                    model.OrderStopTime = companyOrderSchedule.EndDate.Value.Date.Add(companyOrderSchedule.OrderStopTime);
                }
                else
                {
                    model.OrderSendTime = DateTime.Now.Date.Add(companyOrderSchedule.OrderSendTime);
                    model.OrderStartTime = DateTime.Now.Date.Add(companyOrderSchedule.OrderStartTime);
                    model.OrderStopTime = DateTime.Now.Date.Add(companyOrderSchedule.OrderStopTime);
                }

                return model;
            }
        }

        public static CompanyOrderSchedule GetEntity(this CompanyOrderScheduleModel companyOrderSchedule)
        {
            return (companyOrderSchedule == null)
                ? new CompanyOrderSchedule()
                : new CompanyOrderSchedule
                {
                    BeginDate = companyOrderSchedule.BeginDate,
                    CafeId = companyOrderSchedule.CafeId,
                    CompanyDeliveryAdress = companyOrderSchedule.CompanyDeliveryAdress,
                    CompanyId = companyOrderSchedule.CompanyId,
                    CreateDate = companyOrderSchedule.CreateDate,
                    CreatorId = companyOrderSchedule.CreatorId,
                    EndDate = companyOrderSchedule.EndDate,
                    Id = companyOrderSchedule.Id,
                    IsActive = companyOrderSchedule.IsActive ?? false,
                    LastUpdateByUserId = companyOrderSchedule.LastUpdateByUserId,
                    LastUpdDate = companyOrderSchedule.LastUpdDate,
                    OrderSendTime = companyOrderSchedule.OrderSendTime.TimeOfDay,
                    OrderStartTime = companyOrderSchedule.OrderStartTime.TimeOfDay,
                    OrderStopTime = companyOrderSchedule.OrderStopTime.TimeOfDay
                };
        }
    }

}
