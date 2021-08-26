using Food.Data.Entities;
using Food.Services.Controllers;
using Food.Services.Extensions.OrderExtensions;
using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Extensions
{
    public static class BanketExtensions
    {
        public static BanketModel GetContract(this Banket banket)
        {
            return banket == null
                ? null
                : new BanketModel()
                {
                    Id = banket.Id,
                    CompanyId = banket.CompanyId,
                    EventDate = banket.EventDate,
                    MenuId = banket.MenuId,
                    OrderEndDate = banket.OrderEndDate,
                    OrderStartDate = banket.OrderStartDate,
                    CafeId = banket.CafeId,
                    Cafe = banket.Cafe?.GetContract(),
                    Company = banket.Company?.GetContract(),
                    Orders = banket.Orders?.Select(c => c.GetContract()).ToList() ?? new List<OrderModel>(),
                    Menu = banket.Menu?.GetContract(),
                    TotalSum = banket.TotalSum
                };
        }
    }
}
