using Food.Data.Entities;
using Food.Services.Tests.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Food.Services.Tests.FakeFactories
{
    public static class CafeMenuPatternDishFactory
    {
        static Random rand = new Random();
             
        /// <summary>
        /// Создать блюдо для шаблонного меню
        /// </summary>
        /// <param name="saveDB">true - сохранить в БД; false - не сохранять</param>
        public static CafeMenuPatternDish Create(bool saveDB = false)
        {
            var entity = new CafeMenuPatternDish
            {
                DishId = rand.Next(1, 1000),
                Id = rand.Next(1, 1000),
                Name = Guid.NewGuid().ToString(),
                PatternId = rand.Next(1, 1000),
                Price = rand.Next(1, 1000),
                IsDeleted = false,
            };

            if(saveDB) ContextManager.Get().CafeMenuPatternsDishes.Add(entity);

            return entity;
        }

        /// <summary>
        /// Создать несоклько блюд для шаблонного меню
        /// </summary>
        /// <param name="saveDB">true - сохранить в БД; false - не сохранять</param>
        /// <param name="count">Количество элементов</param>
        public static List<CafeMenuPatternDish> CreateFew(int count = 3, bool saveDB = false)
        {
            var lstEntities = new List<CafeMenuPatternDish>();
            for (int i = 0; i < count; i++)
            {
                lstEntities.Add(new CafeMenuPatternDish
                {
                    DishId = rand.Next(1, 1000),
                    Id = rand.Next(1, 1000),
                    Name = Guid.NewGuid().ToString(),
                    PatternId = rand.Next(1, 1000),
                    Price = rand.Next(1, 1000),
                    IsDeleted = false,
                });
            }

            if (saveDB) ContextManager.Get().CafeMenuPatternsDishes.AddRange(lstEntities);

            return lstEntities;
        }
    }
}
