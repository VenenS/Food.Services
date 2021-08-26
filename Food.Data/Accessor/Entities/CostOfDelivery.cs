using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        /// <summary>
        /// Возвращает список стоимостей доставок кафе
        /// </summary>
        /// <param name="cafeId">идентификтаор кафе</param>
        /// <returns></returns>
        public List<CostOfDelivery> GetListOfDeliveryCosts(long cafeId)
        {
            List<CostOfDelivery> deliveryCosts;

            using (var fc = GetContext())
            {
                deliveryCosts = fc.CostOfDelivery.AsNoTracking().Where(c => c.CafeId == cafeId && c.IsDeleted == false).ToList();
            }

            return deliveryCosts;
        }

        /// <summary>
        /// Возвращает список стоимостей доставок на сумму
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="price">сумма</param>
        /// <returns></returns>
        public List<CostOfDelivery> GetListOfDeliveryCosts(long cafeId, double price)
        {
            List<CostOfDelivery> deliveryCosts;

            using (var fc = GetContext())
            {
                deliveryCosts = fc.CostOfDelivery.AsNoTracking().Where(c =>
                        c.CafeId == cafeId
                        && (c.OrderPriceFrom - price) < 1e-3
                        && (c.OrderPriceTo - price) > -1e-3
                        && c.IsDeleted == false).ToList();
            }

            return deliveryCosts;
        }

        /// <summary>
        /// Добавляет стоимость доставки
        /// </summary>
        /// <param name="costOfDelivery">стоимость доставки (сущность)</param>
        /// <returns></returns>
        public long AddNewCostOfDelivery(CostOfDelivery costOfDelivery)
        {
            try
            {
                using (var fc = GetContext())
                {
                    fc.CostOfDelivery.Add(costOfDelivery);

                    fc.SaveChanges();
                }
            }
            catch (Exception)
            {
                return -1;
            }

            return costOfDelivery.Id;
        }

        /// <summary>
        /// Редактировать стоимость доставки
        /// </summary>
        /// <param name="costOfDelivery"></param>
        /// <returns></returns>
        public bool EditCostOfDelivery(CostOfDelivery costOfDelivery)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var oldCostOfDelivery =
                        fc.CostOfDelivery.FirstOrDefault(
                            t => t.Id == costOfDelivery.Id
                                 && t.IsDeleted == false
                            );

                    if (oldCostOfDelivery != null)
                    {
                        oldCostOfDelivery.LastUpdDate =
                            DateTime.Now;
                        oldCostOfDelivery.LastUpdateByUserId =
                            costOfDelivery.LastUpdateByUserId;
                        oldCostOfDelivery.OrderPriceFrom =
                            costOfDelivery.OrderPriceFrom;
                        oldCostOfDelivery.OrderPriceTo =
                            costOfDelivery.OrderPriceTo;
                        oldCostOfDelivery.DeliveryPrice =
                            costOfDelivery.DeliveryPrice;

                        fc.SaveChanges();
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Удаляет стоиомсть доставки
        /// </summary>
        /// <param name="costOfDeliveryId">идентификатор стоимости доставки</param>
        /// <returns></returns>
        public bool RemoveCostOfDelivery(long costOfDeliveryId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var oldCostOfDelivery =
                        fc.CostOfDelivery.FirstOrDefault(
                            t => t.Id == costOfDeliveryId
                                 && t.IsDeleted == false
                            );

                    if (oldCostOfDelivery != null)
                    {
                        oldCostOfDelivery.IsDeleted = true;

                        fc.SaveChanges();
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Возвращает стоимость доставки по идентификатору
        /// </summary>
        /// <param name="id">идентификтаор стоимости доставки</param>
        /// <returns></returns>
        public CostOfDelivery GetCostOfDeliveryById(long id)
        {
            CostOfDelivery deliveryCost;

            using (var fc = GetContext())
            {
                deliveryCost = fc.CostOfDelivery.AsNoTracking().FirstOrDefault(c => c.Id == id && c.IsDeleted == false);
            }

            return deliveryCost;
        }
    }
}
