using Food.Services.ShedulerQuartz;
using Food.Services.ShedulerQuartz.Scaffolding;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Tests.Mocks
{
    public class MockScheduler : IFoodScheduler
    {
        public virtual Task Start()
        {
            return Task.CompletedTask;
        }

        public virtual Task ScheduleCronJob<T>(JobKey jobId, TriggerKey triggerId, string cronExpr, JobDataMap data)
            where T : IJob
        {
            return Task.CompletedTask;
        }

        public virtual Task<bool> CancelJob(TriggerKey triggerId)
        {
            return Task.FromResult(true);
        }

        public virtual Task<bool> IsTriggerExists(TriggerKey triggerId)
        {
            return Task.FromResult(false);
        }

        public Task<DateTimeOffset?> ScheduleOneshot<T>(JobKey jobId, TriggerKey triggerId, DateTime when, JobDataMap data) where T : IJob
        {
            return Task.FromResult((DateTimeOffset?)new DateTimeOffset(when));
        }

        public Task<DateTimeOffset?> ScheduleOneshot<T>(JobKey jobId, TriggerKey triggerId, DateTime when, JobDataMap data, int maxRetries, TimeSpan retryInterval) where T : PersistentOneshotTask
        {
            return Task.FromResult((DateTimeOffset?)new DateTimeOffset(when));
        }

        public Task TriggerJob(JobKey jobId, JobDataMap data)
        {
            return Task.CompletedTask;
        }

        public Task<bool> IsJobExists(JobKey jobId)
        {
            return Task.FromResult(false);
        }

        public Task<bool> CancelJob(JobKey jobId)
        {
            return Task.FromResult(false);
        }

        public Task Start(IServiceProvider sp, string connectionString)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<(ITrigger, TriggerState)>> GetMisbehavingTriggers()
        {
            return Task.FromResult(new List<(ITrigger, TriggerState)>().AsEnumerable());
        }
    }
}
