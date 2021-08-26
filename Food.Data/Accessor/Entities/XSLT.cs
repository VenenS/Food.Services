using System;
using System.Linq;
using System.Collections.Generic;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;
// ReSharper disable CheckNamespace

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region XSLT

        /// <summary>
        /// Возвращает трансформацию по идентификатору
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ReportStylesheet GetXslt(long id)
        {
            ReportStylesheet xslt;

            using (var fc = GetContext())
            {
                xslt =
                    fc.ReportStylesheets.AsNoTracking().FirstOrDefault(
                        x =>
                            x.Id == id
                            && x.IsDeleted == false
                    );
            }

            return xslt;
        }


        /// <summary>
        /// Добавялет трансформацию
        /// </summary>
        /// <param name="xslt"></param>
        /// <returns></returns>
        public long AddXslt(ReportStylesheet xslt)
        {
            try
            {
                using (var fc = GetContext())
                {
                    fc.ReportStylesheets.Add(xslt);
                    fc.SaveChanges();
                }
            }
            catch (Exception)
            {
                return -1;
            }

            return xslt.Id;
        }


        /// <summary>
        /// Редактирует трансформацию
        /// </summary>
        /// <param name="xslt"></param>
        /// <returns></returns>
        public bool EditXslt(ReportStylesheet xslt)
        {
            try
            {
                using (var fc = GetContext())
                {
                    ReportStylesheet oldXslt =
                        fc.ReportStylesheets.FirstOrDefault(
                            x =>
                                x.Id == xslt.Id
                                && x.IsDeleted == false
                        );

                    if (oldXslt != null)
                    {
                        oldXslt.Name = xslt.Name;
                        oldXslt.CafeId = xslt.CafeId;
                        oldXslt.ExtId = xslt.ExtId;
                        oldXslt.Description = xslt.Description;
                        oldXslt.Transformation = xslt.Transformation;
                        oldXslt.LastUpdDate = xslt.LastUpdDate;
                        oldXslt.LastUpdateByUserId = xslt.LastUpdateByUserId;
                        oldXslt.IsCommon = xslt.IsCommon;

                        fc.SaveChanges();
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

            return true;
        }


        /// <summary>
        /// Удаляет трансформацию
        /// </summary>
        /// <param name="xslt"></param>
        /// <returns></returns>
        public bool RemoveXslt(ReportStylesheet xslt)
        {
            try
            {
                using (var fc = GetContext())
                {
                    ReportStylesheet oldXslt =
                        fc.ReportStylesheets.FirstOrDefault(
                            x =>
                                x.Id == xslt.Id
                                && x.IsDeleted == false
                        );

                    if (oldXslt != null)
                    {
                        oldXslt.IsDeleted = true;
                        oldXslt.LastUpdateByUserId = xslt.LastUpdateByUserId;
                        oldXslt.LastUpdDate = xslt.LastUpdDate;

                        fc.SaveChanges();
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

            return true;
        }

        /// <summary>
        /// Получает трансформации для кафе
        /// </summary>
        /// <param name="cafeId"></param>
        /// <returns></returns>
        public List<ReportStylesheet> GetXsltFromCafe(long cafeId)
        {
            List<ReportStylesheet> xslts;

            using (var fc = GetContext())
            {
                xslts = fc.ReportStylesheets
                    .AsNoTracking()
                    .Where(xslt => (xslt.CafeId == cafeId || xslt.IsCommon) && xslt.IsDeleted == false)
                    .ToList();
            }

            return xslts;
        }

        /// <summary>
        /// Получает все трансформации
        /// </summary>
        public List<ReportStylesheet> GetXsltList()
        {
            List<ReportStylesheet> xslts;
            using (var fc = GetContext())
            {
                xslts = fc.ReportStylesheets.AsNoTracking().Where(xslt => xslt.IsDeleted == false).ToList();
            }

            return xslts;
        }       
        #endregion
    }
}
