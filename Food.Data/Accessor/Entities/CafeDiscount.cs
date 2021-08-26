using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region CafeDiscount

        /// <summary>
        /// Добаляет скидку к кафе
        /// </summary>
        /// <param name="cafeDiscount">скидка (сущность))</param>
        /// <returns></returns>
        public long AddCafeDiscount(CafeDiscount cafeDiscount)
        {
            using (var fc = GetContext())
            {
                fc.CafeDiscounts.Add(cafeDiscount);
                fc.SaveChanges();

                return cafeDiscount.Id;
            }
        }

        /// <summary>
        /// Редактирует скидку кафе
        /// </summary>
        /// <param name="cafeDiscount">скидка (сущность)</param>
        /// <returns></returns>
        public bool EditCafeDiscount(CafeDiscount cafeDiscount)
        {
            using (var fc = GetContext())
            {
                var oldCafeDiscount =
                    fc
                        .CafeDiscounts
                        .FirstOrDefault(c => c.Id == cafeDiscount.Id && c.IsDeleted == false);

                if (oldCafeDiscount != null)
                {
                    oldCafeDiscount.CafeId = cafeDiscount.CafeId;
                    oldCafeDiscount.SummFrom = cafeDiscount.SummFrom;
                    oldCafeDiscount.SummTo = cafeDiscount.SummTo;
                    oldCafeDiscount.Summ = cafeDiscount.Summ;
                    oldCafeDiscount.LastUpdateByUserId = cafeDiscount.LastUpdateByUserId;
                    oldCafeDiscount.LastUpdDate = DateTime.Now;
                    oldCafeDiscount.Percent = cafeDiscount.Percent;
                    oldCafeDiscount.CompanyId = cafeDiscount.CompanyId;
                    oldCafeDiscount.IsDeleted = false;

                    fc.SaveChanges();
                }
                else
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Удаляет скидку кафе
        /// </summary>
        /// <param name="cafeDiscountId">идентификатор скидки</param>
        /// <param name="deletedBy">идентификатор пользователя</param>
        /// <returns></returns>
        public bool RemoveCafeDiscount(long cafeDiscountId, long deletedBy)
        {
            using (var fc = GetContext())
            {
                CafeDiscount cafeDiscount =
                    fc.CafeDiscounts.FirstOrDefault(
                        d => d.Id == cafeDiscountId
                        && d.IsDeleted == false
                    );

                if (cafeDiscount != null)
                {

                    cafeDiscount.IsDeleted = true;
                    cafeDiscount.LastUpdateByUserId = deletedBy;
                    cafeDiscount.LastUpdDate = DateTime.Now;

                    fc.SaveChanges();
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Получает значение скидки на выбранную дату и сумму заказа
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="date">дата</param>
        /// <param name="summ">сумма заказа</param>
        /// <returns></returns>
        public CafeDiscount GetCafeDiscountValue(
            long cafeId,
            DateTime date,
            double summ
        )
        {
            CafeDiscount entity;
            using (var fc = GetContext())
            {
                entity = fc.CafeDiscounts.AsNoTracking()
                    .FirstOrDefault(d => d.IsDeleted == false
                        && d.CafeId == cafeId
                        && d.BeginDate <= date
                        && (
                            d.EndDate == null
                            || d.EndDate > date
                        )
                        && d.SummFrom <= summ
                        && (d.SummTo == null || d.SummTo >= summ)
                    );
            }

            return entity;
        }

        /// <summary>
        /// Возвращает список скидок. указанных при запросе
        /// </summary>
        /// <param name="discountIdList">идентификаторы скидок</param>
        /// <returns></returns>
        public List<CafeDiscount> GetCafeDiscounts(long[] discountIdList)
        {
            List<CafeDiscount> cafeDiscounts;

            using (var fc = GetContext())
            {
                var query =
                    fc
                        .CafeDiscounts.AsNoTracking()
                        .Include("cafe")
                        .Where(d => discountIdList.Contains(d.Id) && d.IsDeleted == false
                        );

                cafeDiscounts = query.ToList();
            }

            return cafeDiscounts;
        }
        #endregion
    }
}
