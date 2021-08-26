using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security;
using System.Threading;
using Food.Services;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;
using System.ServiceModel;
using Food.Data.Entities;
using System.Linq;
using ITWebNet.Food.AuthorizationServer.Extensions;
using Serilog;
using Food.Services.Extensions.OrderExtensions;
using Food.Services.Extensions.Dish;
using Food.Services.Extensions;

namespace ITWebNet.Food.AuthorizationServer.Controllers
{
    public static class OrderControllerHelper
    {
        public static DateTime CheckDateTime(DateTime? wantedDate)
        {
            if (wantedDate == null
                || wantedDate == DateTime.MinValue
                || wantedDate == DateTime.MaxValue
            )
                return DateTime.Now;
            return (DateTime)wantedDate;
        }

        private static int TryToConvertFromDoubleToInt32(double value)
        {
            int newValue;
            int.TryParse(value.ToString(CultureInfo.InvariantCulture), out newValue);
            return newValue;
        }

        /// <summary>
        /// Получить заказ с помощью эл. заказа и блюд по заказу и id юзера
        /// </summary>
        //  [PrincipalPermission(SecurityAction.Demand, Role = "User")]
        //  [CheckUserInBase(SecurityAction.Demand)]
        public static KeyValuePair<OrderModel, List<FoodDishModel>> GetOrderWithOrderItemsAndDishesByOrderAndUserId(
            Order order, long? userId)
        {
            try
            {
                var userOrderItemsList = new List<OrderItemModel>();

                var dishesId = new List<long>();

                foreach (var userOrderItem in Accessor.Instance.GetOrderItemsByOrderId(userId, order.Id))
                {
                    dishesId.Add(userOrderItem.DishId);
                    userOrderItemsList.Add(userOrderItem.GetContract());
                }

                var dishes = Accessor.Instance.GetFoodDishesById(dishesId.ToArray());

                var listOfCurrentDishes = new List<FoodDishModel>();

                for (var i = 0; i < dishes.Count; i++) listOfCurrentDishes.Add(dishes[i].GetContractDish());

                var newOrder = order.GetContract();
                newOrder.CreatorLogin = order.User.Name;
                newOrder.OrderItems = userOrderItemsList;

                return new KeyValuePair<OrderModel, List<FoodDishModel>>(
                    newOrder, listOfCurrentDishes
                );
            }
            catch (SecurityException e)
            {
                var fault = new SecurityFault();
                Log.Logger.Error("Attempt of unauthorized access");
                fault.Message = e.Message;
                fault.Description = "Wrong role";
                throw new FaultException<SecurityFault>(fault, new FaultReason(e.Message));
            }
        }

        //  [PrincipalPermission(SecurityAction.Demand, Role = "User")]
        //  [CheckUserInBase(SecurityAction.Demand)]
        public static void UpdateOrderInformationAboutSummAndCount(long orderId)
        {
            var currentUser = Thread.CurrentPrincipal.Identity.GetUserById();

            Accessor.Instance.UpdateOrderInformation(orderId, currentUser.Id);
        }

        // [PrincipalPermission(SecurityAction.Demand, Role = "User")]
        //  [CheckUserInBase(SecurityAction.Demand)]
        public static List<OrderModel> GetUserOrdersWithOrderItems(long userId, DateTime? orderDate)
        {
            try
            {
                var currentUser = Accessor.Instance.GetUserById(userId);

                if (currentUser == null)
                {
                    var fault = new Fault
                    {
                        Message = "Invalid user",
                        Description = "Invalid user"
                    };
                    throw new FaultException<Fault>(fault, new FaultReason("Invalid user"));
                }

                var ordersList = new List<OrderModel>();

                foreach (var order in Accessor.Instance.GetUserOrders(currentUser.Id, orderDate))
                {
                    var userOrderItemsList = new List<OrderItemModel>();

                    foreach (var userOrderItem in Accessor.Instance.GetOrderItemsByOrderId(currentUser.Id, order.Id))
                        userOrderItemsList.Add(userOrderItem.GetContract());

                    var newOrder = order.GetContract();
                    newOrder.CreatorLogin = currentUser.Name;
                    newOrder.OrderItems = userOrderItemsList;

                    ordersList.Add(newOrder);
                }

                return ordersList;
            }
            catch (SecurityException e)
            {
                var fault = new SecurityFault();
                Log.Logger.Error("Attempt of unauthorized access");
                fault.Message = e.Message;
                fault.Description = "Wrong role";
                throw new FaultException<SecurityFault>(fault, new FaultReason(e.Message));
            }
        }
    }

    public static class OrderExtensions
    {
        public static OrderModel GetContract(this Order order, bool loadOrderItems = true, bool cutOrder = false)
        {
            return (order == null)
                ? null
                : new OrderModel
                {
                    Id = order.Id,
                    BanketId = order.BanketId,
                    Comment = order.Comment,
                    CompanyOrderId = order.CompanyOrderId,
                    Create = order.CreationDate,
                    Creator = cutOrder ? null : order.User?.ToDto(),
                    DeliverDate = order.DeliverDate,
                    DeliveryAddressId = order.DeliveryAddressId,
                    DeliveryAddress = order.DeliveryAddress?.GetContract(),
                    itemsCount = order.ItemsCount,
                    OddMoneyComment = order.OddMoneyComment,
                    PhoneNumber = order.PhoneNumber,
                    TotalSum = order.TotalPrice,
                    Status = (long?)order.State,
                    CafeId = order.CafeId,
                    Cafe = cutOrder ? null : order.Cafe.GetContract(),
                    IsDeleted = order.IsDeleted,
                    CreatorId = order.CreatorId ?? order.UserId,
                    OrderInfo = order.OrderInfo?.GetContract(),
                    PayType = order.PayType,
                    ManagerComment = order.ManagerComment,
                    OrderItems = loadOrderItems && order.OrderItems != null
                        ? order.OrderItems?.Where(c => !c.IsDeleted).Select(c => c.GetContract()).ToList()
                        : new List<OrderItemModel>(),
                    CityId = order.CityId,
                    City = order.City.GetContract()
                };
        }

        public static Order GetEntity(this OrderModel order)
        {
            return (order == null)
                ? new Order()
                : new Order
                {
                    Comment = order.Comment,
                    CompanyOrderId = order.CompanyOrderId,
                    CreationDate = order.Create,
                    PhoneNumber = order.PhoneNumber,
                    DeliveryAddress = order.DeliveryAddress?.GetEntity(),
                    TotalPrice = order.TotalSum ?? 0,
                    ItemsCount = order.itemsCount ?? 0,
                    OddMoneyComment = order.OddMoneyComment,
                    DeliverDate = order.DeliverDate,
                    CafeId = order.CafeId,
                    BanketId = order.BanketId,
                    ManagerComment = order.ManagerComment,
                    PayType = order.PayType,
                    CityId = order.CityId,
                    OrderInfo = order.OrderInfo?.GetEntity(),
                    OrderItems = order.OrderItems?.Select(c => c.GetEntity()).ToList()
                };
        }
    }
}