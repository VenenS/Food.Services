using System;
using System.Collections.Generic;
using Food.Data.Entities;
using Food.Services.Tests.Context;

namespace Food.Services.Tests.FakeFactories
{
    public static class CompanyOrderScheduleFactory
    {
        public static CompanyOrderSchedule Create(User creator = null, Company company = null, Cafe cafe = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            company = company ?? CompanyFactory.Create(creator);
            cafe = cafe ?? CafeFactory.Create(creator);
            var schedule = new CompanyOrderSchedule
            {
                CreateDate = DateTime.Now.AddDays(-30),
                CreatorId = creator.Id,
                IsDeleted = false,
                Company = company,
                CompanyId = company.Id, IsActive = true, 
                BeginDate = DateTime.MinValue, EndDate = DateTime.MaxValue, 
                CafeId = cafe.Id,
                Cafe = cafe, OrderStopTime = new TimeSpan(23, 59,59)
            };
            ContextManager.Get().CompanyOrderSchedules.Add(schedule);
            return schedule;
        }

        public static List<CompanyOrderSchedule> CreateFew(int count = 3, User creator = null, Company company = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            company = company ?? CompanyFactory.Create(creator);
            var schedules = new List<CompanyOrderSchedule>();
            for (var i = 0; i < count; i++)
                schedules.Add(Create(creator, company));
            return schedules;
        }

        public static CompanyOrderSchedule Clone(CompanyOrderSchedule ancestor)
        {
            var schedule = new CompanyOrderSchedule
            {
                CreateDate = ancestor.CreateDate,
                CreatorId = ancestor.CreatorId,
                IsDeleted = ancestor.IsDeleted,
                Company = ancestor.Company,
                CompanyId = ancestor.CompanyId,
                IsActive = ancestor.IsActive,
                BeginDate = ancestor.BeginDate,
                EndDate = ancestor.EndDate,
                CafeId = ancestor.CafeId,
                Cafe = ancestor.Cafe,
                OrderStopTime = ancestor.OrderStopTime,
                Id = ancestor.Id
            };
            return schedule;
        }
    }
}
