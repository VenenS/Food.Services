using Food.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface ISchedule
    {
        #region Schedule


        /// <summary>
        /// Возвращает определенное количество первых блюд в каждой категории, выбранных из запланированных меню
        /// для всех кафе на указанную дату (или на текущую дату, если date == null);
        /// </summary>
        /// <param name="cafeIds">Идентификаторы кафе, из которых можно заказывать блюда</param>
        /// <param name="date">дата</param>
        /// <param name="name">URL кафе, для которого нужно показать блюда</param>
        /// <param name="countDishes">Количество блюд, которое нужно взять</param>
        /// <param name="dishCatId">Категория, из которой надо отобразить блюда. Применяется в случае выбора фильтра по категориям.</param>
        /// <returns></returns>
        Task<List<First6DishesFromCat>> GetFirstDishesFromAllSchedules(long[] cafeIds, DateTime? date, string name = null, int? countDishes = 6, string filter = null, long? dishCatId = null);

        /// <summary>
        /// Возвращает список расписаний на дату для всех кафе, за
        /// исключением блюд, указанных в списке
        /// </summary>
        /// <param name="date">дата</param>
        /// <returns></returns>
        Task<List<Dish>> GetSchedulesRestOfCat(long[] cafeIds, DateTime? date, long categoryId, IEnumerable<long> dishIds, string name = null);

        /// <summary>
        /// Возвращает расписание по идентификатору
        /// </summary>
        /// <param name="id">идентификатор</param>
        /// <returns></returns>
        DishInMenu GetScheduleById(long id);

        /// <summary>
        /// Возвращает список расписаний блюда на дату
        /// </summary>
        /// <param name="date">дата</param>
        /// <param name="dishId">идентификатор блюда</param>
        /// <returns></returns>
        List<DishInMenu> GetScheduleActiveByDate(DateTime date, long? dishId);

        /// <summary>
        /// Возващает список распианий блюда на период
        /// </summary>
        /// <param name="startPeriod">начало периода</param>
        /// <param name="endPeriod">окончание периода</param>
        /// <param name="dishId">идентификатор блюда</param>
        /// <returns></returns>
        List<DishInMenu> GetScheduleActiveByDateRange(DateTime startPeriod, DateTime endPeriod, Int64? dishId);

        /// <summary>
        /// Возвращает список расписаний для блюда на дату
        /// </summary>
        /// <param name="dishId">идентификатор блюда</param>
        /// <param name="date">дата</param>
        /// <returns></returns>
        List<DishInMenu> GetScheduleActiveByDishId(long dishId, DateTime date);

        /// <summary>
        /// Возвращает список расписаний для кафе на дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="date">дата</param>
        /// <returns></returns>
        List<DishInMenu> GetSchedulesForCafe(long cafeId, DateTime? date);

        /// <summary>
        /// Возвращает список расписаний для кафе на дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="dishList"></param>
        /// <param name="onlyActive"></param>
        /// <returns></returns>
        List<DishInMenu> GetSchedulesForCafeByDishList(long cafeId, long[] dishList, bool onlyActive);

        /// <summary>
        /// Возвращает список расписаний для кафе на дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="dishList"></param>
        /// <returns></returns>
        Task<List<DishInMenuHistory>> GetDishesInMenuHistoryForCafe(long cafeId, string dishName);

        /// <summary>
        /// Возвращает список расписаний для кафе на дату
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="date">дата</param>
        /// <returns></returns>
        Dictionary<DishCategory, List<Dish>> GetShedulesForManager(long cafeId, DateTime? date);

        /// <summary>
        /// Редактировать существующее расписание
        /// </summary>
        /// <param name="schedule">расписание</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        long EditSchedule(DishInMenu schedule, long userId);

        /// <summary>
        /// Возвращает расписание с указанными параметрами (блюдо, тип, даты, дни месяца);.
        /// Нужно для удаления старого расписания при добавлении нового с такими же параметрами, чтобы не было задвоений.
        /// </summary>
        /// <param name="dishId"></param>
        /// <param name="type"></param>
        /// <param name="beginDate"></param>
        /// <param name="endDate"></param>
        /// <param name="oneDay"></param>
        /// <param name="monthDays"></param>
        /// <returns></returns>
        List<DishInMenu> GetSheduleByParams(long dishId, string type, DateTime? beginDate, DateTime? endDate, DateTime? oneDay);

        /// <summary>
        /// Обновить расписание
        /// </summary>
        /// <param name="schedule">расписание</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        Int64 UpdateSchedule(DishInMenu schedule, long userId);

        /// <summary>
        /// Добавить расписание для блюда
        /// </summary>
        /// <param name="dishId">идентификатор блюда</param>
        /// <param name="scheduleType">тип расписания</param>
        /// <param name="beginDate">начало периода</param>
        /// <param name="endDate">окончание периода</param>
        /// <param name="oneDate">дата</param>
        /// <param name="monthDays">дни месяца</param>
        /// <param name="weekDays">дни недели</param>
        /// <param name="price">цена</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        long AddSchedule(
            long dishId,
            char scheduleType,
            DateTime? beginDate,
            DateTime? endDate,
            DateTime? oneDate,
            string monthDays,
            string weekDays,
            double? price,
            long userId
        );

        /// <summary>
        /// Добавить историю
        /// </summary>
        /// <param name="dishId">идентификатор блюда</param>
        /// <param name="scheduleType">тип расписания</param>
        /// <param name="price">цена</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        long AddDishInMenuHistory(
            long dishId,
            char scheduleType,
            double? price,
            long userId
        );

        /// <summary>
        /// Удалить расписание
        /// </summary>
        /// <param name="scheduleId">идентификатор расписания</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        long RemoveSchedule(long scheduleId, long userId);


        /// <summary>
        /// Пометить блюда определенного типа в расписании как удаленные
        /// </summary>
        /// <param name="type">S, D, W, M, E</param>
        /// <returns></returns>
        bool RemoveSchedules(HashSet<string> hstType, DateTime date, long cafeId);

        long DeleteSchedule(long scheduleId, long userId);

        /// <summary>
        /// Деактивировать блюда в расписании, которые были исключены
        /// </summary>
        bool SetActiveScheduleByDish(long dishId);

        #endregion
    }
}
