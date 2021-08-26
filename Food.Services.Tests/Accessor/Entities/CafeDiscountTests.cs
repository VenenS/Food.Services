using System;

using NUnit.Framework;

using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;

namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    public class CafeDiscountTests
    {
        private FakeContext _context;
        private Cafe _cafe;
        private User _user;

        [SetUp]
        public void SetUp()
        {
            _context = new FakeContext();
            ITWebNet.FoodService.Food.DbAccessor.Accessor.SetTestingModeOn(_context);
            ContextManager.Set(_context);

            _user = UserFactory.CreateUser();
            _cafe = CafeFactory.Create(_user);
        }

        [Test()]
        public void GetCafeDiscountValueTest()
        {
            var discount = CafeDiscountFactory.Create(_user, _cafe);
            var deletedDiscount = CafeDiscountFactory.Create(_user, _cafe);
            var expiredDiscount = CafeDiscountFactory.Create(_user, _cafe);

            discount.SummFrom = deletedDiscount.SummFrom = expiredDiscount.SummFrom = 100;
            discount.SummTo = deletedDiscount.SummTo = expiredDiscount.SummTo = 200;

            deletedDiscount.IsDeleted = true;
            expiredDiscount.BeginDate = DateTime.Now.AddDays(-10);
            expiredDiscount.EndDate = DateTime.Now.AddDays(-5);

            {
                var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafeDiscountValue(_cafe.Id, DateTime.Now, 150.0);
                Assert.AreEqual(result, discount);

                // Сумма заказа не входит в нужные рамки для предоставления скидки.
                result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafeDiscountValue(_cafe.Id, DateTime.Now, 99.9);
                Assert.IsNull(result);

                result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafeDiscountValue(_cafe.Id, DateTime.Now, 200.1);
                Assert.IsNull(result);
            }

            // Верхняя граница по сумме не ограничена (discount.SummTo == null).
            {
                var prevValue = discount.SummTo;
                discount.SummTo = null;

                var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafeDiscountValue(_cafe.Id, DateTime.Now, 200.1);
                Assert.IsNotNull(result);
                result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafeDiscountValue(_cafe.Id, DateTime.Now, System.Double.MaxValue / 2);
                Assert.IsNotNull(result);

                discount.SummTo = prevValue;
            }
        }

        /*
        [Test]
        public void AddCafeDiscountTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void EditCafeDiscountTest()
        {
        }

        [Test()]
        public void RemoveCafeDiscountTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void GetCafeDiscountsTest()
        {
            Assert.Fail();
        }
        */
    }
}