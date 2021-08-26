using Food.Data;
using Food.Data.Entities;
using Food.Services.Models;
using Food.Services.Extensions;
using Food.Services.Models.Order;
using Food.Services.Services;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Controllers;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.ServiceModel;
using System.Threading.Tasks;
using Food.Data.Enums;
using FoodData = ITWebNet.FoodService.Food.Data.Accessor.Models;
using ITWebNet.Food.AuthorizationServer.Controllers;
using Food.Services.Config;

namespace Food.Services.Controllers
{
    [Route("api/orders")]
    public class OrderController : ContextableApiController
    {
        INotificationService _notificationService { get; set; }
        IConfigureSettings ConfigureSettings { get; }
        private readonly OrderProcessingHelper _orderProcessingHelper;

        public OrderController(IFoodContext context, Accessor accessor)
        {
            // Конструктор для обеспечения юнит-тестов
            Accessor = accessor;
            Accessor.SetTestingModeOn(context);
            Context = context;
            TestMode = true;
            // Тестирование сервиса отправки СМС должно выполняться отдельно:
            _notificationService = null;
        }

        [ActivatorUtilitiesConstructor]
        public OrderController(IConfigureSettings configuration,
                               IMenuService menuService,
                               ICurrentUserService currentUserService,
                               INotificationService notificationService)
        {
            ConfigureSettings = configuration;
            _notificationService = notificationService;
            _orderProcessingHelper = new OrderProcessingHelper(menuService, currentUserService);
        }

        [Authorize(Roles = "Consolidator,Manager")]
        [HttpPost]
        [Route("changecompanyorderstatus/{companyOrderId:long}")]
        [Route("changecompanyorderstatus/{companyOrderId:long}/cafe/{cafeId:long}")]
        [Route("changecompanyorderstatus/{companyOrderId:long}/company/{companyId:long}")]
        [Route("changecompanyorderstatus/{companyOrderId:long}/cafe/{cafeId:long}/company/{companyId:long}")]
        public IActionResult ChangeCompanyOrderStatus(long companyOrderId, EnumOrderStatus newStatus, long? cafeId = null, long? companyId = null)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var currentUser = User.Identity.GetUserId();

                var order = accessor.GetCompanyOrderById(companyOrderId);

                var status = (EnumOrderStatus)order.State;
                // Проверяем, является ли пользователь менеджером кафе:
                bool userIsManager = false;
                if (cafeId.HasValue)
                    userIsManager = accessor.IsUserManagerOfCafe(currentUser, cafeId.Value);
                // Проверяем, является ли пользователь куратором кафе:
                bool userIsCurator = false;
                if (companyId.HasValue)
                    userIsCurator = accessor.IsUserCuratorOfCafe(currentUser, companyId.Value);
                // Если пользователь не является ни менеджером, ни куратором - значит, менять состояние заказов ему нельзя:
                if (!userIsManager && !userIsCurator)
                    throw new SecurityException("Attempt of unauthorized access");
                switch (status)
                {
                    case EnumOrderStatus.Created:
                        {
                            if (newStatus != EnumOrderStatus.Accepted
                                && newStatus != EnumOrderStatus.Abort)
                                return base.Ok(false);
                        }
                        break;
                    case EnumOrderStatus.Accepted:
                        {
                            if (newStatus != EnumOrderStatus.Delivery
                                && newStatus != EnumOrderStatus.Abort)
                                return base.Ok(false);
                        }
                        break;
                    case EnumOrderStatus.Delivery:
                        {
                            if (newStatus != EnumOrderStatus.Delivered
                                && newStatus != EnumOrderStatus.Abort)
                                return base.Ok(false);
                        }
                        break;
                    default:
                        return base.Ok(false);
                }

                return Ok(accessor.SetCompanyOrderStatus(companyOrderId, (long)newStatus, currentUser));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Consolidator,Manager")]
        [HttpPost]
        [Route("ChangeStatusOrdersInCompany")]
        public IActionResult ChangeStatusOrdersInCompany([FromBody]OrdersChangeStatusModel model)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var currentUser = User.Identity.GetUserId();
                var lstResult = GetAccessor().SetOrderStatus(new HashSet<long>(model.OrderIds), (long)model.Status, currentUser);
                return Ok(lstResult.Select(e => e.GetContract()));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Изменение пользователем параметров заказа
        /// </summary>
        /// <param name="order">Модель с новым состоянием заказа</param>
        /// <returns></returns>
        [Authorize(Roles = "User, Consolidator")]
        [HttpPost]
        [Route("changeorder")]
        public IActionResult ChangeOrder([FromBody]OrderModel order)
        {
            try
            {
                Ok(ChangeOrderHelper(order));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
            return Ok(new OrderStatusModel());
        }

        public OrderStatusModel ChangeOrderHelper([FromBody]OrderModel order)
        {
            if (order != null)
            {
                Accessor accessor = GetAccessor();
                var orderStatus = new OrderStatusModel();

                var oldOrder = accessor.GetOrderById(order.Id);

                // Пользователю можно только отменять заказ и только в том случае, если заказ не банкетный:
                if ((order.Status != null && oldOrder.State != (EnumOrderState)order.Status.Value &&
                     order.Status != (long)EnumOrderStatus.Abort) ||
                    oldOrder.BanketId.HasValue)
                    return new OrderStatusModel();

                // Пользователь может отменять заказ только в том случае, если у него состояние "создан" или "принят кафе". Отменять уже отправленные заказы нельзя:
                if (oldOrder.State != (EnumOrderState)(long)EnumOrderStatus.Accepted
                    && oldOrder.State != (EnumOrderState)(long)EnumOrderStatus.Created)
                {
                    throw new SecurityException("Status can`t be changed by user");
                }

                // Пользователь не может отменять заказы других пользователей:
                if (oldOrder.User.Name.ToLower() !=
                    User.Identity.Name.ToLower())
                {
                    throw new SecurityException("Invalid user");
                }

                // Если указан новый статус для заказа - устанавливаем его:
                if (order.Status != null)
                {
                    oldOrder.State = (EnumOrderState)order.Status;
                    oldOrder.LastUpdateByUserId = User.Identity.GetUserId();
                }
                // Если заказ не отменён - тогда меняем на новые адрес доставки, телефон и т.д.:
                if (oldOrder.State != (EnumOrderState)(long)EnumOrderStatus.Abort)
                {
                    oldOrder.DeliveryAddress = order.DeliveryAddress?.GetEntity();
                    oldOrder.PhoneNumber = new string(order.PhoneNumber.Where(c => char.IsDigit(c)).ToArray());
                    oldOrder.OddMoneyComment = order.OddMoneyComment;
                    oldOrder.Comment = order.Comment;
                    oldOrder.OrderInfo.OrderAddress = order.OrderInfo.OrderAddress;
                    if (order.DeliverDate != null)
                        oldOrder.DeliverDate =
                            oldOrder.DeliverDate.Value.Date
                            + order.DeliverDate.Value.TimeOfDay;
                }

                accessor.ChangeOrder(oldOrder);

                // Пересчёт общей стоимости и стоимости доставки корпоративного заказа выполняется в функции ChangeOrder, которая вызывается выше.

                // Пересчёт общей стоимости и стоимости доставки корпоративного заказа выполняется в функции ChangeOrder, которая вызывается выше.

                return orderStatus;
            }

            return new OrderStatusModel();
        }

        [Authorize]
        [HttpPost]
        [Route("changeorders")]
        public IActionResult ChangeOrders([FromBody]List<OrderModel> orders)
        {
            try
            {
                List<OrderStatusModel> ordersStatus = null;
                foreach (var order in orders)
                {
                    ordersStatus = new List<OrderStatusModel>();
                    ordersStatus.Add(ChangeOrderHelper(order));
                }

                return Ok(ordersStatus.ToArray());
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Consolidator,Manager")]
        [HttpGet]
        [Route("changestatus/{orderId:long}/cafe/{cafeId:long}")]
        [Route("changestatus/{orderId:long}/company/{companyId:long}")]
        [Route("changestatus/{orderId:long}/cafe/{cafeId:long}/company/{companyId:long}")]
        [Route("changestatus/{orderId:long}")]
        public async Task<IActionResult> ChangeOrderStatus(long orderId, int newStatus, long? cafeId = null, long? companyId = null)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var newOrderStatus = (EnumOrderStatus)newStatus;

                var currentUser = User.Identity.GetUserId();

                if (cafeId.HasValue && accessor.IsUserManagerOfCafe(currentUser, cafeId.Value) ||
                    companyId.HasValue && accessor.IsUserCuratorOfCafe(currentUser, companyId.Value))
                {
                    var order =
                        accessor.GetOrderById(orderId);

                    var status = (EnumOrderStatus)order.State;

                    switch (status)
                    {
                        case EnumOrderStatus.Created:
                            {
                                if (newOrderStatus != EnumOrderStatus.Accepted
                                    && newOrderStatus != EnumOrderStatus.Abort)
                                    return base.Ok(false);
                            }
                            break;
                        case EnumOrderStatus.Accepted:
                            {
                                if (newOrderStatus != EnumOrderStatus.Delivery
                                    && newOrderStatus != EnumOrderStatus.Abort
                                    && newOrderStatus != EnumOrderStatus.Delivered)
                                    return base.Ok(false);
                            }
                            break;
                        case EnumOrderStatus.Delivery:
                            {
                                if (newOrderStatus != EnumOrderStatus.Delivered
                                    && newOrderStatus != EnumOrderStatus.Abort)
                                    return base.Ok(false);
                            }
                            break;
                        default:
                            return base.Ok(false);
                    }

                    accessor.SetOrderStatus(order.Id, newStatus, currentUser);

                    //Отправка смс со статусом
                    string textStatus = String.Empty;
                    switch (newOrderStatus)
                    {
                        case EnumOrderStatus.Accepted:
                            textStatus = "Заказ принят кафе";
                            break;
                        case EnumOrderStatus.Delivery:
                            textStatus = "Заказ в процессе доставки";
                            break;
                        case EnumOrderStatus.Abort:
                            textStatus = "Заказ отменён";
                            break;
                    }
                    if (!String.IsNullOrEmpty(textStatus) && _notificationService != null)
                    {
                        if (order.User.PhoneNumberConfirmed)
                            await _notificationService.SmsSend(order.User.PhoneNumber, $"Новый статус Вашего заказа №{order.Id}: {textStatus}.");
                    }

                    if (newOrderStatus == EnumOrderStatus.Delivered)
                        accessor.AddPointsToReferrals(order.UserId, 3, order.TotalPrice);

                    var CompanyOrderId = order.CompanyOrderId;
                    if (CompanyOrderId.HasValue && newOrderStatus == EnumOrderStatus.Abort)
                        accessor.UpdateCompanyOrderSum(CompanyOrderId.Value);

                    return Ok(true);
                }

                throw new SecurityException("Attempt of unauthorized access");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "User,Manager")]
        [HttpGet]
        [Route("availablestatuses/{orderId:long}/fromreport/{fromReport:bool}")]
        [Route("availablestatuses/{orderId:long}")]
        public IActionResult GetAvailableOrderStatuses(long orderId, bool fromReport = true)
        {
            try
            {
                return Ok(GetListOfAvailableOrderStatuses(GetAccessor(), orderId, fromReport));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Consolidator,Manager,Admin")]
        [HttpPost]
        [Route("companyordersbyfilter")]
        public IActionResult GetCompanyOrderByFilters([FromBody]ReportFilter reportFilter)
        {
            try
            {
                Accessor accessor = GetAccessor();

                var workingFilter = new FoodData.ReportFilter
                {
                    CafeId = reportFilter.CafeId,
                    LoadUserOrders = reportFilter.LoadUserOrders,
                    AvailableStatusList = reportFilter.AvailableStatusList?.Select(o => (Data.Entities.EnumOrderStatus)o).ToList() ?? new List<Data.Entities.EnumOrderStatus>(),
                    StartDate = reportFilter.StartDate,
                    EndDate = reportFilter.EndDate,
                    OrdersIdList = reportFilter.OrdersIdList ?? new List<long>(),
                    CompanyOrdersIdList = reportFilter.CompanyOrdersIdList ?? new List<long>(),
                    BanketOrdersIdList = reportFilter.BanketOrdersIdList ?? new List<long>(),
                    ReportTypeId = reportFilter.ReportTypeId,
                    CompanyId = reportFilter.CompanyId,
                    LoadOrderItems = reportFilter.LoadOrderItems,
                    SortType = (EnumReportSortType)reportFilter.SortType,
                    ResultOrder = (EnumReportResultOrder)reportFilter.ResultOrder
                };

                if (workingFilter.CompanyOrdersIdList.Count > 0)
                {
                    workingFilter.AvailableStatusList.Clear();
                    workingFilter.StartDate = DateTime.MinValue;
                    workingFilter.EndDate = DateTime.MinValue;
                }

                var orders = accessor.GetCompanyOrdersByFilter(workingFilter);
                var currentOrdersList = orders.Select(c => c.GetContract()).ToList();

                foreach (var order in currentOrdersList)
                {
                    order.DeliveryCostDetails = accessor
                        .CalculateShippingCostsForCompanyOrder(order.Id)
                        .Select(x => new CompanyOrderModel.DeliveryCost {
                            Address = x.Item1,
                            Cost = x.Item2
                        }).ToList();
                }

                if (workingFilter.LoadUserOrders)
                    foreach (var order in currentOrdersList)
                    {
                        #region Формирование конкретного заказа компании

                        var companyFromBase = order.Company;

                        var ordersByCompanyOrderFromBase =
                            accessor.GetOrdersByCompanyOrderId(order.Id,
                                    workingFilter.AvailableStatusList);

                        if (companyFromBase != null
                            && ordersByCompanyOrderFromBase != null
                        )
                        {
                            var ordersByCompanyOrder = new List<OrderModel>();

                            foreach (var singleOrder in ordersByCompanyOrderFromBase)
                            {
                                #region Формирование конкретного заказа внутри заказа компании

                                var orderItems = new List<OrderItemModel>();
                                if (reportFilter.LoadOrderItems)
                                {
                                    var orderItemsFromAccessor =
                                        accessor.GetOrderItemsByOrderId(null, singleOrder.Id);

                                    foreach (var orderItem in orderItemsFromAccessor)
                                        orderItems.Add(orderItem.GetContract());
                                }

                                var creator = singleOrder.User.ToDto();
                                var newOrder = singleOrder.GetContract();
                                newOrder.OrderItems = orderItems;
                                newOrder.CreatorLogin = singleOrder.User.DisplayName;
                                newOrder.Creator = creator;

                                ordersByCompanyOrder.Add(newOrder);

                                #endregion
                            }

                            order.UserOrders = ordersByCompanyOrder;
                        }

                        #endregion
                    }

                return Ok(currentOrdersList);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpGet]
        [Route("currentordersbycafewithitems/{cafeId:long}/date/{wantedDateTime:long}")]
        [Route("currentordersbycafewithitems/{cafeId:long}")]
        public IActionResult GetCurrentListOfOrdersAndOrderItemsToCafe(long cafeId, long? wantedDateTime = null)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var date = wantedDateTime != null ? DateTimeExtensions.FromUnixTime(wantedDateTime.Value) : (DateTime?)null;
                var currentOrdersList = new List<OrderModel>();

                var currentUser =
                    User.Identity.GetUserById();

                if (accessor.IsUserManagerOfCafe(currentUser.Id, cafeId))
                {
                    date = OrderControllerHelper.CheckDateTime(date);

                    var currentOrdersListFromAccessor =
                        accessor.GetCurrentListOfOrdersToCafe(cafeId, date);

                    foreach (var order in currentOrdersListFromAccessor)
                    {
                        var orderItemsFromAccessor =
                            accessor.GetOrderItemsByOrderId(null, order.Id);
                        var orderItems = new List<OrderItemModel>();

                        foreach (var orderItem in orderItemsFromAccessor) orderItems.Add(orderItem.GetContract());

                        var newOrder = order.GetContract();
                        newOrder.CreatorLogin = order.User.FullName;
                        newOrder.OrderItems = orderItems;

                        currentOrdersList.Add(newOrder);
                    }

                    return Ok(currentOrdersList.ToArray());
                }

                throw new SecurityException("Attempt of unauthorized access");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpGet]
        [Route("currentordersbycafe/{cafeId:long}/date/{wantedDateTime:long}")]
        [Route("currentordersbycafe/{cafeId:long}")]
        public IActionResult GetCurrentListOfOrdersToCafe(long cafeId, long? wantedDateTime = null)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var date = wantedDateTime != null ? DateTimeExtensions.FromUnixTime(wantedDateTime.Value) : (DateTime?)null;
                var currentOrdersList = new List<OrderModel>();

                var currentUser =
                    User.Identity.GetUserById();

                if (accessor.IsUserManagerOfCafe(currentUser.Id, cafeId))
                {
                    date = OrderControllerHelper.CheckDateTime(date);

                    var currentOrdersListFromAccessor =
                        accessor.GetCurrentListOfOrdersToCafe(cafeId, date);

                    foreach (var order in currentOrdersListFromAccessor)
                    {
                        var newOrder = order.GetContract();
                        newOrder.CreatorLogin = currentUser.Name;


                        currentOrdersList.Add(newOrder);
                    }

                    return Ok(currentOrdersList.ToArray());
                }

                throw new SecurityException("Attempt of unauthorized access");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpGet]
        [Route("currentordersbycafe/orderbyclients/{cafeId:long}/date/{wantedDateTime:long}")]
        [Route("currentordersbycafe/orderbyclients/{cafeId:long}")]
        public IActionResult GetCurrentListOfOrdersToCafeOrderByClients(long cafeId, long? wantedDateTime = null)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var date = wantedDateTime != null ? DateTimeExtensions.FromUnixTime(wantedDateTime.Value) : (DateTime?)null;
                var currentOrdersList =
                    new List<KeyValuePair<UserModel, OrderModel>>();

                var currentUser =
                    User.Identity.GetUserById();

                if (accessor.IsUserManagerOfCafe(currentUser.Id, cafeId))
                {
                    date = OrderControllerHelper.CheckDateTime(date);

                    var currentOrdersListFromAccessor =
                        accessor.GetCurrentListOfOrdersToCafe(cafeId, date);

                    foreach (var order in currentOrdersListFromAccessor)
                    {
                        var orderItemsFromAccessor =
                            accessor.GetOrderItemsByOrderId(null, order.Id);
                        var orderItems = new List<OrderItemModel>();

                        foreach (var orderItem in orderItemsFromAccessor) orderItems.Add(orderItem.GetContract());

                        var newOrder = order.GetContract();
                        newOrder.CreatorLogin = currentUser.Name;
                        newOrder.OrderItems = orderItems;

                        currentOrdersList.Add(
                            new KeyValuePair<UserModel, OrderModel>(
                                order.User.GetContract(),
                                newOrder
                            )
                        );
                    }

                    return Ok(currentOrdersList);
                }

                throw new SecurityException("Attempt of unauthorized access");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpGet]
        [Route("currentordersbycafe/orderbycompanies/{cafeId:long}/date/{wantedDateTime:long}")]
        [Route("currentordersbycafe/orderbycompanies/{cafeId:long}")]
        public IActionResult GetCurrentListOfOrdersToCafeOrderByCompanies(long cafeId, long? wantedDateTime = null)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var date = wantedDateTime != null ? DateTimeExtensions.FromUnixTime(wantedDateTime.Value) : (DateTime?)null;
                var currentOrdersList =
                    new List<KeyValuePair<CompanyModel, OrderModel>>();

                var currentUser =
                    User.Identity.GetUserById();

                if (accessor.IsUserManagerOfCafe(currentUser.Id, cafeId))
                {
                    date = OrderControllerHelper.CheckDateTime(date);

                    var currentOrdersListFromAccessor =
                        accessor.GetCurrentListOfOrdersToCafe(cafeId, date)
                        .Where(o => o.CompanyOrder != null).ToList();

                    foreach (var order in currentOrdersListFromAccessor)
                    {
                        var orderItemsFromAccessor =
                            accessor.GetOrderItemsByOrderId(null, order.Id);
                        var orderItems = new List<OrderItemModel>();

                        foreach (var orderItem in orderItemsFromAccessor)
                            orderItems.Add(orderItem.GetContract());

                        var company = order.CompanyOrder.Company.GetContract();
                        company.DeliveryAddressId = order.CompanyOrder.DeliveryAddress;

                        var newOrder = order.GetContract();
                        newOrder.CreatorLogin = currentUser.Name;
                        newOrder.OrderItems = orderItems;

                        currentOrdersList.Add(
                            new KeyValuePair<CompanyModel, OrderModel>(company, newOrder)
                        );
                    }

                    return Ok(currentOrdersList);
                }

                throw new SecurityException("Attempt of unauthorized access");
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "User, Consolidator")]
        [HttpGet]
        [Route("order/{orderId:long}")]
        public IActionResult GetOrder(long orderId)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var order = accessor.GetOrderById(orderId);

                if (order == null) return null;
                var currentUser = User.Identity.GetUserById();
                if (currentUser == null)
                {
                    throw new SecurityException("Invalid user");
                }

                var orderItemList = new List<OrderItemModel>();

                foreach (var orderItem in accessor.GetOrderItemsByOrderId(currentUser.Id, orderId))
                    orderItemList.Add(orderItem.GetContract());

                var newOrder = order.GetContract();
                newOrder.CreatorLogin = order.User.Name;
                newOrder.OrderItems = orderItemList;

                return Ok(newOrder);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Consolidator,Manager,Admin")]
        [HttpPost]
        [Route("orderbyfilter")]
        public async Task<IActionResult> GetOrderByFilters([FromBody]ReportFilter reportFilter)
        {
            try
            {

                var workingFilter = new FoodData.ReportFilter
                {
                    CafeId = reportFilter.CafeId,
                    AvailableStatusList = reportFilter.AvailableStatusList?.Select(o => (Data.Entities.EnumOrderStatus)o).ToList() ?? new List<Data.Entities.EnumOrderStatus>(),
                    SearchType = (EnumSearchType)(int)reportFilter.SearchType,
                    Search = reportFilter.Search,
                    StartDate = reportFilter.StartDate,
                    EndDate = reportFilter.EndDate,
                    OrdersIdList = reportFilter.OrdersIdList ?? new List<long>(),
                    CompanyOrdersIdList = reportFilter.CompanyOrdersIdList ?? new List<long>(),
                    BanketOrdersIdList = reportFilter.BanketOrdersIdList ?? new List<long>(),
                    ReportTypeId = reportFilter.ReportTypeId,
                    CompanyId = reportFilter.CompanyId,
                    LoadOrderItems = reportFilter.LoadOrderItems,
                    SortType = (EnumReportSortType)(int)reportFilter.SortType,
                    ResultOrder = (EnumReportResultOrder)reportFilter.ResultOrder
                };

                if (workingFilter.OrdersIdList.Count > 0)
                {
                    workingFilter.AvailableStatusList.Clear();
                    workingFilter.StartDate = DateTime.MinValue;
                    workingFilter.EndDate = DateTime.MinValue;
                }

                var orders = new List<OrderModel>();
                User.Identity.GetUserById();

                Accessor accessor = GetAccessor();
                var ordersList = await accessor.GetOrdersByFilter(workingFilter);
                foreach (var order in ordersList)
                {
                    var creator = order.User?.ToDto();
                    var newOrder = order.GetContract(true, true);
                    newOrder.Creator = creator;
                    newOrder.CreatorLogin = order.User.DisplayName;
                    // Загрузка позиций в ордер:
                    if (workingFilter.LoadOrderItems)
                        newOrder.OrderItems = order.OrderItems.Where(i => !i.IsDeleted)
                            .Select(i => i.GetContract()).ToList();
                    else
                        newOrder.OrderItems = new List<OrderItemModel>();
                    orders.Add(newOrder);
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

        [Authorize(Roles = "User")]
        [HttpGet]
        [Route("orderitems/user/{userId:long}/order/{orderId:long}")]
        public IActionResult GetOrderItems(long userId, long orderId)
        {
            try
            {
                return Ok(GetListOfOrderItems(GetAccessor(), userId, orderId));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Consolidator,Manager,Admin")]
        [HttpGet]
        [Route("orderformanager/{orderId:long}")]
        public IActionResult GetOrderForManager(long orderId)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var order = accessor.GetOrderById(orderId);

                if (order == null) return null;

                var orderItemList = new List<OrderItemModel>();

                foreach (var orderItem in accessor.GetOrderItemsByOrderId(null, orderId))
                    orderItemList.Add(orderItem.GetContract());

                var newOrder = order.GetContract();
                newOrder.CreatorLogin = order.User.Name;
                newOrder.OrderItems = orderItemList;

                return Ok(newOrder);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// не используется
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [Authorize(Roles = "User,Manager")]
        [HttpPost]
        [Route("orderpricedetails")]

        public IActionResult GetOrderPriceDetails([FromBody]OrderModel order)
        {
            try
            {
                var details = new TotalDetailsModel();

                var currentUser =
                    User.Identity.GetUserById();

                var summ = _orderProcessingHelper
                    .GetOrderSumm(
                        order.OrderItems,
                        order.CafeId,
                        order.DeliverDate,
                        currentUser.Id
                    );

                details.Discount =
                    _orderProcessingHelper
                        .GetDiscountValue(
                            summ,
                            order.CafeId,
                            order.DeliverDate,
                            currentUser.Id
                        );

                details.Delivery =
                    _orderProcessingHelper
                        .GetDeliveryPrice(
                            summ,
                            order.CafeId,
                            currentUser.Id
                        );

                details.BasePrice = summ;

                details.TotalSumm =
                    summ - details.Discount + details.Delivery;

                details.Order = order;

                return Ok(details);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Нужна проверка на роли в тестах
        /// </summary>
        [Authorize(Roles = "Consolidator,Manager")]
        [HttpGet]
        [Route("companyordersbyorderid/txt/{companyOrderId:long}")]
        public IActionResult GetOrdersInCompanyOrderByCompanyOrderIdInTxt(long companyOrderId)
        {
            try
            {
                var resultedTxt = string.Empty;

                var resultedXml = GetOrdersInCompanyOrderByCompanyOrderIdInXmlHelper(companyOrderId);

                if (!string.IsNullOrWhiteSpace(resultedXml))
                {
                    var assembly = Assembly.GetExecutingAssembly();

                    string xslt;

                    using (
                        var stream =
                            assembly.GetManifestResourceStream(
                                "ITWebNet.FoodService.Food.Core.XSLTOrdersToTxtTransform.xsl"
                            )
                    )
                    {
                        using (
                            var reader =
                                new StreamReader(stream)
                        )
                        {
                            xslt = reader.ReadToEnd();
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(xslt))
                    {
                        var xsltTransformer = new XslttRansform();

                        resultedTxt = xsltTransformer.XsltTransformation(resultedXml, xslt);
                    }
                }

                return Ok(resultedTxt);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Нужна проверка на роли в тестах
        /// </summary>
        [Authorize(Roles = "Consolidator,Manager")]
        [HttpGet]
        [Route("companyordersbyorderid/xml/{companyOrderId:long}")]
        public IActionResult GetOrdersInCompanyOrderByCompanyOrderIdInXml(long companyOrderId)
        {
            try
            {
                return Ok(GetOrdersInCompanyOrderByCompanyOrderIdInXmlHelper(companyOrderId));
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Нужна проверка на роли в тестах
        /// </summary>
        public string GetOrdersInCompanyOrderByCompanyOrderIdInXmlHelper(long companyOrderId)
        {
            Accessor accessor = GetAccessor();
            var companyOrder =
                    accessor.GetCompanyOrderById(companyOrderId);

            var currentUser =
                User.Identity.GetUserById();

            var resultedXml = string.Empty;

            if (
                companyOrder != null
                && currentUser != null
                &&
                (
                    User.IsInRole(EnumUserRole.Manager)
                    && accessor.IsUserManagerOfCafe(currentUser.Id, companyOrder.CafeId)
                    ||
                    User.IsInRole(EnumUserRole.Consolidator)
                    && accessor.CanUserReadInfoAboutCompanyOrders(
                        currentUser.Id,
                        companyOrder.CompanyId
                    )
                )
            )
            {
                var orders =
                    accessor.GetOrdersByCompanyOrderId(companyOrderId);

                var temp =
                    new List<OrderWithItemsAndListOfDishes>();

                foreach (var order in orders)
                {
                    var orderWithItems =
                        OrderControllerHelper.GetOrderWithOrderItemsAndDishesByOrderAndUserId(
                            order,
                            null
                        );

                    temp.Add(
                        new OrderWithItemsAndListOfDishes
                        {
                            Dishes = orderWithItems.Value,
                            Order = orderWithItems.Key
                        }
                    );
                }

                var xmlProcessor =
                    new XmlProcessing<List<OrderWithItemsAndListOfDishes>>();

                resultedXml = xmlProcessor.SerializeObject(temp);
            }

            return resultedXml;
        }

        [Authorize]
        [HttpGet]
        [Route("totalordersperday")]
        public IActionResult GetTotalOrdersPerDay()
        {
            var totalOrders =
                GetAccessor().GetTotalNumberOfOrdersPerDay();

            return Ok(totalOrders);
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        [Route("currentcart")]
        public IActionResult GetUserCurrentCart()
        {
            try
            {
                Accessor accessor = GetAccessor();
                var currentUser =
                    User.Identity.GetUserById();

                var ordersList = new List<OrderModel>();

                foreach (var order in accessor.GetUserOrdersWithSameStatus(
                        currentUser.Id,
                        (long)EnumOrderStatus.Cart,
                        null))
                {
                    var userOrderItemsList = new List<OrderItemModel>();

                    foreach (var userOrderItem in accessor.GetOrderItemsByOrderId(
                            currentUser.Id,
                            order.Id
                        )
                    )
                        userOrderItemsList.Add(userOrderItem.GetContract());

                    var newOrder = order.GetContract();
                    newOrder.CreatorLogin = currentUser.Name;
                    newOrder.OrderItems = userOrderItemsList;

                    ordersList.Add(newOrder);
                }

                return Ok(ordersList);
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
        [Route("userorders/{userId:long}/date/{orderDate:long}")]
        [Route("userorders/{userId:long}")]
        public IActionResult GetUserOrders(long userId, long? orderDate = null)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var date = orderDate != null ? DateTimeExtensions.FromUnixTime(orderDate.Value) : (DateTime?)null;
                var currentUser = accessor.GetUserById(userId);

                if (currentUser == null)
                {
                    throw new SecurityException("Invalid User");
                }

                var ordersList = new List<OrderModel>();

                foreach (var order in accessor.GetUserOrders(currentUser.Id, date))
                {
                    var newOrder = order.GetContract();
                    newOrder.CreatorLogin = currentUser.Name;

                    ordersList.Add(newOrder);
                }

                return Ok(ordersList);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Consolidator,Manager,Admin")]
        [HttpGet]
        [Route("userincompanyorders/{companyOrderId:long}")]
        public IActionResult GetUserOrdersInCompanyOrder(long companyOrderId)
        {
            var ordersByCompanyOrderFromBase =
                Accessor
                    .Instance
                    .GetUserOrderFromCompanyOrder(
                        companyOrderId
                    );

            var ordersByCompanyOrder = new List<OrderModel>();

            foreach (var singleOrder in ordersByCompanyOrderFromBase)
            {
                #region Формирование конкретного заказа внутри заказа компании

                var creator = singleOrder.User.ToDto();
                var newOrder = singleOrder.GetContract(false);
                newOrder.CreatorLogin = singleOrder.User.DisplayName;
                newOrder.Creator = creator;

                ordersByCompanyOrder.Add(newOrder);

                #endregion
            }

            return Ok(ordersByCompanyOrder);
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        [Route("userordersindates/{userId:long}/from/{startOrderDate:long}/to/{endOrderDate:long}")]
        [Route("userordersindates/{userId:long}/from/{startOrderDate:long}")]
        [Route("userordersindates/{userId:long}/to/{endOrderDate:long}")]
        [Route("userordersindates/{userId:long}")]
        public IActionResult GetUserOrdersInDateRange(long userId, long? startOrderDate = null, long? endOrderDate = null)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var startDate = startOrderDate != null ? DateTimeExtensions.FromUnixTime(startOrderDate.Value) : (DateTime?)null;
                var endDate = endOrderDate != null ? DateTimeExtensions.FromUnixTime(endOrderDate.Value) : (DateTime?)null;
                var currentUser = accessor.GetUserById(userId);

                if (currentUser == null)
                {
                    throw new SecurityException("Invalid user");
                }

                if (
                    startOrderDate == null
                    || endOrderDate == null
                    || startOrderDate > endOrderDate
                )
                {
                    var fault = new Fault
                    {
                        Message = "Invalid date range",
                        Description = "Invalid date range"
                    };
                    throw new SecurityException("Invalid date range");
                }

                var ordersList = new List<OrderModel>();
                var lstUserOrders = accessor.GetUserOrders(currentUser.Id, null, startDate, endDate);

                foreach (var order in lstUserOrders)
                {
                    var newOrder = order.GetContract();
                    newOrder.CreatorLogin = currentUser.Name;
                    ordersList.Add(newOrder);
                }

                return Ok(ordersList);
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
        [Route("userorderWithitemsanddishes/{userId:long}/date/{orderDate:long}")]
        [Route("userorderWithitemsanddishes/{userId:long}")]
        public IActionResult GetUserOrderWithItemsAndDishes(long userId, long? orderDate = null)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var date = orderDate != null ? DateTimeExtensions.FromUnixTime(orderDate.Value) : (DateTime?)null;
                var currentUser = accessor.GetUserById(userId);

                var userOrders = new Dictionary<OrderModel, List<FoodDishModel>>();

                if (currentUser == null)
                {
                    throw new SecurityException("Invalid user");
                }

                foreach (var order in accessor.GetUserOrders(currentUser.Id, date))
                {
                    var orderWithInfo = OrderControllerHelper.GetOrderWithOrderItemsAndDishesByOrderAndUserId(
                        order,
                        currentUser.Id
                    );

                    userOrders.Add(
                        orderWithInfo.Key,
                        orderWithInfo.Value
                    );
                }

                return Ok(userOrders);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("postorders")]
        public async Task<IActionResult> PostOrdersHttp([FromBody]PostOrderRequest orderRequest)
        {
            try
            {
                var response = await PostOrders(orderRequest);
                return Ok(response);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        public async Task<PostOrderResponse> PostOrders(PostOrderRequest orderRequest)
        {
            var currentUser = User.Identity.GetUserById();
            var orderProcessor = new OrderProcessor(_orderProcessingHelper, orderRequest, currentUser?.Id ?? 0, ConfigureSettings);

            await orderProcessor.Processing(orderRequest.Order.PreOrder);

            return orderProcessor.GetResult();
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        [Route("saveuserordercart")]
        public async Task<IActionResult> SaveUserOrderCartIntoBase([FromBody]OrderModel userOrder)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var currentUser =
                    User.Identity.GetUserById();

                var orders =
                    accessor.GetUserOrdersWithSameStatus(
                        currentUser.Id,
                        (long)EnumOrderStatus.Cart,
                        null
                    );

                foreach (var order in orders)
                {
                    foreach (var orderItem in accessor.GetOrderItemsByOrderId(currentUser.Id, order.Id))
                        accessor.DeleteOrderItem(orderItem.Id, currentUser.Id);

                    accessor.RemoveOrder(order.Id, currentUser.Id);
                }

                var response =
                    await PostOrders(
                        new PostOrderRequest
                        {
                            Order = userOrder
                        }
                    );

                accessor.SetOrderStatus(response.OrderStatus.Order.Id, (long)EnumOrderStatus.Cart, currentUser.Id);

                return Ok(response.OrderStatus);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        public List<OrderItemModel> GetListOfOrderItems(Accessor accessor, long userId, long orderId)
        {
            var currentUser = accessor.GetUserById(userId);
            if (currentUser == null)
            {
                throw new SecurityException("Invalid user");
            }

            return accessor.GetOrderItemsByOrderId(currentUser.Id, orderId)
                .Select(oi => oi.GetContract()).ToList();
        }

        public List<OrderStatusEnum> GetListOfAvailableOrderStatuses(Accessor accessor, long orderId, bool fromReport)
        {
            var order = accessor.GetOrderById(orderId);

            if (order == null) throw new SecurityException("Attempt of unauthorized access");

            var currentUser =
                User.Identity.GetUserById();

            if (currentUser != null
                && currentUser.Id == order.UserId
                && !fromReport)
                return new List<OrderStatusEnum> { OrderStatusEnum.Abort };

            List<OrderStatusEnum> orderStatuses = new List<OrderStatusEnum>();

            if (accessor.IsUserManagerOfCafe(currentUser.Id, order.CafeId))
            {
                var status = (OrderStatusEnum)order.State;

                switch (status)
                {
                    case OrderStatusEnum.Created:
                        {
                            orderStatuses.Add(OrderStatusEnum.Accepted);
                            orderStatuses.Add(OrderStatusEnum.Abort);
                        }
                        break;
                    case OrderStatusEnum.Accepted:
                        {
                            orderStatuses.Add(OrderStatusEnum.Delivery);
                            orderStatuses.Add(OrderStatusEnum.Abort);
                            orderStatuses.Add(OrderStatusEnum.Delivered);
                        }
                        break;
                    case OrderStatusEnum.Delivery:
                        {
                            orderStatuses.Add(OrderStatusEnum.Abort);
                            orderStatuses.Add(OrderStatusEnum.Delivered);
                        }
                        break;
                }
            }
            else
            {
                throw new SecurityException("Attempt of unauthorized access");
            }

            return orderStatuses;
        }

        [HttpPost]
        [Route("getOrderHistory/{userId:long}")]
        public IActionResult GetOrderHistory(long userId, [FromBody]OrderModel order)
        {
            try
            {                         
                OrderHistoryModel orderHistory = new OrderHistoryModel();
                Accessor accessor = GetAccessor();

                orderHistory.Cafe = CafesController.GetCafeById(accessor.GetContext(), order.CafeId);
                orderHistory.AvailableStatuses = GetListOfAvailableOrderStatuses(accessor, order.Id, false);

                orderHistory.Dishes = new List<FoodDishModel>();
                if (order.OrderItems == null)
                    order.OrderItems = GetListOfOrderItems(accessor, userId, order.Id);
                orderHistory.Order = order;
                foreach (var orderItem in order.OrderItems)
                {
                    FoodDishModel dish = DishesController.GetFoodDishModelByIdAndDate(
                        accessor, orderItem.FoodDishId, order.Create);
                    if (dish != null)
                        orderHistory.Dishes.Add(dish);
                }

                if (order.BanketId.HasValue)
                    orderHistory.Banket = BanketsController.GetBanketById(accessor, order.BanketId.Value);

                return Ok(orderHistory);
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
