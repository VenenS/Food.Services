using Food.Services.ShedulerQuartz;
using ITWebNet.FoodService.Food.DbAccessor;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.ShedulerQuartz.Tasks
{
    public class RunDeleteFromDishInMenuTask : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using (var c = Accessor.Instance.GetContext())
                {
                    var itemsForDeleting = c.DishesInMenus.
                        Where(d => d.IsDeleted == true ||
                        d.IsActive == false ||
                        d.Type == "E" ||
                            (d.Type == "S" &&
                            d.OneDate != null &&
                            d.OneDate < DateTime.Today)).ToList();
                    foreach (var item in itemsForDeleting)
                        c.DishesInMenus.Remove(item);
                    //Сохраняем удаление
                    c.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Accessor.Instance.LogError(
                    "Scheduler",
                    $"Исключение при выполнении задачи по очистке блюд в меню "
                        + $"(Exception='{e}' Backtrace='{e.StackTrace}')");
            }
        }
    }
}

namespace Food.Services.ShedulerQuartz
{
    public static class RunDeleteFromDishInMenuSchedulerExtensions
    {
        /// <summary>
        /// Добавляет задачу в планирощик по очистке таблицы dish_in_menu от удаленных
        /// записей.
        /// </summary>
        public static async Task ScheduleDishInMenuEntityCleanupTask(this IFoodScheduler scheduler)
        {
            await scheduler.ScheduleCronJob<Tasks.RunDeleteFromDishInMenuTask>(
                new JobKey("RunDeleteFromDishInMenu_v2"),
                new TriggerKey("TriggerRunDeleteFromDishInMenu"),
                "0 0 0 1,15 * ?", // 1, 15 числа каждого месяца.
                new JobDataMap()
            ).ConfigureAwait(false);
        }
    }
}
