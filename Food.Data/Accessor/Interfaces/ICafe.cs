using Food.Data.Entities;
using System;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface ICafe
    {
        /// <summary>
        /// Получаем список всех активных кафе
        /// </summary>
        /// <returns></returns>
        List<Cafe> GetCafes();

        /// <summary>
        /// Возвращает кафе
        /// </summary>
        /// <param name="id">идентификатор кафе</param>
        /// <returns></returns>
        Cafe GetCafeById(long id);

        /// <summary>
        /// Возвращает кафе
        /// </summary>
        /// <param name="id">идентификатор кафе</param>
        /// <returns></returns>
        Cafe GetCafeByIdIgnoreActivity(long id);

        /// <summary>
        /// Возвращает кафе по названию (cleanUrl)
        /// </summary>
        /// <param name="cleanUrl">Название кафе</param>
        /// <returns></returns>
        Cafe GetCafeByUrl(string cleanUrl);

        IEnumerable<Tuple<DateTime, Company, List<Cafe>>> GetCafesAvailableToEmployee(long userId, List<DateTime> dates);

        /// <summary>
        /// Добавляем кафе
        /// </summary>
        /// <param name="cafe">кафе (сущность)</param>
        /// <returns></returns>
        long AddCafe(Cafe cafe);

        /// <summary>
        /// Редактируем кафе
        /// </summary>
        /// <param name="cafe">кафе (сущность))</param>
        /// <returns></returns>
        Tuple<long, IEnumerable<Order>> EditCafe(Cafe cafe);

        /// <summary>
        /// Проверяет уникальность названия кафе
        /// </summary>
        bool СheckUniqueName(string name, long cafeId = -1);

        /// <summary>
        /// Удаляем кафе
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        Tuple<bool, IEnumerable<Order>> RemoveCafe(long cafeId, long userId);

        /// <summary>
        /// Возвращает список кафе, которыми пользователь управляет
        /// </summary>
        /// <param name="userId">идентификатор пользователя</param>
        /// <returns></returns>
        List<Cafe> GetManagedCafes(long userId);

        /// <summary>
        /// Получает рабочее время кафе.
        /// </summary>
        /// <param name="cafeId">Идентификатор кафе.</param>
        /// <returns>Рабочее время кафе.</returns>
        BusinessHours GetCafeBusinessHours(long cafeId);

        /// <summary>
        /// Задает рабочее время кафе.
        /// </summary>
        /// <param name="businessHours">Рабочее время кафе.</param>
        /// <param name="cafeId">Идентификатор кафе.</param>
        void SetCafeBusinessHours(BusinessHours businessHours, long cafeId);

        List<Cafe> GetCafesByUserId(long userId);
    }
}
