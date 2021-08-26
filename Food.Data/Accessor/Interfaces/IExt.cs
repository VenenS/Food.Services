using System;
using System.Linq;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface IExt
    {
        #region Ext

        /// <summary>
        /// Возвращает расширение отчета по идентификтаору
        /// </summary>
        /// <param name="id">идентификатор отчета</param>
        /// <returns></returns>
        Ext GetExt(long id);

        /// <summary>
        /// добавляет расширение отчета
        /// </summary>
        /// <param name="ext">расришение отчета (сущность)</param>
        /// <returns></returns>
        bool AddExt(Ext ext);

        /// <summary>
        /// Редактирование расширения отчета
        /// </summary>
        /// <param name="ext">расшиерение отчета (сущность)</param>
        /// <returns></returns>
        bool EditExt(Ext ext);
        #endregion
    }
}
