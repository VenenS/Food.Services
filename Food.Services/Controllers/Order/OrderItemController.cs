using Food.Data;
using Food.Data.Entities;
using Food.Services.Extensions;
using ITWebNet.Food.AuthorizationServer.Controllers;
using ITWebNet.Food.Core.DataContracts.Common;
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

namespace Food.Services.Controllers
{
    [Route("api/orderitem")]
    public class OrderItemController : ContextableApiController
    {
        [ActivatorUtilitiesConstructor]
        public OrderItemController()
        {
        }
        public OrderItemController(IFoodContext context, Accessor accessor)
        {
            // Конструктор для обеспечения юнит-тестов
            Accessor = accessor;
            Accessor.SetTestingModeOn(context);
            Context = context;
            TestMode = true;
        }

        [Authorize(Roles = "User, Consolidator")]
        [HttpPost]
        [Route("changeitem")]
        public IActionResult ChangeOrderItem([FromBody]OrderItemModel orderItem)
        {
            try
            {
                Accessor accessor = GetAccessor();
                if (orderItem == null) return Ok(new OrderStatusModel());
                var orderStatus = new OrderStatusModel();
                if (orderItem.DishCount <= 0 || orderItem.DishCount >= 100000) return Ok(orderStatus);
                var oldOrderItem = accessor.GetOrderItemById(orderItem.Id);

                if (oldOrderItem == null) return Ok(orderStatus);
                var currentUser = User.Identity.GetUserId();

                var oldOrder = accessor.GetOrderById(oldOrderItem.OrderId);
                if (OrderItemServiceHelper.CheckCompanyOrderAvailability(oldOrder))
                {
                    oldOrderItem.LastUpdateByUserId = currentUser;
                    oldOrderItem.DishCount = orderItem.DishCount;
                    oldOrderItem.Comment = orderItem.Comment;
                    oldOrderItem.TotalPrice = orderItem.DishCount * orderItem.DishBasePrice;

                    accessor.ChangeOrderItem(oldOrderItem);
                    if (oldOrder != null)
                        OrderControllerHelper.UpdateOrderInformationAboutSummAndCount(oldOrder.Id);
                    // Если заказ входит в корпоративный - надо пересчитать общую стоимость всего корпоративного заказа и стоимость доставки:
                    if (oldOrder.CompanyOrderId.HasValue)
                    {
                        var companyOrder = accessor.GetCompanyOrderById(oldOrder.CompanyOrderId.Value);
                        if (companyOrder != null && companyOrder.State != (long)EnumOrderState.Abort)
                            accessor.UpdateCompanyOrderSum(companyOrder.Id);
                    }
                }
                else
                {
                    return Ok(new OrderStatusModel
                    {
                        ExceptionList = new List<Exception>
                        {
                            new Exception("Access to order is denied")
                        }
                    });
                }
                return Ok(orderStatus);
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        [Route("additems/{orderId:long}")]
        public IActionResult PostOrderItems([FromBody]OrderItemModel[] orderItems, long orderId)
        {
            try
            {
                Accessor accessor = GetAccessor();
                if (orderItems == null) return Ok(new OrderStatusModel());
                var postedOrderItems =
                    orderItems.Where(i => i.DishCount > 0).ToList();

                var oldOrder = accessor.GetOrderById(orderId);

                if (oldOrder != null)
                {
                    if (!OrderItemServiceHelper.CheckCompanyOrderAvailability(oldOrder))
                        return Ok(new OrderStatusModel
                        {
                            ExceptionList = new List<Exception>
                            {
                                new Exception("Access to order is denied")
                            }
                        });
                }
                else
                {
                    return Ok(new OrderStatusModel
                    {
                        ExceptionList = new List<Exception>
                        {
                            new Exception("Old order is not exists")
                        }
                    });
                }

                var newOrderItems = new List<OrderItem>();
                var currentUser =
                    User.Identity.GetUserId();

                #region Agregate Order Item If Exists

                var currentOrderItems =
                    accessor.GetOrderItemsByOrderId(currentUser, orderId);

                for (var i = 0; i < postedOrderItems.Count; i++)
                {
                    var existedOrderItemsWithSameDishes =
                        currentOrderItems
                            .Where(
                                oi => oi.DishId == postedOrderItems[i].FoodDishId
                            ).ToList();

                    if (existedOrderItemsWithSameDishes.Count <= 0) continue;
                    var changedOldOrderitem =
                        new OrderItemModel
                        {
                            Comment = postedOrderItems[i].Comment,
                            Discount = existedOrderItemsWithSameDishes[0].DishDiscountPrc,
                            DishCount =
                                existedOrderItemsWithSameDishes[0].DishCount
                                + postedOrderItems[i].DishCount,
                            FoodDishId = postedOrderItems[i].FoodDishId,
                            Id = existedOrderItemsWithSameDishes[0].Id,
                            IsDeleted = postedOrderItems[i].IsDeleted
                        };
                    ChangeOrderItem(changedOldOrderitem);
                    postedOrderItems.RemoveAt(i);
                    i--;
                }

                #endregion

                foreach (var orderItem in postedOrderItems)
                {
                    var newOrderItem = orderItem.GetEntity();
                    newOrderItem.CreatorId = currentUser;
                    newOrderItem.OrderId = orderId;
                    newOrderItem.DishDiscountPrc =
                        orderItem.Discount == null
                        || orderItem.Discount < 0
                        || orderItem.Discount > 99
                            ? 0
                            : orderItem.Discount;
                    newOrderItem.IsDeleted = false;
                    newOrderItems.Add(newOrderItem);
                }

                var dishesId = new List<long>();
                foreach (var newOrderItem in newOrderItems)
                    dishesId.Add(newOrderItem.DishId);

                var dishes = accessor.GetFoodDishesById(dishesId.ToArray());

                foreach (var newOrderItem in newOrderItems)
                {
                    newOrderItem.Dish = dishes.Find(d => d.Id == newOrderItem.DishId);
                    if (newOrderItem.Dish != null)
                    {
                        newOrderItem.DishKcalories = newOrderItem.Dish.Kcalories;
                        newOrderItem.DishName = newOrderItem.Dish.DishName;
                        newOrderItem.DishWeight = newOrderItem.Dish.Weight;
                        newOrderItem.DishBasePrice =
                            OrderItemServiceHelper.GetCurrentActualPriceForDish
                            (
                                newOrderItem.Dish.Id,
                                (DateTime)oldOrder.DeliverDate
                            );
                        if (Math.Abs(newOrderItem.DishBasePrice - newOrderItem.DishBasePrice) > 0.01)
                            continue;
                        newOrderItem.IsDeleted = false;
                        newOrderItem.TotalPrice =
                            newOrderItem.DishCount *
                            newOrderItem.DishBasePrice *
                            (
                                1.0 -
                                (
                                    newOrderItem.DishDiscountPrc == null
                                    || newOrderItem.DishDiscountPrc < 1
                                    || newOrderItem.DishDiscountPrc > 99
                                        ? 0.0
                                        : (double)newOrderItem.DishDiscountPrc / 100.0
                                )
                            );
                        newOrderItem.Order = oldOrder;

                        var result = accessor.PostOrderItem(newOrderItem);

                        if (
                            result != null
                            && result.Length == 2
                            && result[0] == orderId
                        )
                        {
                            //TODO ������� ��������� ��������� ���������� ������ � ������
                        }
                    }
                }
                Accessor.Instance.UpdateOrderInformation(oldOrder.Id, currentUser);
                return Ok(new OrderStatusModel());
            }
            catch (SecurityException e)
            {
                Logger.Error("[{0} {1}] - {2} in {3}",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"), LogLevel.Error, e, e.Method);
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "User, Consolidator")]
        [HttpDelete]
        [Route("{orderItemId:long}")]
        public IActionResult RemoveOrderItem(long orderItemId)
        {
            try
            {
                Accessor accessor = GetAccessor();
                var oldOrderItem = accessor.GetOrderItemById(orderItemId);
                var oldOrder = accessor.GetOrderById(oldOrderItem.OrderId);

                var currentUser =
                    User.Identity.GetUserId();

                if (oldOrder.State != EnumOrderState.Created)
                    return Ok(false);

                var result = accessor.DeleteOrderItem(orderItemId, currentUser);
                if (result)
                {
                    Accessor.Instance.UpdateOrderInformation(oldOrder.Id, currentUser);
                    // Если заказ входит в корпоративный - надо пересчитать общую стоимость всего корпоративного заказа и стоимость доставки:
                    if (oldOrder.CompanyOrderId.HasValue)
                    {
                        var companyOrder = accessor.GetCompanyOrderById(oldOrder.CompanyOrderId.Value);
                        if (companyOrder != null && companyOrder.State != (long)EnumOrderState.Abort)
                            accessor.UpdateCompanyOrderSum(companyOrder.Id);
                    }
                }

                return Ok(Convert.ToBoolean(result));
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
