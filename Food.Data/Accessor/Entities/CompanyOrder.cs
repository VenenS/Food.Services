using Food.Data;
using Food.Data.Entities;
using ITWebNet.FoodService.Food.Data.Accessor.Extensions;
using ITWebNet.FoodService.Food.Data.Accessor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        


        /// <summary>
        /// Воазвращает список заказов, осуществляемых компанией
        /// </summary>
        /// <param name="companyId">идентификатор компании</param>
        /// <returns></returns>
        public List<CompanyOrder> GetCompanyOrders(long companyId)
        {
            var fc = GetContext();

            var query = fc.CompanyOrders.AsNoTracking()
                .Where(c => c.CompanyId == companyId && c.IsDeleted == false)
                .ToList();

            return query;
        }

        /// <summary>
        /// Воазвращает список заказов, осуществляемых компанией, на указанную дату
        /// </summary>
        /// <param name="companyId">идентификатор компании</param>
        /// <param name="time">дата</param>
        /// <returns></returns>
        public List<CompanyOrder> GetAvailableCompanyOrdersForTime(long companyId, DateTime? time)
        {
            var fc = GetContext();

            time = time ?? DateTime.Now;

            var query = fc.CompanyOrders.AsNoTracking()
                .Where(c => c.CompanyId == companyId
                    && c.AutoCloseDate >= time
                    && c.OpenDate <= time
                    && c.IsDeleted == false)
                .ToList();

            return query;
        }

        /// <summary>
        /// Возвращает список компанейских заказов связанных с данной компанией на
        /// сегодня и на будущее.
        /// </summary>
        /// <param name="companyId">Идентификатор компании</param>
        public List<CompanyOrder> GetCompanyOrdersAvailableToEmployees(long companyId)
        {
            var fc = GetContext();

            var now = DateTime.Now;
            var orders =
                from co in fc.CompanyOrders.AsNoTracking()
                    .Include(x => x.Cafe)
                    .Include(x => x.Company)
                where co.CompanyId == companyId
                      && ((co.OpenDate <= now && now <= co.AutoCloseDate) || DateTime.Today < co.OpenDate.Value.Date)
                      && co.IsDeleted == false
                      && co.State == (long)EnumOrderStatus.Created
                select co;
            return orders.ToList();
        }

        /// <summary>
        /// TODO: проверить работает ли планировщик
        /// Воазвращает список всех компанейских заказов от указанной даты
        /// </summary>
        public List<CompanyOrder> GetAllCompanyOrdersGreaterDate(DateTime? date)
        {
            try
            {
                var fc = GetContext();
                date = date?.Date ?? DateTime.Today;
                return fc.CompanyOrders
                    .Where(o => !o.IsDeleted
                    && o.OpenDate.Value.Date >= date
                    )
                    .ToList();
            }
            catch (Exception)
            {
                return new List<CompanyOrder>();
            }
        }

        /// <summary>
        /// Возвращает заказ, осуществляемый компанией, по идентификатору
        /// </summary>
        /// <param name="companyOrderId">идентификатор компании</param>
        /// <returns></returns>
        public CompanyOrder GetCompanyOrderById(long companyOrderId)
        {
            var fc = GetContext();

            var query = fc.CompanyOrders
                .Include(c => c.Cafe).FirstOrDefault(c => c.Id == companyOrderId);

            return query;
        }

        /// <summary>
        /// Возвращает список индивидуальных заказов из заказа компании
        /// </summary>
        /// <param name="companyOrderId">идентификатор заказа компании</param>
        /// <returns></returns>
        public List<Order> GetUserOrderFromCompanyOrder(long companyOrderId)
        {
            var fc = GetContext();

            var query = fc.Orders.AsNoTracking()
                .Include(o => o.Cafe)
                .Include(o => o.OrderInfo)
                .Include(o => o.Cafe)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Where(o => o.CompanyOrderId == companyOrderId && o.IsDeleted == false).ToList();

            return query;
        }

        /// <summary>
        /// Возвращает список компанейских заказов для кафе на указанный период
        /// </summary>
        /// <param name="companyId">идентификатор компании</param>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="startDate">дата начала</param>
        /// <param name="endDate">дата окончания</param>
        /// <returns></returns>
        public List<CompanyOrder> GetCompanyOrdersByDate(long companyId, long? cafeId, DateTime startDate, DateTime endDate)
        {
            using (var fc = GetContext())
            {
                var companyOrders = fc.CompanyOrders.AsNoTracking()
                    .Where(c => c.CompanyId == companyId
                    && c.CreationDate >= startDate
                    && c.CreationDate <= endDate
                    && c.IsDeleted == false);

                if (cafeId != null)
                    companyOrders = companyOrders.Where(e => e.CafeId == cafeId);

                return companyOrders.ToList();
            }
        }
        /// <summary>
        /// Возвращает список компанейских заказов для кафе за день
        /// </summary>
        /// <param name="date">дата начала</param>
        /// <returns></returns>
        public List<CompanyOrder> GetCompanyOrdersByDate(DateTime date)
        {
            var fc = GetContext();
            var nextDay = date.Date.AddDays(1);
            var companyOrders = fc.CompanyOrders.AsNoTracking()
                .Include(c => c.Cafe)
                .Include(c => c.Company)
                .Where(c =>
                c.OpenDate != null &&
                c.OpenDate >= date.Date &&
                c.OpenDate < nextDay
                && c.IsDeleted == false);

            return companyOrders.ToList();
        }

        /// <summary>
        /// Возвращает список компании, осуществляющие компанейский заказ, для выбранного кафе на указанный период
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="startDate">дата начала</param>
        /// <param name="endDate">дата окончания</param>
        /// <returns></returns>
        public List<Company> GetCompaniesByCompanyOrders(long cafeId, DateTime startDate, DateTime endDate)
        {
            List<Company> companies;

            using (var fc = GetContext())
            {
                companies = fc
                    .CompanyOrders.AsNoTracking()
                    .Include("company")
                    .Where(co => co.CafeId == cafeId
                        && co.DeliveryDate >= startDate
                        && co.DeliveryDate <= endDate
                        && co.IsDeleted == false)
                    .Select(co => co.Company)
                    .GroupBy(c => c.Id)
                    .Select(group => group.FirstOrDefault())
                    .ToList();
            }

            return companies.ToList();
        }

        /// <summary>
        /// Возвращает список компанейских заказов по фильтру
        /// </summary>
        /// <returns></returns>
        public List<CompanyOrder> GetCompanyOrdersByFilter(ReportFilter filter)
        {
            var fc = GetContext();
            // Подготовка запроса к БД, включение компании с адресами, включение кафе:
            var query = fc
                .CompanyOrders.AsNoTracking()
                .Include(c => c.Company)
                .Include(c => c.Company.MainDeliveryAddress)
                .Include(c => c.Company.JuridicalAddress)
                .Include(c => c.Cafe)
                .Include(c => c.Orders)
                // Сразу же исключаем удалённые записи:
                .Where(co => co.IsDeleted == false &&
                // И накладываем фильтр по кафе и компании:
                    (co.CafeId == filter.CafeId || co.CompanyId == filter.CompanyId));
            // Если включены фильтры по датам - тогда накладываем их тоже:
            if (filter.StartDate > DateTime.MinValue)
                query = query.Where(co => co.DeliveryDate >= filter.StartDate);
            if (filter.EndDate > DateTime.MinValue)
                query = query.Where(co => co.DeliveryDate <= filter.EndDate);
            // Если включен фильтр по перечисленным id заказов - применяем его:
            if (filter.CompanyOrdersIdList.Count > 0)
                query = query.Where(co => filter.CompanyOrdersIdList.Contains(co.Id));
            // Если включен фильтр по статусам - применяем его:
            if (filter.AvailableStatusList != null && filter.AvailableStatusList.Count > 0) {
                var cast = filter.AvailableStatusList.Select(x => (long)x);
                query = query.Where(co => cast.Contains(co.State));
            }

            // Сортировка.
            switch (filter.SortType)
            {
                case EnumReportSortType.OrderByDate:
                    query = filter.ResultOrder == EnumReportResultOrder.Ascending
                            ? query.OrderBy(co => co.AutoCloseDate)
                            : query.OrderByDescending(co => co.AutoCloseDate);
                    break;
                case EnumReportSortType.OrderByStatus:
                    query = filter.ResultOrder == EnumReportResultOrder.Ascending
                            ? query.OrderBy(co => co.State)
                            : query.OrderByDescending(co => co.State);
                    break;
                case EnumReportSortType.OrderByPrice:
                    query = filter.ResultOrder == EnumReportResultOrder.Ascending
                            ? query.OrderBy(co => co.TotalPrice)
                            : query.OrderByDescending(co => co.TotalPrice);
                    break;
                case EnumReportSortType.OrderByOrderNumber:
                    query = filter.ResultOrder == EnumReportResultOrder.Ascending
                            ? query.OrderBy(co => co.Id)
                            : query.OrderByDescending(co => co.Id);
                    break;
                case EnumReportSortType.OrderByDeliveryDate:
                    query = filter.ResultOrder == EnumReportResultOrder.Ascending
                            ? query.OrderBy(co => co.DeliveryDate)
                            : query.OrderByDescending(co => co.DeliveryDate);
                    break;
            }
            // Выполняем запрос к БД и получаем объекты:
            //Показываем только те которые имеют Пользовательские заказы
            var companyOrders = query.Where(e => e.Orders.Count > 0).ToList();

            //  query = query.Where(e => e.State != abortState);
            //   query = query.Where(e => e.Orders != null);


            // Обновляем общую сумму заказов для тех заказов, у которых она не проставлена:
            return companyOrders
                .Where(c => c.State != (long)EnumOrderStatus.Abort)
                .Select(co => co.TotalPrice.HasValue ? co :
                new CompanyOrder()
                {
                    CafeId = co.CafeId,
                    CompanyId = co.CompanyId,
                    State = co.State,
                    TotalPrice = co.Orders.Where(o => !o.IsDeleted && o.State != EnumOrderState.Abort).Sum(o => o.TotalPrice),
                    Cafe = co.Cafe,
                    Company = co.Company,
                    AutoCloseDate = co.AutoCloseDate,
                    DeliveryAddress = co.DeliveryAddress,
                    DeliveryDate = co.DeliveryDate,
                    ContactEmail = co.ContactEmail,
                    ContactPhone = co.ContactPhone,
                    OpenDate = co.OpenDate,
                    OrderCreateDate = co.OrderCreateDate,
                    CreationDate = co.CreationDate,
                    CreatorId = co.CreatorId,
                    LastUpdate = co.LastUpdate,
                    LastUpdateByUserId = co.LastUpdateByUserId,
                    Orders = co.Orders
                }).ToList();
        }

        /// <summary>
        /// Добавление нового заказа компании.
        /// </summary>
        /// <param name="companyOrder">Заказ компании.</param>
        /// <returns>Id заказа, если добавление успешно. Иначе - -1.</returns>
        public long AddCompanyOrder(CompanyOrder companyOrder)
        {
            try
            {
                using (var fc = GetContext())
                {
                    if (!Enum.IsDefined(typeof(EnumOrderStatus), companyOrder.State))
                    {
                        companyOrder.State = (long)EnumOrderStatus.Created;
                    }

                    fc.CompanyOrders.Add(companyOrder);
                    fc.SaveChanges();

                    // FIXME: userId == 0.
                    AddCompanyOrderStatusTransition(companyOrder.Id, companyOrder.State, 0);
                    return companyOrder.Id;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public CompanyOrderStateTransition AddCompanyOrderStatusTransition(long orderId, long newStatus, long userId)
        {
            using (var fc = GetContext())
            {
                var now = DateTime.Now;
                var transition = new CompanyOrderStateTransition
                {
                    CompanyOrderId = orderId,
                    Status = newStatus,
                    CreateDate = now,
                    CreatedBy = userId,
                    LastUpdDate = now,
                    LastUpdateByUserId = userId,
                    IsDeleted = false
                };

                fc.CompanyOrderStateTransitions.Add(transition);
                return transition;
            }
        }

        /// <summary>
        /// Меняет статус компанейского заказа
        /// </summary>
        /// <param name="orderId">идентификтаор заказа</param>
        /// <param name="orderStatus">идентификатор статуса</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        public bool SetCompanyOrderStatus(long orderId, long orderStatus, long userId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    CompanyOrder order = fc.CompanyOrders.FirstOrDefault(
                        o => o.Id == orderId && o.IsDeleted == false
                    );

                    if (order == null) return false;
                    order.State = orderStatus;
                    order.LastUpdateByUserId = userId;
                    order.LastUpdate = DateTime.Now;

                    fc.CompanyOrderStateTransitions.Add(
                        new CompanyOrderStateTransition()
                        {
                            CompanyOrderId = order.Id,
                            Status = order.State,
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
        /// Изменение заказа компании.
        /// </summary>
        /// <param name="companyOrder">Заказ компании.</param>
        /// <returns>Id заказа, если изменение успешно, иначе - -1.</returns>
        public long EditCompanyOrder(CompanyOrder companyOrder)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var order = fc.CompanyOrders.FirstOrDefault(
                            s => s.Id == companyOrder.Id && s.IsDeleted == false);

                    if (order == null) return -1;
                    order.CafeId = companyOrder.CafeId;
                    order.CompanyId = companyOrder.CompanyId;
                    order.LastUpdateByUserId =
                        companyOrder.LastUpdateByUserId;
                    order.LastUpdate = companyOrder.LastUpdate;
                    order.OpenDate = companyOrder.OpenDate;
                    order.AutoCloseDate = companyOrder.AutoCloseDate;
                    order.CreationDate = companyOrder.CreationDate;
                    order.DeliveryAddress = companyOrder.DeliveryAddress;
                    order.DeliveryDate = companyOrder.DeliveryDate;
                    order.State = companyOrder.State;
                    order.TotalPrice = companyOrder.TotalPrice;
                    order.CreatorId = companyOrder.CreatorId;
                    order.OrderCreateDate = companyOrder.OrderCreateDate;

                    fc.SaveChanges();
                    return order.Id;
                }
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Удаление заказа компании.
        /// </summary>
        /// <param name="companyOrderId">Идентификатор заказа.</param>
        /// <param name="userId">Идентификатор администратора.</param>
        /// <returns>true - если удаление успешно, иначе - false.</returns>
        public bool RemoveCompanyOrder(long companyOrderId, long userId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    CompanyOrder order = fc.CompanyOrders.FirstOrDefault(
                            s => s.Id == companyOrderId
                            && s.IsDeleted == false);

                    if (order != null)
                    {
                        order.IsDeleted = true;
                        order.LastUpdateByUserId = userId;
                        order.LastUpdate = DateTime.Now;

                        fc.SaveChanges();
                    }
                    else
                        return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Получить корпоративный заказ для пользователя по идентификатору кафе на дату.
        /// </summary>
        public CompanyOrder GetCompanyOrderForUserByCompanyId(long cafeId, long companyId, DateTime? dateTime = null)
        {
            var cafes = new HashSet<long> { cafeId };
            var dates = new HashSet<DateTime> { dateTime ?? DateTime.Now };
            return GetCompanyOrderForUserByCompanyId(cafes, companyId, dates).FirstOrDefault();
        }

        /// <summary>
        /// Получить корпоративные заказы для пользователя по списку кафе на дату.
        /// </summary>
        public List<CompanyOrder> GetCompanyOrderForUserByCompanyId(HashSet<long> cafeIds, long companyId, HashSet<DateTime> dateTimes = null)
        {
            try
            {
                if (dateTimes == null || dateTimes.Count == 0) dateTimes = new HashSet<DateTime>() { DateTime.Now };
                var dates = new HashSet<string>(dateTimes.Select(e => e.Date.ToShortDateString()));
                var today = DateTime.Now;

                var fc = GetContext();

                //Проверяем есть ли корп. заказы у компаний для этих кафе
                var query = fc.CompanyOrders
                    .AsNoTracking()
                    .Include(e => e.Company.Addresses)
                    .ThenInclude(e => e.Address)
                    .Include(e => e.Cafe)
                    .Where(
                    e => !e.IsDeleted
                    && e.State != (long)EnumOrderStatus.Abort
                    && ((e.OpenDate <= today && e.AutoCloseDate >= today) || e.OpenDate.Value.Date > DateTime.Today)
                    && cafeIds.Contains(e.CafeId)
                    && e.CompanyId == companyId
                    )
                    //Берем заказы, которые подходят под передаваемые даты
                    .Where(e => dates.Contains(e.OpenDate.Value.ToShortDateString()))
                    .ToList();

                query.ForEach(x =>
                {
                    x.Company.Addresses = x.Company.Addresses.Where(a => a.IsActive == true).ToList();
                });

                return query;
            }
            catch(Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// Метод для рассчета стоимости доставки по адресам
        /// </summary>
        /// <param name="companyOrderId">Ид корпоративного заказа</param>
        /// <param name="extraCash">(наверное)Стоимость доставки для нового заказа, которого нет в бд</param>
        /// <returns>(адрес, количество заказов, стоимость доставки на этот адрес)</returns>
        public IEnumerable<Tuple<string, int, double>>
        CalculateShippingCostsForCompanyOrder(long companyOrderId, double extraCash = 0)
        {
            extraCash = Math.Max(extraCash, 0);

            using (var fc = GetContext())
            {
                // Группировка заказов внутри корп. заказа по адресу и подсчет стоимости
                // каждой группы заказов.
                var sumsQ =
                    from o in fc.Orders.AsNoTracking()
                    where (o.CompanyOrderId == companyOrderId &&
                           o.State != EnumOrderState.Abort &&
                           !o.IsDeleted)
                    join c in fc.Cafes on o.CafeId equals c.Id
                    group o by o.OrderInfo.OrderAddress into g
                    select new {
                        Address = g.Key,
                        CafeId = g.FirstOrDefault().Cafe.Id,
                        Count = g.Count(),
                        Sum = extraCash + g.Sum(o => o.OrderItems
                            .Where(oi => !oi.IsDeleted)
                            .Select(oi => oi.TotalPrice).Sum())
                    };
                var sums = sumsQ.ToList();
                if (sums.Count == 0)
                    return new List<Tuple<string, int, double>>();

                // Выборка ценовых категорий стоимости доставки.
                var cafeId = sums.First().CafeId;
                var costsQ =
                    from cod in fc.CostOfDelivery.AsNoTracking()
                    where !cod.IsDeleted && cod.CafeId == cafeId && cod.ForCompanyOrders == true
                    orderby cod.OrderPriceFrom
                    select cod;
                var costs = costsQ.ToList();

                return sums.Select(x => Tuple.Create(
                    x.Address,
                    x.Count,
                    costs.FirstOrDefault(c => x.Sum >= c.OrderPriceFrom && x.Sum <= c.OrderPriceTo)?.DeliveryPrice ?? 0
                ));
            }
        }

        public Tuple<string, int, double>
        CalculateShippingCostsToSingleCompanyAddress(string companyAddress, long companyOrderId, double extraCash = 0)
        {
            var addrs = CalculateShippingCostsForCompanyOrder(companyOrderId, extraCash);
            var maybeAddr = addrs.FirstOrDefault(x => x.Item1 == companyAddress);
            if (maybeAddr != null)
                return maybeAddr;

            // Заказов на этот адрес еще нет - посчитать стоимость доставки по extraCash.
            using (var fc = GetContext())
            {
                var shippingCostQ = 
                    from co in fc.CompanyOrders.AsNoTracking()
                    join cod in fc.CostOfDelivery on co.CafeId equals cod.CafeId
                    where (co.Id == companyOrderId &&
                            cod.IsDeleted == false &&
                            cod.OrderPriceFrom <= extraCash && extraCash <= cod.OrderPriceTo
                            && cod.ForCompanyOrders == true)
                    select cod.DeliveryPrice;

                return Tuple.Create(companyAddress, 0, shippingCostQ.FirstOrDefault());
            }
        }


        /// <summary>
        /// Метод для рассчета стоимости доставки по адресам
        /// </summary>
        /// <param name="companyOrderId">Ид корпоративного заказа</param>
        /// <param name="extraCash">(наверное)Стоимость доставки для нового заказа, которого нет в бд</param>
        /// <returns>(адрес, количество заказов, стоимость доставки на этот адрес)</returns>
        public IEnumerable<Tuple<string, int, double>>
        CalculateTotalPriceForCompanyOrderByAddresses(long companyOrderId, double extraCash = 0)
        {
            extraCash = Math.Max(extraCash, 0);

            using (var fc = GetContext())
            {
                // Группировка заказов внутри корп. заказа по адресу и подсчет стоимости
                // каждой группы заказов.
                var sumsQ =
                    from o in fc.Orders.AsNoTracking()
                    where (o.CompanyOrderId == companyOrderId &&
                           o.State != EnumOrderState.Abort &&
                           !o.IsDeleted)
                    join c in fc.Cafes on o.CafeId equals c.Id
                    group o by o.OrderInfo.OrderAddress into g
                    select new
                    {
                        Address = g.Key,
                        Count = g.Count(),
                        Sum = extraCash + g.Sum(o => o.OrderItems
                            .Where(oi => !oi.IsDeleted)
                            .Select(oi => oi.TotalPrice).Sum())
                    };
                var sums = sumsQ.ToList()
                    .Select(e => Tuple.Create(e.Address, e.Count, e.Sum));
                return sums;
            }
        }
        /// <summary>
        /// Создает CompanyOrder на основе CompanyOrderSchedule.
        /// </summary>
        private CompanyOrder ScheduleToCompanyOrder(DateTime date, CompanyOrderSchedule companyOrderShedule)
        {
            return new CompanyOrder()
            {
                AutoCloseDate = date.Date.Add(companyOrderShedule.OrderStopTime),
                CafeId = companyOrderShedule.CafeId,
                CompanyId = companyOrderShedule.CompanyId,
                CreationDate = DateTime.Now,
                CreatorId = 0,
                TotalPrice = 0,
                DeliveryAddress = companyOrderShedule.CompanyDeliveryAdress,
                DeliveryDate = date.Date.Add(companyOrderShedule.OrderSendTime),
                OpenDate = date.Date.Add(companyOrderShedule.OrderStartTime),
                OrderCreateDate = DateTime.Now,
                State = (long)EnumOrderStatus.Created,
            };
        }

        /// <summary>
        ///   Создает компанейские заказы от лица каждой компании (или всех компаний) согласно
        ///   расписаниям куратора для указанного кафе (или всех связанных с компанией кафе).
        /// 
        ///   Если в кафе включены заказы на неделю, будет создано 7 корпоративных заказов где
        ///   на каждый день недели приходится по заказу.
        /// </summary>
        /// 
        /// <param name="cafeId">
        ///   Кафе для которого необходимо создать заказы (если null, все кафе, связанные с
        ///   хотя бы одной компанией).
        /// </param>
        /// <param name="companyId">
        ///   Компания от лица которой необходимо создать заказы (если null, заказы создаются
        ///   для всех компаний согласно их расписаниям).
        /// </param>
        public void CreateCompanyOrders(long? cafeId, long? companyId)
        {
            using (var fc = GetContext())
            {
                var today = DateTime.Today;
                var schedules = (from cos in fc.CompanyOrderSchedules
                                 join c in fc.Cafes on cos.CafeId equals c.Id
                                 where ((cafeId == null || cos.CafeId == cafeId)
                                       && (companyId == null || cos.CompanyId == companyId)
                                       && (cos.BeginDate == null || cos.BeginDate.Value.Date <= today)
                                       && (cos.EndDate == null || cos.EndDate.Value.Date >= today)
                                       && cos.IsActive == true && cos.IsDeleted == false)
                                 orderby cos.CompanyId, cos.BeginDate
                                 select new { Schedule = cos, Cafe = c }).ToList();

                var companyIds = schedules.Select(x => x.Schedule.CompanyId).Distinct();
                var validCompanyOrders = (from co in fc.CompanyOrders
                                          where (co.OpenDate >= today
                                                && companyIds.Contains(co.CompanyId)
                                                && co.State != (long)EnumOrderStatus.Abort
                                                && !co.IsDeleted)
                                          select co).ToList();

                foreach (var pair in schedules)
                {
                    var schedule = pair.Schedule;
                    var cafe = pair.Cafe;
                    var nrDays = cafe.WeekMenuIsActive ? 7 : 1;

                    var workSchedule = CafeExtensions.GetBusinessHours(cafe.BusinessHours);
                    var workingHours = new List<List<BusinessHoursItem>>
                    {
                        workSchedule.Sunday,
                        workSchedule.Monday,
                        workSchedule.Tuesday,
                        workSchedule.Wednesday,
                        workSchedule.Thursday,
                        workSchedule.Friday,
                        workSchedule.Saturday
                    };

                    for (var i = 0; i < nrDays; i++)
                    {
                        var targetDate = DateTime.Today.AddDays(i);
                        if (schedule.EndDate != null && targetDate > schedule.EndDate.Value.Date)
                            break;

                        // Если выходной или праздник - не создавать корп. заказ на эту дату.
                        var todaysHours = workingHours[(int)targetDate.DayOfWeek];
                        var isHoliday =
                            workSchedule.Departures?.FirstOrDefault(
                                x => x.Date.Date == targetDate && (x.IsDayOff || x.Items.Count == 0)
                            ) != null;
                        
                        if (isHoliday || todaysHours == null || todaysHours.Count == 0)
                            continue;

                        var isCompanyOrderExists = validCompanyOrders.Any(
                            x => x.CompanyId == schedule.CompanyId
                                 && x.CafeId == schedule.CafeId
                                 && x.OpenDate?.Date == targetDate
                        );

                        if (!isCompanyOrderExists)
                            AddCompanyOrder(ScheduleToCompanyOrder(targetDate, schedule));
                    }
                }

                fc.SaveChanges();
            }
        }

        /// <summary>
        ///   Отменяет компанейские заказы попадающие в предоставленный временной промежуток. Только заказы со
        ///   статусом `EnumOrderState.Created` будут отменены.
        /// </summary>
        /// <param name="start">Начальная дата</param>
        /// <param name="end">Конечная дата (включительно)</param>
        /// <param name="companyId">
        ///   Идентификатор компании, заказы которой следует отменить (в случае null, заказы все
        ///   компаний будут подходящих по остальным критериям будут отменены).
        /// </param>
        /// <param name="cafeId">
        ///   Заказы предназначенные для данного кафе следует отменить (если null, заказы всем
        ///   кафе будут рассматироваться).
        /// </param>
        /// <param name="userId">Идентификатор пользователя производящего изменения.</param>
        public IEnumerable<Order> CancelCompanyOrdersBetween(DateTime start, DateTime end, long? companyId, long? cafeId, long userId)
        {
            var fc = GetContext();
            var now = DateTime.Now;
            start = start.Date;
            end = end.Date;

            var okToDeleteStateIds = new List<long> { (long)EnumOrderStatus.Created };
            var cancelledChildOrders = new List<Order> { };

            var companyOrders = (from co in fc.CompanyOrders.Include(x => x.Orders).ThenInclude(o => o.User)
                                 where ((co.OpenDate != null && co.OpenDate.Value.Date >= start)
                                       && (co.OpenDate != null && co.OpenDate.Value.Date <= end)
                                       && okToDeleteStateIds.Contains(co.State)
                                       && (cafeId == null || co.CafeId == cafeId)
                                       && (companyId == null || co.CompanyId == companyId)
                                       && co.IsDeleted == false)
                                 select co).ToList();

            foreach (var companyOrder in companyOrders)
            {
                var childOrders = companyOrder.Orders?.Where(b => b.State == EnumOrderState.Created);

                AddCompanyOrderStatusTransition(companyOrder.Id, (long)EnumOrderStatus.Abort, userId);

                companyOrder.State = (long)EnumOrderStatus.Abort;
                companyOrder.LastUpdate = now;
                companyOrder.LastUpdateByUserId = userId;

                if (childOrders == null)
                    continue;

                foreach (var order in childOrders)
                {
                    SetOrderStatus(order.Id, (long)EnumOrderState.Abort, userId);
                    cancelledChildOrders.Add(order);
                }
            }

            fc.SaveChanges();
            return cancelledChildOrders;
        }
    }
}
