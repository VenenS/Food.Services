using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region ScheduledTask

        /// <summary>
        /// Получение списка активных запланированных задач отсортированных по времени выполнения.
        /// </summary>
        /// <returns>Список активных запланированных задач.</returns>
        public List<ScheduledTask> GetScheduledTasksList()
        {
            List<ScheduledTask> itemFromBase = new List<ScheduledTask>();

            try
            {
                using (var fc = GetContext())
                {
                    var currentDate = DateTime.Now.Date;
                    var query = from o in fc.ScheduledTask.AsNoTracking()
                        where (
                            o.ScheduledExecutionTime <= DateTime.Now  // нужные задачи на текущий момент
                            && o.TaskType == EnumScheduledTaskType.NotificationCafe
                            && o.IsDeleted == false
                            )
                        orderby o.ScheduledExecutionTime ascending
                        select o;

                    itemFromBase = query.ToList();

                    return itemFromBase;
                }
            }
            catch (Exception)
            {
                return itemFromBase;
                //throw new Exception("Ошибка получения списка задач планировщика. " + ex);
            }
        }

        /// <summary>
        /// Получить задачу на создание компанейских заказов на дату
        /// </summary>
        public ScheduledTask GetTaskCreateCompanyOrderByDate(DateTime? date = null)
        {
            try
            {
                ScheduledTask task;

                if (date == null)
                    date = DateTime.Now.Date;
                using (var fc = GetContext())
                {
                    task = fc.ScheduledTask.AsNoTracking().FirstOrDefault(
                        o => o.ScheduledExecutionTime.HasValue
                             && o.ScheduledExecutionTime.Value == date.Value
                             && o.TaskType == EnumScheduledTaskType.CreateCompanyOrders);
                }

                return task;
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// Получить задачу на удалениезаписей из dish_in_menu со статусом is_active = false , is_deleted = true , s_type = E
        /// </summary>
        public ScheduledTask GetTaskDeleteFromDishInMenu(DateTime? date = null)
        {
            try
            {
                ScheduledTask task;

                if (date == null)
                    date = DateTime.Now.Date;
                using (var fc = GetContext())
                {
                    task = fc.ScheduledTask.FirstOrDefault(
                        o => o.ScheduledExecutionTime.HasValue
                             && o.ScheduledExecutionTime.Value == date.Value
                             && o.TaskType == EnumScheduledTaskType.DeleteFromDishInMenu);
                }

                return task;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Получение задачи по идентификатору
        /// </summary>
        /// <param name="taskId">Идентификатор задачи</param>
        /// <returns>Задача</returns>
        public ScheduledTask GetTaskById(long? taskId)
        {
            ScheduledTask task = null;

            if (taskId > -1)
            {
                try
                {
                    using (var fc = GetContext())
                    {
                        var query = from o in fc.ScheduledTask.AsNoTracking()
                                    where (
                                        o.Id == taskId
                                        && o.IsDeleted == false
                                        )
                                    select o;

                        task = query.FirstOrDefault();

                        return task;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return task;
        }

        /// <summary>
        /// Получить идентификатор задачи по параметрам
        /// </summary>
        /// <param name="orderId">Идентификатор заказа</param>
        /// <param name="cafeId">Идентификатор кафе</param>
        /// <param name="companyOrderId">Идентифиактор компанейского заказа</param>
        /// <param name="banketId"></param>
        /// <returns>Идентификатор запланированной задачи</returns>
        public long? GetTaskIdByParameters(long? orderId, long? cafeId, long? companyOrderId, long? banketId = null)
        {
            try
            {
                using (var fc = GetContext())
                {
                    var query = fc.ScheduledTask.AsNoTracking().Where(c =>
                        c.OrderId == orderId && c.CafeId == cafeId && c.TaskType == EnumScheduledTaskType.NotificationCafe && c.IsDeleted == false);

                    if (companyOrderId.HasValue)
                        query = query.Where(c => c.CompanyOrderId == companyOrderId);

                    if (banketId.HasValue)
                        query = query.Where(c => c.BanketId == banketId);

                    ScheduledTask item = query.FirstOrDefault();

                    return item.Id;
                }
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Создание новой задачи для планировщика.
        /// </summary>
        /// <param name="newTask">Новая задача.</param>
        /// <returns>true - добавление прошло успешно,
        /// false - ошибка добавления</returns>
        public bool AddTask(ScheduledTask newTask)
        {
            try
            {
                using (var fc = GetContext())
                {
                    fc.ScheduledTask.Add(newTask);
                    fc.SaveChanges();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Удалить задачу.
        /// </summary>
        /// <param name="taskId">Задача.</param>
        /// <returns>true - удаление прошло успешно,
        /// false - ошибка удаления</returns>
        public bool RemoveTask(Int64 taskId)
        {
            var ci = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
            if (ci == null)
                return false;

            var value = ci.FindFirst(ClaimTypes.NameIdentifier);
            var id = "0";
            if (value != null)
            {
                id = value.Value;
            }
            var currentUser = _instance.GetUserById(long.Parse(id));

            try
            {
                using (var fc = GetContext())
                {
                    ScheduledTask remTask =
                            fc.ScheduledTask.FirstOrDefault(
                                o => o.Id == taskId
                                && o.IsDeleted == false
                                );

                    if (remTask != null)
                    {
                        remTask.IsDeleted = true;
                        remTask.LastUpdDate = DateTime.Now;
                        remTask.LastUpdateBy = currentUser?.Id;

                        fc.SaveChanges();

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Проверка отсутствия задачи на отправку уведомления конкретного компанейского заказа.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns>true - задача не найдена
        /// false - для данного заказа уже существует задача</returns>
        public bool CheckForTaskInDb(long? orderId)
        {
            try
            {
                using (var fc = GetContext())
                {
                    if (orderId != null)
                    {
                        IQueryable<ScheduledTask> query =
                            from oi in fc.ScheduledTask.AsNoTracking()
                            where (
                                oi.CompanyOrderId == orderId
                                && oi.IsDeleted == false
                            )
                            select oi;

                        if (query.Count() == 0)
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        #endregion
    }
}
