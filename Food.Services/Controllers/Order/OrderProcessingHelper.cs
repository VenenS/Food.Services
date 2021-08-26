using Food.Data.Entities;
using Food.Data.Enums;
using Food.Services.Services;

using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;

using Microsoft.EntityFrameworkCore;

using Serilog;
using Serilog.Events;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Controllers
{
    public class OrderProcessingHelper 
    {
        private readonly IMenuService _menuService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public OrderProcessingHelper(IMenuService menuService, ICurrentUserService currentUserService)
        {
            _menuService = menuService;
            _currentUserService = currentUserService;

            // FIXME: fetch from DI.
            _logger = Log.ForContext<OrderProcessingHelper>();
        }

        public double GetDiscountValue(
            double summ,
            long cafeId,
            DateTime? deliverDate,
            long userId
            )
        {
            var company =
                Accessor
                    .Instance
                    .GetCompanies(userId)
                    .FirstOrDefault();

            var discountValue =
                Accessor.Instance.GetDiscountValue(
                    cafeId,
                    deliverDate ?? DateTime.Now,
                    userId,
                    company == null
                        ? null
                        : (long?)company.Id
                    ) * summ;

            double cafeDiscountValue = 0;

            var cafeDiscount =
                Accessor.Instance.GetCafeDiscountValue(
                    cafeId,
                    deliverDate ?? DateTime.Now,
                    summ
                    );

            if (cafeDiscount != null)
            {
                var percentFromCafeDiscount =
                    cafeDiscount.Percent != null
                        ? cafeDiscount.Percent.Value / 100.0 * summ
                        : 0;

                var summFromCafeDiscount =
                    cafeDiscount.Summ != null
                        ? cafeDiscount.Summ.Value
                        : 0;

                cafeDiscountValue =
                    percentFromCafeDiscount > summFromCafeDiscount
                        ? percentFromCafeDiscount
                        : summFromCafeDiscount;
            }

            var discount = cafeDiscountValue > discountValue
                ? cafeDiscountValue
                : discountValue;

            return discount;
        }

        public double GetDeliveryPrice(
            double summ,
            long cafeId,
            long userId
            )
        {
            var costOfDelivery =
                Accessor
                    .Instance
                    .GetListOfDeliveryCosts(cafeId, summ)
                    .FirstOrDefault();

            var delivery = costOfDelivery != null
                ? costOfDelivery.DeliveryPrice
                : 0;

            return delivery;
        }

        public double GetOrderSumm(
            List<OrderItemModel> orderItems,
            long cafeId,
            DateTime? deliverDate,
            long userId,
            bool needThrowExceptions = false
            )
        {
            if (orderItems == null)
            {
                if (needThrowExceptions)
                {
                    throw new Exception("Отсутствует хотя бы один элемент заказа");
                }

                return 0;
            }

            orderItems =
                orderItems
                    .Where(
                        oi =>
                            oi != null
                            && oi.DishCount > 0
                    )
                    .ToList();

            if (needThrowExceptions && orderItems.Count == 0)
            {
                throw new Exception("Отсутствует хотя бы один элемент заказа");
            }

            var dishIds =
                orderItems.Select(oi => oi.FoodDishId).ToList();

            var dishList =
                Accessor
                    .Instance
                    .GetFoodDishesById(
                        dishIds.ToArray()
                    );

            var scheduleForDish =
                Accessor.Instance.GetSchedulesForCafe
                    (
                        cafeId,
                        deliverDate ?? DateTime.Now
                    );

            double summ = 0;

            if (scheduleForDish != null && scheduleForDish.Count > 0)
            {
                foreach (var orderItem in orderItems)
                {
                    var simpleSchedule =
                        scheduleForDish
                            .Find(
                                s =>
                                    s.Type.Equals("S")
                                    && s.DishId == orderItem.FoodDishId
                            );

                    var dish =
                        dishList
                            .FirstOrDefault(
                                d =>
                                    d.Id == orderItem.FoodDishId
                            );

                    if (needThrowExceptions && dish == null)
                    {
                        throw new Exception("В заказе присутствует несуществующее блюдо");
                    }

                    if (
                        needThrowExceptions
                        && !scheduleForDish
                            .Any(
                                s =>
                                    s.DishId == orderItem.FoodDishId
                            )
                        )
                    {
                        throw new Exception("В заказе присутствуют блюда, которые отсутствуют в расписании");
                    }

                    if (simpleSchedule != null)
                    {
                        summ +=
                            (
                                simpleSchedule.Price
                                ??
                                (
                                    dish != null
                                        ? dish.BasePrice
                                        : 0
                                    )
                                ) * orderItem.DishCount;
                    }
                    else
                    {
                        var scheduleWithPrice =
                            scheduleForDish
                                .FirstOrDefault(
                                    s =>
                                        s.Price != null
                                        && s.Price > 0
                                        && s.DishId == orderItem.FoodDishId
                                );

                        if (scheduleWithPrice != null)
                        {
                            summ +=
                                scheduleWithPrice
                                    .Price
                                    .Value
                                * orderItem.DishCount;
                        }
                        else
                        {
                            summ +=
                                (
                                    dish != null
                                        ? dish.BasePrice
                                        : 0
                                    ) * orderItem.DishCount;
                        }
                    }
                }
            }
            else
            {
                if (needThrowExceptions)
                    throw new Exception("В заказе присутствуют блюда, которые отсутствуют в расписании");
            }

            return summ;
        }

        public OrderStatusModel CheckAvailabilityOfCompanyOrder(CompanyOrder currentCompanyOrder, DateTime? date = null)
        {
            if (currentCompanyOrder != null)
            {
                var compareDate = DateTime.Now;
                if (date.HasValue && date.Value.Date > DateTime.Now.Date)
                    compareDate = date.Value.Date.Add(DateTime.Now.TimeOfDay);
                if (
                    (
                        currentCompanyOrder.AutoCloseDate != null
                        && DateTime.Compare(
                            (DateTime)currentCompanyOrder.AutoCloseDate,
                            compareDate
                        ) >= 0
                    )
                    &&
                    (
                        currentCompanyOrder.OpenDate != null
                        && DateTime.Compare(
                            (DateTime)currentCompanyOrder.OpenDate,
                            compareDate
                        ) <= 0
                    )
                    || compareDate.Date > DateTime.Now.Date
                )

                {
                    return new OrderStatusModel { ExceptionList = null };
                }
            }

            return
                new OrderStatusModel
                {
                    Order = null,
                    ExceptionList = new List<Exception>
                    {
                            new Exception(
                                "Ваш заказ не был обработан, так как время для заказа истекло."
                                )
                    }
                };
        }

        public OrderStatusModel CheckAvailabilityOfCompanyOrder(CompanyOrder currentCompanyOrder, long cafeId, DateTime? date = null)
        {
            if (currentCompanyOrder != null)
            {
                var compareDate = DateTime.Now;
                if (date.HasValue && date.Value.Date > DateTime.Now.Date)
                    compareDate = date.Value.Date.Add(DateTime.Now.TimeOfDay);
                if (currentCompanyOrder.OpenDate != null
                    && currentCompanyOrder.OpenDate.Value.Date == compareDate.Date
                    && currentCompanyOrder.CafeId == cafeId
                )
                {
                    return new OrderStatusModel { ExceptionList = null };
                }
            }

            return
                new OrderStatusModel
                {
                    Order = null,
                    ExceptionList = new List<Exception>
                    {
                            new Exception("Отсутствует заказ компании")
                    }
                };
        }

        public OrderStatusModel CheckAvailabilityOfCafeOrderTime(Cafe cafe, bool preOrder = false)
        {
            if (cafe != null)
            {
                if (cafe.WorkingTimeFrom.HasValue
                    && cafe.WorkingTimeTo.HasValue
                    )
                {
                    if (preOrder && cafe.DeferredOrder)
                        return new OrderStatusModel { ExceptionList = null };

                    if (
                        TimeSpan.Compare(
                            cafe.WorkingTimeTo.Value,
                            DateTime.Now.TimeOfDay
                            ) >= 0
                        &&
                        TimeSpan.Compare(
                            cafe.WorkingTimeFrom.Value,
                            DateTime.Now.TimeOfDay
                            ) <= 0
                        )
                    {
                        return new OrderStatusModel { ExceptionList = null };
                    }
                }
            }
            else
            {
                return new OrderStatusModel { ExceptionList = null };
            }

            return
                new OrderStatusModel
                {
                    Order = null,
                    ExceptionList = new List<Exception>
                    {
                            new Exception(
                                "Ваш заказ не был обработан, так как время для заказа истекло."
                                )
                    }
                };
        }

        public OrderStatusModel CheckCafeAvailability(Cafe cafe, double summ, bool preOrder = false)
        {
            var orderStatus =
                CheckAvailabilityOfCafeOrderTime(cafe, preOrder);

            if (orderStatus.ExceptionList == null)
            {
                if (cafe.MinimumSumRub > summ)
                {
                    orderStatus.ExceptionList =
                        new List<Exception>
                        {
                                new Exception(
                                    "Ваш заказ не был обработан, так как время для заказа истекло."
                                    )
                        };
                }
            }

            return orderStatus;
        }

        public OrderStatusModel CheckPostedOrder(PostOrderRequest orderRequest)
        {
            var orderModel = orderRequest.Order;

            if (orderRequest.PaymentType == EnumOrderPayType.InternetAcquiring
                && (string.IsNullOrWhiteSpace(orderRequest.SuccessURL)
                    || string.IsNullOrWhiteSpace(orderRequest.FailureURL)))
                return new OrderStatusModel(orderModel, "Отсутствуют ссылки для онлайн оплаты");

            if (orderModel.DeliverDate == null || orderModel.DeliverDate.Value.Date < DateTime.Now.Date)
                return new OrderStatusModel(orderModel, "Неверная дата доставки");

            // Проверка на возможность заказа блюд по активности связанных категорий.
            var orderDate = orderModel.DeliverDate.Value.Date;
            var deliveryTime = orderModel.DeliverDate.Value.Date != orderModel.DeliverDate.Value
                ? orderModel.DeliverDate.Value.TimeOfDay
                : (TimeSpan?)null;

            var dishIds = orderRequest.Order.OrderItems.Select(x => x.FoodDishId).Distinct();
            using (var fc = Accessor.Instance.GetContext())
            {
                var exceptionList = new List<Exception>();
                var dishCategoriesMap = fc.DishCategoryLinks.AsNoTracking()
                    .Where(x => x.CafeCategory.CafeId == orderModel.CafeId && dishIds.Contains(x.DishId)
                             && x.IsActive == true && !x.IsDeleted
                             && x.CafeCategory.IsActive == true && !x.CafeCategory.IsDeleted)
                    .Select(x => new { x.DishId, x.Dish.DishName, x.CafeCategory, x.CafeCategory.DishCategory.CategoryName })
                    .ToList()
                    .GroupBy(x => x.DishId, x => new { x.DishName, x.CafeCategory, x.CategoryName });

                foreach (var group in dishCategoriesMap)
                {
                    var canOrder = group
                        .Any(x => _menuService.CanOrderFromCategory(x.CafeCategory, orderDate, deliveryTime));

                    if (!canOrder)
                    {
                        if (_logger.IsEnabled(LogEventLevel.Debug))
                        {
                            _logger
                                .ForContext("userId", _currentUserService.GetUserId())
                                .ForContext("cafeId", orderModel.CafeId)
                                .ForContext("dishId", group.Key)
                                .ForContext("categories", group.Select(x => new { x.CafeCategory.Id, x.CategoryName }))
                                .Debug("Невозможно заказать {dish}: ни одна из связанных категорий не активна",
                                group.First().DishName);
                        }

                        var msg = deliveryTime != null
                            ? $"Блюдо '{group.First().DishName}' невозможно заказать на " +
                              $"{deliveryTime:hh\\:mm} {orderDate:dd.MM.yyyy} в связи с тем " +
                              "что блюдо не доступно на выбранную дату и время доставки"

                            : $"Блюдо '{group.First().DishName}' невозможно заказать " +
                              $"на {orderDate:dd.MM.yyyy} в связи с тем что блюдо не " +
                              "доступно во время открытия кафе";

                        exceptionList.Add(new Exception(msg));
                    }
                }

                if (exceptionList.Count > 0)
                    return new OrderStatusModel { ExceptionList = exceptionList };
            }

            return new OrderStatusModel { };
        }

        public int GetCountCompanyOrders(long orderId)
        {
            return Accessor.Instance.GetOrdersByCompanyOrderId(orderId).Count();
        }

        /// <summary>
        /// для всех заказов корпоративного заказа установить новую сумму доставки
        /// </summary>
        /// <param name="companyOrderId">Ид заказа</param>
        /// <param name="address">Для заказов на этот адрес будет установлена новая стоимость доставки</param>
        /// <param name="deliveryCost">Новая сумма доставки</param>
        /// <returns></returns>
        public bool SetNewDeliveryForCompanyOrders(long companyOrderId, string address, double deliveryCost)
        {
            try
            {
                var orderList = Accessor.Instance.GetOrdersByCompanyOrderId(companyOrderId);
                foreach (var item in orderList)
                {
                    if (String.Compare(address, item.OrderInfo.OrderAddress, true) != 0)
                    {
                        continue;
                    }

                    if (item.OrderInfo.DeliverySumm > 0)
                    {
                        item.TotalPrice -= item.OrderInfo.DeliverySumm;
                    }
                    item.TotalPrice += deliveryCost;
                    item.OrderInfo.DeliverySumm = deliveryCost;
                    Accessor.Instance.ChangeOrder(item);
                }
                return true;
            }
            catch
            {
                return false;
            }

        }
    }
}
