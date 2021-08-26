using Food.Data.Entities;
using System;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface IDiscount
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
        double GetDiscountValue(
            long cafeId, DateTime date,
            long? userId = null, long? companyId = null
        );

        /// <summary>
        /// Возвращает значение скидки для пользователя в кафе на указанную дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="date">дата</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <param name="companyId">идентификатор компании</param>
        /// <returns></returns>
        double GetDiscountValue(
            List<Discount> lstDiscounts, long cafeId, DateTime date,
            long? userId = null, long? companyId = null
        );

        List<Discount> GetDiscounts(long userId, DateTime date);

        /// <summary>
        /// Возвращает список скидок по идентификтаорам
        /// </summary>
        /// <param name="discountIdList">идентификаторы скидок</param>
        /// <returns></returns>
        List<Discount> GetDiscounts(long[] discountIdList);

        /// <summary>
        /// Добавляет скидку
        /// </summary>
        /// <param name="discount">скидка (сущность)</param>
        /// <returns></returns>
        long AddDiscount(Discount discount);

        /// <summary>
        /// Редактирует скидку
        /// </summary>
        /// <param name="discount">скидка (сущность)</param>
        /// <returns></returns>
        bool EditDiscount(Discount discount);


        /// <summary>
        /// Удаляет скидку
        /// </summary>
        /// <param name="discountId">идентификатор скидки</param>
        /// <param name="deleteBy">идентификатор пользователя</param>
        /// <returns></returns>
        bool RemoveDiscount(long discountId, long deleteBy);


        List<Discount> GetUserDiscounts(long userId);
        #endregion
    }
}
