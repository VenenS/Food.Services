using System;
using System.Linq;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using ITWebNet.FoodService.Food.DbAccessor;
using NUnit.Framework;

namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    [SingleThreaded]
    public class BanketTests
    {
        private FakeContext _context;
        private readonly Random _random = new Random();
        private User _user;

        [SetUp]
        public void SetUp()
        {
            _context = new FakeContext();
            ContextManager.Set(_context);
            ITWebNet.FoodService.Food.DbAccessor.Accessor.SetTestingModeOn(_context);
            _user = UserFactory.CreateUser();
        }

        [Test]
        public void GetOrdersInBanketTest()
        {
            var banket = BanketFactory.Create();
            var orders = OrderFactory.CreateFew(3, _user, banket, banket.Cafe);
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.ListOrdersInBanket(banket.Id);
            Assert.IsTrue(result.Sum(e => e.Id) == orders.Sum(e => e.Id));
        }

        [Test]
        public void GetBanketByIdTest()
        {
            var banket = BanketFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetBanketById(banket.Id);
            Assert.IsTrue(result == banket);
        }

        [Test]
        public void GetBanketById_AliveOnly_Test()
        {
            var banket = BanketFactory.Create();
            banket.IsDeleted = true;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetBanketById(banket.Id);
            Assert.IsNull(result);
        }
    }
}