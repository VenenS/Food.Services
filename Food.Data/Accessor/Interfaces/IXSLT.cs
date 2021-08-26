using Food.Data.Entities;
using System.Collections.Generic;
// ReSharper disable CheckNamespace

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface IXSLT
    {
        #region XSLT

        /// <summary>
        /// Возвращает трансформацию по идентификатору
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ReportStylesheet GetXslt(long id);

        /// <summary>
        /// Добавялет трансформацию
        /// </summary>
        /// <param name="xslt"></param>
        /// <returns></returns>
        long AddXslt(ReportStylesheet xslt);

        /// <summary>
        /// Редактирует трансформацию
        /// </summary>
        /// <param name="xslt"></param>
        /// <returns></returns>
        bool EditXslt(ReportStylesheet xslt);

        /// <summary>
        /// Удаляет трансформацию
        /// </summary>
        /// <param name="xslt"></param>
        /// <returns></returns>
        bool RemoveXslt(ReportStylesheet xslt);

        /// <summary>
        /// Получает трансформации для кафе
        /// </summary>
        /// <param name="cafeId"></param>
        /// <returns></returns>
        List<ReportStylesheet> GetXsltFromCafe(long cafeId);

        /// <summary>
        /// Получает все трансформации
        /// </summary>
        List<ReportStylesheet> GetXsltList();

        #endregion
    }
}
