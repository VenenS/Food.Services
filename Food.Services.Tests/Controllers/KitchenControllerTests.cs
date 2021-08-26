using Food.Data.Entities;
using Food.Services.Controllers;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using Food.Services.Tests.Tools;
using ITWebNet.Food.Core.DataContracts.Manager;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;

namespace Food.Services.Tests.Controllers
{
    [TestFixture]
    public class KitchenControllerTests
    {
        [SetUp]
        public void SetUp()
        {
            _accessor = new Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor>();
            _context = new FakeContext();
            _controller = new KitchenController(_context, _accessor.Object);
            ContextManager.Set(_context);
            _user = UserFactory.CreateUser();
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()));
            Thread.CurrentPrincipal = new GenericPrincipal(identity, null);
        }

        private FakeContext _context;
        private KitchenController _controller;
        private Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor> _accessor;
        private readonly Random _random = new Random();
        private User _user;

        /// Что за наркомания происходит в проверяемом методе? Где реализация???
        [Test]
        public void AddKitchenToCafeTest_Success()
        {
            var cafeId = _random.Next();
            var kitchenId = _random.Next();
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafeId)).Returns(true);
            var response = _controller.AddKitchenToCafe(cafeId, kitchenId);
            var result = TransformResult.GetPrimitive<bool>(response);
            Assert.IsTrue(result);
        }

        [Test]
        public void GetCurrentListOfKitchenToCafeTest()
        {
            var cafe = CafeFactory.Create();
            var kitchens = KitchenInCafeFactory.CreateFew();
            _accessor.Setup(e => e.GetListOfKitchenToCafe(cafe.Id)).Returns(kitchens);
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            var response = _controller.GetCurrentListOfKitchenToCafe(cafe.Id);
            var result = TransformResult.GetObject<KitchenModel[]>(response);
            Assert.IsTrue(result.Sum(e => e.Id) == kitchens.Sum(e => e.KitchenId));
        }

        [Test]
        public void GetFullListOfKitchenTest()
        {
            var kitchens = KitchenFactory.CreateFew();
            _accessor.Setup(e => e.GetListOfKitchen()).Returns(kitchens);
            var response = _controller.GetFullListOfKitchen();
            var result = TransformResult.GetObject<KitchenModel[]>(response);
            Assert.IsTrue(result.Sum(e => e.Id) >= kitchens.Sum(e => e.Id));
        }

        //Опять, что за функционал здесь?? Почему метод фактически ничего не делает?
        [Test]
        public void RemoveKitchenFromCafeTest()
        {
            var cafeId = _random.Next();
            var kitchenId = _random.Next();
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafeId)).Returns(true);
            var response = _controller.RemoveKitchenFromCafe(cafeId, kitchenId);
            var result = TransformResult.GetPrimitive<bool>(response);
            Assert.IsTrue(result);
        }
    }
}