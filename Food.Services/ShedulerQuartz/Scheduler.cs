using Food.Services.ShedulerQuartz.Scaffolding;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Spi;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Food.Services.ShedulerQuartz
{
    public interface IFoodScheduler
    {
        Task Start(IServiceProvider sp, string connectionString);

        /// <summary>
        /// Создает задачу которая выполняется по расписанию указанному в формате cron.
        ///
        /// Краткое описание выражений cron:
        /// http://www.quartz-scheduler.org/documentation/quartz-2.3.0/tutorials/crontrigger.html
        /// </summary>
        /// <typeparam name="T">Тип задачи, которую необходимо выполнить</typeparam>
        /// <param name="jobId">Идентификатор задачи</param>
        /// <param name="triggerId">Идентификатор триггера</param>
        /// <param name="cronExpr">Расписание cron</param>
        /// <param name="data">Параметры, которые нужно передать задаче</param>
        Task ScheduleCronJob<T>(JobKey jobId, TriggerKey triggerId, string cronExpr, JobDataMap data)
            where T : IJob;

        /// <summary>
        /// Создает задачу которая начнет выполняться не раньше указанного времени.
        /// При регистрации задачи, в планировщике создается новая задача с единственным
        /// триггером, поэтому <paramref name="jobId"/> и <paramref name="triggerId"/>
        /// должны быть уникальными.
        /// 
        /// Этот метод следует использовать для задач которые должны выполниться не больше
        /// Создает задачу выполняющуюся один раз в запланированное время. В случае
        /// ошибки задача перезапущена не будет. Если требуется перезапуск задачи
        /// в случае неуспешного выполнения, см.
        /// <see cref="ScheduleOneshot{T}(JobKey, TriggerKey, DateTime, JobDataMap, int, TimeSpan)"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jobId">Идентификатор работы</param>
        /// <param name="triggerId">Идентификатор триггера</param>
        /// <param name="when">Дата с которой начнет выполняться задача</param>
        /// <param name="data">Параметры передаваемые задаче</param>
        Task<DateTimeOffset?> ScheduleOneshot<T>(JobKey jobId, TriggerKey triggerId, DateTime when, JobDataMap data)
            where T : IJob;

        /// <summary>
        /// Создает задачу выполняющуюся один раз в запланированное время. Если задача
        /// будет выполнена с ошибкой, то будет произведено <paramref name="maxRetries"/>
        /// попыток повторного выполнения с интервалом <paramref name="retryInterval"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jobId">Идентификатор работы</param>
        /// <param name="triggerId">Идентификатор триггера</param>
        /// <param name="when">Дата с которой начнет выполняться задача</param>
        /// <param name="data">Параметры передаваемые задаче</param>
        /// <param name="maxRetries">Количество повторных попыток в случае неуспешного
        /// выполнения</param>
        /// <param name="retryInterval">Интервал ожидания между попытками</param>
        /// <returns>Возвращает дату когда задача будет запущена</returns>
        Task<DateTimeOffset?> ScheduleOneshot<T>(JobKey jobId, TriggerKey triggerId, DateTime when, JobDataMap data, int maxRetries, TimeSpan retryInterval)
            where T : PersistentOneshotTask;

        /// <summary>
        /// Запускает выполнение задачи.
        /// </summary>
        /// <param name="jobId">Идентификатор задачи</param>
        /// <param name="data">Параметры которые необходимо передать задаче</param>
        Task TriggerJob(JobKey jobId, JobDataMap data);

        /// <summary>
        /// Удаляет задачу и все связанные с ней триггеры.
        /// </summary>
        /// <param name="jobId">Идентификатор задачи</param>
        /// <returns>Возвращает true если задача существовала и была успешно отменена</returns>
        Task<bool> CancelJob(JobKey jobId);

        /// <summary>
        /// Удаляет триггер в задаче.
        /// </summary>
        /// <param name="triggerId">Идентификатор триггера</param>
        /// <returns>Возвращает true если триггер существовал и был успешно удален</returns>
        Task<bool> CancelJob(TriggerKey triggerId);
        Task<bool> IsTriggerExists(TriggerKey triggerId);
        Task<bool> IsJobExists(JobKey jobId);

        /// <summary>
        /// Возвращает список триггеров в состоянии <see cref="TriggerState.Blocked"/> или
        /// <see cref="TriggerState.Error"/>.
        /// </summary>
        Task<IEnumerable<(ITrigger, TriggerState)>> GetMisbehavingTriggers();
    }

    public class Scheduler : IFoodScheduler
    {
        public static IFoodScheduler Instance { get; set; }
        private ISchedulerFactory _schedulerFactory;
        private IScheduler _scheduler;
        private readonly ILogger _logger = Log.ForContext<Scheduler>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        static Scheduler()
        {
            Instance = new Scheduler();
        }

        public async Task ScheduleCronJob<T>(JobKey jobId, TriggerKey triggerId, string cronExpr, JobDataMap data)
            where T : IJob
        {
            var logger = GetTaskLogger<T>(jobId, triggerId);
            logger.ForContext("jobDataMap", data)
                .Information("Adding new cron job {task} ({cronExpr})", typeof(T), cronExpr);

            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);

                var job = await _scheduler.GetJobDetail(jobId).ConfigureAwait(false);
                if (job == null)
                    job = JobBuilder.Create<T>().WithIdentity(jobId).UsingJobData(data).Build();
                else
                    logger.Debug("Cron job {task} already exists. Previous job instance will be replaced.");

                var trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerId)
                    .WithCronSchedule(cronExpr, x => x.WithMisfireHandlingInstructionFireAndProceed())
                    .ForJob(job)
                    .Build();

                await _scheduler
                    .ScheduleJob(job, new List<ITrigger> { trigger }, replace: true)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.Error(e, "Exception when trying to schedule a cron job");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<DateTimeOffset?> ScheduleOneshot<T>(JobKey jobId, TriggerKey triggerId, DateTime when, JobDataMap data, Action<SimpleScheduleBuilder> schedFn)
            where T : IJob
        {
            Debug.Assert(_scheduler != null, "Scheduler must be initialized first");

            var logger = GetTaskLogger<T>(jobId, triggerId);
            logger.ForContext("jobDataMap", data)
                .Information("Adding new oneshot task {task} ({date})", typeof(T), when);

            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);

                if (await _scheduler.CheckExists(jobId).ConfigureAwait(false))
                {
                    logger.Warning("Job {jobId} already exists, bailing out", jobId);

                    try
                    {
                        var triggers = await _scheduler.GetTriggersOfJob(jobId);
                        return triggers?.Select(x => x.GetNextFireTimeUtc()).OrderBy(x => x).FirstOrDefault();
                    }
                    catch
                    {
                        return null;
                    }
                }

                var job = JobBuilder.Create<T>().WithIdentity(jobId).UsingJobData(data).Build();
                var trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerId)
                    .StartAt(when)
                    .WithSimpleSchedule(schedFn)
                    .ForJob(job)
                    .Build();

                await _scheduler
                    .ScheduleJob(job, new List<ITrigger> { trigger }, replace: true)
                    .ConfigureAwait(false);

                return trigger.GetNextFireTimeUtc();
            }
            catch (Exception e)
            {
                logger.Error(e, "Exception when trying to schedule a oneshot task");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public Task<DateTimeOffset?> ScheduleOneshot<T>(JobKey jobId, TriggerKey triggerId, DateTime when, JobDataMap data)
            where T : IJob
        {
            return ScheduleOneshot<T>(jobId, triggerId, when, data, x => x.WithRepeatCount(0));
        }

        public Task<DateTimeOffset?> ScheduleOneshot<T>(JobKey jobId, TriggerKey triggerId, DateTime when, JobDataMap data, int maxRetries, TimeSpan retryInterval)
            where T : PersistentOneshotTask
        {
            return ScheduleOneshot<T>(jobId, triggerId, when, data,
                x => x.WithRepeatCount(maxRetries)
                      .WithInterval(retryInterval)
                      .WithMisfireHandlingInstructionNowWithExistingCount()
            );
        }

        public async Task TriggerJob(JobKey jobId, JobDataMap data)
        {
            await _scheduler.TriggerJob(jobId, data);
        }

        public async Task<bool> CancelJob(JobKey jobId)
        {
            Debug.Assert(_scheduler != null, "Scheduler must be initialized first");
            if (!await _scheduler.CheckExists(jobId).ConfigureAwait(false))
                return false;
            return await _scheduler.DeleteJob(jobId).ConfigureAwait(false);
        }

        public async Task<bool> CancelJob(TriggerKey triggerId)
        {
            Debug.Assert(_scheduler != null, "Scheduler must be initialized first");

            if (!await _scheduler.CheckExists(triggerId).ConfigureAwait(false))
                return false;
            return await _scheduler.UnscheduleJob(triggerId).ConfigureAwait(false);
        }

        public async Task<bool> IsTriggerExists(TriggerKey triggerId)
        {
            Debug.Assert(_scheduler != null, "Scheduler must be initialized first");
            return await _scheduler.CheckExists(triggerId).ConfigureAwait(false);
        }

        public async Task<bool> IsJobExists(JobKey jobId)
        {
            Debug.Assert(_scheduler != null, "Scheduler must be initialized first");
            return await _scheduler.CheckExists(jobId).ConfigureAwait(false);
        }

        /// <summary>
        /// Запускает диспетчер задач.
        /// </summary>
        public async Task Start(IServiceProvider sp, string connectionString)
        {
            if (_scheduler == null)
            {
                var props = new NameValueCollection
                {
                    { "quartz.serializer.type", "json" },
                    { "quartz.jobStore.type" , "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz" },
                    { "quartz.jobStore.useProperties" , "false"},
                    { "quartz.jobStore.dataSource" , "default"},
                    { "quartz.jobStore.tablePrefix" , "QRTZ_"},
                    { "quartz.jobStore.driverDelegateType" , "Quartz.Impl.AdoJobStore.PostgreSQLDelegate, Quartz"},
                    { "quartz.dataSource.default.provider" , "Npgsql" },
                    { "quartz.dataSource.default.connectionString", connectionString }
                };

                _schedulerFactory = new StdSchedulerFactory(props);
                _scheduler = await _schedulerFactory.GetScheduler().ConfigureAwait(false);
                _scheduler.JobFactory = new FoodSchedulerJobFactory(sp);

                await Migration.MigrateJobs(_scheduler).ConfigureAwait(false);

                _logger.Debug("====================== Scheduled jobs ======================");
                await DumpJobsToLog().ConfigureAwait(false);
                _logger.Debug("=================== End of scheduled jobs ==================");
            }

            if (!_scheduler.IsStarted)
            {
                _logger.Information("Starting scheduler ...");

                try
                {
                    await _scheduler.Start().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Unable to start scheduler");
                    throw;
                }
            }
        }

        public async Task<IEnumerable<(ITrigger, TriggerState)>> GetMisbehavingTriggers()
        {
            var badTriggers = new List<(ITrigger, TriggerState)>();

            foreach (var jobKey in await ListSchedulerJobs().ConfigureAwait(false))
            {
                var triggers = await _scheduler.GetTriggersOfJob(jobKey).ConfigureAwait(false);
                if (triggers == null || triggers.Count == 0)
                    continue;

                foreach (var trigger in triggers)
                {
                    var state = await _scheduler.GetTriggerState(trigger.Key).ConfigureAwait(false);
                    if (state == TriggerState.Blocked || state == TriggerState.Error)
                        badTriggers.Add((trigger, state));
                }
            }

            return badTriggers;
        }

        private async Task DumpJobsToLog()
        {
            foreach (var jobKey in await ListSchedulerJobs().ConfigureAwait(false))
            {
                var triggers = await _scheduler.GetTriggersOfJob(jobKey).ConfigureAwait(false);

                _logger.Debug("Job {jobId}: TriggerCount={triggerCount})", jobKey, triggers.Count);
                if (triggers == null || triggers.Count == 0)
                    continue;

                foreach (var trigger in triggers)
                {
                    var state = await _scheduler.GetTriggerState(trigger.Key).ConfigureAwait(false);

                    _logger.Debug(
                        "  Trigger {triggerId}: State={state} PrevFireTime={prevFireTime} NextFireTime={nextFireTime}",
                        trigger.Key, state,
                        trigger.GetPreviousFireTimeUtc()?.ToLocalTime(),
                        trigger.GetNextFireTimeUtc()?.ToLocalTime());
                }
            }
        }

        private async Task<IEnumerable<JobKey>> ListSchedulerJobs()
        {
            var jobIds = new List<JobKey>();
            var jobGroups = await _scheduler.GetJobGroupNames().ConfigureAwait(false);

            foreach (var jobGroup in jobGroups)
            {
                var jobKeys = await _scheduler
                    .GetJobKeys(GroupMatcher<JobKey>.GroupEquals(jobGroup))
                    .ConfigureAwait(false);
                jobIds.AddRange(jobKeys);
            }

            return jobIds;
        }

        private ILogger GetTaskLogger<T>(JobKey jobId, TriggerKey triggerId) where T : IJob
        {
            return _logger.ForContext("jobId", jobId)
                .ForContext("triggerId", triggerId)
                .ForContext("task", typeof(T));
        }
    }

    internal class FoodSchedulerJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public FoodSchedulerJobFactory(IServiceProvider sp)
        {
            _serviceProvider = sp ?? throw new ArgumentNullException(nameof(sp));
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return (IJob)ActivatorUtilities.CreateInstance(_serviceProvider, bundle.JobDetail.JobType, new object[] { });
        }

        public void ReturnJob(IJob job)
        {
            (job as IDisposable)?.Dispose();
        }
    }
}
