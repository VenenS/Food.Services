using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using ITWebNet.FoodService.Food.DbAccessor;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    class ScheduleTests
    {
        FakeContext _context;
        User _user;

        [SetUp]
        public void SetUp()
        {
            _context = new FakeContext();
            ContextManager.Set(_context);
            ITWebNet.FoodService.Food.DbAccessor.Accessor.SetTestingModeOn(_context);
            _user = UserFactory.CreateUser();
        }

        [Test]
        public async Task GetFirstDishesFromAllSchedules()
        {
            Func<List<First6DishesFromCat>, int> countDishes = a => a.Select(x => x.Dishes.Count()).Aggregate(0, (acc, x) => acc + x);
            var menu = DishInMenuFactory.CreateFew(10);

            foreach (var dim in menu)
            {
                dim.Type = "D";
                dim.BeginDate = DateTime.Now.AddDays(-10);
                dim.EndDate = DateTime.Now.AddDays(10);
            }

            // Запрос на все блюда.
            var cafeIds = menu.Select(x => x.Dish.Cafe.Id);
            var result = await ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetFirstDishesFromAllSchedules(cafeIds.ToArray(), null, null, 10000);
            Assert.AreEqual(menu.Count, countDishes(result));

            // Запрос на блюда из ограниченного списка кафе.
            var someIds = cafeIds.Take(cafeIds.Count() / 5);
            Assert.IsTrue(someIds.Count() > 0);
            result = await ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetFirstDishesFromAllSchedules(someIds.ToArray(), null, null, 10000);
            Assert.AreEqual(someIds.Count(), countDishes(result));

            // Пустой список кафе.
            result = await ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetFirstDishesFromAllSchedules(new long[] {}, null, null, 10000);
            Assert.AreEqual(0, countDishes(result));

            // Список блюд из 1 категории.
            result = await ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetFirstDishesFromAllSchedules(cafeIds.ToArray(), null, null, 10000, null,
                menu[0].Dish.DishCategoryLinks.Where(l => l.IsActive == true && !l.IsDeleted).Select(x => x.CafeCategoryId).ToArray());
            Assert.AreEqual(1, countDishes(result));

            // "Отключить" некоторые записи в меню.
            menu[menu.Count - 1].IsDeleted = true;
            menu[menu.Count - 2].BeginDate = DateTime.MinValue;
            menu[menu.Count - 2].EndDate = DateTime.MinValue.AddDays(1000);
            menu[menu.Count - 3].BeginDate = DateTime.Now.AddDays(360);
            menu[menu.Count - 3].EndDate = DateTime.Now.AddDays(420);

            result = await ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetFirstDishesFromAllSchedules(cafeIds.ToArray(), null, null, 10000);
            Assert.AreEqual(menu.Count - 3, countDishes(result));
        }

        [Test]
        public void GetScheduleByIdTest()
        {
            var menu = DishInMenuFactory.CreateFew(10);

            foreach (var dim in menu)
            {
                var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetScheduleById(dim.Id);
                Assert.NotNull(result);
                Assert.AreEqual(dim.Id, result.Id);
            }

            var deleted = DishInMenuFactory.Create();
            deleted.IsDeleted = true;
            Assert.IsNull(ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetScheduleById(deleted.Id));
        }

        [Test]
        public void GetScheduleActiveByDateTest()
        {
            var menu = DishInMenuFactory.CreateFew(6);

            foreach (var dim in menu)
            {
                dim.BeginDate = DateTime.Now.AddDays(-10);
                dim.EndDate = DateTime.Now.AddDays(10);
                dim.Type = "D";
            }

            // Пометить одно блюдо в меню как бесконечно активное.
            menu[0].EndDate = null;

            // Пометить блюдо как активное только сегодня.
            menu[1].Type = "S";
            menu[1].OneDate = DateTime.Now.Date;
            menu[1].BeginDate = menu[1].EndDate = null;

            // Удаленная сущность.
            var deletedEntry = menu[menu.Count - 3];
            deletedEntry.IsDeleted = true;

            // Сделать чтобы некоторые записи меню были вне срока их активности.
            var pastEntry = menu[menu.Count - 2];
            var futureEntry = menu[menu.Count - 1];

            pastEntry.BeginDate = DateTime.MinValue;
            pastEntry.EndDate = DateTime.MinValue.AddDays(500);
            futureEntry.BeginDate = DateTime.Now.AddDays(360);
            futureEntry.EndDate = DateTime.Now.AddDays(370);

            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetScheduleActiveByDate(DateTime.Now, null);
            Assert.AreEqual(3, result.Count);
            foreach (var dim in result)
            {
                var isActiveTodayOnly = dim.OneDate == DateTime.Now.Date;
                var isWithinActivityPeriod = dim.BeginDate <= DateTime.Now && (dim.EndDate == null || dim.EndDate >= DateTime.Now);
                Assert.True(isActiveTodayOnly || isWithinActivityPeriod);
                Assert.False(dim.IsDeleted);
            }

            result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetScheduleActiveByDate(DateTime.Now, menu[0].DishId);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(menu[0].DishId, result[0].DishId);

            result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetScheduleActiveByDate(DateTime.Now, pastEntry.DishId);
            Assert.AreEqual(0, result.Count);
        }
    }
}
