using Food.Data.Entities;
using Food.Data.Enums;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using ITWebNet.FoodService.Food.DbAccessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Food.Services.Extensions
{
    public static class CafeExtensions
    {
        public static CafeModel GetContract(this Cafe cafe)
        {
            return (cafe == null)
                ? null
                : new CafeModel
                {
                    Id = cafe.Id,
                    Name = cafe.CafeName,
                    FullName = cafe.CafeFullName,
                    Description = cafe.Description,
                    ShortDescription = cafe.CafeShortDescription,
                    Uuid = cafe.Uuid,
                    CafeRatingCount = cafe.CafeRatingCount,
                    CafeRatingSumm = cafe.CafeRatingSumm,
                    CleanUrlName = cafe.CleanUrlName,
                    AverageDeliveryTime = cafe.AverageDelivertyTime,
                    DeliveryPriceRub = cafe.DeliveryPriceRub,
                    MinimumSumRub = cafe.MinimumSumRub,
                    WorkingHours = cafe.WorkingHours.Select(
                        x => x.Select(y => new CafeBusinessHoursItemModel
                        {
                            OpeningTime = y.OpeningTime,
                            ClosingTime = y.ClosingTime
                        }
                        ).ToList()
                    ).ToList(),
                    WorkingTimeFrom = cafe.WorkingTimeFrom.HasValue
                        ? DateTime.Today.Add(cafe.WorkingTimeFrom.Value)
                        : DateTime.Now,
                    WorkingTimeTo =
                        cafe.WorkingTimeTo.HasValue ? DateTime.Now.Date.Add(cafe.WorkingTimeTo.Value) : DateTime.Now,
                    IsRest = cafe.IsRest,
                    //OnlinePaymentSign = cafe.OnlinePaymentSign,
                    CafeSpecializationId = cafe.SpecializationId,
                    CafeType = string.Compare(cafe.CafeAvailableOrdersType, "PERSON_ONLY") == 0
                        ? CafeType.PersonOnly
                        : string.Compare(cafe.CafeAvailableOrdersType, "COMPANY_PERSON") == 0
                        ? CafeType.CompanyPerson
                        : CafeType.CompanyOnly,
                    AllowPaymentByPoints = cafe.AllowPaymentByPoints,
                    SmallImage = cafe.SmallImage,
                    BigImage = cafe.BigImage,
                    Logo = cafe.Logo,
                    WeekMenuIsActive = cafe.WeekMenuIsActive,
                    IsActive = cafe.IsActive,
                    Address = cafe.Address,
                    Phone = cafe.Phone,
                    DeferredOrder = cafe.DeferredOrder,
                    DaylyCorpOrderSum = cafe.DailyCorpOrderSum,
                    OrderAbortTime = cafe.OrderAbortTime,
                    CostOfDelivery = CostOfDeliveryCafe(cafe),
                    CityId = cafe.CityId,
                    City = cafe.City.GetContract(),
                    PaymentMethod = (PaymentTypeEnum)cafe.PaymentMethod,
                    DeliveryComment = cafe.DeliveryComment,
                    Kitchens = new List<KitchenModel>(),
                    MinimumOrderSum = cafe.MinimumSumRub,
                    Regions = cafe.DeliveryRegions
                };
        }

        private static List<CostOfDeliveryModel> CostOfDeliveryCafe(Cafe cafe)
        {
            var context = Accessor.Instance.GetContext();
            var costOfDeliverySelect = context.CostOfDelivery.Where(c => c.CafeId == cafe.Id && !c.IsDeleted).OrderBy(c => c.OrderPriceFrom).ToList();

            List<CostOfDeliveryModel> costOfDeliveryModelsResult = new List<CostOfDeliveryModel>();

            foreach (CostOfDelivery element in costOfDeliverySelect)
            {
                costOfDeliveryModelsResult.Add(
                    new CostOfDeliveryModel
                    {
                        Id = element.Id,
                        CafeId = element.CafeId,
                        OrderPriceFrom = element.OrderPriceFrom,
                        OrderPriceTo = element.OrderPriceTo,
                        DeliveryPrice = element.DeliveryPrice,
                        CreateDate = element.CreateDate,
                        CreatorId = element.CreatorId,
                        LastUpdDate = element.LastUpdDate,
                        LastUpdateByUserId = element.LastUpdateByUserId,
                        ForCompanyOrders = element.ForCompanyOrders
                    });
            }

            return costOfDeliveryModelsResult;
        }


        public static Cafe GetEntity(this CafeModel cafe)
        {
            return (cafe == null)
                ? new Cafe()
                : new Cafe
                {
                    Id = cafe.Id,
                    CafeName = cafe.Name,
                    CafeFullName = cafe.FullName,
                    Description = cafe.Description,
                    CafeShortDescription = cafe.ShortDescription,
                    Uuid = cafe.Uuid,
                    CafeRatingCount = cafe.CafeRatingCount,
                    CafeRatingSumm = cafe.CafeRatingSumm,
                    CleanUrlName = cafe.CleanUrlName,
                    AverageDelivertyTime = cafe.AverageDeliveryTime,
                    DeliveryPriceRub = cafe.DeliveryPriceRub,
                    MinimumSumRub = cafe.MinimumSumRub,
                    WorkingTimeFrom = cafe.WorkingTimeFrom?.TimeOfDay,
                    WorkingTimeTo = cafe.WorkingTimeTo?.TimeOfDay,
                    //OnlinePaymentSign = cafe.OnlinePaymentSign,
                    SpecializationId = cafe.CafeSpecializationId,
                    AllowPaymentByPoints = cafe.AllowPaymentByPoints,
                    SmallImage = cafe.SmallImage,
                    BigImage = cafe.BigImage,
                    Logo = cafe.Logo,
                    CafeAvailableOrdersType = cafe.CafeType == CafeType.CompanyOnly ? "COMPANY_ONLY" : "PERSON_ONLY",
                    WeekMenuIsActive = cafe.WeekMenuIsActive,
                    IsActive = cafe.IsActive,
                    DailyCorpOrderSum = cafe.DaylyCorpOrderSum,
                    OrderAbortTime = cafe.OrderAbortTime,
                    CityId = cafe.CityId,
                    PaymentMethod = (EnumPaymentType)cafe.PaymentMethod
                };
        }

        public static CafeManagerModel GetContract(this CafeManager manager)
        {
            return (manager == null)
                ? null
                : new CafeManagerModel()
                {
                    Id = manager.Id,
                    CafeId = manager.CafeId,
                    User = manager.User?.ToAdminDto(),
                    UserId = manager.UserId
                };
        }
    }
}
