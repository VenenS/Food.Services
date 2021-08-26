using Food.Data.Entities;
using Food.Services.Config;
using Food.Services.Extensions.OrderExtensions;
using Food.Services.ShedulerQuartz;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Food.Data.Enums;

namespace Food.Services.Controllers
{
    public class OrderProcessor
    {
        #region Members

        #region Private Members

        private readonly OrderProcessingHelper _orderProcessingHelper;
        private readonly List<Exception> _exceptions;
        private readonly long _userId;
        private IConfigureSettings ConfigureSettings { get; }

        #endregion Private Members

        #region Public Members

        public List<OrderItem> OrderItems { get; }

        public Order Order { get; }

        public bool MistakesInOrder
        {
            get
            {
                if (_exceptions != null && _exceptions.Count > 0) return true;
                return false;
            }
        }

        #endregion Public Members

        #endregion Members

        #region Methods

        #region Private Methods

        private void CheckOrderAvailability(bool preOrder = false)
        {
            if (MistakesInOrder) return;
            var cafe = Accessor.Instance.GetCafeById(Order.CafeId);

            var orderStatus = new OrderStatusModel
            {
                Order = null,
                ExceptionList = new List<Exception>()
            };

            var cafeAvailabilityStatus =
                _orderProcessingHelper
                    .CheckCafeAvailability(
                        cafe,
                        Order.TotalPrice,
                        preOrder
                    );

            if (cafeAvailabilityStatus.ExceptionList?.Count > 0)
                orderStatus.ExceptionList.AddRange(cafeAvailabilityStatus.ExceptionList);

            if (Order.CompanyOrderId != null)
            {
                var currentCompanyOrder =
                    Accessor
                        .Instance
                        .GetCompanyOrderById((long)Order.CompanyOrderId);

                var companyOrderStatus =
                    _orderProcessingHelper
                        .CheckAvailabilityOfCompanyOrder(currentCompanyOrder, Order.DeliverDate);

                if (
                    companyOrderStatus.ExceptionList != null
                    && companyOrderStatus.ExceptionList.Count > 0
                )
                    orderStatus.ExceptionList.AddRange(companyOrderStatus.ExceptionList);
            }

            Order.CreationDate =
                Order.CreationDate == DateTime.MinValue
                    ? DateTime.Now
                    : Order.CreationDate;

            Order.PhoneNumber = new string(Order.PhoneNumber.Where(c => char.IsDigit(c)).ToArray()) ?? string.Empty;

            Order.DeliveryAddressId = Order.DeliveryAddressId ?? -1;

            Order.DeliverDate =
                Order.DeliverDate == DateTime.MinValue
                || Order.DeliverDate == null
                    ? DateTime.Now
                    : Order.DeliverDate;

            _exceptions.AddRange(orderStatus.ExceptionList);
        }

        private async Task<bool> SaveToDb()
        {
            if (MistakesInOrder || OrderItems == null || OrderItems.Count <= 0) return false;
            Order.OrderItems = null;
            var resultPostOrder = Accessor.Instance.PostOrder(Order, _userId);

            if (resultPostOrder == -1)
            {
                if (Order.Id > 0) Accessor.Instance.RemoveOrder(Order.Id, _userId);
                return false;
            }

            Order.Id = resultPostOrder;

            foreach (var orderItem in OrderItems)
            {
                orderItem.Order = Order;
                orderItem.OrderId = Order.Id;

                var resultPostOrderItem = Accessor.Instance.PostOrderItem(orderItem);

                if (resultPostOrderItem[0] == -1 || resultPostOrderItem[1] == -1) return false;

                orderItem.Id = resultPostOrderItem[1];
            }

            // Для корпоративных заказов надо обновить общую стоимость всего корпоративного заказа после добавления всех позиций по добавляемому заказу:
            if (Order.CompanyOrderId.HasValue)
                Accessor.Instance.UpdateCompanyOrderSum(Order.CompanyOrderId.Value);

            var cafe =
                Accessor.Instance.GetCafeById(Order.CafeId);

            // Проверяем на дублирование и создаем задачу в планировщике.
            if (Order.CompanyOrderId != null)
            {
                var executeTime = DateTime.Today;
                var changeTime = false;

                // Получаем часы оформления компанейских заказов в кафе
                try
                {
                    var companyOrder = Accessor.Instance.GetCompanyOrderById(
                        (long)Order.CompanyOrderId
                    );

                    if (companyOrder != null)
                    {
                        executeTime = (DateTime)companyOrder.AutoCloseDate;
                        changeTime = true;
                    }

                    // Добавление новой задачи в базу, при условии что заказ сделан в рабочий день для кафе.
                    if (changeTime)
                    {
                        await ShedulerQuartz.Scheduler.Instance
                            .DispatchCompanyOrderNotificationAt(executeTime, companyOrder.Id);
                        //Если у кафе установлено время автоотмены, создаем задачу на проверку минимальной суммы корпоративного заказа
                    }
                }
                catch (Exception ex)
                {
                    _exceptions.Add(ex);
                }
            }

            //Индивидуальные заказы
            if (Order.CompanyOrderId == null && Order.DeliverDate != null)
            {
                var changeTime = DateTime.Today < Order.DeliverDate.Value.Date;

                try
                {
                    var cafeteria = Accessor.Instance.GetCafeById(Order.CafeId);

                    if (cafeteria != null && changeTime)
                    {
                        int d1 = (int)Order.DeliverDate.Value.DayOfWeek;
                        int day = (int)DateTime.Today.DayOfWeek;
                        if (d1 > day)
                        {
                            day = d1 - day;
                        }
                        else
                        {
                            day = 7 - day + d1;
                        }

                        if (cafeteria.WorkingHours[day][0].OpeningTime.TimeOfDay.ToString() == Order.DeliverDate.Value.ToString("hh:mm:ss"))
                        {
                            //время доставки заказа == времени открытия кафе, посылаем сообщение в момент открытия кафе
                            await ShedulerQuartz.Scheduler.Instance.DispatchIndividualOrderNotification(Order.DeliverDate.Value, Order.Id);
                        }
                        else
                        {
                            //время доставки заказа != времени открытия кафе, вычитаем из времени доставки, указанному в заказе два часа
                            await ShedulerQuartz.Scheduler.Instance.DispatchIndividualOrderNotification(Order.DeliverDate.Value.AddHours(-2), Order.Id);
                        }

                        Order.DeliverDate = Order.DeliverDate.Value.AddHours(cafeteria.WorkingTimeFrom?.Hours ?? 0);
                        Order.DeliverDate = Order.DeliverDate.Value.AddMinutes((cafeteria.WorkingTimeFrom?.Minutes ?? 0) + 30);

                        await ShedulerQuartz.Scheduler.Instance
                            .DispatchIndividualOrderNotificationAt(Order.DeliverDate.Value, Order.Id);
                    }
                }
                catch (Exception ex)
                {
                    _exceptions.Add(ex);
                }
            }

            if (Order.CompanyOrderId == null && DateTime.Today >= (Order.DeliverDate?.Date ?? DateTime.MinValue))
            {
                NotificationBase notification =
                    new EmailNotification(ConfigureSettings);

                NotificationBodyBase notificationBody =
                    new NewOrderToCafeNotificationBody(cafe, Order, ConfigureSettings);

                notification.FormNotification(notificationBody);

                notification.SendNotificationAsync();

                return true;
            }

            return true;

        }

        #endregion Private Methods

        #region Public Methods

        public OrderProcessor(OrderProcessingHelper orderProcessingHelper, PostOrderRequest orderRequest, long userId, IConfigureSettings config)
        {
            ConfigureSettings = config;

            _orderProcessingHelper = orderProcessingHelper;
            _exceptions = new List<Exception>();
            OrderItems = new List<OrderItem>();

            if (orderRequest == null || orderRequest.Order == null)
            {
                _exceptions.Add(new Exception("Пустой запрос на отправку заказа"));
                return;
            }

            var result = _orderProcessingHelper.CheckPostedOrder(orderRequest);
            if (result.ExceptionList?.Count > 0)
                _exceptions.AddRange(result.ExceptionList);

            _userId = userId;

            Order = orderRequest.Order.GetEntity();
            Order.PayType = orderRequest.PaymentType;

            try
            {
                Order.TotalPrice =
                    _orderProcessingHelper
                        .GetOrderSumm(
                            orderRequest.Order.OrderItems,
                            orderRequest.Order.CafeId,
                            orderRequest.Order.DeliverDate,
                            _userId,
                            true
                        );
            }
            catch (Exception exc)
            {
                _exceptions.Add(exc);
            }

            if (!MistakesInOrder)
                foreach (var orderItem in orderRequest.Order.OrderItems)
                    OrderItems.Add(orderItem.GetEntity());

            Order.State =
                EnumOrderState.Created;
        }

        /// <summary>
        ///     Обработка заказа. Производятся следующие действия
        ///     1. Проверяются все блюда из пунктов заказа, что они есть в меню на дату доставки.
        ///     2. Проверяются цены блюд с учётом меню и базовой стоимости блюд.
        ///     3. Проверяются итоговые суммы количества и стоимости заказа.
        /// </summary>
        /// <returns></returns>
        public async Task Processing(bool preOrder = false)
        {
            CheckOrderAvailability(preOrder);

            if (!MistakesInOrder)
            {
                var dishIds = new List<long>();

                foreach (var item in OrderItems) dishIds.Add(item.DishId);

                try
                {
                    var dishList
                        = Accessor.Instance.GetFoodDishesById(dishIds.ToArray());

                    Order.TotalPrice = 0;

                    foreach (var item in OrderItems)
                    {
                        item.Dish = dishList.Find(d => d.Id == item.DishId);

                        if (item.Dish != null)
                        {
                            var scheduleForDish = Accessor.Instance
                                    .GetSchedulesForCafe(Order.CafeId, (DateTime)Order.DeliverDate)
                                    .Where(s => s.DishId == item.DishId).ToList();

                            if (scheduleForDish.Count > 0)
                            {
                                var simpleSchedule = scheduleForDish.Find(s => s.Type.Equals("S"));

                                if (simpleSchedule != null)
                                {
                                    item.Dish.BasePrice = simpleSchedule.Price ?? item.Dish.BasePrice;
                                }
                                else
                                {
                                    var scheduleWithPrice = scheduleForDish.Find(s => s.Price != null && s.Price > 0);

                                    item.Dish.BasePrice = scheduleWithPrice?.Price ?? item.Dish.BasePrice;
                                }
                            }
                            else
                            {
                                _exceptions.Add(
                                    new Exception(string.Format("Dish(id={0}) is missing in current schedule",
                                        OrderItems.IndexOf(item)))
                                );
                                item.Dish = null;
                                continue;
                            }

                            item.DishName = item.Dish.DishName;
                            item.DishKcalories = item.Dish.Kcalories;
                            item.DishBasePrice = item.Dish.BasePrice;
                            item.DishWeight = item.Dish.Weight;

                            if (item.DishCount >= 1)
                            {
                                item.TotalPrice = item.Dish.BasePrice * item.DishCount;

                                if (item.DishDiscountPrc != null && item.DishDiscountPrc != 0)
                                {
                                    var dishDiscount =
                                        item.DishDiscountPrc == null
                                        || item.DishDiscountPrc < 1
                                        || item.DishDiscountPrc > 100
                                            ? 0.0
                                            : (double)item.DishDiscountPrc / 100.0;

                                    item.TotalPrice =
                                        item.TotalPrice * (1.0 - dishDiscount);
                                }

                                Order.TotalPrice += item.TotalPrice;
                                Order.ItemsCount += item.DishCount;
                            }
                            else
                            {
                                _exceptions.Add(
                                    new Exception(string.Format("Wrong dish(id={0}) count", OrderItems.IndexOf(item)))
                                );
                            }
                        }
                        else
                        {
                            _exceptions.Add(
                                new Exception(
                                    string.Format("Dish(id={0}) is missing in base", OrderItems.IndexOf(item)))
                            );
                        }
                    }
                }
                catch (Exception e)
                {
                    _exceptions.Add(e);
                }

                //var discount =
                //    OrderProcessingHelper
                //        .Instance
                //        .GetDiscountValue(
                //            Order.TotalSum,
                //            Order.CafeId,
                //            Order.DeliverDate,
                //            _userId
                //        );

                //если у кафе есть минимальная сумма для корпоративной доставки + заказ является корпоративным
                double delivery = 0;
                if (Order.CafeId != null && Order.CompanyOrderId != null)
                {
                    var cafe = Accessor.Instance.GetCafeById(Order.CafeId);
                    var companyOrderId = (long)Order.CompanyOrderId;
                    var companyOrder = Accessor.Instance.GetCompanyOrderById(companyOrderId);
                    if (cafe.DailyCorpOrderSum > (companyOrder.TotalPrice + Order.TotalPrice))
                    {
                        var shippingCost = Accessor.Instance.CalculateShippingCostsToSingleCompanyAddress(
                            Order.OrderInfo.OrderAddress,
                            companyOrderId,
                            Order.TotalPrice);

                        var count = shippingCost.Item2 + 1;
                        //var count = OrderProcessingHelper.Instance.GetCountCompanyOrders(companyOrderId) + 1;
                        delivery = (double)cafe.DailyCorpOrderSum / count;

                        _orderProcessingHelper.SetNewDeliveryForCompanyOrders(
                            companyOrderId,
                            Order.OrderInfo.OrderAddress,
                            delivery);
                    }
                }
                else
                {
                    delivery = _orderProcessingHelper.GetDeliveryPrice(
                        Order.TotalPrice,
                        Order.CafeId,
                        _userId
                    );
                }

                Order.TotalPrice =
                    Order.TotalPrice /*- discount*/ + delivery;
                if (!String.IsNullOrEmpty(Order.OddMoneyComment))
                {
                    if (Order.OddMoneyComment.StartsWith("Сумма заказа -"))
                        Order.OddMoneyComment += $" {Order.TotalPrice} руб. (без сдачи)";
                }
                else
                    Order.OddMoneyComment = "Корпоративный заказ";
                Order.OrderInfo.TotalSumm = Order.TotalPrice;
                Order.OrderInfo.DeliverySumm = delivery;
                //Order.OrderInfo.DiscountSumm = discount;

                for (var i = 0; i < OrderItems.Count; i++)
                    if (OrderItems[i].Dish == null)
                    {
                        OrderItems.RemoveAt(i);
                        i--;
                    }
            }

            await SaveToDb();
        }

        /// <summary>
        ///     Сохранение в базу.
        /// </summary>
        /// <returns></returns>
        public PostOrderResponse GetResult()
        {
            return new PostOrderResponse
            {
                OrderStatus = new OrderStatusModel
                {
                    ExceptionList = _exceptions,
                    Order = Order.GetContract()
                },
                PaymentURL = string.Empty
            };
        }

        #endregion Public Methods

        #endregion Methods
    }
}
