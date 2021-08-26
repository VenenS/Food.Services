using Quartz;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Food.Services.ShedulerQuartz
{
    public static class Migration
    {
        public static async Task MigrateJobs(IScheduler scheduler)
        {
            {
                var jobs = new List<JobKey> {
                    // Следующие 2 задачи были переименованы и перенесены в новое пространство имен
                    // Food.Services.ShedulerQuartz.Tasks.
                    new JobKey("SchedulerCreateCompanyOrders"),
                    new JobKey("RunDeleteFromDishInMenu"),
                    // Задача больше не нужна
                    new JobKey("SendNotificationsToCafe")
                };

                foreach (var jobId in jobs)
                {
                    try
                    {
                        if (await scheduler.CheckExists(jobId).ConfigureAwait(false))
                            await scheduler.DeleteJob(jobId).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        Log.Logger.Warning($"Couldn't delete job: {jobId.Name} ({jobId.Group}): {e}");
                    }
                }
            }
        }
    }
}
