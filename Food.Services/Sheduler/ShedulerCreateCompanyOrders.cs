using Food.Data.Entities;
using ITWebNet.FoodService.Food.DbAccessor;
using System;
using System.Threading;

namespace Food.Services
{
    /// <summary>
    /// Планировщик по созданию компанейских заказов
    /// </summary>
    public static class ShedulerCreateCompanyOrders
    {
        private static Timer _scheduleTimerCheckStatusTask;
        
        /// <summary>
        /// Запустить планировщик по созданию компанейских заказов
        /// </summary>
        public static void RunCheck()
        {
            try
            {
                var autoEventTask = new AutoResetEvent(false);

                //Каждые 5 мин таймер запускает CheckStatusTask
                _scheduleTimerCheckStatusTask = new Timer(
                    CheckStatusTask,
                    autoEventTask,
                    60000 * 5,
                    Timeout.Infinite
                    );

                autoEventTask.WaitOne();
                _scheduleTimerCheckStatusTask.Dispose();
                RunCheck();
            }
            catch(Exception e)
            {
                _scheduleTimerCheckStatusTask.Dispose();
                Accessor.Instance.WriteShedulerError("RunCheck: " + e.Message + " " + e.StackTrace);
                RunCheck();
            }
        }

        /// <summary>
        /// Проверяем выполнилась ли на сегодня задача по созданию компанейских заказов
        /// Если нет, то выполняем
        /// </summary>
        /// <param name="stateInfo"></param>
        static void CheckStatusTask(object stateInfo)
        {
            try
            {
                var executionTimeThreshold = new TimeSpan(0, 0, 10, 0, 0);
                if (DateTime.Now.TimeOfDay <= executionTimeThreshold)
                {
                    /// Cоздание компанейских заказов в полночь по расписаниям кураторов
                    /// и создание задачи на следующий день в sheduled_tasks
                    new CreateCompanyOrdersTask().Run().GetAwaiter().GetResult();
                    Accessor.Instance.WriteShedulerError("Создание корп. заказов в полночь");
                }

                var task = Accessor.Instance.GetTaskCreateCompanyOrderByDate();
                var runTask = false;
                if (task == null)
                {
                    runTask = Accessor.Instance.AddTask(new Data.Entities.ScheduledTask()
                    {
                        CreateDate = DateTime.Now,
                        CreatorId = 0,
                        ScheduledExecutionTime = DateTime.Now.Date,
                        IsRepeatable = 1,
                        IsDeleted = false,
                        TaskType = EnumScheduledTaskType.CreateCompanyOrders
                    });
                }

                if (runTask || (task != null && task.IsDeleted == false))
                {
                    new CreateCompanyOrdersTask().Run().GetAwaiter().GetResult();
                    Accessor.Instance.WriteShedulerError("Выполнение задачи по созданию корп. заказов");
                }

                
                var autoEventTask = (AutoResetEvent)stateInfo;
                autoEventTask.Set();
            }
            catch (Exception e)
            {
                Accessor.Instance.WriteShedulerError("CheckStatusTask: " + e.Message + " " + e.StackTrace);
            }
        }
    }
}