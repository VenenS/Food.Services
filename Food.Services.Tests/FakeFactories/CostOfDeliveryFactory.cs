using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using ITWebNet.Food.Core.DataContracts.Common;

namespace Food.Services.Tests.FakeFactories
{
    class CostOfDeliveryFactory
    {
        public static CostOfDelivery Create(Cafe cafe, double dPrice = 5, double orderPriceFrom = 0, double priceTo = 100)
        {
            var cost = new CostOfDelivery()
            {
                Cafe = cafe,
                CafeId= cafe.Id,
                DeliveryPrice = dPrice,
                OrderPriceFrom = orderPriceFrom,
                OrderPriceTo = priceTo
            };
            ContextManager.Get().CostOfDelivery.Add(cost);
            return cost;
        }
        public static List<CostOfDelivery> CreateFew(Cafe cafe, int count = 3)
        {
            var listCost = new List<CostOfDelivery>();
            for (var i = 0; i < count; i++)
            {
                var cost = new CostOfDelivery()
                {
                    Cafe = cafe,
                    CafeId = cafe.Id
                    
                };
                ContextManager.Get().CostOfDelivery.Add(cost);
                listCost.Add(cost);
            }
            return listCost;
        }
        public static CostOfDeliveryModel CreateModel(Cafe cafe, double dPrice = 5, double orderPriceFrom = 0, double priceTo = 100)
        {
            var model = new CostOfDeliveryModel()
            {
                CafeId = cafe.Id,
                DeliveryPrice = dPrice,
                OrderPriceFrom = orderPriceFrom,
                OrderPriceTo = priceTo
            };
            return model;
        }


    }
}
