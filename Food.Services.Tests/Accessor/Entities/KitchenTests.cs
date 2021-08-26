using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using NUnit.Framework;

namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    [SingleThreaded]
    class KitchenTests
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
        public void GetListOfKitchen()
        {
            var lstKitchens = KitchenFactory.CreateFew(count: 3);
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetListOfKitchen();
            //
            Assert.IsTrue(lstKitchens.Count == response.Count);
        }

        [Test]
        public void GetListOfKitchenToCafe()
        {
            var cafe = CafeFactory.Create();
            var lstKitchens = KitchenInCafeFactory.CreateFew(count: 3, cafe: cafe);
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetListOfKitchenToCafe(cafe.Id);
            //
            Assert.IsTrue(lstKitchens.Count == response.Count);
        }
    }
}
