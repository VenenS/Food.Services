using Food.Data.Entities;

namespace Food.Data.Accessor.Extensions
{
    public static class DIshExtensions
    {
        public static Dish GetEntityFromDishVersion(this DishVersion dishVersionItem)
        {
            return dishVersionItem == null
                ? new Dish()
                : new Dish
                {
                    Id = dishVersionItem.DishId,
                    DishName = dishVersionItem.DishName,
                    BasePrice = dishVersionItem.BasePrice,
                    Kcalories = dishVersionItem.Kcalories,
                    Weight = dishVersionItem.Weight,
                    WeightDescription = dishVersionItem.WeightDescription,
                    VersionFrom = dishVersionItem.VersionFrom,
                    VersionTo = dishVersionItem.VersionTo,
                    Composition = string.Empty
                };
        }
    }
}