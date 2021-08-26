using Food.Data.Entities;

using System;

namespace Food.Services.Services
{
    /// <summary>
    /// Кэширующий сервис, содержащий операции по получении данных текущего пользователя.
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Возвращает текущего пользователя.
        /// </summary>
        User GetUser();

        /// <summary>
        /// Возвращает компанию к которой привязан текущий пользователь.
        /// </summary>
        Company GetUserCompany();

        /// <summary>
        /// Возвращает корп. заказ для компании текущего пользователя.
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <param name="when">дата заказа</param>
        CompanyOrder GetUserCompanyOrder(long cafeId, DateTime when);

        /// <summary>
        /// Возвращает идентификатор текущего пользователя.
        /// </summary>
        /// <returns>Возвращает идентификатор пользователя или null, если пользователь
        /// не аутентифицирован.</returns>
        long? GetUserId();

        /// <summary>
        /// Возвращает является ли текущий пользователем сотрудником компании.
        /// </summary>
        bool IsCompanyEmployee();

        /// <summary>
        /// Возвращает является ли текущий пользователь менеджером кафе.
        /// </summary>
        bool IsCafeManager(long cafeId);

        /// <summary>
        /// Сбрасывает кэш. Используется после мутации объектов пользователя
        /// чтобы избежать проблем с неактуальными закешированными данными.
        /// </summary>
        void InvalidateCache();
    }
}