using ITWebNet.FoodService.Food.DbAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
// ReSharper disable CheckNamespace

namespace Food.Services
{
    public static class Scheduler
    {
        private static object Lockobject { get; set; }
        private static object LockobjectCreateCompanyOrders { get; set; }
        private static object LockobjectDeleteForDishInMenu { get; set; }
        private static Data.Entities.ScheduledTask _task;
        private static List<Data.Entities.ScheduledTask> _taskList;

        /// <summary>
        ///     Запустить планировщик
        /// </summary>
        static void Run()
        {
            var autoEvent = new AutoResetEvent(false);

            // Каждую минуту таймер запускает CheckStatus
            var scheduleTimer = new Timer(
                CheckStatus,
                autoEvent,
                1000,
                60000 * 5
                );

            autoEvent.WaitOne();
            // Если здесь, то задач нет - удаляем таймер.
            scheduleTimer.Dispose();
        }

        // This method is called by the timer delegate.
        public static void CheckStatus(object stateInfo)
        {
            var autoEvent = (AutoResetEvent)stateInfo;

            _taskList = Accessor.Instance.GetScheduledTasksList();

            // Закрываем таймер, если задач больше не осталось
            if (_taskList.Count == 0)
            {
                Lockobject = null;
                autoEvent.Set();
                return;
            }

            #region Занимается отправкой уведомлений в кафе о заказах

            _task = _taskList.FirstOrDefault();

            // Время выполнения подошло - запускаем выполнение задачи
            if (_task.ScheduledExecutionTime <= DateTime.Now)
            {
                var order =
                    Accessor.Instance.GetOrderById((long)_task.OrderId);
                var cafe =
                    Accessor.Instance.GetCafeById((long)_task.CafeId);

                if (_task.BanketId.HasValue)
                {
                    //банкетный заказ
                    var banketTask = new BanketOrderTask(
                        _task.ScheduledExecutionTime,
                        _task.IsRepeatable,
                        _task.IsDeleted,
                        _task.BanketId.Value,
                        cafe
                    );
                    banketTask.Run();
                }
                else
                {
                    if (_task.CompanyOrderId == null)
                    {
                        //индивидуальный заказ
                        var individualTask = new IndividualOrderTask(
                             _task.ScheduledExecutionTime,
                                _task.IsRepeatable,
                                _task.IsDeleted,
                                cafe,
                                order
                            );

                        individualTask.Run();
                    }
                    else
                    {
                        //компанейский заказ
                        var executeTask =
                            new CompanyOrderTask(
                                _task.ScheduledExecutionTime,
                                _task.IsRepeatable,
                                _task.IsDeleted,
                                (long)_task.CompanyOrderId,
                                cafe,
                                order
                            );

                        executeTask.Run();
                    }
                }
            }

            #endregion
        }

        public static void Start()
        {
            //Запуск планировщика создания компанейских заказов
            if (LockobjectCreateCompanyOrders == null)
            {
                LockobjectCreateCompanyOrders = new object();
                var t2 = new Thread(ShedulerCreateCompanyOrders.RunCheck);
                t2.IsBackground = true;
                t2.Start();
            }

            //Запуск планировщика по отправлению уведомлений в кафе
            if (Lockobject == null)
            {
                Lockobject = new object();
                var t = new Thread(Run);
                t.Start();
            }

            //Запуск планировщика на удаление записей из dish_in_menu
            if (LockobjectDeleteForDishInMenu == null)
            {
                LockobjectDeleteForDishInMenu = new object();
                /*запуск планировщика на удаление из dish_in_menu*/
                var t = new Thread(RunDeleteFromDishInMenu);
                t.Start();
            }
        }
        static void RunDeleteFromDishInMenu()
        {
            var autoEvent = new AutoResetEvent(false);

            // Раз в 15 дней таймер запускает CheckStatus
            var scheduleTimer = new Timer(
                CheckStatusForDeletingFromDishInMenu,
                autoEvent,
                60000 * 60 * 24 * 15,//вызовется через 15 дней
                60000 * 60 * 24 * 15//здесь неважно
                );

            autoEvent.WaitOne();
            // Если здесь, то задач нет - удаляем таймер.
            scheduleTimer.Dispose();
            //Перезапускаем задачу
            RunDeleteFromDishInMenu();
        }
        public static void CheckStatusForDeletingFromDishInMenu(object stateInfo)
        {
            var autoEvent = (AutoResetEvent)stateInfo;

            _task = Accessor.Instance.GetTaskDeleteFromDishInMenu(new DateTime(2019, 7, 1));
            //Задача должна быть в базе на это число
            if (_task == null)
            {
                Accessor.Instance.AddTask(
                            new Data.Entities.ScheduledTask
                            {
                                CreateDate = DateTime.Now,
                                ScheduledExecutionTime = new DateTime(2019, 7, 1),
                                TaskType = Data.Entities.EnumScheduledTaskType.DeleteFromDishInMenu
                            });
                LockobjectDeleteForDishInMenu = null;
                autoEvent.Set();
                return;
            }
            //Если есть задача, то делаем
            else if (_task != null)
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
                LockobjectDeleteForDishInMenu = null;
                autoEvent.Set();
                return;
            }
        }
    }
}
