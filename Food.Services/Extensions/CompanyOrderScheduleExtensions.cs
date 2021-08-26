using Food.Data.Entities;
using ITWebNet.Food.Core.DataContracts.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Extensions
{
    public static class CompanyOrderScheduleExtensions
    {
        public static CompanyOrderScheduleModel GetContract(this CompanyOrderSchedule companyOrderSchedule)
        {
            return (companyOrderSchedule == null)
                ? null
                : new CompanyOrderScheduleModel
                {
                    BeginDate = companyOrderSchedule.BeginDate,
                    CafeId = companyOrderSchedule.CafeId,
                    CompanyDeliveryAdress = companyOrderSchedule.CompanyDeliveryAdress,
                    CompanyId = companyOrderSchedule.CompanyId,
                    CreateDate = companyOrderSchedule.CreateDate,
                    CreatorId = companyOrderSchedule.CreatorId,
                    EndDate = companyOrderSchedule.EndDate,
                    Id = companyOrderSchedule.Id,
                    IsActive = companyOrderSchedule.IsActive,
                    LastUpdateByUserId = companyOrderSchedule.LastUpdateByUserId,
                    LastUpdDate = companyOrderSchedule.LastUpdDate,
                    OrderSendTime = DateTime.Now.Add(companyOrderSchedule.OrderSendTime),
                    OrderStartTime = DateTime.Now.Add(companyOrderSchedule.OrderStartTime),
                    OrderStopTime = DateTime.Now.Add(companyOrderSchedule.OrderStopTime)
                };
        }

        public static CompanyOrderSchedule GetEntity(this CompanyOrderScheduleModel companyOrderSchedule)
        {
            return (companyOrderSchedule == null)
                ? new CompanyOrderSchedule()
                : new CompanyOrderSchedule
                {
                    BeginDate = companyOrderSchedule.BeginDate,
                    CafeId = companyOrderSchedule.CafeId,
                    CompanyDeliveryAdress = companyOrderSchedule.CompanyDeliveryAdress,
                    CompanyId = companyOrderSchedule.CompanyId,
                    CreateDate = companyOrderSchedule.CreateDate,
                    CreatorId = companyOrderSchedule.CreatorId,
                    EndDate = companyOrderSchedule.EndDate,
                    Id = companyOrderSchedule.Id,
                    IsActive = companyOrderSchedule.IsActive ?? false,
                    LastUpdateByUserId = companyOrderSchedule.LastUpdateByUserId,
                    LastUpdDate = companyOrderSchedule.LastUpdDate,
                    OrderSendTime = companyOrderSchedule.OrderSendTime.TimeOfDay,
                    OrderStartTime = companyOrderSchedule.OrderStartTime.TimeOfDay,
                    OrderStopTime = companyOrderSchedule.OrderStopTime.TimeOfDay
                };
        }
    }
}
