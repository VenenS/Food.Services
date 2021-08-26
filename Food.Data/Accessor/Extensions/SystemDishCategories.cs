using System.Collections.Generic;
using System.Linq;
using accessor = ITWebNet.FoodService.Food.DbAccessor.Accessor;

namespace ITWebNet.FoodService.Food.Data.Accessor.Extensions
{
    public static class SystemDishCategories
    {
        public const string DeletedDishesCategory = "удаленные блюда";
        public const string CategoryForDeletedDishes = "блюда удаленных категорий";

        public static HashSet<long> GetSystemCtegoriesIds()
        {
            using (var fc = accessor.Instance.GetContext())
            {
                return new HashSet<long>(fc.DishCategories.Where(
                    e => (e.CategoryName.Trim().ToLower() == DeletedDishesCategory
                    || e.CategoryName.Trim().ToLower() == CategoryForDeletedDishes)
                    && !e.IsDeleted).Select(e => e.Id));
            }
        }
    }
}
