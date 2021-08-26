using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using NUnit.Framework;

namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    class OrderInfoTests
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

        /// <summary>
        /// Успешное добавление OrderInfo
        /// </summary>
        [Test]
        public void PostOrderInfo_Test1()
        {
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.PostOrderInfo(OrderInfoFactory.Create());
            //
            Assert.IsTrue(response);
        }

        /// <summary>
        /// Добавление OrderInfo с ошибкой
        /// </summary>
        [Test]
        public void PostOrderInfo_Test2()
        {
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.PostOrderInfo(null);
            //
            Assert.IsFalse(response);
        }
    }
}
