using Food.Data.Entities;
using System.Collections.Generic;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public interface ICafeMenuPattern
    {
        bool AddCafeMenuPattern(CafeMenuPattern pattern);

        CafeMenuPattern GetCafeMenuPatternById(long cafeId, long patternId);

        List<CafeMenuPattern> GetMenuPatternsByCafeId(long cafeId);

        Dictionary<DishCategory, List<Dish>> GetMenuByPatternId(long cafeId, long id);

        bool RemoveCafeMenuPattern(long cafeId, long patternId);

        bool UpdateCafeMenuPattern(CafeMenuPattern pattern);
    }
}