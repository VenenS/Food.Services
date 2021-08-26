using Food.Data.Entities;
using ITWebNet.FoodService.Food.DbAccessor;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Food.Services.ShedulerQuartz.Tasks
{
    /// <summary>
    /// Планировщик по созданию компанейских заказов
    /// </summary>
    public class CreateCompanyOrdersTask : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Accessor.Instance.LogInfo("Scheduler", "Начато выполнение задачи по созданию корп. заказов");
                Accessor.Instance.CreateCompanyOrders(cafeId: null, companyId: null);
                Accessor.Instance.LogInfo("Scheduler", "Задача по созданию корп. заказов успешно завершена");

                Accessor.Instance.LogInfo("Scheduler", "Начато создание задачи по отмене заказов при недоборе суммы корпоративного заказа");
                var companyOrders = Accessor.Instance.GetAllCompanyOrdersGreaterDate(DateTime.Today);
                foreach (var co in companyOrders)
                {
                    var cafe = Accessor.Instance.GetCafeById(co.CafeId);
                    if (cafe != null && cafe.OrderAbortTime.HasValue && co.DeliveryDate.HasValue)
                    {
                        var abortedDate = co.DeliveryDate.Value.Date.Add(co.Cafe.OrderAbortTime.Value);
                        await Scheduler.Instance.AbortOrdersByAddressAt(abortedDate, co.Id).ConfigureAwait(false);
                    }
                }
                Accessor.Instance.LogInfo("Scheduler", "Создание задачи по отмене заказов при недоборе суммы корпоративного заказа завершено");
            }
            catch (Exception e)
            {
                Accessor.Instance.LogError("Scheduler",
                    $"Исключение при выполнении задачи по созданию корп. заказов (Exception='{e}' "
                        + $"Backtrace='{e.StackTrace}')");
            }
        }
    }
}

namespace Food.Services.ShedulerQuartz
{
    public static class CreateCompanyOrdersTaskSchedulerExtensions
    {
        /// <summary>
        /// Добавляет задачу в планирощик по созданию корпоративных заказов.
        /// </summary>
        public static async Task ScheduleCreateCompanyOrdersTask(this IFoodScheduler scheduler)
        {
            await scheduler.ScheduleCronJob<Tasks.CreateCompanyOrdersTask>(
                GetJobKey(),
                GetTriggerKey(),
                "0 1 0 1/1 * ? *", // Каждый день, в полночь (00:01:00)
                new JobDataMap()
            ).ConfigureAwait(false);
        }

        /// <summary>
        /// Перепланирует задачу по созданию корп. заказов (полезна при отладке).
        /// </summary>
        /// <param name="scheduler"></param>
        /// <returns></returns>
        public static async Task ResetCreateCompanyOrdersTask(this IFoodScheduler scheduler)
        {
            await scheduler.CancelJob(GetJobKey());
            await scheduler.ScheduleCreateCompanyOrdersTask();
        }

        /// <summary>
        /// Запускает задачу по созданию корпоративных заказов немедленно (полезна при отладке).
        /// </summary>
        public static async Task RunCreateCompanyOrdersTaskNow(this IFoodScheduler scheduler)
        {
            var jobId = GetJobKey();
            if (!await scheduler.IsJobExists(jobId))
                await scheduler.ScheduleCreateCompanyOrdersTask();
            await scheduler.TriggerJob(jobId, new JobDataMap());
        }

        private static JobKey GetJobKey() => new JobKey("CreateCompanyOrders");
        private static TriggerKey GetTriggerKey() => new TriggerKey("TriggerCreateCompanyOrders");
    }
}
