using System;
using System.Linq;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using NUnit.Framework;


namespace ITWebNet.FoodService.Food.DbAccessor.Tests
{
    [TestFixture]
    public class DishVersionTests
    {
        [SetUp]
        public void SetUp()
        {
            _context = new FakeContext();
            ContextManager.Set(_context);
            Accessor.SetTestingModeOn(_context);
        }

        private FakeContext _context;

        [Test]
        public void GetFoodDishVersionByCafeIdTest()
        {
            DishVersionFactory.Create();
            var dv = DishVersionFactory.Create();
            var result =
                Accessor.Instance.GetFoodDishVersionByCafeId(dv.CafeCategory.CafeId, DateTime.Now);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First() == dv);
        }

        [Test]
        public void GetFoodDishVersionByCafeIdTest_DateCheck()
        {
            var temp = DishVersionFactory.Create();
            temp.VersionTo = DateTime.MinValue;
            var dv = DishVersionFactory.Create(category: temp.CafeCategory);
            dv.VersionFrom = DateTime.Now.AddDays(-1);
            dv.VersionTo = DateTime.Now.AddDays(1);
            var result =
                Accessor.Instance.GetFoodDishVersionByCafeId(dv.CafeCategory.CafeId, DateTime.Now);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First() == dv);
        }

        [Test]
        public void GetFoodDishVersionByCafeIdTest_Only_Active()
        {
            var temp = DishVersionFactory.Create();
            var dv = DishVersionFactory.Create(category: temp.CafeCategory);
            temp.IsActive = false;
            var result =
                Accessor.Instance.GetFoodDishVersionByCafeId(dv.CafeCategory.CafeId, DateTime.Now);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First() == dv);
        }

        [Test]
        public void GetFoodDishVersionByCafeIdTest_Only_Alive()
        {
            var temp = DishVersionFactory.Create();
            var dv = DishVersionFactory.Create(category: temp.CafeCategory);
            temp.IsDeleted = true;
            var result =
                Accessor.Instance.GetFoodDishVersionByCafeId(dv.CafeCategory.CafeId, DateTime.Now);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First() == dv);
        }

        [Test]
        public void GetFoodDishVersionByCategoryIdAndCafeIdTest()
        {
            DishVersionFactory.Create();
            var dv = DishVersionFactory.Create();
            var result =
                Accessor.Instance.GetFoodDishVersionByCategoryIdAndCafeId(dv.CafeCategory.CafeId, dv.CafeCategoryId,
                    DateTime.Now);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First() == dv);
        }

        [Test]
        public void GetFoodDishVersionByCategoryIdAndCafeIdTest_DateCheck()
        {
            var temp = DishVersionFactory.Create();
            temp.VersionTo = DateTime.MinValue;
            var dv = DishVersionFactory.Create(category: temp.CafeCategory);
            dv.VersionFrom = DateTime.Now.AddDays(-1);
            dv.VersionTo = DateTime.Now.AddDays(1);
            var result =
                Accessor.Instance.GetFoodDishVersionByCategoryIdAndCafeId(dv.CafeCategory.CafeId, dv.CafeCategoryId,
                    DateTime.Now);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First() == dv);
        }

        [Test]
        public void GetFoodDishVersionByCategoryIdAndCafeIdTest_Only_Active()
        {
            var temp = DishVersionFactory.Create();
            var dv = DishVersionFactory.Create(category: temp.CafeCategory);
            temp.IsActive = false;
            var result =
                Accessor.Instance.GetFoodDishVersionByCategoryIdAndCafeId(dv.CafeCategory.CafeId, dv.CafeCategoryId,
                    DateTime.Now);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First() == dv);
        }

        [Test]
        public void GetFoodDishVersionByCategoryIdAndCafeIdTest_Only_Alive()
        {
            var temp = DishVersionFactory.Create();
            var dv = DishVersionFactory.Create(category: temp.CafeCategory);
            temp.IsDeleted = true;
            var result =
                Accessor.Instance.GetFoodDishVersionByCategoryIdAndCafeId(dv.CafeCategory.CafeId, dv.CafeCategoryId,
                    DateTime.Now);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First() == dv);
        }

        [Test]
        public void GetFoodDishVersionByDishIdAndDateTest()
        {
            DishVersionFactory.Create();
            var dv = DishVersionFactory.Create();
            var result =
                Accessor.Instance.GetFoodDishVersionByDishIdAndDate(dv.DishId, DateTime.Now);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First() == dv);
        }

        [Test]
        public void GetFoodDishVersionByDishIdAndDateTest_DateCheck()
        {
            var temp = DishVersionFactory.Create();
            temp.VersionTo = DateTime.MinValue;
            var dv = DishVersionFactory.Create();
            dv.VersionFrom = DateTime.Now.AddDays(-1);
            dv.VersionTo = DateTime.Now.AddDays(1);
            dv.DishId = temp.DishId;
            var result =
                Accessor.Instance.GetFoodDishVersionByDishIdAndDate(dv.DishId, DateTime.Now);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First() == dv);
        }

        [Test]
        public void GetFoodDishVersionByDishIdAndDateTest_Only_Active()
        {
            var temp = DishVersionFactory.Create();
            var dv = DishVersionFactory.Create();
            dv.DishId = temp.DishId;
            temp.IsActive = false;
            var result =
                Accessor.Instance.GetFoodDishVersionByDishIdAndDate(dv.DishId, DateTime.Now);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First() == dv);
        }

        [Test]
        public void GetFoodDishVersionByDishIdAndDateTest_Only_Alive()
        {
            var temp = DishVersionFactory.Create();
            var dv = DishVersionFactory.Create();
            dv.DishId = temp.DishId;
            temp.IsDeleted = true;
            var result =
                Accessor.Instance.GetFoodDishVersionByDishIdAndDate(dv.DishId, DateTime.Now);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First() == dv);
        }
    }
}