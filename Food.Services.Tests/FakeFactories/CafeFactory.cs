using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;

namespace Food.Services.Tests.FakeFactories
{
    public static class CafeFactory
    {
        public static Cafe Create(User creator = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            var cafe = new Cafe
            {
                CreationDate = DateTime.Now.AddDays(-30),
                CreatedBy = creator.Id,
                IsDeleted = false,
                IsActive = true,
                CafeName = Guid.NewGuid().ToString("N"),
                Address = Guid.NewGuid().ToString("N"),
                CafeFullName = Guid.NewGuid().ToString("N"),
                CleanUrlName = Guid.NewGuid().ToString("N"),
                BusinessHours = @"<?xml version=""1.0"" encoding=""utf-16""?> <businessHours xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">   <departures>   </departures>   <friday>     <item closingTime=""2019-06-10T23:59:00"" openingTime=""2019-06-10T00:00:00"" />   </friday>   <monday>     <item closingTime=""2019-06-10T23:59:00"" openingTime=""2019-06-10T00:00:00"" />   </monday>   <saturday>     <item closingTime=""2019-06-10T23:59:00"" openingTime=""2019-06-10T00:00:00"" />   </saturday>   <sunday>     <item closingTime=""2019-06-10T23:59:00"" openingTime=""2019-06-10T00:00:00"" />   </sunday>   <thursday>     <item closingTime=""2019-06-10T23:59:00"" openingTime=""2019-06-10T00:00:00"" />   </thursday>   <tuesday>     <item closingTime=""2019-06-10T23:59:00"" openingTime=""2019-06-10T00:00:00"" />   </tuesday>   <wednesday>     <item closingTime=""2019-06-10T23:59:00"" openingTime=""2019-06-10T00:00:00"" />   </wednesday> </businessHours>",
        };
            ContextManager.Get().Cafes.Add(cafe);
            return cafe;
        }

        public static List<Cafe> CreateFew(int count = 3, User creator = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            var cafes = new List<Cafe>();
            for (var i = 0; i < count; i++)
                cafes.Add(Create(creator));
            return cafes;
        }

        public static CafeModel CreateModel()
        {
            var cafeModel = new CafeModel
            {
                IsActive = true,
                Name = Guid.NewGuid().ToString("N")
            };
            return cafeModel;
        }
        public static CafeModel CreateModel(Cafe cafe)
        {
            var cafeModel = new CafeModel
            {

                IsActive = true,
                Name = cafe.CafeName,
                Address = cafe.Address,
                FullName = cafe.CafeFullName,
                CleanUrlName = cafe.CleanUrlName,
                Description = Guid.NewGuid().ToString("N"),
                
            };
            return cafeModel;
        }
        public static List<CafeModel> CreateFewModels(List<Cafe> cafeList, int count = 3)
        {
            var cafeModels = new List<CafeModel>();
            for (var i = 0; i < count; i++)
            {
                cafeModels.Add(CreateModel(cafeList[i]));
            }
                
            return cafeModels;
        }

        public static List<CafeModel> CreateFewModels(int count = 3)
        {
            var cafeModels = new List<CafeModel>();
            for (var i = 0; i < count; i++)
                cafeModels.Add(CreateModel());
            return cafeModels;
        }
        public static CafeBusinessHoursModel CreateBusinessHoursModel(Cafe cafe)
        {
            var itemList = new List<CafeBusinessHoursItemModel>
            {
                new CafeBusinessHoursItemModel()
                {
                    OpeningTime = DateTime.Now,
                    ClosingTime = DateTime.Now.AddHours(4)
                }
            };

            var hoursModel = new CafeBusinessHoursModel()
            {
                CafeId = cafe.Id,
                Monday = itemList,
                Tuesday = itemList,
                Wednesday = itemList,
                Thursday = itemList
            };
            return hoursModel;
        }

        public static CafeInfoModel CreateCafeInfo(Cafe cafe)
        {
            var cafeInfoModel = new CafeInfoModel()
            {
                Address = cafe.Address,
                CafeId = cafe.Id,
                Description = cafe.Description,
                Phone=cafe.Phone,
                WeekMenuIsActive = cafe.WeekMenuIsActive,
                PaymentMethod = (PaymentTypeEnum)cafe.PaymentMethod
            };
            return cafeInfoModel;
        }
    }
}
