using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Food.Data;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region Cafe
        /// <summary>
        /// Получаем список всех активных кафе
        /// </summary>
        /// <returns></returns>
        public virtual List<Cafe> GetCafes()
        {
            var fc = GetContext();
            var cafes = fc.Cafes.AsNoTracking().Where(e => e.IsActive == true && e.IsDeleted == false).OrderBy(e => e.CafeName).ToList();

            return cafes;
        }

        /// <summary>
        /// Возвращает кафе
        /// </summary>
        /// <param name="id">идентификатор кафе</param>
        /// <returns></returns>
        public virtual Cafe GetCafeById(long id)
        {
            Cafe cafe;

            if (!_cafeCache.TryGetValue("FullListOfCafes", out var cached))
            {
                var fc = GetContext();
                cafe = fc.Cafes.AsNoTracking().Include(a => a.City).FirstOrDefault(c => c.Id == id && c.IsActive && c.IsDeleted == false);
            }
            else
            {
                cafe = ((List<Cafe>)cached).FirstOrDefault(c => c.Id == id && c.IsDeleted == false);
            }

            return cafe;
        }

        /// <summary>
        /// Возвращает кафе
        /// </summary>
        /// <param name="id">идентификатор кафе</param>
        /// <returns></returns>
        public virtual Cafe GetCafeByIdIgnoreActivity(long id)
        {
            Cafe cafe;
            if (!_cafeCache.TryGetValue("FullListOfCafes", out var cached))
                cafe = GetContext().Cafes.AsNoTracking().FirstOrDefault(c => c.Id == id && !c.IsDeleted);
            else
                cafe = ((List<Cafe>)cached).FirstOrDefault(c => c.Id == id && c.IsDeleted == false);
            return cafe;
        }

        /// <summary>
        /// Возвращает кафе по названию (cleanUrl)
        /// </summary>
        /// <param name="cleanUrl">Название кафе</param>
        /// <returns></returns>
        public virtual Cafe GetCafeByUrl(string cleanUrl)
        {
            Cafe cafe;

            if (!_cafeCache.TryGetValue("FullListOfCafes", out var cached))
            {
                var fc = GetContext();
                cafe = fc.Cafes.AsNoTracking().FirstOrDefault(c => c.CleanUrlName == cleanUrl && c.IsActive && !c.IsDeleted);
            }
            else
            {
                cafe = ((List<Cafe>)cached).FirstOrDefault(c => c.CleanUrlName == cleanUrl && c.IsDeleted == false);
            }

            return cafe;
        }

        public IEnumerable<Tuple<DateTime, Company, List<Cafe>>> GetCafesAvailableToEmployee(long userId, List<DateTime> dates, long? cityId = null)
        {
            var fc = GetContext();
            var result = from uc in fc.UsersInCompanies
                         where (uc.UserId == userId &&
                                //тут был удалена проверка на is_deleted, иначе если is_deleted=true
                                //кнопка блокировалась и нельзя было нажать на нее.
                                //если что то после uc.IsActive надо написать
                                // && !uc.IsDeleted
                                uc.IsActive)
                         join comp in fc.Companies on uc.CompanyId equals comp.Id
                         join co in fc.CompanyOrders on comp.Id equals co.CompanyId
                         join cafe in fc.Cafes on co.CafeId equals cafe.Id
                         where (co.State != (long)EnumOrderStatus.Abort
                                //А это просто раскоментрировать
                                //&& !co.IsDeleted 
                                && co.OpenDate != null
                                && dates.Contains(co.OpenDate.Value.Date)
                                && ((cityId != null && cafe.CityId == cityId) || cityId == null)
                                )
                         select new
                         {
                             Date = co.OpenDate.Value.Date,
                             Company = comp,
                             Cafe = cafe
                         };

            return result
                .ToList()
                .GroupBy(x => new { x.Date, x.Company })
                .Select(x => Tuple.Create(x.Key.Date, x.Key.Company, x.Select(y => y.Cafe).OrderBy(c => c.CafeName).ToList()));
        }

        /// <summary>
        /// Добавляем кафе
        /// </summary>
        /// <param name="cafe">кафе (сущность)</param>
        /// <returns></returns>
        public virtual long AddCafe(Cafe cafe)
        {
            var fc = GetContext();
            fc.Cafes.Add(cafe);
            fc.SaveChanges();
            return cafe.Id;
        }

        /// <summary>
        /// Редактируем кафе
        /// </summary>
        /// <param name="cafe">кафе (сущность))</param>
        /// <returns></returns>
        public virtual Tuple<long, IEnumerable<Order>> EditCafe(Cafe cafe)
        {
            var fc = GetContext();
            var oldCafe =
                fc.Cafes.FirstOrDefault(
                    c => c.Id == cafe.Id
                            //&& c.IsActive == true 
                            && c.IsDeleted == false
                    );

            if (oldCafe != null)
            {
                IEnumerable<Order> cancelledOrders = new List<Order> { };
                var weekMenuOptionChanged = cafe.WeekMenuIsActive != oldCafe.WeekMenuIsActive;

                oldCafe.Address = cafe.Address;
                oldCafe.Phone = cafe.Phone;
                oldCafe.AverageDelivertyTime = cafe.AverageDelivertyTime;
                oldCafe.Description = cafe.Description;
                oldCafe.CafeFullName = cafe.CafeFullName;
                oldCafe.CafeName = cafe.CafeName;
                oldCafe.CafeShortDescription = cafe.CafeShortDescription;
                oldCafe.DeliveryComment = cafe.DeliveryComment;
                oldCafe.DeliveryPriceRub = cafe.DeliveryPriceRub;
                oldCafe.LastUpdateBy = cafe.LastUpdateBy;
                oldCafe.LastUpdateDate = DateTime.Now;
                oldCafe.MinimumSumRub = cafe.MinimumSumRub;
                //oldCafe.OnlinePaymentSign = cafe.OnlinePaymentSign;
                oldCafe.SpecializationId = cafe.SpecializationId;
                oldCafe.WorkingTimeFrom = cafe.WorkingTimeFrom;
                oldCafe.WorkingTimeTo = cafe.WorkingTimeTo;
                oldCafe.WorkingWeekDays = cafe.WorkingWeekDays;
                oldCafe.Logo = cafe.Logo;
                oldCafe.SmallImage = cafe.SmallImage;
                oldCafe.BigImage = cafe.BigImage;
                oldCafe.CleanUrlName = cafe.CleanUrlName;
                oldCafe.WeekMenuIsActive = cafe.WeekMenuIsActive;
                oldCafe.DeferredOrder = cafe.DeferredOrder;
                oldCafe.DailyCorpOrderSum = cafe.DailyCorpOrderSum;
                oldCafe.OrderAbortTime = cafe.OrderAbortTime;
                oldCafe.CityId = cafe.CityId;
                oldCafe.PaymentMethod = cafe.PaymentMethod;
                oldCafe.DeliveryRegions = cafe.DeliveryRegions;

                fc.SaveChanges();

                if (weekMenuOptionChanged)
                {
                    if (!cafe.WeekMenuIsActive)
                    {
                        // Заказы на неделю были деактивированы, удаляем предзаказы от сотрудников
                        // и компанейские заказы, созданные на неделю вперед.
                        // TODO: userId == 0.
                        cancelledOrders = cancelledOrders.Concat(
                            CancelCompanyOrdersBetween(
                                DateTime.Now.Date.AddDays(1), DateTime.Now.Date.AddDays(8),
                                companyId: null,
                                cafeId: cafe.Id,
                                userId: 0));
                    }
                    else
                    {
                        // Заказы на неделю были активированы => создать компанейские заказы на неделю.
                        CreateCompanyOrders(cafeId: cafe.Id, companyId: null);
                    }
                }

                return Tuple.Create(oldCafe.Id, cancelledOrders);
            }
            else
            {
                return Tuple.Create(-1L, new List<Order> { }.AsEnumerable());
            }
        }

        /// <summary>
        /// Проверяет уникальность названия кафе
        /// </summary>
        public virtual bool СheckUniqueName(string name, long cafeId = -1)
        {
            using (var fc = GetContext())
            {
                name = name.Trim().ToLower();
                Cafe existEntity;
                if (cafeId <= 0)
                    existEntity = fc.Cafes.AsNoTracking().FirstOrDefault(o => o.CafeName.Trim().ToLower() == name);
                else
                    existEntity = fc.Cafes.AsNoTracking().FirstOrDefault(o => o.Id != cafeId && o.CafeName.Trim().ToLower() == name);

                return existEntity == null;
            }
        }

        /// <summary>
        /// Удаляем кафе
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        public virtual Tuple<bool, IEnumerable<Order>> RemoveCafe(long cafeId, long userId)
        {
            using (var fc = GetContext())
            {
                Cafe oldCafe =
                    fc.Cafes.FirstOrDefault(
                        c => c.Id == cafeId
                        && c.IsActive == true
                        && c.IsDeleted == false
                    );

                if (oldCafe != null)
                {
                    var cancelledOrders = Accessor.Instance.CancelCompanyOrdersBetween(
                        DateTime.MinValue, DateTime.MaxValue,
                        companyId: null,
                        cafeId: cafeId,
                        userId: userId
                    );

                    oldCafe.IsActive = false;
                    oldCafe.IsDeleted = true;
                    oldCafe.LastUpdateBy = userId;
                    oldCafe.LastUpdateDate = DateTime.Now;

                    fc.SaveChanges();
                    return Tuple.Create(true, cancelledOrders);
                }
            }

            return Tuple.Create(false, new List<Order> { }.AsEnumerable());
        }

        /// <summary>
        /// Возвращает список кафе, которыми пользователь управляет
        /// </summary>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        public virtual List<Cafe> GetManagedCafes(long userId)
        {
            List<Cafe> cafes;
            var fc = GetContext();
            cafes = fc
                    .CafeManagers
                    .AsNoTracking()
                    .Include(c => c.Cafe)
                    .ThenInclude(c => c.City)
                    .Include(c => c.Cafe)
                    .ThenInclude(c => c.KitchenInCafes)
                    .Where(cm => cm.UserId == userId && !cm.Cafe.IsDeleted && cm.IsDeleted == false)
                    .Select(cm => cm.Cafe)
                    .OrderBy(c => c.CafeName)
                    .ToList();

            return cafes;
        }

        /// <summary>
        /// Получает рабочее время кафе.
        /// </summary>
        /// <param name="cafeId">Идентификатор кафе.</param>
        /// <returns>Рабочее время кафе.</returns>
        public virtual BusinessHours GetCafeBusinessHours(long cafeId)
        {
            using (var fc = GetContext())
            {
                var cafe = fc.Cafes.AsNoTracking().FirstOrDefault(e => e.Id == cafeId && e.IsDeleted == false);
                if (cafe != null)
                {
                    try
                    {
                        using (StringReader reader = new StringReader(cafe.BusinessHours))
                        {
                            XmlSerializer serialiser = new XmlSerializer(typeof(BusinessHours));
                            return (BusinessHours)serialiser.Deserialize(reader);
                        }
                    }
                    catch
                    {
                        return new BusinessHours();
                    }
                }

                return new BusinessHours();
            }
        }

        /// <summary>
        /// Задает рабочее время кафе.
        /// </summary>
        /// <param name="businessHours">Рабочее время кафе.</param>
        /// <param name="cafeId">Идентификатор кафе.</param>
        public virtual void SetCafeBusinessHours(BusinessHours businessHours, long cafeId)
        {
            using (var fc = GetContext())
            {
                var cafe = fc.Cafes.FirstOrDefault(e => e.Id == cafeId && e.IsDeleted == false);
                if (cafe == null) return;

                using (StringWriter writer = new StringWriter())
                {
                    XmlSerializer serialiser = new XmlSerializer(typeof(BusinessHours));
                    serialiser.Serialize(writer, businessHours);

                    cafe.BusinessHours = writer.ToString();
                }

                fc.SaveChanges();

                // Удалить корп. заказы если появились новые выходные дни.
                var workingHours = cafe.WorkingHours;
                var today = DateTime.Today;

                for (var i = 0; i < workingHours.Length; i++)
                {
                    var currentDate = today.AddDays(i).Date;

                    if (workingHours[0].Count == 0)
                        Accessor.Instance.CancelCompanyOrdersBetween(currentDate, currentDate, companyId: null, cafeId: cafe.Id, userId: 0);
                }

                // Создать корп. заказы для новых рабочих дней.
                Accessor.Instance.CreateCompanyOrders(cafeId: cafe.Id, companyId: null);
            }
        }

        public virtual List<Cafe> GetCafesByUserId(long userId)
        {
            using (var fc = GetContext())
            {
                var queryCafeIds = (
                    from company in fc.Companies.AsNoTracking()
                    join userCompany in fc.UsersInCompanies.AsNoTracking()
                        on company.Id equals userCompany.CompanyId
                    join companyOrderSchedule in fc.CompanyOrderSchedules.AsNoTracking()
                        on company.Id equals companyOrderSchedule.CompanyId
                    join cafe in fc.Cafes.AsNoTracking()
                        on true equals cafe.IsActive
                    where
                    (
                        cafe.CafeAvailableOrdersType.Equals("COMPANY_PERSON")
                        || cafe.CafeAvailableOrdersType.Equals("PERSON_ONLY")
                        ||
                            (
                                userId == userCompany.UserId
                                && cafe.Id == companyOrderSchedule.CafeId
                                && cafe.CafeAvailableOrdersType.Equals("COMPANY_ONLY")
                            )
                    )
                    select cafe.Id)
                    .Distinct().ToList();
                var queryToGetManagementCafesIds = (
                    from e in fc.CafeManagers.AsNoTracking()
                    where e.UserId == userId
                    select e.CafeId)
                    .Distinct().ToList();
                var allCafeIds = queryCafeIds.Concat(queryToGetManagementCafesIds).Distinct().ToList();
                return fc.Cafes.Where(c => allCafeIds.Contains(c.Id)).OrderBy(c => c.CafeName).ToList();
            }
        }
        #endregion
    }
}
