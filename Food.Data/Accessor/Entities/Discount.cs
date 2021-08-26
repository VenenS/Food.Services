using System;
using System.Collections.Generic;
using System.Linq;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region Discount

        /// <summary>
        /// Возвращает значение скидки для пользователя в кафе на указанную дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="date">дата</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <param name="companyId">идентификатор компании</param>
        /// <returns></returns>
        public virtual double GetDiscountValue(
            long cafeId, DateTime date,
            long? userId = null, long? companyId = null
        )
        {
            var fc = GetContext();

            if (companyId == null && userId == null)
                return 0;

            var query = fc.Discounts.AsNoTracking().Where(d =>
                    d.IsDeleted == false
                    && d.CafeId == cafeId
                    && (
                        (
                            d.UserId == userId
                            && d.CompanyId == null
                        )
                        || (
                            d.UserId == null
                            && d.CompanyId == companyId
                        )
                        || (
                            d.UserId == userId
                            && d.CompanyId == companyId
                        )
                    )
                    && d.BeginDate <= date
                    && (
                        d.EndDate == null
                        || d.EndDate > date
                    )
                );

            if (query.Count() > 0)
            {
                return query.Max(d => d.Value);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Возвращает значение скидки для пользователя в кафе на указанную дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="date">дата</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <param name="companyId">идентификатор компании</param>
        /// <returns></returns>
        public virtual double GetDiscountValue(
            List<Discount> lstDiscounts, long cafeId, DateTime date,
            long? userId = null, long? companyId = null
        )
        {
            if (companyId == null && userId == null)
                return 0;

            var query =
                from d in lstDiscounts
                where (
                    d.IsDeleted == false
                    && d.CafeId == cafeId
                    && (
                        (
                            d.UserId == userId
                            && d.CompanyId == null
                        )
                        || (
                            d.UserId == null
                            && d.CompanyId == companyId
                        )
                        || (
                            d.UserId == userId
                            && d.CompanyId == companyId
                        )
                    )
                    && d.BeginDate <= date
                    && (
                        d.EndDate == null
                        || d.EndDate > date
                    )
                )
                select d;

            if (query.Count() > 0)
            {
                return query.Max(d => d.Value);
            }
            else
            {
                return 0;
            }
        }

        public virtual List<Discount> GetDiscounts(long userId, DateTime date)
        {
            var query = GetContext().Discounts.AsNoTracking().Where(
                e => e.IsDeleted == false
                && e.UserId == userId
                && e.BeginDate <= date
                    && (
                        e.EndDate == null
                        || e.EndDate > date
                    )).ToList();

            return query;
        }

        /// <summary>
        /// Возвращает список скидок по идентификтаорам
        /// </summary>
        /// <param name="discountIdList">идентификаторы скидок</param>
        /// <returns></returns>
        public virtual List<Discount> GetDiscounts(long[] discountIdList)
        {
            var fc = GetContext();

            var query = fc.Discounts.AsNoTracking().Include("Cafe").Where(d => discountIdList.Contains(d.Id) && d.IsDeleted == false);

            return query.ToList();
        }

        /// <summary>
        /// Добавляет скидку
        /// </summary>
        /// <param name="discount">скидка (сущность)</param>
        /// <returns></returns>
        public virtual long AddDiscount(Discount discount)
        {

            try
            {
                long result = -1;
                using (var fc = GetContext())
                {
                    var oldDiscount =
                        fc.Discounts.FirstOrDefault(d =>
                            d.CafeId == discount.CafeId && d.UserId == discount.UserId
                        );

                    if (oldDiscount == null)
                    {
                        fc.Discounts.Add(discount);
                        fc.SaveChanges();
                        result = discount.Id;
                    }

                    else
                    {
                        if (oldDiscount.IsDeleted)
                        {
                            oldDiscount.CafeId = discount.CafeId;
                            oldDiscount.CompanyId = discount.CompanyId;
                            oldDiscount.UserId = discount.UserId;
                            oldDiscount.Value = discount.Value;
                            oldDiscount.LastUpdateByUserId = discount.LastUpdateByUserId;
                            oldDiscount.LastUpdDate = DateTime.Now;
                            oldDiscount.BeginDate = discount.BeginDate;
                            oldDiscount.EndDate = discount.EndDate;
                            oldDiscount.IsDeleted = false;

                            fc.SaveChanges();

                            result = oldDiscount.Id;
                        }
                    }
                }

                return result;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Редактирует скидку
        /// </summary>
        /// <param name="discount">скидка (сущность)</param>
        /// <returns></returns>
        public virtual bool EditDiscount(Discount discount)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var oldDiscount = fc.Discounts.FirstOrDefault(
                        d => d.IsDeleted == false && d.Id == discount.Id
                    );

                    var userDiscount =
                        fc.Discounts.FirstOrDefault(
                            d => d.CafeId == discount.CafeId && d.UserId == discount.UserId && d.IsDeleted == false
                        );

                    if (oldDiscount == null)
                        return false;

                    if (userDiscount != null)
                    {
                        if (oldDiscount.CafeId != userDiscount.CafeId)
                            return false;
                    }

                    oldDiscount.CafeId = discount.CafeId;
                    oldDiscount.CompanyId = discount.CompanyId;
                    oldDiscount.UserId = discount.UserId;
                    oldDiscount.Value = discount.Value;
                    oldDiscount.LastUpdateByUserId = discount.LastUpdateByUserId;
                    oldDiscount.LastUpdDate = DateTime.Now;
                    oldDiscount.BeginDate = discount.BeginDate;
                    oldDiscount.EndDate = discount.EndDate;

                    fc.SaveChanges();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Удаляет скидку
        /// </summary>
        /// <param name="discountId">идентификатор скидки</param>
        /// <param name="deleteBy">идентификатор пользователя</param>
        /// <returns></returns>
        public virtual bool RemoveDiscount(long discountId, long deleteBy)
        {
            try
            {
                using (var fc = GetContext())
                {
                    Discount discount =
                        fc.Discounts.FirstOrDefault(
                            d => d.Id == discountId
                            && d.IsDeleted == false
                    );

                    discount.IsDeleted = true;
                    discount.LastUpdateByUserId = deleteBy;
                    discount.LastUpdDate = DateTime.Now;

                    fc.SaveChanges();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


        public virtual List<Discount> GetUserDiscounts(long userId)
        {
            var result = new List<Discount>();
            try
            {
                var fc = GetContext();

                var query = fc.Discounts.AsNoTracking().Include("Cafe").Where(d => d.UserId == userId && d.IsDeleted == false);

                result = query.ToList();

                return result;

            }
            catch (Exception)
            {
                return result;
            }
        }
        #endregion
    }
}
