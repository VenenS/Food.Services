using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using Food.Services.Tests.Tools;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.FoodService.Food.DbAccessor;
using Moq;
using NUnit.Framework;
// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace Food.Services.Tests.Controllers.RatingController
{
    [TestFixture]
    public class RatingControllerTests
    {
        private FakeContext _context;
        private Food.Services.Controllers.RatingController _controller;
        private Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor> _accessor;
        private readonly Random _random = new Random();
        private User _user;

        private void SetUp()
        {
            _accessor = new Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor>();
            _context = new FakeContext();
            _controller = new Food.Services.Controllers.RatingController(_context, _accessor.Object, null);
            ContextManager.Set(_context);
            _user = UserFactory.CreateUser();
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()));
            Thread.CurrentPrincipal = new GenericPrincipal(identity, null);
        }


        [Test]
        public void GetAllRatingFromUserTest()
        {
            SetUp();
            var isFilter = false;
            var typeOfObject = 0;
            var ratings = RatingFactory.CreateFew();
            _accessor.Setup(e => e.GetAllRatingFromUser(_user.Id, typeOfObject, isFilter)).Returns(ratings);
            var result = _controller.GetAllRatingFromUser(isFilter);
            var response = TransformResult.GetObject<List<RatingModel>>(result);
            Assert.IsTrue(response.Sum(e => e.Id) == ratings.Sum(e => e.Id));
        }

        [Test]
        public void GetAllRatingToObjectTest()
        {
            SetUp();
            var objectId = _random.Next();
            var typeOfObject = 0;
            var ratings = RatingFactory.CreateFew();
            _accessor.Setup(e => e.GetAllRatingToObject(objectId, typeOfObject)).Returns(ratings);
            var result = _controller.GetAllRatingToObject(objectId, typeOfObject);
            var response = TransformResult.GetObject<List<RatingModel>>(result);
            Assert.IsTrue(response.Sum(e => e.Id) == ratings.Sum(e => e.Id));
        }

        [Test]
        public void GetFinalRateToCafeTest()
        {
            SetUp();
            var cafeId = _random.Next();
            var rateExpected = (double)_random.Next();
            _accessor.Setup(e => e.GetFinalRateToCafe(cafeId)).Returns(rateExpected);
            var result = _controller.GetFinalRateToCafe(cafeId);
            var response = TransformResult.GetPrimitive<double>(result);
            Assert.IsTrue(Math.Abs(response - rateExpected) < 0.1);
        }

        [Test]
        public void GetFinalRateToDishTest()
        {
            SetUp();
            var dishId = _random.Next();
            var rateExpected = (double)_random.Next();
            _accessor.Setup(e => e.GetFinalRateToDish(dishId)).Returns(rateExpected);
            var result = _controller.GetFinalRateToDish(dishId);
            var response = TransformResult.GetPrimitive<double>(result);
            Assert.IsTrue(Math.Abs(response - rateExpected) < 0.1);
        }

        [TestCase(0)]
        [TestCase(6)]
        public void InsertNewRatingTest(int valueOfRating)
        {
            SetUp();
            var objectId = _random.Next();
            var typeOfObject = _random.Next();
            //var valueOfRating = _random.Next();
            //_accessor.Setup(e => e.GetAllRatingToObject(objectId, typeOfObject)).Returns(ratings);
            var result = _controller.InsertNewRating(objectId, typeOfObject, valueOfRating);
            var response = TransformResult.GetPrimitive<long>(result);
            Assert.IsTrue(response == 0);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void InsertNewRatingTest_AccessorCalled(int valueOfRating)
        {
            SetUp();
            var objectId = _random.Next();
            var typeOfObject = _random.Next();
            var rateExpected = (long)_random.Next();
            _accessor.Setup(e => e.InsertNewRating(_user.Id, objectId, typeOfObject, valueOfRating)).Returns(rateExpected);
            var result = _controller.InsertNewRating(objectId, typeOfObject, valueOfRating);
            var response = TransformResult.GetPrimitive<long>(result);
            Assert.IsTrue(response == rateExpected);
        }
    }
}