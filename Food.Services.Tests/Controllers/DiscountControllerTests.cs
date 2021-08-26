using Food.Data.Entities;
using Food.Services.Controllers;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using Food.Services.Tests.Tools;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;

namespace Food.Services.Tests.Controllers
{
    [TestFixture]
    class DiscountControllerTests
    {
        private FakeContext _context;
        private DiscountController _controller;
        private Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor> _accessor;
        private User _user;
        private readonly Random _random = new Random();

        private void SetUp()
        {
            _accessor = new Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor>();
            _context = new FakeContext();
            ContextManager.Set(_context);
            _controller = new DiscountController(_context, _accessor.Object);
            _user = UserFactory.CreateUser();
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()));
            Thread.CurrentPrincipal = new GenericPrincipal(identity, null);
        }

        [Test()]
        public void CreateDiscount_Exception_EmptyDiscount()
        {
            SetUp();
            var discount = new DiscountModel();
            discount = null;
            Assert.Catch<FaultException<DiscountFault>>(() => _controller.CreateDiscount(discount));
        }
        [Test()]
        public void CreateDiscountTest()
        {
            SetUp();
            var discount = DiscountFactory.CreateModel();
            long addedDiscountId = _random.Next();
            _accessor.Setup(e => e.AddDiscount(It.IsAny<Discount>())).Returns(addedDiscountId);
            var responce = _controller.CreateDiscount(discount);
            var result = TransformResult.GetPrimitive<long>(responce);
            Assert.IsTrue(result == addedDiscountId);
        }
        [Test()]
        public void UpdateDiscount_Exception_EmptyDiscount()
        {
            SetUp();
            var discount = new DiscountModel();
            discount = null;
            Assert.Catch<FaultException<DiscountFault>>(() => _controller.UpdateDiscount(discount));
        }
        [Test()]
        public void UpdateDiscountTest()
        {
            SetUp();
            var discount = DiscountFactory.CreateModel();
            _accessor.Setup(e => e.EditDiscount(It.IsAny<Discount>())).Returns(true);
            var responce = _controller.UpdateDiscount(discount);
            var result = TransformResult.GetPrimitive<bool>(responce);
            Assert.IsTrue(result);
        }
        [Test()]
        public void GetDiscountAmountTest()
        {
            SetUp();
            var cafe = CafeFactory.Create(_user);
            DateTime date = DateTime.Now;
            long companyId = 0;
            double returnDiscount = 1.3;
            _accessor.Setup(e => e.GetDiscountValue(cafe.Id, date, _user.Id, companyId)).Returns(returnDiscount);
            var responce = _controller.GetDiscountAmount(cafe.Id, date, companyId);
            var result = TransformResult.GetPrimitive<int>(responce);
            Assert.IsTrue(result == 1);
        }
        [Test()]
        public void GetDiscountsTests()
        {
            SetUp();
            var cafe = CafeFactory.CreateFew();
            long[] discountIdList = { 0, 1, 2 };
            List<Discount> returnDiscountList = new List<Discount>();
            for (var i = 0; i < 3; i++)
            {
                returnDiscountList.Add(DiscountFactory.Create(_user, cafe[i]));
            }
            _accessor.Setup(e => e.GetDiscounts(discountIdList)).Returns(returnDiscountList);
            var responce = _controller.GetDiscounts(discountIdList);
            var result = TransformResult.GetObject<List<DiscountModel>>(responce);
            Assert.IsTrue(result.Count == 3);
        }

        [Test()]
        public void GetUserDiscountsTest()
        {
            SetUp();
            var cafe = CafeFactory.CreateFew();
            List<Discount> returnDiscountList = new List<Discount>();
            for (var i = 0; i < 3; i++)
            {
                returnDiscountList.Add(DiscountFactory.Create(_user, cafe[i]));
            }
            _accessor.Setup(e => e.GetUserDiscounts(_user.Id)).Returns(returnDiscountList);
            var responce = _controller.GetUserDiscounts(_user.Id);
            var result = TransformResult.GetObject<List<DiscountModel>>(responce);
            Assert.IsTrue(result.Count == 3);
        }

        [Test]
        public void DeleteDiscountTest()
        {
            SetUp();
            var cafe = CafeFactory.Create(_user);
            Discount discount = DiscountFactory.Create(_user, cafe);

            _accessor.Setup(x => x.GetUserById(_user.Id)).Returns(_user);

            // Discount does not exist in DB.
            _accessor.Setup(x => x.RemoveDiscount(It.IsAny<long>(), It.IsAny<long>()))
                .Returns(false);
            _accessor.Setup(x => x.GetDiscounts(new long[] { 42 }))
                .Returns(new List<Discount> { });
            Assert.Catch(() => { _controller.DeleteDiscount(42); });

            // Discount exists and should be successfully deleted.
            _accessor.Setup(x => x.RemoveDiscount(It.IsAny<long>(), It.IsAny<long>()))
                .Returns(true);
            _accessor.Setup(x => x.GetDiscounts(new long[] { discount.Id }))
                .Returns(new List<Discount> { discount });
            Assert.IsTrue(TransformResult.GetPrimitive<bool>(_controller.DeleteDiscount(discount.Id)));
        }
    }
}
