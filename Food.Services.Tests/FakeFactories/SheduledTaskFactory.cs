using System;
using System.Collections.Generic;
using Food.Data.Entities;

namespace Food.Services.Tests.FakeFactories
{
    public static class SheduledTaskFactory
    {
        public static Food.Data.Entities.ScheduledTask Create(User creator = null, Banket banket = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            banket = banket ?? BanketFactory.Create(creator);
            var task = new Data.Entities.ScheduledTask()
            {
                BanketId = banket.Id,
                CafeId = banket.CafeId,
                CreateDate = DateTime.Now,
                CreatorId = creator.Id,
                IsDeleted = false,
                OrderId = banket.Id,
                IsRepeatable = (long)EnumTaskRepeatable.Once,
                ScheduledExecutionTime = banket.OrderEndDate
            };
            Context.ContextManager.Get().ScheduledTask.Add(task);
            return task;
        }
        public static List<Data.Entities.ScheduledTask> CreateFew(int count = 3, 
            User creator = null, Banket banket = null)
        {
            creator = creator ?? UserFactory.CreateUser();
            banket = banket ?? BanketFactory.Create(creator);
            var scheduledTasks = new List<Data.Entities.ScheduledTask>();
            for (var i = 0; i < count; i++)
                scheduledTasks.Add(Create(creator, banket));
            return scheduledTasks;
        }

    }
}
