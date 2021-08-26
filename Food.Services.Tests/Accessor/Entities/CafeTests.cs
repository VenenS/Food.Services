using System;
using System.Linq;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using ITWebNet.FoodService.Food.DbAccessor;
using NUnit.Framework;
// ReSharper disable PossibleInvalidOperationException

namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    [SingleThreaded]
    public class CafeTests
    {
        private FakeContext _context;
        private readonly Random _random = new Random();

        private void SetUp()
        {
            _context = new FakeContext();
            ITWebNet.FoodService.Food.DbAccessor.Accessor.SetTestingModeOn(_context);
            ContextManager.Set(_context);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetCafeByIdIgnoreActivityTest(bool isActive)
        {
            SetUp();
            var cafe = CafeFactory.Create();
            cafe.IsActive = isActive;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafeByIdIgnoreActivity(cafe.Id);
            Assert.IsTrue(result == cafe);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetCafeByIdIgnoreActivity_OnlyAlive_Test(bool isActive)
        {
            SetUp();
            var cafe = CafeFactory.Create();
            cafe.IsDeleted = true;
            cafe.IsActive = isActive;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafeByIdIgnoreActivity(cafe.Id);
            Assert.IsTrue(result == null);
        }

        [Test]
        public void AddCafeTest()
        {
            SetUp();
            var cafe = new Cafe {CafeName = Guid.NewGuid().ToString("N")};
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.AddCafe(cafe);
            Assert.IsTrue(result == cafe.Id);
        }

        [Test]
        public void EditCafeTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var changed = new Cafe {CafeName = Guid.NewGuid().ToString("N"), Id = cafe.Id};
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.EditCafe(changed);
            Assert.IsTrue(changed.CafeName == cafe.CafeName);
            Assert.IsTrue(result.Item1 == cafe.Id);
        }

        [Test]
        public void GetCafeById_OnlyActiveTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            cafe.IsActive = false;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafeById(cafe.Id);
            Assert.IsTrue(result == null);
        }

        [Test]
        public void GetCafeById_OnlyAliveTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            cafe.IsDeleted = true;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafeById(cafe.Id);
            Assert.IsTrue(result == null);
        }

        [Test]
        public void GetCafeByIdTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafeById(cafe.Id);
            Assert.IsTrue(result == cafe);
        }

        [Test]
        public void GetCafeByUrlTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            //cafe.IsDeleted = true;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafeByUrl(cafe.CleanUrlName);
            Assert.IsTrue(result == cafe);
        }

        [Test]
        public void GetCafeByUrlTest_OnlyAlive()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            cafe.IsDeleted = true;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafeByUrl(cafe.CleanUrlName);
            Assert.IsTrue(result == null);
        }

        [Test]
        public void GetCafesTest_OnlyActive()
        {
            SetUp();
            var cafeNonActive = CafeFactory.Create();
            cafeNonActive.IsActive = false;
            CafeFactory.CreateFew();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafes();
            Assert.IsNull(result.FirstOrDefault(e => e.IsActive == false));
        }

        [Test]
        public void GetCafesTest_OnlyLive()
        {
            SetUp();
            var cafeDeleted = CafeFactory.Create();
            cafeDeleted.IsDeleted = true;
            CafeFactory.CreateFew();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafes();
            Assert.IsNull(result.FirstOrDefault(e => e.IsDeleted));
        }

        [Test]
        public void GetManagedCafesTest()
        {
            SetUp();
            var manager = CafeManagerFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetManagedCafes(manager.UserId);
            Assert.IsTrue(result.First() == manager.Cafe);
        }

        [Test]
        public void RemoveCafeTest()
        {
            SetUp();
            var userId = _random.Next();
            var cafe = CafeFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RemoveCafe(cafe.Id, userId);
            Assert.IsTrue(result.Item1);
            Assert.IsTrue(cafe.IsDeleted);
            Assert.IsTrue(cafe.LastUpdateBy == userId);
            Assert.IsTrue(cafe.LastUpdateDate.Value.Date == DateTime.Now.Date);
        }

        [Test]
        public void СheckUniqueName_Exited_Fail_Test()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.СheckUniqueName(cafe.CafeName, _random.Next());
            Assert.IsFalse(result);
        }

        [Test]
        public void СheckUniqueName_Exited_Sucess_Test()
        {
            SetUp();
            var cafe = CafeFactory.CreateFew();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.СheckUniqueName(cafe.Last().CafeName, cafe.Last().Id);
            Assert.IsTrue(result);
        }

        [Test]
        public void СheckUniqueName_ForCreation_Fail_Test()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.СheckUniqueName(cafe.CafeName);
            Assert.IsFalse(result);
        }

        [Test]
        public void СheckUniqueName_ForCreation_Sucess_Test()
        {
            SetUp();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.СheckUniqueName(Guid.NewGuid().ToString("N"));
            Assert.IsTrue(result);
        }
    }
}