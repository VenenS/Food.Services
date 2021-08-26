using System;
using System.Collections.Generic;
using System.Linq;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        /// <summary>
        /// Возвращает список расписаний компанейских заказов для кафе
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <returns></returns>
        public List<CompanyOrderSchedule> GetCompanyOrderScheduleByCafeId(long cafeId)
        {
            var fc = GetContext();

            var query = fc.CompanyOrderSchedules
                .Where(cos => cos.CafeId == cafeId
                && cos.IsActive == true
                && cos.IsDeleted == false);

            return query.ToList();
        }

        /// <summary>
        /// Возвращает список расписаний для компании
        /// </summary>
        /// <param name="companyId">идентификатор компании</param>
        /// <returns></returns>
        public List<CompanyOrderSchedule> GetCompanyOrderScheduleByCompanyId(long companyId)
        {
            var fc = GetContext();

            var query = fc.CompanyOrderSchedules.Where(cos =>
                            cos.CompanyId == companyId
                            && cos.IsActive == true
                            && cos.IsDeleted == false);

            return query.ToList();
        }

        /// <summary>
        /// Возвращает список всех расписаний компаний
        /// </summary>
        /// <returns></returns>
        public List<CompanyOrderSchedule> GetCompanyOrderSchedules()
        {
            var fc = GetContext();

            var query = fc.CompanyOrderSchedules.Where (cos => cos.IsActive == true && cos.IsDeleted == false);

            return query.ToList();
        }

        /// <summary>
        /// Добавляет расписание компании
        /// </summary>
        /// <param name="companyOrderSchedule"></param>
        /// <returns></returns>
        public long AddCompanyOrderSchedule(CompanyOrderSchedule companyOrderSchedule)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var today = DateTime.Now.Date;

                    fc.CompanyOrderSchedules.Add(companyOrderSchedule);
                    fc.SaveChanges();

                    // Создать компанейские заказы для кафе если добавленное расписание
                    // активно сегодня.
                    if ((companyOrderSchedule.BeginDate == null || companyOrderSchedule.BeginDate.Value.Date >= today) &&
                        (companyOrderSchedule.EndDate == null || today <= companyOrderSchedule.EndDate.Value.Date))
                    {
                        CreateCompanyOrders(companyOrderSchedule.CafeId, companyOrderSchedule.CompanyId);
                    }

                    return companyOrderSchedule.Id;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Редактировать расписание компании
        /// </summary>
        /// <param name="companyOrderSchedule">распиание (сущность)</param>
        /// <returns></returns>
        public Tuple<long, IEnumerable<Order>> EditCompanyOrderSchedule(CompanyOrderSchedule companyOrderSchedule)
        {
            try
            {
                using (var fc = GetContext())
                {
                    CompanyOrderSchedule schedule =
                        fc.CompanyOrderSchedules.FirstOrDefault(
                            s => s.Id == companyOrderSchedule.Id
                            && s.IsActive == true
                            && s.IsDeleted == false
                     );

                    if (schedule != null)
                    {
                        var regenOrders = false;
                        var today = DateTime.Now.Date;
                        bool isEditedScheduleActive =
                            (schedule.BeginDate == null || schedule.BeginDate.Value.Date <= today) &&
                            (schedule.EndDate == null || today <= schedule.EndDate.Value.Date);
                        var cancelledOrders = new List<Order> { }.AsEnumerable();

                        // Если длительность действия расписания была укорочена - отменить заказы
                        // на выпавшие даты.
                        if ((companyOrderSchedule.EndDate < schedule.EndDate) ||
                            (companyOrderSchedule.EndDate != null && schedule.EndDate == null))
                        {
                            // FIXME: userId == 0.
                            cancelledOrders = cancelledOrders.Concat(
                                CancelCompanyOrdersBetween(
                                    companyOrderSchedule.EndDate.Value.AddDays(1), schedule.EndDate ?? DateTime.MaxValue,
                                    companyId: schedule.CompanyId,
                                    cafeId: schedule.CafeId,
                                    userId: 0));
                        }

                        if (isEditedScheduleActive &&
                            (schedule.OrderSendTime != companyOrderSchedule.OrderSendTime ||
                             schedule.OrderStartTime != companyOrderSchedule.OrderStartTime ||
                             schedule.OrderStopTime != companyOrderSchedule.OrderStopTime ||
                             schedule.BeginDate != companyOrderSchedule.BeginDate ||
                             schedule.EndDate < companyOrderSchedule.EndDate))
                        {
                            // FIXME: userId == 0.
                            cancelledOrders = cancelledOrders.Concat(
                                CancelCompanyOrdersBetween(
                                    schedule.BeginDate ?? DateTime.MinValue, schedule.EndDate ?? DateTime.MaxValue,
                                    companyId: schedule.CompanyId,
                                    cafeId: schedule.CafeId,
                                    userId: 0));
                            regenOrders = true;
                        }

                        schedule.BeginDate = companyOrderSchedule.BeginDate;
                        schedule.OrderStartTime = companyOrderSchedule.OrderStartTime;
                        schedule.CafeId = companyOrderSchedule.CafeId;
                        schedule.CompanyDeliveryAdress = companyOrderSchedule.CompanyDeliveryAdress;
                        schedule.CompanyId = companyOrderSchedule.CompanyId;
                        schedule.EndDate = companyOrderSchedule.EndDate;
                        schedule.IsActive = companyOrderSchedule.IsActive;
                        schedule.LastUpdateByUserId = companyOrderSchedule.LastUpdateByUserId;
                        schedule.LastUpdDate = DateTime.Now;
                        schedule.OrderSendTime = companyOrderSchedule.OrderSendTime;
                        schedule.OrderStopTime = companyOrderSchedule.OrderStopTime;
                        fc.SaveChanges();

                        if (regenOrders)
                        {
                            CreateCompanyOrders(companyOrderSchedule.CafeId, companyOrderSchedule.CompanyId);
                        }

                        return Tuple.Create(schedule.Id, cancelledOrders);
                    }
                }
            }
            catch(Exception)
            {
            }
            return Tuple.Create(-1L, new List<Order> {}.AsEnumerable());
        }

        /// <summary>
        /// Удалить расписание компании
        /// </summary>
        /// <param name="companyOrderScheduleId">идентификатор компании</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        public Tuple<bool, IEnumerable<Order>> RemoveCompanyOrderSchedule(long companyOrderScheduleId, long userId)
        {
            using (var fc = GetContext())
            {
                CompanyOrderSchedule schedule =
                    fc.CompanyOrderSchedules.FirstOrDefault(
                        s => s.Id == companyOrderScheduleId
                        && s.IsActive == true
                        && s.IsDeleted == false
                );

                if (schedule != null)
                {
                    var today = DateTime.Now.Date;
                    IEnumerable<Order> cancelledOrders = new List<Order> {};

                    schedule.IsDeleted = true;
                    schedule.LastUpdateByUserId = userId;
                    schedule.LastUpdDate = DateTime.Now;
                    fc.SaveChanges();

                    // Отменить компанейские заказы, если удаленное расписание было
                    // активным на сегодняшний день.
                    if ((schedule.BeginDate == null || schedule.BeginDate.Value.Date <= today) &&
                        (schedule.EndDate == null || today <= schedule.EndDate.Value.Date))
                    {
                        cancelledOrders = cancelledOrders.Concat(
                            CancelCompanyOrdersBetween(
                                today, schedule.EndDate ?? DateTime.MaxValue,
                                companyId: schedule.CompanyId,
                                cafeId: schedule.CafeId,
                                userId: userId));
                    }
                    return Tuple.Create(true, cancelledOrders);
                }
            }
            return Tuple.Create(false, new List<Order> { }.AsEnumerable());
        }

        /// <summary>
        /// Возвращает расписание компании по идентификатору
        /// </summary>
        /// <param name="id">идентификатор расписания</param>
        /// <returns></returns>
        public CompanyOrderSchedule GetCompanyOrderScheduleById(long id)
        {
            var fc = GetContext();

            var query = fc.CompanyOrderSchedules
                .FirstOrDefault(cos => cos.IsActive == true && cos.Id == id && cos.IsDeleted == false);

            return query;
        }

        /// <summary>
        /// Получение списка щаблонов заказов компании для указанного временного промежутка.
        /// </summary>
        /// <param name="companyId">Идентификатор компании.</param>
        /// <param name="cafeId">Идентификатор кафе. Либо null.</param>
        /// <param name="startDate">Начальная дата поиска шаблона.</param>
        /// <param name="endDate">Конечная дата поиска шаблона.</param>
        /// <returns>Список шаблонов заказов компании.</returns>
        public List<CompanyOrderSchedule> GetCompanyOrderScheduleByRangeDate(
            long companyId, long? cafeId, DateTime startDate, DateTime endDate)
        {
            var fc = GetContext();
            var companyOrderSchedule = fc.CompanyOrderSchedules
                .Where(c => c.CompanyId == companyId
                        && c.BeginDate >= startDate
                        && c.EndDate <= endDate
                        && c.IsActive == true
                        && c.IsDeleted == false);

            if (cafeId != null)
                companyOrderSchedule = companyOrderSchedule.Where(e => e.CafeId == cafeId);

            return companyOrderSchedule.ToList();
        }

        /// <summary>
        /// Получение всех расписаний компаний на дату
        /// не учитываются удаленные и неактивыне кафе
        /// </summary>
        public List<CompanyOrderSchedule> GetAllCompanyOrderScheduleByDate(DateTime date)
        {
            List<CompanyOrderSchedule> listCo;

            var context = GetContext();
            listCo = context.CompanyOrderSchedules
                .Include("Cafe").Where(
                o => !o.IsDeleted
                && o.IsActive
                && o.BeginDate.Value <= date
                && o.EndDate.Value >= date
                && o.Cafe.IsActive
                && !o.Cafe.IsDeleted
                ).ToList();

            return listCo;
        }
    }
}
