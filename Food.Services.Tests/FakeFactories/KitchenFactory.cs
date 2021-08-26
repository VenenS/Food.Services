using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    public static class KitchenFactory
    {
        public static Kitchen Create()
        {
            var kitchen = new Kitchen
            {
                Name = Guid.NewGuid().ToString("N"),
            };
            ContextManager.Get().Kitchens.Add(kitchen);
            return kitchen;
        }

        public static List<Kitchen> CreateFew(int count = 3)
        {
            var Dishs = new List<Kitchen>();
            for (int i = 0; i < count; i++)
                Dishs.Add(Create());
            return Dishs;
        }
    }
}
