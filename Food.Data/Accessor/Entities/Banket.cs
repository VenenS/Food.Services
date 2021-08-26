using Food.Data;
using Food.Data.Entities;
using ITWebNet.FoodService.Food.Data.Accessor.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        /// <summary>
        /// Добавить новую сущность банкета в БД.
        /// </summary>
        /// <param name="b">Сущность банкета</param>
        /// <returns>ID новой сущности или -1, в случае ошибки.</returns>
        public long AddBanket(Banket b)
        {
            try
            {
                using (var fc = GetContext())
                {
                    fc.Bankets.Add(b);
                    fc.SaveChanges();
                    return b.Id;
                }
            }
            catch (Exception)
            {
                // TODO: handle exception.
            }

            return -1;
        }

        /// <summary>
        /// Редактировать сущействующий банкет.
        /// </summary>
        /// <param name="b">Сущность содержащая изменения</param>
        public bool EditBanket(Banket b)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var existingEntry = fc.Bankets.FirstOrDefault(x => x.Id == b.Id && !x.IsDeleted);
                    if (existingEntry == null)
                        return false;

                    existingEntry.CafeId = b.CafeId;
                    existingEntry.CompanyId = b.CompanyId;
                    existingEntry.EventDate = b.EventDate;
                    existingEntry.MenuId = b.MenuId;
                    existingEntry.OrderEndDate = b.OrderEndDate;
                    existingEntry.OrderStartDate = existingEntry.OrderStartDate;
                    existingEntry.TotalSum = existingEntry.TotalSum;
                    existingEntry.Name = b.Name;
                    existingEntry.Status = b.Status;
                    existingEntry.Url = b.Url;

                    fc.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        /// <summary>
        /// Удалить банкет и все связанные с банкетом заказы.
        /// </summary>
        /// <param name="id">Идентификатор банкета</param>
        /// <returns>true, если банкет был найден и успешно удален. false в противном случае.</returns>
        public bool DeleteBanket(long id)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var banket = fc.Bankets.FirstOrDefault(c => c.Id == id && !c.IsDeleted);

                    if (banket != null)
                    {
                        banket.IsDeleted = true;

                        var orders = fc.Orders
                            .Include(c => c.OrderItems)
                            .Where(c => c.BanketId == banket.Id)
                            .ToList();
                        orders.ForEach(c => c.IsDeleted = true);
                        orders
                            .SelectMany(c => c.OrderItems)
                            .ToList()
                            .ForEach(c => c.IsDeleted = true);

                        fc.SaveChanges();
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // TODO: handle exception.
            }

            return false;
        }

        /// <summary>
        /// Возвращает все сущности банкетов.
        /// </summary>
        public List<Banket> GetAllBankets()
        {
            try
            {
                var fc = GetContext();
                return fc.Bankets
                    .Include(c => c.Company)
                    .Include(c => c.Cafe)
                    .Where(c => !c.IsDeleted)
                    .ToList();
            }
            catch (Exception)
            {
                // TODO: handle exception.
            }
            return new List<Banket> { };
        }

        public Banket GetBanketByEventDate(DateTime date)
        {
            try
            {
                var fc = GetContext();
                var datePart = date.Date;
                return fc.Bankets
                    .AsNoTracking()
                    .FirstOrDefault(c => !c.IsDeleted && c.EventDate.Date == datePart);
            }
            catch (Exception)
            {
                // TODO: handle exception.
            }

            return null;
        }

        /// <summary>
        /// Возвращает список заказов в банкете.
        /// </summary>
        /// <param name="banketId">Идентификатор банкета</param>
        /// <param name="includeStatuses">
        ///   Фильтр по статусу заказа. Если null, фильтрация по статусу не производится
        /// </param>
        public List<Order> ListOrdersInBanket(long banketId, IEnumerable<EnumOrderState> includeStatuses = null)
        {
            return ListOrdersInBankets(new List<long> { banketId }.AsEnumerable(), includeStatuses);
        }

        /// <summary>
        /// Возвращает список заказов связанных с одним или несколькими банкетами.
        /// </summary>
        /// <param name="banketIds">Список ID банкетов заказы которых нужно вернуть</param>
        /// <param name="includeStatuses">Фильтр по статусу заказа</param>
        public List<Order> ListOrdersInBankets(IEnumerable<long> banketIds, IEnumerable<EnumOrderState> includeStatuses = null)
        {
            if (!banketIds.Any())
                return new List<Order> { };
            if (includeStatuses == null)
            {
                includeStatuses = new List<EnumOrderState>() {
                    EnumOrderState.Accepted,
                    EnumOrderState.Created,
                    EnumOrderState.Delivered,
                    EnumOrderState.Delivery,
                    EnumOrderState.Abort
                };
            }

            try
            {
                var fc = GetContext();

                return fc.Orders
                    .AsNoTracking()
                    .Include(c => c.User)
                    .Include(c => c.OrderInfo)
                    .Where(c => !c.IsDeleted && banketIds.Contains(c.BanketId.Value) && includeStatuses.Contains(c.State))
                    .ToList();
            }
            catch (Exception)
            {
                // TODO: handle exception.
            }

            return new List<Order> { };
        }

        /// <summary>
        /// Фильтрует банкеты согласно указанному фильтру.
        /// </summary>
        public List<Banket> GetBanketsByFilter(BanketFilter filter)
        {
            try
            {
                var fc = GetContext();
                var query = fc.Bankets.Include(c => c.Company).Include(c => c.Menu)
                    .Where(c => !c.IsDeleted);

                if (filter.CafeId.HasValue)
                {
                    query = query.Where(c => c.CafeId == filter.CafeId);
                }

                if (filter.BanketIds != null && filter.BanketIds.Count > 0)
                {
                    query = query.Where(c => filter.BanketIds.Contains(c.Id));
                }
                else
                {
                    query = query.Where(c => filter.StartDate.Date <= c.EventDate.Date && c.EventDate.Date <= filter.EndDate.Date);
                }

                switch (filter.SortType)
                {
                    case BanketFilterSortType.OrderByDate:
                        query = query.OrderBy(o => o.EventDate);
                        break;
                    case BanketFilterSortType.OrderByPrice:
                        query = query.OrderBy(o => o.TotalSum);
                        break;
                    case BanketFilterSortType.OrderByOrderNumber:
                        query = query.OrderBy(o => o.Id);
                        break;
                    default:
                        break;
                }

                return query.ToList();
            }
            catch (Exception)
            {
                // TODO: handle exception.
            }

            return new List<Banket> { };
        }

        /// <summary>
        /// Берет первый заказ по банкету и и пользователю
        /// </summary>
        public Order GetOrderInBanketByUserId(long banketId, long userId)
        {
            try
            {
                var fc = GetContext();
                return fc.Orders
                    .AsNoTracking()
                    .Include(c => c.OrderItems)
                    .FirstOrDefault(c => c.BanketId == banketId && !c.IsDeleted && c.UserId == userId && !c.IsDeleted);
            }
            catch (Exception)
            {
                // TODO: handle exceptions.
            }

            return null;
        }

        /// <summary>
        /// Возвращает банкет по идентификатору.
        /// </summary>
        public Banket GetBanketById(long id)
        {
            try
            {
                var fc = GetContext();
                return fc.Bankets
                    .AsNoTracking()
                    .Include(c => c.Company)
                    .Include(c => c.Cafe)
                    .Include(c => c.Orders)
                    .FirstOrDefault(c => !c.IsDeleted && c.Id == id);
            }
            catch (Exception)
            {
                // TODO: handle exception.
            }
            return null;
        }

        /// <summary>
        /// Пересчитывает общую сумму банкета.
        /// </summary>
        public void RecalculateBanketTotalSum(Banket banket0)
        {
            using (var fc = GetContext())
            {
                var banket = fc.Bankets.FirstOrDefault(b => b.Id == banket0.Id);
                var orders = fc.Orders.AsNoTracking().Where(c => c.BanketId == banket.Id && !c.IsDeleted);

                banket.TotalSum = orders.Count() > 0 ? orders.Sum(d => d.TotalPrice) : 0;
                fc.SaveChanges();
            }
        }

        /// <summary>
        /// Пересчитывает общую сумму заказа.
        /// </summary>
        /// <param name="order"></param>
        public void RecalculateOrderTotalSum(Order order)
        {
            using (var fc = GetContext())
            {
                var o = fc.Orders.FirstOrDefault(x => x.Id == order.Id);
                var orderItems = fc.OrderItems.AsNoTracking().Where(c => c.OrderId == order.Id && !c.IsDeleted);
                order.TotalPrice = orderItems.Count() > 0 ? orderItems.Sum(d => d.TotalPrice) : 0;
                fc.SaveChanges();
            }
        }

        /// <summary>
        /// Меняет статус банкета
        /// </summary>
        /// <param name="banketId">идентификатор банкета</param>
        /// <param name="status">устанавливаемый статус</param>
        /// <returns></returns>
        public bool SetBanketStatus(long banketId, EnumBanketStatus status)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var banket = fc.Bankets.FirstOrDefault(
                        o => o.Id == banketId
                        && o.IsDeleted == false
                    );

                    if (banket == null) return false;
                    banket.Status = status;
                    fc.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
