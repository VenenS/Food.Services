using Food.Data.Entities;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface ICostOfDelivery
    {
        /// <summary>
        /// Возвращает список стоимостей доставок кафе
        /// </summary>
        /// <param name="cafeId">идентификтаор кафе</param>
        /// <returns></returns>
        List<CostOfDelivery> GetListOfDeliveryCosts(long cafeId);

        /// <summary>
        /// Возвращает список стоимостей доставок на сумму
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="price">сумма</param>
        /// <returns></returns>
        List<CostOfDelivery> GetListOfDeliveryCosts(long cafeId, double price);

        /// <summary>
        /// Добавляет стоимость доставки
        /// </summary>
        /// <param name="costOfDelivery">стоимость доставки (сущность)</param>
        /// <returns></returns>
        long AddNewCostOfDelivery(CostOfDelivery costOfDelivery);

        /// <summary>
        /// Редактировать стоимость доставки
        /// </summary>
        /// <param name="costOfDelivery"></param>
        /// <returns></returns>
        bool EditCostOfDelivery(CostOfDelivery costOfDelivery);

        /// <summary>
        /// Удаляет стоиомсть доставки
        /// </summary>
        /// <param name="costOfDeliveryId">идентификатор стоимости доставки</param>
        /// <returns></returns>
        bool RemoveCostOfDelivery(long costOfDeliveryId);

        /// <summary>
        /// Возвращает стоимость доставки по идентификатору
        /// </summary>
        /// <param name="id">идентификтаор стоимости доставки</param>
        /// <returns></returns>
        CostOfDelivery GetCostOfDeliveryById(long id);
    }
}
