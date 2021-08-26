using Food.Data.Entities;
using ITWebNet.Food.Core.DataContracts.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Extensions.OrderExtensions
{
    public static class OrderInfoExteinsions
    {
        public static OrderInfoModel GetContract(this OrderInfo orderInfo)
        {
            return orderInfo == null
                ? null
                : new OrderInfoModel
                {
                    CreateDate = orderInfo.CreateDate,
                    Id = orderInfo.Id,
                    DeliverySumm = orderInfo.DeliverySumm,
                    DiscountSumm = orderInfo.DeliverySumm,
                    LastUpdate = orderInfo.LastUpdate,
                    LastUpdateBy = orderInfo.LastUpdateBy,
                    OrderAddress = orderInfo.OrderAddress,
                    OrderEmail = orderInfo.OrderEmail,
                    OrderPhone = orderInfo.OrderPhone,
                    PaymentType = orderInfo.PaymentType,
                    TotalSumm = orderInfo.TotalSumm,
                    CityId = orderInfo.CityId,
                    City = orderInfo.City.GetContract()
                };
        }

        public static OrderInfo GetEntity(this OrderInfoModel orderInfo)
        {
            return orderInfo == null
                ? new OrderInfo()
                : new OrderInfo
                {
                    LastUpdateBy = orderInfo.LastUpdateBy,
                    OrderAddress = orderInfo.OrderAddress,
                    OrderEmail = orderInfo.OrderEmail,
                    OrderPhone = orderInfo.OrderPhone,
                    PaymentType = orderInfo.PaymentType,
                    TotalSumm = orderInfo.TotalSumm,
                    CreateDate = orderInfo.CreateDate,
                    DeliverySumm = orderInfo.DeliverySumm,
                    DiscountSumm = orderInfo.DiscountSumm,
                    Id = orderInfo.Id,
                    LastUpdate = orderInfo.LastUpdate,
                    CityId = orderInfo.CityId
                };
        }
    }
}
