using Food.Data.Entities;
using ITWebNet.FoodService.Food.Data.Accessor.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface ICompanyCurator
    {
        /// <summary>
        /// Добавление куратора в компаниюы
        /// </summary>
        bool AddCompanyCurator(CompanyCuratorModel model, long authorId);

        /// <summary>
        /// Удаление куратора из компании
        /// </summary>
        bool DeleteCompanyCurator(long companyId, long userId, long authorId);

        /// <summary>
        /// Проверка пользователя на куратора компании
        /// </summary>
        bool IsUserCuratorOfCafe(long userId, long companyId);

        /// <summary>
        /// Получение курирующей компании
        /// </summary>
        Company GetCurationCompany(long userId);

        /// <summary>
        /// Получение привязки куратора и компании
        /// </summary>
        CompanyCurator GetCompanyCurator(long userId, long companyId);
    }
}
