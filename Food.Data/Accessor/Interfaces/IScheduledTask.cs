using Food.Data.Entities;
using System;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface IScheduledTask
    {
        #region ScheduledTask

        /// <summary>
        /// Получение списка активных запланированных задач отсортированных по времени выполнения.
        /// </summary>
        /// <returns>Список активных запланированных задач.</returns>
        List<ScheduledTask> GetScheduledTasksList();

        /// <summary>
        /// Получить задачу на создание компанейских заказов на дату
        /// </summary>
        ScheduledTask GetTaskCreateCompanyOrderByDate(DateTime? date = null);

        /// <summary>
        /// Получить задачу на удалениезаписей из dish_in_menu со статусом is_active = false , is_deleted = true , s_type = E
        /// </summary>
        ScheduledTask GetTaskDeleteFromDishInMenu(DateTime? date = null);

        /// <summary>
        /// Получение задачи по идентификатору
        /// </summary>
        /// <param name="taskId">Идентификатор задачи</param>
        /// <returns>Задача</returns>
        ScheduledTask GetTaskById(long? taskId);

        /// <summary>
        /// Получить идентификатор задачи по параметрам
        /// </summary>
        /// <param name="orderId">Идентификатор заказа</param>
        /// <param name="cafeId">Идентификатор кафе</param>
        /// <param name="companyOrderId">Идентифиактор компанейского заказа</param>
        /// <param name="banketId"></param>
        /// <returns>Идентификатор запланированной задачи</returns>
        long? GetTaskIdByParameters(long? orderId, long? cafeId, long? companyOrderId, long? banketId = null);

        /// <summary>
        /// Создание новой задачи для планировщика.
        /// </summary>
        /// <param name="newTask">Новая задача.</param>
        /// <returns>true - добавление прошло успешно,
        /// false - ошибка добавления</returns>
        bool AddTask(ScheduledTask newTask);

        /// <summary>
        /// Удалить задачу.
        /// </summary>
        /// <param name="taskId">Задача.</param>
        /// <returns>true - удаление прошло успешно,
        /// false - ошибка удаления</returns>
        bool RemoveTask(Int64 taskId);

        /// <summary>
        /// Проверка отсутствия задачи на отправку уведомления конкретного компанейского заказа.
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns>true - задача не найдена
        /// false - для данного заказа уже существует задача</returns>
        bool CheckForTaskInDb(long? orderId);

        #endregion
    }
}
