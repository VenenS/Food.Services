using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Quartz;

using Serilog;

namespace Food.Services.ShedulerQuartz.Scaffolding
{
    [PersistJobDataAfterExecution]
    public abstract class PersistentOneshotTask : IJob
    {
        private ILogger _logger;

        public PersistentOneshotTask()
        {
            _logger = Log.ForContext<PersistentOneshotTask>();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var success = false;
            var exception = false;

            try
            {
                success = await TryExecute(context);
            }
            catch (Exception e)
            {
                // Задача должна быть перезапущена по расписанию, которое было указано
                // при регистрации триггера. Поэтому здесь необходимо "проглотить" исключение,
                // в противном случае Quartz немедленно перезапустит эту задачу, что,
                // вероятнее всего, приведет к повторному исключению и исчерпает количество
                // повторов.
                _logger.ForContext("jobDataMap", context.MergedJobDataMap)
                    .Error(e, "Task {task} raised an exception", GetType());
                exception = true;
            }
            finally
            {
                if (success)
                {
                    await context.Scheduler.UnscheduleJob(context.Trigger.Key).ConfigureAwait(false);
                    _logger.ForContext("jobDataMap", context.MergedJobDataMap)
                        .Information("Task {task} executed successfully", GetType());
                }
                else if (!exception)
                {
                    _logger.ForContext("jobDataMap", context.MergedJobDataMap)
                        .Warning("Task {task} returned with an error", GetType());
                }
            }
        }

        protected abstract Task<bool> TryExecute(IJobExecutionContext context);
    }
}