using Food.Data.Entities;
using Food.Services.Controllers;
using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Extensions.CompanyOrderExtensions
{
    public static class CompanyOrderExtensions
    {
        public static CompanyOrderModel GetContract(this CompanyOrder order)
        {
            return order == null
                ? null
                : new CompanyOrderModel
                {
                    CafeId = order.CafeId,
                    CompanyId = order.CompanyId,
                    CreateDate = order.CreationDate,
                    DeliveryAddressId = order.DeliveryAddress,
                    DeliveryDate = order.DeliveryDate,
                    OrderAutoCloseDate = order.AutoCloseDate,
                    Id = order.Id,
                    OrderOpenDate = order.OpenDate,
                    OrderStatus = order.State,
                    ContactEmail = order.ContactEmail,
                    ContactPhone = order.ContactPhone,
                    Company = order.Company?.GetContract(),
                    Cafe = order.Cafe?.GetContract(),
                    UserOrders = new List<OrderModel>(),
                    TotalPrice = order.TotalPrice ?? 0
                };
        }

        public static CompanyOrder GetEntity(this CompanyOrderModel order)
        {
            return order == null
                ? new CompanyOrder()
                : new CompanyOrder
                {
                    CafeId = order.CafeId,
                    CompanyId = order.CompanyId,
                    CreationDate = order.CreateDate,
                    DeliveryAddress = order.DeliveryAddressId,
                    DeliveryDate = order.DeliveryDate,
                    AutoCloseDate = order.OrderAutoCloseDate,
                    Id = order.Id,
                    ContactEmail = order.ContactEmail,
                    ContactPhone = order.ContactPhone,
                    OpenDate = order.OrderOpenDate,
                    State = order.OrderStatus
                };
        }
    }
}
