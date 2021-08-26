using System.Collections.Generic;
using System.Linq;
using Food.Data.Entities;
using Food.Data;
using Microsoft.EntityFrameworkCore;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        #region Kitchen

        /// <summary>
        /// Возвращает список всех кухонь
        /// </summary>
        /// <returns></returns>
        public virtual List<Kitchen> GetListOfKitchen()
        {
            var fc = GetContext();

            var query = fc.Kitchens.AsNoTracking().Where(k => k.IsDeleted == false);

            return query.ToList();
        }

        /// <summary>
        /// Возвращает список привязок кафе-кухня по идентификатору кафе
        /// </summary>
        /// <param name="cafeId">идентификатор кафе</param>
        /// <returns></returns>
        public virtual List<KitchenInCafe> GetListOfKitchenToCafe(long cafeId)
        {
            var fc = GetContext();

            var query = fc.KitchensInCafes.Include(k => k.Kitchen).AsNoTracking().Where(k => k.CafeId == cafeId && k.IsDeleted == false);

            return query.ToList();
        }
        #endregion
    }
}
