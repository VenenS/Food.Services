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
    public class DiscountTests
    {
        [SetUp]
        public void SetUp()
        {
            _context = new FakeContext();
            ContextManager.Set(_context);
            ITWebNet.FoodService.Food.DbAccessor.Accessor.SetTestingModeOn(_context);
        }

        private FakeContext _context;
        private readonly Random _rnd = new Random();

        [Test]
        public void GetDiscountValueTest_No_User_And_Company_Ids()
        {
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetDiscountValue(_rnd.Next(), DateTime.Now);
            Assert.IsTrue(Math.Abs(result) < 0.01);
        }

        [Test]
        public void GetDiscountValueTest_Only_Alive()
        {
            var discount = DiscountFactory.Create();
            discount.IsDeleted = true;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetDiscountValue(discount.CafeId, DateTime.Now);
            Assert.IsTrue(Math.Abs(result) < 0.01);
        }

        [Test]
        public void GetDiscountValueTest_Success()
        {
            var discount = DiscountFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetDiscountValue(discount.CafeId, DateTime.Now, discount.UserId, discount.CompanyId);
            Assert.IsTrue(Math.Abs(result - discount.Value) < 0.01);
        }

        [Test]
        public void GetDiscountValueTest_Wrong_UserId()
        {
            var discount = DiscountFactory.Create();
            var userId = _rnd.Next();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetDiscountValue(discount.CafeId, DateTime.Now, userId);
            Assert.IsTrue(Math.Abs(result) < 0.01);
        }

        [Test]
        public void GetDiscountValueTest_No_Company_Success()
        {
            var discount = DiscountFactory.Create();
            discount.CompanyId = null;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetDiscountValue(discount.CafeId, DateTime.Now, discount.UserId, discount.CompanyId);
            Assert.IsTrue(Math.Abs(result - discount.Value) < 0.01);
        }

        [Test]
        public void GetDiscountValueTest_No_User_Success()
        {
            var discount = DiscountFactory.Create();
            discount.UserId = null;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetDiscountValue(discount.CafeId, DateTime.Now, discount.UserId, discount.CompanyId);
            Assert.IsTrue(Math.Abs(result - discount.Value) < 0.01);
        }

        [Test]
        public void GetDiscountValueTest_Wrong_CompanyId()
        {
            var discount = DiscountFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetDiscountValue(discount.CafeId, DateTime.Now, companyId: _rnd.Next());
            Assert.IsTrue(Math.Abs(result) < 0.01);
        }

        [Test]
        public void GetDiscountValueTest_WrongBeginDate()
        {
            var discount = DiscountFactory.Create();
            discount.BeginDate = DateTime.MaxValue;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetDiscountValue(discount.CafeId, DateTime.Now, discount.UserId, discount.CompanyId);
            Assert.IsTrue(Math.Abs(result) < 0.01);
        }

        [Test]
        public void GetDiscountValueTest_WrongEndDate()
        {
            var discount = DiscountFactory.Create();
            discount.EndDate = DateTime.MinValue;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetDiscountValue(discount.CafeId, DateTime.Now, discount.UserId, discount.CompanyId);
            Assert.IsTrue(Math.Abs(result) < 0.01);
        }

        [Test]
        public void GetDiscountValueTest_No_EndDate_Success()
        {
            var discount = DiscountFactory.Create();
            discount.EndDate = null;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetDiscountValue(discount.CafeId, DateTime.Now, discount.UserId, discount.CompanyId);
            Assert.IsTrue(Math.Abs(result - discount.Value) < 0.01);
        }

        [Test]
        public void GetDiscountsTest()
        {
            var discounts = DiscountFactory.CreateFew();
            var idsToFind = discounts.Take(2).Select(e => e.Id).ToArray();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetDiscounts(idsToFind);
            Assert.IsTrue(result.Sum(e => e.Id) == idsToFind.Sum());
        }

        [Test]
        public void AddDiscountTest()
        {
            var discount = new Discount() { UserId = _rnd.Next(), CafeId = _rnd.Next()};
            ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.AddDiscount(discount);
            Assert.IsNotNull(ContextManager.Get().Discounts.Where(e => e.UserId == discount.UserId).SingleOrDefault(e => e.CafeId == discount.CafeId));
        }

        [Test]
        public void EditDiscountTest_Update_Ids_Success()
        {
            var discount = DiscountFactory.Create();
            var model = new Discount
            {
                Id = discount.Id,
                CafeId = _rnd.Next(),
                CompanyId = _rnd.Next(),
                UserId = _rnd.Next()
            };
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.EditDiscount(model);
            Assert.IsTrue(result);
            Assert.IsTrue(model.CafeId == discount.CafeId);
            Assert.IsTrue(model.CompanyId == discount.CompanyId);
            Assert.IsTrue(model.UserId == discount.UserId);
        }

        [Test]
        public void EditDiscountTest_Update_Dates_And_Value_Success()
        {
            var discount = DiscountFactory.Create();
            var model = new Discount
            {
                Id = discount.Id,
                BeginDate = DateTime.Now.AddDays(_rnd.Next(5, 100)),
                EndDate = DateTime.Now.AddDays(_rnd.Next(5, 100)),
                Value = _rnd.Next()
            };
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.EditDiscount(model);
            Assert.IsTrue(result);
            Assert.IsTrue(model.BeginDate.Date == discount.BeginDate.Date);
            Assert.IsTrue(model.EndDate.Value.Date == discount.EndDate.Value.Date);
            Assert.IsTrue(Math.Abs(model.Value - discount.Value) < 0.01);
        }

        [Test]
        public void EditDiscountTest_Update_Log_Information_Success()
        {
            var discount = DiscountFactory.Create();
            var model = new Discount
            {
                Id = discount.Id,
                LastUpdateByUserId = _rnd.Next()
            };
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.EditDiscount(model);
            Assert.IsTrue(result);
            Assert.IsTrue(discount.LastUpdDate.Value.Date == DateTime.Now.Date);
            Assert.IsTrue(model.LastUpdateByUserId == discount.LastUpdateByUserId);
        }

        [Test]
        public void EditDiscountTest_No_Such_Discount()
        {
            var discount = new Discount() {Id = _rnd.Next(3000, 3000000)};
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.EditDiscount(discount);
            Assert.IsFalse(result);
        }

        [Test]
        public void RemoveDiscountTest()
        {
            var admin = UserFactory.CreateUser();
            var discount = DiscountFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RemoveDiscount(discount.Id, admin.Id);
            Assert.IsTrue(result);
            Assert.IsTrue(discount.IsDeleted);
            Assert.IsTrue(discount.LastUpdateByUserId == admin.Id);
            Assert.IsTrue(discount.LastUpdDate.Value.Date == DateTime.Now.Date);
        }

        [Test]
        public void RemoveDiscountTest_No_Such_Discount()
        {
            var admin = UserFactory.CreateUser();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RemoveDiscount(_rnd.Next(), admin.Id);
            Assert.IsFalse(result);
        }

        [Test]
        public void RemoveDiscountTest_Already_Deleted()
        {
            var admin = UserFactory.CreateUser();
            var discount = DiscountFactory.Create();
            discount.IsDeleted = true;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RemoveDiscount(discount.Id, admin.Id);
            Assert.IsFalse(result);
        }

        [Test]
        public void GetUserDiscountsTest()
        {
            var user = UserFactory.CreateUser();
            var discounts = DiscountFactory.CreateFew(user:user);
            DiscountFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetUserDiscounts(user.Id);
            Assert.IsTrue(result.Sum(e => e.Id) == discounts.Sum(e => e.Id));
        }

        [Test]
        public void GetUserDiscountsTest_No_Such_User()
        {
            DiscountFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetUserDiscounts(_rnd.Next());
            Assert.IsTrue(!result.Any());
        }

        [Test]
        public void GetUserDiscountsTest_Only_Alive()
        {
            var user = UserFactory.CreateUser();
            var discount = DiscountFactory.Create(user);
            discount.IsDeleted = true;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetUserDiscounts(user.Id);
            Assert.IsTrue(!result.Any());
        }
    }
}