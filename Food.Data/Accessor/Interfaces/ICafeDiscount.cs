using Food.Data.Entities;
using System;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface ICafeDiscount
    {
        /// <summary>
        /// Добаляет скидку к кафе
        /// </summary>
        /// <param name="cafeDiscount">скидка (сущность))</param>
        /// <returns></returns>
        long AddCafeDiscount(CafeDiscount cafeDiscount);

        /// <summary>
        /// Редактирует скидку кафе
        /// </summary>
        /// <param name="cafeDiscount">скидка (сущность)</param>
        /// <returns></returns>
        bool EditCafeDiscount(CafeDiscount cafeDiscount);

        /// <summary>
        /// Удаляет скидку кафе
        /// </summary>
        /// <param name="cafeDiscountId">идентификатор скидки</param>
        /// <param name="deletedBy">идентификатор пользователя</param>
        /// <returns></returns>
        bool RemoveCafeDiscount(long cafeDiscountId, long deletedBy);

        /// <summary>
        /// Получает значение скидки на выбранную дату и сумму заказа
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="date">дата</param>
        /// <param name="summ">сумма заказа</param>
        /// <returns></returns>
        CafeDiscount GetCafeDiscountValue(
            long cafeId,
            DateTime date,
            double summ
        );

        /// <summary>
        /// Возвращает список скидок. указанных при запросе
        /// </summary>
        /// <param name="discountIdList">идентификаторы скидок</param>
        /// <returns></returns>
        List<CafeDiscount> GetCafeDiscounts(long[] discountIdList);
    }
}
