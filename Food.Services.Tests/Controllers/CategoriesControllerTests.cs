using Food.Data.Entities;
using Food.Services.Controllers;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using Food.Services.Tests.Tools;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Security.Claims;

namespace AccessorTests.Entites
{
    [TestFixture()]
    public class CategoriesControllerTests
    {
        private FakeContext _context;
        private CategoriesController _controller;
        private Mock<Accessor> _accessor;
        private User _user;
        private Random _rnd = new Random();

        private void SetUp()
        {
            _accessor = new Mock<Accessor>();
            _context = new FakeContext();
            ContextManager.Set(_context);
            _controller = new CategoriesController(_context, _accessor.Object);
            _user = UserFactory.CreateUser();
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        [Test()]
        public void AddCafeFoodCategory_Success()
        {
            SetUp();
            var cafeId = _rnd.Next();
            var categoryId = _rnd.Next();
            var categoryIndex = _rnd.Next();
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafeId)).Returns(true);
            _accessor.Setup(e => e.AddCafeFoodCategory(cafeId, categoryId, categoryIndex, _user.Id)).Returns(1);
            var responce = _controller.AddCafeFoodCategory(cafeId, categoryId, categoryIndex);
            var result = TransformResult.GetPrimitive<long>(responce);
            Assert.IsTrue(result == 1);
        }

        [Test()]
        public void AddCafeFoodCategory_Not_Manager()
        {
            SetUp();
            var cafeId = _rnd.Next();
            var categoryId = _rnd.Next();
            var categoryIndex = _rnd.Next();
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafeId)).Returns(false);
            var result = _controller.AddCafeFoodCategory(cafeId, categoryId, categoryIndex);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test()]
        public void ChangeFoodCategoryOrder_Success()
        {
            SetUp();
            var cafeId = _rnd.Next();
            var categoryId = _rnd.Next();
            var categoryIndex = _rnd.Next();
            _accessor.Setup(e => e.ChangeFoodCategoryOrder(cafeId, categoryId, categoryIndex, _user.Id));
            var response = _controller.ChangeFoodCategoryOrder(cafeId, categoryId, categoryIndex);
            Assert.IsInstanceOf<OkResult>(response);
            _accessor.Verify(e => e.ChangeFoodCategoryOrder(cafeId, categoryId, categoryIndex, _user.Id), Times.Once);
        }

        [Test()]
        public void GetTest()
        {

        }

        [Test()]
        public void GetTest1()
        {

        }

        [Test()]
        public void PostTest()
        {

        }

        [Test()]
        public void PutTest()
        {

        }

        [Test()]
        public void DeleteTest()
        {

        }

        [Test()]
        public void RemoveCafeFoodCategoryTest()
        {

        }
    }
}