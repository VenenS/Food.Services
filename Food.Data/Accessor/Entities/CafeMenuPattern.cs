using Food.Data;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Xsl;

namespace ITWebNet.FoodService.Food.DbAccessor
{
    public partial class Accessor
    {
        public bool AddCafeMenuPattern(CafeMenuPattern pattern)
        {
            var fc = GetContext();

            fc.CafeMenuPatterns.Add(pattern);

            fc.SaveChanges();

            return true;
        }

        public CafeMenuPattern GetCafeMenuPatternById(long cafeId, long patternId)
        {
            var fc = GetContext();
            var pattern =
                fc.CafeMenuPatterns
                .Include(c => c.Dishes)
                .AsNoTracking()
                .FirstOrDefault(c => !c.IsDeleted && c.Id == patternId && c.CafeId == cafeId);

            return pattern;
        }

        public List<CafeMenuPattern> GetMenuPatternsByCafeId(long cafeId)
        {
            var fc = GetContext();
            var pattern = fc.CafeMenuPatterns.AsNoTracking().Where(c => !c.IsDeleted && c.CafeId == cafeId).ToList();

            return pattern;
        }

        public Dictionary<DishCategory, List<Dish>> GetMenuByPatternId(long cafeId, long id)
        {
            try
            {
                var fc = GetContext();

                var pattern = fc.CafeMenuPatterns.AsNoTracking().FirstOrDefault(c => !c.IsDeleted && c.CafeId == cafeId && c.Id == id);

                var result = new Dictionary<DishCategory, List<Dish>>();
                if (pattern == null)
                    return result;

                pattern.Dishes.ForEach(d => {
                    d.Dish.BasePrice = d.Price;
                    d.Dish.DishName = d.Name;
                });

                var categoryIds = new HashSet<long>();
                foreach(var item in pattern.Dishes)
                {
                    categoryIds.Union(item.Dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted && l.CafeCategory.CafeId == cafeId).Select(c => c.CafeCategory.DishCategoryId));
                }
                var categories = fc.DishCategories.Where(c => categoryIds.Contains(c.Id));
                foreach (var item in categories)
                {
                    result.Add(item, pattern.Dishes.Where(d => d.Dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted)
                    .Select(l => l.CafeCategory.DishCategoryId).Contains(item.Id)).Select(d => d.Dish).ToList());
                }
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new Dictionary<DishCategory, List<Dish>>();
            }
        }

        public bool RemoveCafeMenuPattern(long cafeId, long patternId)
        {
            try
            {
                var fc = GetContext();

                var pattern = fc.CafeMenuPatterns.FirstOrDefault(c => c.Id == patternId && c.CafeId == cafeId);

                if (pattern == null)
                    return false;

                pattern.IsDeleted = true;
                pattern.Dishes.ForEach(c => { c.IsDeleted = true; });

                fc.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        public bool UpdateCafeMenuPattern(CafeMenuPattern pattern)
        {
            try
            {
                var fc = GetContext();

                var patternM =
                    fc.CafeMenuPatterns.FirstOrDefault(c =>
                        !c.IsDeleted && c.CafeId == pattern.CafeId && c.Name.ToLower() == pattern.Name.ToLower());

                if (patternM == null)
                {
                    return false;
                }

                patternM.IsBanket = pattern.IsBanket;
                var currentDishes = patternM.Dishes?.ToList();
                if (pattern.Dishes == null)
                {
                    currentDishes.ForEach(s => s.IsDeleted = true);
                }
                else
                {
                    var lstPatternDishesIds = pattern.Dishes.Select(c => c.DishId);

                    //проверяем, есть ли записи в таблице, которые удалены, новые добавлять не будем
                    var deletedDishes = currentDishes.Where(c => !c.IsDeleted).Select(c => c.DishId)
                        .Except(lstPatternDishesIds).ToList();
                    foreach (var dish in currentDishes)
                    {
                        if (deletedDishes.Contains(dish.DishId))
                        {
                            dish.IsDeleted = true;
                        }
                    }

                    var lstCurrentDishesIds = currentDishes.Select(s => s.DishId);

                    var addedDishes = lstPatternDishesIds.Except(lstCurrentDishesIds).ToList();
                    var changedIds = lstPatternDishesIds.Intersect(lstCurrentDishesIds).ToList();
                    var changed = fc.CafeMenuPatternsDishes.Where(c => changedIds.Contains(c.DishId) && c.PatternId == patternM.Id).ToList();
                    foreach (var item in changed)
                    {
                        var newDish = pattern.Dishes.FirstOrDefault(c => !c.IsDeleted && c.DishId == item.DishId);
                        if (newDish == null)
                            continue;

                        item.Name = newDish.Name;
                        item.Price = newDish.Price;
                        item.IsDeleted = false;
                    }

                    var newDishes = pattern.Dishes.Where(c => !c.IsDeleted && addedDishes.Contains(c.DishId)).Select(
                        c => new CafeMenuPatternDish()
                        {
                            DishId = c.DishId,
                            Name = c.Name,
                            Price = c.Price,
                            PatternId = patternM.Id
                        }).ToList();
                    fc.CafeMenuPatternsDishes.AddRange(newDishes);

                    fc.SaveChanges();
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }
    }
}