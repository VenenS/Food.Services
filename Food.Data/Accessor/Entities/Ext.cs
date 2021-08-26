using System;
using System.Linq;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region Ext

        /// <summary>
        /// Возвращает расширение отчета по идентификтаору
        /// </summary>
        /// <param name="id">идентификатор отчета</param>
        /// <returns></returns>
        public Ext GetExt(long id)
        {
            Ext ext;

            using (var fc = GetContext())
            {
                ext = fc.Ext.AsNoTracking().FirstOrDefault(x => x.Id == id && x.IsDeleted == false);
            }

            return ext;
        }

        /// <summary>
        /// добавляет расширение отчета
        /// </summary>
        /// <param name="ext">расришение отчета (сущность)</param>
        /// <returns></returns>
        public bool AddExt(Ext ext)
        {
            try
            {
                using (var fc = GetContext())
                {
                    fc.Ext.Add(ext);

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
        /// Редактирование расширения отчета
        /// </summary>
        /// <param name="ext">расшиерение отчета (сущность)</param>
        /// <returns></returns>
        public bool EditExt(Ext ext)
        {
            try
            {
                using (var fc = GetContext())
                {
                    Ext oldExt =
                        fc.Ext.FirstOrDefault(
                            x =>
                                x.Id == ext.Id
                                && x.IsDeleted == false
                        );

                    if (oldExt != null)
                    {
                        oldExt.Name = ext.Name;
                        oldExt.LastUpdateByUserId = ext.LastUpdateByUserId;
                        oldExt.LastUpdDate = oldExt.LastUpdDate;

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
        #endregion
    }
}
