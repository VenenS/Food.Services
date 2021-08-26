using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using ITWebNet.Food.Core.DataContracts.Common;
using Moq;
using NUnit.Framework;
using Serilog;
using System;
using System.Security;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;

namespace Food.Services.Tests.Controllers.RatingController
{
    [TestFixture]
    public class RatingControllerExceptions
    {
        private FakeContext _context;
        private Food.Services.Controllers.RatingController _controller;
        private Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor> _accessor;
        private readonly Random _random = new Random();
        private Data.Entities.User _user;
        private Mock<ILogger> _logger;

        private void SetUp()
        {
            _accessor = new Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor>();
            _context = ContextManager.Get();
            _logger = new Mock<ILogger>();
            _controller =
                new Food.Services.Controllers.RatingController(_context, _accessor.Object, _logger.Object);
            _user = UserFactory.CreateUser();
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()));
            Thread.CurrentPrincipal = new GenericPrincipal(identity, null);
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
            _accessor.Setup(e => e.InsertNewRating(_user.Id, objectId, typeOfObject, valueOfRating))
                .Throws(new SecurityException());
            Assert.Catch<FaultException<SecurityFault>>(() =>
                _controller.InsertNewRating(objectId, typeOfObject, valueOfRating));
        }


        [Test]
        public void GetAllRatingFromUserTest()
        {
            SetUp();
            var isFilter = false;
            var typeOfObject = 0;
            _accessor.Setup(e => e.GetAllRatingFromUser(_user.Id, typeOfObject, isFilter))
                .Throws(new SecurityException());
            Assert.Catch<FaultException<SecurityFault>>(() => _controller.GetAllRatingFromUser(isFilter));
        }

        [Test]
        public void GetAllRatingToObjectTest()
        {
            SetUp();
            var objectId = _random.Next();
            var typeOfObject = 0;
            RatingFactory.CreateFew();
            _accessor.Setup(e => e.GetAllRatingToObject(objectId, typeOfObject)).Throws(new SecurityException());
            Assert.Catch<FaultException<SecurityFault>>(() => _controller.GetAllRatingToObject(objectId, typeOfObject));
        }

        [Test]
        public void GetFinalRateToCafeTest()
        {
            SetUp();
            var cafeId = _random.Next();
            _accessor.Setup(e => e.GetFinalRateToCafe(cafeId)).Throws(new SecurityException());
            Assert.Catch<FaultException<SecurityFault>>(() => _controller.GetFinalRateToCafe(cafeId));
        }

        [Test]
        public void GetFinalRateToDishTest()
        {
            SetUp();
            var dishId = _random.Next();
            _accessor.Setup(e => e.GetFinalRateToDish(dishId)).Throws(new SecurityException());
            Assert.Catch<FaultException<SecurityFault>>(() => _controller.GetFinalRateToDish(dishId));
        }
    }
}