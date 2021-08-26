using Food.Data;
using Food.Data.Entities;
using ITWebNet.FoodService.Food.Data.Accessor.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region Order
        /// <summary>
        /// Возвращает заказ
        /// </summary>
        /// <param name="id">идентификатор заказа</param>
        /// <returns></returns>
        public Order GetOrderById(long id)
        {
            var fc = GetContext();

            var query = fc.Orders.AsNoTracking()
                .Include(o => o.OrderItems)
                .Include(o => o.Cafe)
                .Include(o => o.OrderInfo)
                .Include(o => o.User)
                .FirstOrDefault(o => o.Id == id && !o.IsDeleted);

            return query;
        }

        /// <summary>
        /// Возвращает все заказы пользователя на дату
        /// </summary>
        /// <param name="userId">ид пользователя</param>
        /// <param name="date">дата</param>
        /// <returns></returns>
        public List<Order> GetUserOrders(long userId, DateTime? date, DateTime? startDeliverDate = null, DateTime? endDeliverDate = null)
        {
            var fc = GetContext();

            var query = fc.Orders.AsNoTracking()
                .Include(o => o.Cafe)
                .Include(o => o.OrderItems)
                .Include(o => o.OrderInfo)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.City)
                .Where(o => o.User.Id == userId && !o.User.IsDeleted && !o.IsDeleted);

            if (date != null)
                query = query.Where(e => e.CreationDate == date);

            if (startDeliverDate != null && endDeliverDate != null)
                query = query.Where(o => o.DeliverDate >= startDeliverDate && o.DeliverDate.Value.Date <= endDeliverDate);

            return query.OrderBy(e => e.CreationDate).ToList();
        }

        /// <summary>
        /// Возвращает заказы пользователя за указанный период
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="startDate">начало периода</param>
        /// <param name="endDate">окончание периода</param>
        /// <returns></returns>
        public List<Order> GetUserOrders(long userId, DateTime startDate, DateTime endDate)
        {
            var fc = GetContext();

            var query = fc.Orders.AsNoTracking().Where(o => o.User.Id == userId
                      && !o.User.IsDeleted && !o.IsDeleted
                      && o.CreationDate >= startDate
                      && o.CreationDate <= endDate);

            return query.ToList();
        }

        /// <summary>
        /// Возвращает все блюда заказа пользователя
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="orderId">идентификатор заказа</param>
        /// <returns></returns>
        public List<OrderItem> GetOrderItemsByOrderId(long? userId, long orderId)
        {
            var fc = GetContext();
            var query = fc.OrderItems.AsNoTracking()
                .Include(oi=>oi.Dish)
                .Where(oi => oi.OrderId == orderId && !oi.IsDeleted);

            if (userId.HasValue)
                query = query.Where(e => e.Order.User.Id == userId.Value && !e.Order.User.IsDeleted);
            return query.ToList();
        }

        /// <summary>
        /// Отправка заказа
        /// </summary>
        /// <param name="order">заказ</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        public int PostOrder(Order order, long userId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    order.State = EnumOrderState.Created;
                    order.IsDeleted = false;
                    order.CreatorId = userId;
                    order.UserId = userId;
                    order.CreationDate = DateTime.Now;
                    order.OrderInfo.CreateDate = DateTime.Now;
                    order.CreatorId = userId;
                    order.OrderInfo.IsDeleted = false;
                    // Если город не выбран, по умоллчанию Киров
                    if (!fc.Cities.Any(c=>c.Id==order.CityId) && fc.Cities.FirstOrDefault(
                        c => c.Name.Trim().ToLower() == "киров" && c.SubjectId == 43) is City city)
                    {
                        order.CityId = city.Id;
                        order.OrderInfo.CityId = city.Id;
                    }

                    fc.Orders.Add(order);
                    fc.SaveChanges();

                    SetOrderStatus(order.Id, (long)order.State, userId);

                    return (int)order.Id;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Изменить заказ
        /// </summary>
        /// <param name="order">заказ</param>
        /// <returns></returns>
        public int ChangeOrder(Order order)
        {
            try
            {
                using (var fc = GetContext())
                {
                    Order oldOrder = fc.Orders.FirstOrDefault(
                            o => o.Id == order.Id && !o.IsDeleted);

                    if (oldOrder == null) return -1;
                    oldOrder.DeliveryAddressId = order.DeliveryAddressId;
                    oldOrder.PhoneNumber = new string(order.PhoneNumber.Where(c => char.IsDigit(c)).ToArray());
                    oldOrder.OddMoneyComment = order.OddMoneyComment;
                    oldOrder.Comment = order.Comment;
                    oldOrder.DeliverDate = order.DeliverDate;
                    oldOrder.State = order.State;
                    oldOrder.LastUpdDate = DateTime.Now;
                    oldOrder.LastUpdateByUserId = order.LastUpdateByUserId;
                    oldOrder.OrderInfo.OrderAddress = order.OrderInfo.OrderAddress;
                    oldOrder.City = order.City;
                    oldOrder.ManagerComment = order.ManagerComment;
                    oldOrder.PayType = order.PayType;


                    SetOrderStatus(order.Id, (long)order.State, order.LastUpdateByUserId.Value);

                    if (order.OrderInfo != null)
                    {
                        OrderInfo orderInfo = fc.OrderInfo.FirstOrDefault(o => o.Id == order.OrderInfoId);
                        orderInfo.DeliverySumm = order.OrderInfo.DeliverySumm;
                    }

                    fc.SaveChanges();
                    // Если заказ корпоративный и он отменен - надо изменить общую стоимость корпоративного заказа:
                    if (oldOrder.CompanyOrderId.HasValue && order.State == EnumOrderState.Abort)
                        // Изменение стоимости корпоративного заказа:
                        UpdateCompanyOrderSum((long)oldOrder.CompanyOrderId);

                    return (int)oldOrder.Id;

                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Обновить информацию в заказе
        /// </summary>
        /// <param name="orderId">идентификатор заказа</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        public int UpdateOrderInformation(long orderId, long userId)
        {

            try
            {
                using (var fc = GetContext())
                {
                    Order order = fc.Orders.FirstOrDefault(
                        o => o.Id == orderId && o.IsDeleted == false);

                    if (order == null) return -1;
                    List<OrderItem> orderItems =
                        fc.OrderItems.Where(
                            o => o.OrderId == orderId
                                 && o.IsDeleted == false
                        ).ToList();

                    var itemPrice = orderItems.Sum(i => i.TotalPrice);

                    order.ItemsCount = orderItems.Sum(i => i.DishCount);
                    order.TotalPrice = itemPrice;

                    // Если заказ входит в корпоративный, то стоимость доставки должна делиться на всех и будет вычислена в процессе обновления общей суммы корпоративного заказа.
                    // Если заказ не корпоративный и стоимость блюд больше 0 - определяем для него стоимость доставки.
                    if (!order.CompanyOrderId.HasValue && order.TotalPrice > 0.01)
                    {
                        var deliveryPrice = GetListOfDeliveryCosts(order.CafeId, itemPrice).FirstOrDefault();
                        var deliverySumm = deliveryPrice?.DeliveryPrice ?? 0;
                        order.TotalPrice += deliverySumm;

                        var orderInfo = fc.OrderInfo.FirstOrDefault(o => o.Id == order.OrderInfoId && !o.IsDeleted);
                        if (orderInfo != null)
                        {
                            orderInfo.DeliverySumm = deliverySumm;
                            orderInfo.TotalSumm = order.TotalPrice;
                        }
                    }

                    order.LastUpdateByUserId = userId;
                    order.LastUpdDate = DateTime.Now;

                    fc.SaveChanges();

                    return (int)order.Id;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Возвращает список заказов кафе на указанную дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="wantedDateTime">дата</param>
        /// <returns></returns>
        public List<Order> GetCurrentListOfOrdersToCafe(long cafeId, DateTime? wantedDateTime)
        {
            var fc = GetContext();

            var query = fc.Orders.AsNoTracking().Where(o => o.CafeId == cafeId
                      &&
                      (
                          o.State == EnumOrderState.Accepted
                          || o.State == EnumOrderState.Created
                          || o.State == EnumOrderState.Delivered
                          || o.State == EnumOrderState.Delivery
                          || o.State == EnumOrderState.Abort
                      )
                      && o.IsDeleted == false);

            if (wantedDateTime != null)
            {
                var date = wantedDateTime.Value.Date;
                query = query.Where(e => e.DeliverDate >= date &&
                                         e.DeliverDate < date.AddDays(1));
            }

            return query.ToList();
        }

        /// <summary>
        /// Возвращает список заказов кафе за указанный период
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="startDate">начало периода</param>
        /// <param name="endDate">окончание периода</param>
        /// <returns></returns>
        public List<Order> GetCurrentListOfOrdersToCafe(long cafeId, DateTime startDate, DateTime endDate)
        {
            //var fc = GetContext();

            List<Order> orders;

            using (var fc = GetContext())
            {
                orders = fc.Orders.AsNoTracking().Where(o => o.CafeId == cafeId
                          && o.DeliverDate.Value.Date >= startDate
                          && o.DeliverDate.Value.Date <= endDate
                          &&
                          (
                              o.State == EnumOrderState.Accepted
                              || o.State == EnumOrderState.Created
                              || o.State == EnumOrderState.Delivered
                              || o.State == EnumOrderState.Delivery
                              || o.State == EnumOrderState.Abort
                          )
                          && o.IsDeleted == false)
                    .ToList();
            }

            return orders;
        }

        /// <summary>
        /// Меняет статус заказа
        /// </summary>
        /// <param name="orderId">идентификтаор заказа</param>
        /// <param name="orderStatus">идентификатор статуса</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        public bool SetOrderStatus(long orderId, long orderStatus, long userId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    Order order = fc.Orders.FirstOrDefault(
                        o => o.Id == orderId
                        && o.IsDeleted == false
                    );

                    if (order == null) return false;
                    order.State = (EnumOrderState)orderStatus;
                    order.LastUpdateByUserId = userId;
                    order.LastUpdDate = DateTime.Now;

                    fc.OrderStateTransitions.Add(
                        new OrderStateTransition()
                        {
                            OrderId = order.Id,
                            Status = (long)order.State,
                            CreatedBy = userId,
                            CreateDate = DateTime.Now,
                            LastUpdateByUserId = userId,
                            LastUpdDate = DateTime.Now,
                            IsDeleted = false
                        }
                    );
                    fc.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Меняет статус у списка заказов
        /// </summary>
        /// <param name="orderIds">Список идентификаторов заказа</param>
        /// <param name="orderStatus">идентификатор статуса</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        public List<Order> SetOrderStatus(HashSet<long> orderIds, long orderStatus, long userId)
        {
            try
            {
                var fc = GetContext();

                var arrOrder = fc.Orders.Where(o => orderIds.Contains(o.Id) && o.IsDeleted == false).ToList();

                if (arrOrder.Count == 0) return null;

                foreach (var order in arrOrder)
                {
                    if (order.State != EnumOrderState.Abort && order.State != EnumOrderState.Delivered)
                    {
                        order.State = (EnumOrderState)orderStatus;
                        order.LastUpdateByUserId = userId;
                        order.LastUpdDate = DateTime.Now;

                        fc.OrderStateTransitions.Add(
                            new OrderStateTransition()
                            {
                                OrderId = order.Id,
                                Status = (long)order.State,
                                CreatedBy = userId,
                                CreateDate = DateTime.Now,
                                LastUpdateByUserId = userId,
                                LastUpdDate = DateTime.Now,
                                IsDeleted = false
                            }
                        );
                    }
                }

                fc.SaveChanges();
                return arrOrder;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Проверяет есть ли блюдо в заказах за указанный период или дату
        /// </summary>
        /// <param name="dishId">идентификатор блюда</param>
        /// <param name="wantedDate">дата</param>
        /// <param name="beginDate">начало периода</param>
        /// <param name="endDate">окончание периода</param>
        /// <returns></returns>
        public bool CheckExistingDishInOrders(long dishId, DateTime? wantedDate, DateTime? beginDate, DateTime? endDate)
        {
            var fc = GetContext();

            var query = fc.OrderItems.AsNoTracking().Where(oi => oi.IsDeleted == false
                      && oi.DishId == dishId
                      && !oi.Order.BanketId.HasValue
                      &&
                      (
                          oi.Order.State == EnumOrderState.Accepted
                          || oi.Order.State == EnumOrderState.Created
                          || oi.Order.State == EnumOrderState.Delivered
                          || oi.Order.State == EnumOrderState.Delivery
                          || oi.Order.State == EnumOrderState.Abort
                      ));

            if (wantedDate != null)
                query = query.Where(e => e.Order.DeliverDate.Value == wantedDate.Value);
            if (beginDate != null)
                query = query.Where(e => e.Order.DeliverDate.Value >= beginDate.Value &&
                (endDate == null || e.Order.DeliverDate.Value <= endDate.Value));

            return query.FirstOrDefault() != null;
        }

        /// <summary>
        /// Возвращает список заказов пользователя с укзанным статусом на дату
        /// </summary>
        /// <param name="userId">идентификатор пользователя</param>
        /// <param name="orderStatus">идентификатор статуса</param>
        /// <param name="date">дата</param>
        /// <returns></returns>
        public List<Order> GetUserOrdersWithSameStatus(long userId, long orderStatus, DateTime? date)
        {
            var fc = GetContext();

            var query = fc.Orders.AsNoTracking()
                .Where(o => o.UserId == userId
                && o.State == (EnumOrderState)orderStatus
                && o.IsDeleted == false);

            if (date != null)
                query = query.Where(e => e.CreationDate == date);
            return query.ToList();
        }

        /// <summary>
        /// Удалить заказ
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int RemoveOrder(long orderId, long userId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var order = fc.Orders.FirstOrDefault(
                            o => o.Id == orderId && o.IsDeleted == false);

                    if (order == null) return -1;
                    order.IsDeleted = true;
                    order.LastUpdateByUserId = userId;
                    order.LastUpdDate = DateTime.Now;
                    fc.SaveChanges();
                    UpdateCompanyOrderSum(order.CompanyOrderId.Value);
                    return (int)order.Id;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Возвращает список всех заказов в компанейском заказе
        /// </summary>
        /// <param name="companyOrderId">идентификатор компанейского заказа</param>
        /// <param name="orderState"></param>
        /// <returns></returns>
        public List<Order> GetOrdersByCompanyOrderId(long companyOrderId, List<EnumOrderStatus> orderState = null)
        {
            var fc = GetContext();

            IQueryable<Order> query;

            if (orderState == null || orderState.Count == 0)
            {
                query = fc.Orders.AsNoTracking()
                        .Include(c => c.OrderItems)
                        .Include(c => c.OrderInfo)
                        .Include(c => c.CompanyOrder)
                        .Include(c => c.User)
                        .Where(o => o.CompanyOrderId != null
                              && o.CompanyOrderId == companyOrderId
                              &&
                              (
                                  o.State == EnumOrderState.Accepted
                                  || o.State == EnumOrderState.Created
                                  || o.State == EnumOrderState.Delivered
                                  || o.State == EnumOrderState.Delivery
                                  || o.State == EnumOrderState.Abort
                              )
                              && o.IsDeleted == false);
            }
            else//для отчета куратора
            {
                var orderStateIds = orderState.Select(s => (long)s);

                query = fc.Orders.AsNoTracking()
                    .Include(c => c.OrderItems)
                    .Include(c => c.OrderInfo)
                    .Include(c => c.CompanyOrder)
                    .Include(c => c.User)
                    .Where(o => o.CompanyOrderId != null
                              && o.CompanyOrderId == companyOrderId
                              && orderStateIds.Contains((long)o.State)
                              && o.IsDeleted == false);
            }

            var orders = query.ToList();

            return orders;
        }

        /// <summary>
        /// Возвращает список заказов по фильтру
        /// </summary>
        /// <returns></returns>
        public async Task<List<Order>> GetOrdersByFilter(ReportFilter filter)
        {
            var fc = GetContext();
            var query = fc
                .Orders
                .AsNoTracking()
                .Include(c => c.OrderInfo)
                .Include(c => c.User)
                .Include(c => c.Cafe)
                .Include(c => c.Banket)
                .Include(c => c.CompanyOrder)
                .Include(c => c.CompanyOrder.Cafe)
                .Include(c => c.CompanyOrder.Company)
                .AsQueryable();
            // Если в фильтре указано включать позиции или есть фильтр по блюду - включаем позиции:
            if (filter.LoadOrderItems || (!string.IsNullOrWhiteSpace(filter.Search) &&
                filter.SearchType == EnumSearchType.SearchByDish))
                query = query.Include(c => c.OrderItems);
            // Сразу же указываем фильтр по кафе и отфильтровываем удалённые:
            query = query.Where(o => o.CafeId == filter.CafeId && o.IsDeleted == false);
            // Если есть фильтры по дате - накладываем их:
            if (filter.StartDate > DateTime.MinValue)
                query = query.Where(o => o.DeliverDate.Value.Date >= filter.StartDate.Date);
            if (filter.EndDate > DateTime.MinValue)
                query = query.Where(o => o.DeliverDate.Value.Date <= filter.EndDate.Date);
            // Если включен фильтр по состоянию - добавляем его:
            if (filter.AvailableStatusList.Count > 0)
            {
                long[] statuses = filter.AvailableStatusList.Select(s => (long)s).ToArray();
                query = query.Where(o => statuses.Contains((long)o.State));
            }
            // Если включен фильтр по order id - добавляем его:
            if (filter.OrdersIdList.Count > 0)
                query = query.Where(o => filter.OrdersIdList.Contains(o.Id));

            //если просматриваются банкеты или индивидуальные заказы внутри банкетов
            if (filter.BanketOrdersIdList.Count == 0 && filter.OrdersIdList.Count == 0)
                query = query.Where(o => o.BanketId == null);

            // Если есть фильтр по строке поиска - применяем его:
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                string workingSearchString = filter.Search.Trim().ToLower();
                switch (filter.SearchType)
                {
                    // Поиск по имени пользователя:
                    case EnumSearchType.SearchByName:
                        query = query.Where(o =>
                            o.User.FullName.ToLower().Contains(workingSearchString) ||
                            o.User.Email.ToLower().Contains(workingSearchString));
                        break;
                    // Фильтр по названию блюда:
                    case EnumSearchType.SearchByDish:
                        query = query.Where(o => o.OrderItems.Any(
                            i => i.DishName.ToLower().Contains(workingSearchString)));
                        break;
                    // Фильтр по номеру телефона:
                    case EnumSearchType.SearchByPhone:
                        query = query.Where(o => o.PhoneNumber.ToLower().Contains(workingSearchString));
                        break;
                    // Фильтр по id заказа:
                    case EnumSearchType.SearchByOrderNumber:
                        long num;
                        if (long.TryParse(workingSearchString, out num))
                            query = query.Where(o => o.Id == num);
                        else
                            // Некорректный фильтр по id заказа - возвращаем пустой результат:
                            return new List<Order>();
                        break;
                }
            }
            // Сортировка:
            switch (filter.SortType)
            {
                case EnumReportSortType.OrderByDate:
                    query = filter.ResultOrder == EnumReportResultOrder.Ascending
                        ? query.OrderBy(o => o.CreationDate)
                        : query.OrderByDescending(o => o.CreationDate);
                    break;
                case EnumReportSortType.OrderByStatus:
                    query = filter.ResultOrder == EnumReportResultOrder.Ascending
                        ? query.OrderBy(o => o.State)
                        : query.OrderByDescending(o => o.State);
                    break;
                case EnumReportSortType.OrderByPrice:
                    query = filter.ResultOrder == EnumReportResultOrder.Ascending
                        ? query.OrderBy(o => o.TotalPrice)
                        : query.OrderByDescending(o => o.TotalPrice);
                    break;
                case EnumReportSortType.OrderByOrderNumber:
                    query = filter.ResultOrder == EnumReportResultOrder.Ascending
                        ? query.OrderBy(o => o.Id)
                        : query.OrderByDescending(o => o.Id);
                    break;
                case EnumReportSortType.OrderByDeliveryDate:
                    query = filter.ResultOrder == EnumReportResultOrder.Ascending
                            ? query.OrderBy(co => co.DeliverDate)
                            : query.OrderByDescending(co => co.DeliverDate);
                    break;
            }
            // Выбор первых 1000 элементов:
            query = query.Take(1000);
            // Выполняем запрос к БД и возвращаем результат:
            return await query.ToListAsync();
        }

        public static double RoundUpTo2DecPlaces(double v) => Math.Ceiling(v * 100.0) / 100;

        /// <summary>
        /// Обновить сумму компанейского заказа
        /// </summary>
        /// <param name="companyOrderId">идентификатор компанейского заказа</param>
        /// <returns></returns>
        public bool UpdateCompanyOrderSum(long companyOrderId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    // Вычисление и установка новой общей суммы корпоративного заказа
                    CompanyOrder companyOrder = fc.CompanyOrders.FirstOrDefault(
                        o => o.Id == companyOrderId && o.IsDeleted == false
                    );
                    var allClientOrders = fc.Orders
                        .Where(
                            o => o.CompanyOrderId.Value == companyOrderId
                            && o.IsDeleted == false
                            && o.State != EnumOrderState.Abort
                        ).Select(o => o.Id);
                    // Стоимость корпоративного заказа считаем по входящим в клиентские заказы блюдам.
                    // Если считать по стоимости заказов, то будет добавлена стоимость доставки, уже начисленная для клиентских заказов. А стоимость доставки ещё только предстоит вычислить.
                    var shippingByAddress = CalculateShippingCostsForCompanyOrder(companyOrderId).ToDictionary(
                        x => x.Item1,
                        x => new { Count = x.Item2, Cost = x.Item3 }
                    );
                    companyOrder.TotalDeliveryCost = RoundUpTo2DecPlaces(shippingByAddress.Sum(x => x.Value.Cost));
                    companyOrder.TotalPrice = fc.OrderItems
                        .Where(oi => !oi.IsDeleted && allClientOrders.Contains(oi.OrderId))
                        .Select(oi => oi.TotalPrice)
                        .DefaultIfEmpty(0.0).Sum();
                    companyOrder.TotalPrice = RoundUpTo2DecPlaces(companyOrder.TotalPrice.Value);

                    // Вычисление и обновление стоимости доставки для заказов, входящих в корпоративный заказ, а также стоимости самих заказов:
                    var clientOrders = fc.Orders
                        .Include(o => o.OrderInfo)
                        .Include(o => o.OrderItems)
                        .Where(o => !o.IsDeleted && o.CompanyOrderId.Value == companyOrderId && o.State != EnumOrderState.Abort)
                        .ToList();
                    // Обновление клиентских заказов, входящих в корпоративный заказ:
                    foreach (Order order in clientOrders)
                    {
                        var shippingCost = shippingByAddress[order.OrderInfo.OrderAddress];
                        order.OrderInfo.DeliverySumm = RoundUpTo2DecPlaces(shippingCost.Cost / shippingCost.Count);
                        order.TotalPrice = order.OrderItems
                            .Where(oi => !oi.IsDeleted)
                            .Select(oi => oi.TotalPrice)
                            .DefaultIfEmpty(0.0)
                            .Sum() + order.OrderInfo.DeliverySumm;
                        order.OrderInfo.TotalSumm = order.TotalPrice;
                    }

                    fc.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }

        /// <summary>
        /// Возвращает список всех заказов за день
        /// </summary>
        /// <returns></returns>
        public long GetTotalNumberOfOrdersPerDay()
        {
            try
            {
                using (var fc = GetContext())
                {
                    List<Order> totalList = fc.Orders.AsNoTracking().Where(
                        c => c.CreationDate >= DateTime.Today
                        &&
                        (
                            c.State == EnumOrderState.Accepted
                            || c.State == EnumOrderState.Created
                            || c.State == EnumOrderState.Delivered
                            || c.State == EnumOrderState.Delivery
                        )
                        && c.IsDeleted == false
                    ).ToList();

                    long total = totalList.Count;

                    return total;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Возвращает список заказов пользователя за период сделанные в рамках компании
        /// </summary>
        public List<Order> GetCompanyOrderByUserId(long userId, long companyId, DateTime startDate, DateTime endDate, List<EnumOrderStatus> availableStatusList)
        {
            List<Order> orders;
            var fc = GetContext();
            {
                orders = fc.Orders
                    .AsNoTracking()
                    .Include(c => c.OrderInfo)
                    .Include(c => c.Cafe)
                    .Include(c => c.OrderItems)
                    .Include(c => c.CompanyOrder)
                    .Include(c => c.User)
                    .Include(c => c.Banket)
                    .Include(c => c.CompanyOrder.Cafe)
                    .Include(c => c.CompanyOrder.Company)
                    .Where(
                        o => !o.IsDeleted
                        && o.UserId == userId
                        && o.CompanyOrderId != null
                        && o.CompanyOrder.DeliveryDate >= startDate
                        && o.CompanyOrder.DeliveryDate <= endDate
                        && o.CompanyOrder.CompanyId == companyId
                        && availableStatusList.Contains((EnumOrderStatus)o.State)
                        )
                    .ToList();
            }

            return orders;
        }

        /// <summary>
        /// Добавляет комментарий менеджера к заказу
        /// </summary>
        /// <param name="comment">Комментарий</param>
        /// <param name="id">Id заказа</param>
        public bool? AddManagerCommentToTheOrder(string comment, long id, User currentUser)
        {
            bool flag;
            try
            {
                var order = GetOrderById(id);
                var trueManager = IsUserManagerOfCafe(currentUser.Id, order.CafeId);
                if (!trueManager)
                    return false;
                if (order.ManagerComment == comment)
                    return true;

                order.ManagerComment = comment;
                ChangeOrder(order);
                return flag = true;
            }
            catch (Exception ex)
            {
                Accessor.Instance.LogInfo("Food.Data/Accessor.Entities.Order.cs",
                    $"Что то пошло не так в методе AddManagerCommentToTheOrder," +
                    $" ошибка - {ex.Message}");
                return flag = false;
            }
        }

        #endregion
    }
}
