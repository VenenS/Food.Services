using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using Food.Services.Tests.Tools;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    [SingleThreaded]
    class CafeMenuPatternTests
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
        public void AddCafeMenuPatternTest()
        {
            var entity = CafeMenuPatternFactory.Create(new Cafe());
            //
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.AddCafeMenuPattern(entity);
            //
            Assert.IsTrue(result);
        }

        [Test]
        public void GetCafeMenuPatternByIdTest()
        {
            var cafeMenuPatterns = CafeMenuPatternFactory.CreateFew(new Cafe() { Id = 1 });
            _context.CafeMenuPatterns.AddRange(cafeMenuPatterns);
            var findEntity = cafeMenuPatterns.Last();
            findEntity.IsDeleted = false;
            _context.SaveChanges();
            //
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafeMenuPatternById(findEntity.CafeId, findEntity.Id);
            //
            Assert.NotNull(result);
        }

        [Test]
        public void GetMenuPatternsByCafeIdTest()
        {
            var cafeMenuPatterns = CafeMenuPatternFactory.CreateFew(new Cafe() { Id = 1 });
            cafeMenuPatterns.ForEach(e => e.IsDeleted = false);
            _context.CafeMenuPatterns.AddRange(cafeMenuPatterns);
            _context.SaveChanges();
            //
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetMenuPatternsByCafeId(1);
            //
            Assert.NotNull(result);
            Assert.IsTrue(result.Count == cafeMenuPatterns.Count);
        }

        [Test]
        public void GetMenuByPatternIdTest()
        {
            var lstMenuPatternDish = CafeMenuPatternDishFactory.CreateFew();
            var lstCafeMenuPattern = CafeMenuPatternFactory.CreateFew(new Cafe() { Id = 1 });
            lstCafeMenuPattern.ForEach(e => e.IsDeleted = false);
            _context.CafeMenuPatterns.AddRange(lstCafeMenuPattern);
            var cafeMenuPattern = lstCafeMenuPattern.First();
            cafeMenuPattern.Dishes = lstMenuPatternDish;
            var dishes = DishFactory.CreateFew();
            for (int i = 0; i < cafeMenuPattern.Dishes.Count; i++)
            {
                cafeMenuPattern.Dishes[i].DishId = i;
                cafeMenuPattern.Dishes[i].Dish = dishes[i];
            }
            _context.SaveChanges();
            //
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetMenuByPatternId(cafeMenuPattern.CafeId, cafeMenuPattern.Id);
            //
            Assert.NotNull(result);
            Assert.IsTrue(result.Keys.Count == 3);
        }

        /// <summary>
        /// Удалить шаблон меню вместе с его блюдами
        /// </summary>
        [Test]
        public void RemoveCafeMenuPatternTest()
        {
            var cafeMenuPatterns = CafeMenuPatternFactory.CreateFew(new Cafe() { Id = 1 });
            cafeMenuPatterns.ForEach(e => e.IsDeleted = false);
            _context.CafeMenuPatterns.AddRange(cafeMenuPatterns);
            var firstCafeMPs = cafeMenuPatterns.First();
            var dishes = CafeMenuPatternDishFactory.CreateFew();
            firstCafeMPs.Dishes = dishes;
            _context.SaveChanges();
            //
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RemoveCafeMenuPattern(1, firstCafeMPs.Id);
            //
            Assert.NotNull(result);
            Assert.IsTrue(result);
        }

        [Test]
        public void UpdateCafeMenuPatternTest()
        {
            var cafeMenuPatterns = CafeMenuPatternFactory.CreateFew(new Cafe() { Id = 1 });
            cafeMenuPatterns.ForEach(e => e.IsDeleted = false);
            _context.CafeMenuPatterns.AddRange(cafeMenuPatterns);
            var firstCafeMPs = cafeMenuPatterns.First();
            firstCafeMPs.Dishes = new List<CafeMenuPatternDish>();
            _context.SaveChanges();
            //
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.UpdateCafeMenuPattern(firstCafeMPs);
            //
            Assert.NotNull(result);
            Assert.IsTrue(result);
        }
    }
}