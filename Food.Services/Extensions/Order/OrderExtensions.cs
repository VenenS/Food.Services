using Food.Data.Entities;
using ITWebNet.Food.Core.DataContracts.Common;
using System.Collections.Generic;
using System.Linq;

namespace Food.Services.Extensions.OrderExtensions
{
    public static class OrderExtensions
    {
        //TODO проверить этот класс на нужность новых полей
        public static OrderModel GetContract(this Order order, bool loadOrderItems = true)
        {
            return (order == null)
                ? null
                : new OrderModel
                {
                    Id = order.Id,
                    BanketId = order.BanketId,
                    Comment = order.Comment,
                    CompanyOrderId = order.CompanyOrderId,
                    Create = order.CreationDate,
                    Creator = order.User?.ToDto(),
                    DeliverDate = order.DeliverDate,
                    DeliveryAddressId = order.DeliveryAddressId,
                    itemsCount = order.ItemsCount,
                    OddMoneyComment = order.OddMoneyComment,
                    PhoneNumber = order.PhoneNumber,
                    TotalSum = order.TotalPrice,
                    Status = (long?)order.State,
                    CafeId = order.CafeId,
                    IsDeleted = order.IsDeleted,
                    CreatorId = order.CreatorId ?? order.UserId,
                    OrderInfo = order.OrderInfo?.GetContract(),
                    OrderItems = loadOrderItems && order.OrderItems != null
                        ? order.OrderItems?.Where(c => !c.IsDeleted).Select(c => c.GetContract()).ToList()
                        : new List<OrderItemModel>(),
                    CityId = order.CityId,
                    City = order.City.GetContract()
                };
        }

        public static Order GetEntity(this OrderModel order)
        {
            return (order == null)
                ? new Order()
                : new Order
                {
                    Comment = order.Comment,
                    CompanyOrderId = order.CompanyOrderId,
                    CreationDate = order.Create,
                    PhoneNumber = order.PhoneNumber,
                    DeliveryAddressId = order.DeliveryAddressId,
                    TotalPrice = order.TotalSum ?? 0,
                    ItemsCount = order.itemsCount ?? 0,
                    OddMoneyComment = order.OddMoneyComment,
                    DeliverDate = order.DeliverDate,
                    CafeId = order.CafeId,
                    BanketId = order.BanketId,
                    OrderInfo = order.OrderInfo?.GetEntity(),
                    OrderItems = order.OrderItems?.Select(c => c.GetEntity()).ToList(),
                    CityId = order.CityId
                };
        }
    }
}
