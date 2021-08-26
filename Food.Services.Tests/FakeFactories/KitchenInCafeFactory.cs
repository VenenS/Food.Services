using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    public static class KitchenInCafeFactory
    {
        public static KitchenInCafe Create(Cafe cafe = null, Kitchen kitchen = null)
        {
            cafe = cafe ?? CafeFactory.Create();
            kitchen = kitchen ?? KitchenFactory.Create();
            var kitchenInCafe = new KitchenInCafe
            {
                Cafe = cafe,
                CafeId = cafe.Id,
                CreateDate = DateTime.MinValue,
                Kitchen = kitchen,
                KitchenId = kitchen.Id
            };
            ContextManager.Get().KitchensInCafes.Add(kitchenInCafe);
            return kitchenInCafe;
        }

        public static List<KitchenInCafe> CreateFew(int count = 3, Cafe cafe = null)
        {
            cafe = cafe ?? CafeFactory.Create();
            var cafes = new List<KitchenInCafe>();
            for (var i = 0; i < count; i++)
                cafes.Add(Create(cafe));
            return cafes;
        }
    }
}