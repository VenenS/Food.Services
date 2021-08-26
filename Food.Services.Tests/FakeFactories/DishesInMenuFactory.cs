using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;

namespace Food.Services.Tests.FakeFactories
{
    class DishesInMenuFactory
    {
        public static DishInMenu Create(User creator = null, Dish dish = null, string type = "S", 
            DateTime? beginDate = null, DateTime? endDate = null, 
            DateTime? oneDate = null, double? price = 15,
            DateTime? createDate = null)
        {
            var _creator = creator == null ? UserFactory.CreateUser() : creator;
            var _dish = dish == null ? DishFactory.Create() : dish;
            var dishInMenu = new DishInMenu
            {
                IsDeleted = false,
                IsActive = true,
                Dish = _dish,
                Type = type,
                BeginDate = beginDate,
                EndDate = endDate,
                OneDate = oneDate,
                Price = price,
                CreateDate = createDate,
                Creator = _creator
            };
            ContextManager.Get().DishesInMenus.Add(dishInMenu);
            return dishInMenu;
        }
        public static List<DishInMenu> CreateFew(int count = 3, User creator = null, 
            Dish dish = null, string type = "S",
            DateTime? beginDate = null, DateTime? endDate = null,
            DateTime? oneDate = null, double? price = 15,
            DateTime? createDate = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            var dishesInMenu = new List<DishInMenu>();
            for (var i = 0; i < count; i++)
                dishesInMenu.Add(Create(creator));
            return dishesInMenu;
        }
    }
}
