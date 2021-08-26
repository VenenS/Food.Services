using Food.Data.Entities;
using System;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface ICompanyOrderSchedule
    {
        /// <summary>
        /// Возвращает список расписаний компанейских заказов для кафе
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <returns></returns>
        List<CompanyOrderSchedule> GetCompanyOrderScheduleByCafeId(long cafeId);

        /// <summary>
        /// Возвращает список расписаний для компании
        /// </summary>
        /// <param name="companyId">идентификатор компании</param>
        /// <returns></returns>
        List<CompanyOrderSchedule> GetCompanyOrderScheduleByCompanyId(long companyId);

        /// <summary>
        /// Возвращает список всех расписаний компаний
        /// </summary>
        /// <returns></returns>
        List<CompanyOrderSchedule> GetCompanyOrderSchedules();

        /// <summary>
        /// Добавляет расписание компании
        /// </summary>
        /// <param name="companyOrderSchedule"></param>
        /// <returns></returns>
        long AddCompanyOrderSchedule(CompanyOrderSchedule companyOrderSchedule);

        /// <summary>
        /// Редактировать расписание компании
        /// </summary>
        /// <param name="companyOrderSchedule">распиание (сущность)</param>
        /// <returns></returns>
        Tuple<long, IEnumerable<Order>> EditCompanyOrderSchedule(CompanyOrderSchedule companyOrderSchedule);

        /// <summary>
        /// Удалить расписание компании
        /// </summary>
        /// <param name="companyOrderScheduleId">идентификатор компании</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        Tuple<bool, IEnumerable<Order>> RemoveCompanyOrderSchedule(long companyOrderScheduleId, long userId);

        /// <summary>
        /// Возвращает расписание компании по идентификатору
        /// </summary>
        /// <param name="id">идентификатор расписания</param>
        /// <returns></returns>
        CompanyOrderSchedule GetCompanyOrderScheduleById(long id);

        /// <summary>
        /// Получение списка щаблонов заказов компании для указанного временного промежутка.
        /// </summary>
        /// <param name="companyId">Идентификатор компании.</param>
        /// <param name="cafeId">Идентификатор кафе. Либо null.</param>
        /// <param name="startDate">Начальная дата поиска шаблона.</param>
        /// <param name="endDate">Конечная дата поиска шаблона.</param>
        /// <returns>Список шаблонов заказов компании.</returns>
        List<CompanyOrderSchedule> GetCompanyOrderScheduleByRangeDate(
            long companyId, long? cafeId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Получение всех расписаний компаний на дату
        /// не учитываются удаленные и неактивыне кафе
        /// </summary>
        List<CompanyOrderSchedule> GetAllCompanyOrderScheduleByDate(DateTime date);
    }
}
