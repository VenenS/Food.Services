using Food.Services.Controllers;
using ITWebNet.Food.Core.DataContracts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Extensions
{
    public static class DiscountExtensions
    {
        public static DiscountModel GetContract(this Data.Entities.Discount discount)
        {
            return (discount == null)
                ? null
                : new DiscountModel
                {
                    BeginDate = discount.BeginDate,
                    CafeId = discount.CafeId,
                    CompanyId = discount.CompanyId,
                    CreateDate = discount.CreationDate ?? DateTime.Now,
                    CreatorId = discount.CreatorId,
                    EndDate = discount.EndDate,
                    LastUpdateByUserId = discount.LastUpdateByUserId,
                    Id = discount.Id,
                    LastUpdDate = discount.LastUpdDate,
                    UserId = discount.UserId,
                    Value = (int)discount.Value,
                    Cafe = discount.Cafe != null ? discount.Cafe.GetContract() : null
                };
        }

        public static Data.Entities.Discount GetEntity(this DiscountModel discount)
        {
            return (discount == null)
                ? new Data.Entities.Discount()
                : new Data.Entities.Discount
                {
                    Id = discount.Id,
                    CafeId = discount.CafeId,
                    CompanyId = discount.CompanyId,
                    BeginDate = discount.BeginDate,
                    EndDate = discount.EndDate,
                    UserId = (int)discount.UserId,
                    Value = discount.Value
                };
        }
    }
}
