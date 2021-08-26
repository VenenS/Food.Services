using Quartz;

using Serilog;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.ShedulerQuartz.Tasks
{
    /// <summary>
    /// Задача регулярно записывающая в лог диагностическую информацию.
    /// </summary>
    public class ApplicationDiagnosticsTask : IJob
    {
        private readonly ILogger _logger;
        private readonly IFoodScheduler _scheduler;

        public ApplicationDiagnosticsTask(IFoodScheduler scheduler)
        {
            _logger = Log.ForContext<ApplicationDiagnosticsTask>();
            _scheduler = scheduler;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.Information("Collecting application diagnostics...");
                await DumpSchedulerHealth();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Exception while collecting application diagnostics");
            }
        }

        private async Task DumpSchedulerHealth()
        {
            var badTriggers = (await _scheduler.GetMisbehavingTriggers()).ToList();

            if (badTriggers.Any())
            {
                _logger.Warning($"Found {badTriggers.Count} task triggers that are misbehaving. " +
                    "These triggers will be ignored by scheduler unless manually recovered.");
            }

            foreach (var (trigger, state) in badTriggers)
            {
                _logger.Warning(
                    "Found misbehaving trigger {triggerId}: State={state} PrevFireTime={prevFireTime} NextFireTime={nextFireTime}",
                    trigger.Key, state,
                    trigger.GetPreviousFireTimeUtc()?.ToLocalTime(),
                    trigger.GetNextFireTimeUtc()?.ToLocalTime());
            }
        }
    }
}

namespace Food.Services.ShedulerQuartz
{
    public static class ApplicationDiagnosticsTaskSchedulerExtensions
    {
        public static async Task ScheduleApplicationDiagnosticsTask(this IFoodScheduler scheduler)
        {
            await scheduler.ScheduleCronJob<Tasks.ApplicationDiagnosticsTask>(
                new JobKey("ApplicationDiagnostics"),
                new TriggerKey("TriggerApplicationDiagnostics"),
                "0 0 * * * ?", // Каждый час в hh:00:00
                new JobDataMap()
            ).ConfigureAwait(false);
        }
    }
}