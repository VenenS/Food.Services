using Food.Data.Entities;
using Food.Services.Controllers;
using Food.Services.ExceptionHandling;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using Food.Services.Tests.Tools;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using ITWebNet.FoodService.Food.DbAccessor;
using Microsoft.AspNetCore.Mvc;
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
    class CafesControllerTestsExceptions
    {
        private FakeContext _context;
        private CafesController _controller;
        private Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor> _accessor;
        private User _user;

        private void SetUp()
        {
            _accessor = new Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor>();
            _context = new FakeContext();
            ContextManager.Set(_context);
            _controller = new CafesController(_context, _accessor.Object);
            _user = UserFactory.CreateUser();
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()));
            Thread.CurrentPrincipal = new GenericPrincipal(identity, null);
        }
        [Test]
        public void AddNewCostOfDeliveryTestExNoManager()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var costOfDelivery = CostOfDeliveryFactory.CreateModel(cafe);
            Assert.Catch<FaultException<SecurityFault>>(() => _controller.AddNewCostOfDelivery(costOfDelivery));
        }
        [Test]
        public void GetCostOfDeliveryNull()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var price = 5;
            var response = _controller.GetCostOfDelivery(cafe.Id, price);
            var result = TransformResult.GetObject<OrderDeliveryPriceModel>(response.Result);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.TotalPrice);
        }
        [Test]
        public void UpdateCafeTestExCafeName()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var cafe1 = CafeFactory.Create();
            cafe1.CafeFullName = cafe.CafeFullName;
            var cafeModel = CafeFactory.CreateModel(cafe);
            var responce = _controller.UpdateCafe(cafeModel);
            var result = TransformResult.GetObject<CafeModel>(responce);
            Assert.Null(result);
        }
        [Test]
        public void CreateCafeTestExBad()
        {
            SetUp();
            var cafeModel = CafeFactory.CreateModel();
            var responce = _controller.CreateCafe(cafeModel);
            var result = TransformResult.GetObject<CafeModel>(responce.Result);
            Assert.Null(result);
        }
        [Test]
        public void CreateCafeTestEx()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var cafeModel = CafeFactory.CreateModel(cafe);
            var responce = _controller.CreateCafe(cafeModel);
            var result = TransformResult.GetObject<CafeModel>(responce.Result);
            Assert.Null(result);
        }
        [Test]
        public void СheckUniqueNameExName()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var responce = _controller.СheckUniqueName(cafe.CafeName);
            var result = TransformResult.GetPrimitive<bool>(responce.Result);
            Assert.IsFalse(result);
        }
        [Test]
        public void СheckUniqueNameExid()
        {
            SetUp();
            var tmp = CafeFactory.Create();
            var cafe = new Cafe()
            {
                IsActive = true,
                IsDeleted = false,
                Id = 1,
                CafeName = tmp.CafeName
            };
            ContextManager.Get().Cafes.Add(cafe);
            var responce = _controller.СheckUniqueName(cafe.CafeName, cafe.Id);
            var result = TransformResult.GetPrimitive<bool>(responce.Result);
            Assert.IsFalse(result);
        }
        [Test]
        public void AddNewCafeNotificationContactTestEx()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var notify = new CafeNotificationContactModel()
            {
                CafeId = cafe.Id,
                NotificationContact = Guid.NewGuid().ToString("N")
            };
            Assert.CatchAsync<FaultException<SecurityFault>>(() => _controller.AddNewCafeNotificationContact(notify));
        }
        [Test]
        public void PostCafeManagerTestExRole()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var model = CafeManagerFactory.CreateModel(cafe, _user.Id);
            var responce = _controller.PostCafeManager(model);
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), responce.Result);
        }
        [Test]
        public void DeleteCafeManagerTestExRole()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var role = RoleFactory.CreateRole(EnumUserRole.Manager);
            var responce = _controller.DeleteCafeManager(cafe.Id, _user.Id);
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), responce.Result);
        }
        [Test]
        public void DeleteCafeManagerTestExManager()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var role = RoleFactory.CreateRole(EnumUserRole.Manager);
            var responce = _controller.DeleteCafeManager(cafe.Id, _user.Id);
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), responce.Result);
        }
        [Test]
        public void AddCafeMenuPatternTestEx()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var pattern = CafeMenuPatternFactory.CreateModel(cafe);
            var responce = _controller.AddCafeMenuPattern(pattern);
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), responce.Result);
        }
        [Test]
        public void GetMenuByPatternIdTestExNull()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var responce = _controller.GetMenuByPatternId(cafe.Id, 5);
            Assert.IsInstanceOf(typeof(BadRequestResult), responce.Result);
        }
        [Test]
        public void GetMenuByPatternIdTestNoDish()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var pattern = CafeMenuPatternFactory.Create(cafe);
            var responce = _controller.GetMenuByPatternId(cafe.Id, pattern.Id);
            Assert.IsInstanceOf(typeof(InternalServerError), responce.Result);
        }
        [Test]
        public void GetMenuFilteredMenuByPatternIdTestEx()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var dish = DishFactory.Create(_user);
            var pattern = CafeMenuPatternFactory.Create(cafe);
            var responce = _controller.GetMenuFilteredMenuByPatternId(cafe.Id, dish.Id, "");
            Assert.IsInstanceOf(typeof(InternalServerError), responce.Result);
        }
        [Test]
        public void GetMenuFilteredMenuByPatternIdTestExNull()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var responce = _controller.GetMenuFilteredMenuByPatternId(cafe.Id, 0, Guid.NewGuid().ToString("n"));
            Assert.IsInstanceOf(typeof(BadRequestResult), responce.Result);
        }
        [Test]
        public void GetMenuFilteredMenuByPatternIdTestNull()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var pattern = CafeMenuPatternFactory.CreateWithDishes(cafe);
            var dish = pattern.Dishes[0];
            var responce = _controller.GetMenuFilteredMenuByPatternId(cafe.Id, 0, Guid.NewGuid().ToString("n"));
            var result = TransformResult.GetObject<IEnumerable<CafeMenuModel>>(responce.Result);
            Assert.IsEmpty(result);
        }

        [Test]
        public void RemoveCafeMenuPatternTestExNotManager()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var pattern = CafeMenuPatternFactory.Create(cafe);
            var responce = _controller.RemoveCafeMenuPattern(cafe.Id, pattern.Id);
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), responce.Result);
        }

        [Test]
        public void RemoveCafeMenuPatternTestExNoPattern()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var responce = _controller.RemoveCafeMenuPattern(cafe.Id, 0);
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), responce.Result);
        }
        [Test]
        public void RemoveCafeMenuPatternTestExBankets()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var pattern = CafeMenuPatternFactory.CreateWithBankets(cafe, DateTime.Now.AddDays(2));
            pattern.IsBanket = true;
            pattern.PatternDate = DateTime.Today;
            CafeManagerFactory.Create(_user, cafe);
            var responce = _controller.RemoveCafeMenuPattern(cafe.Id, 0);
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), responce.Result);
        }
        [Test]
        public void RemoveCafeMenuPatternTestEx()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var pattern = CafeMenuPatternFactory.Create(cafe);
            pattern.IsBanket = true;
            pattern.PatternDate = DateTime.Today;
            CafeManagerFactory.Create(_user, cafe);
            var responce = _controller.RemoveCafeMenuPattern(cafe.Id, 0);
            Assert.IsInstanceOf(typeof(InternalServerError), responce.Result);
        }
        [Test]
        public void UpdateCafeMenuPatternTestExManager()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var pattern = CafeMenuPatternFactory.CreateModel(cafe);
            var responce = _controller.UpdateCafeMenuPattern(pattern);
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), responce.Result);
        }
        [Test]
        public void UpdateCafeMenuPatternTestExNoPattern()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var pattern = CafeMenuPatternFactory.CreateModel(cafe);
            CafeManagerFactory.Create(_user, cafe);
            var responce = _controller.UpdateCafeMenuPattern(pattern);
            Assert.IsInstanceOf(typeof(BadRequestResult), responce.Result);
        }
        [Test]
        public void UpdateCafeMenuPatternTestExBanket()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var pattern1 = CafeMenuPatternFactory.CreateWithBankets(cafe, DateTime.Today.AddDays(1));
            var patternModel1 = CafeMenuPatternFactory.CreateModel(cafe);
            patternModel1.IsBanket = true;
            patternModel1.PatternToDate = DateTime.Today.AddDays(1);
            var pattern = CafeMenuPatternFactory.CreateWithBankets(cafe, DateTime.Today.AddDays(1));
            var patternModel = CafeMenuPatternFactory.CreateModel(cafe);
            patternModel.IsBanket = false;
            patternModel.PatternToDate = DateTime.Today.AddDays(1);
            patternModel.Name = pattern.Name;
            pattern1.Name = pattern.Name;
            var responce = _controller.UpdateCafeMenuPattern(patternModel);
            Assert.IsInstanceOf(typeof(BadRequestObjectResult), responce.Result);
        }
        [Test]
        public void UpdateCafeMenuPatternTestEx()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var pattern = CafeMenuPatternFactory.Create(cafe);
            var patternModel = CafeMenuPatternFactory.CreateModel(cafe);
            patternModel.IsBanket = false;
            patternModel.PatternToDate = DateTime.Today.AddDays(1);
            patternModel.Name = pattern.Name;

            var responce = _controller.UpdateCafeMenuPattern(patternModel);
            Assert.IsInstanceOf(typeof(InternalServerError), responce.Result);
        }
        [Test]
        public void EditCostOfDeliveryTestExNull()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var costOfDelivery = CostOfDeliveryFactory.Create(cafe);
            var responce = _controller.EditCostOfDelivery(new CostOfDeliveryModel() { Id = 50, OrderPriceFrom = 10, OrderPriceTo = 100, DeliveryPrice = 10 });
            Assert.IsInstanceOf(typeof(AggregateException), responce.Exception);
        }
        [Test]
        public void EditCostOfDeliveryTestExManager()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var costOfDelivery = CostOfDeliveryFactory.Create(cafe);
            var responce = _controller.EditCostOfDelivery(new CostOfDeliveryModel() { Id = costOfDelivery.Id, OrderPriceFrom = 10, OrderPriceTo = 100, DeliveryPrice = 10 });
            Assert.IsInstanceOf(typeof(AggregateException), responce.Exception);
        }
        [Test]
        public void RemoveCostOfDeliveryTestExNull()
        {
            SetUp();
            var responce = _controller.RemoveCostOfDelivery(10);
            Assert.IsInstanceOf(typeof(AggregateException), responce.Exception);
        }
        [Test]
        public void RemoveCostOfDeliveryTestExManager()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var costOfDelivery = CostOfDeliveryFactory.Create(cafe);
            var responce = _controller.RemoveCostOfDelivery(costOfDelivery.Id);
            Assert.IsInstanceOf(typeof(AggregateException), responce.Exception);
        }
        [Test]
        public void AddUserCafeLinkTestExCafe()
        {
            SetUp();
            var responce = _controller.AddUserCafeLink(50, _user.Id);
            Assert.IsInstanceOf(typeof(Exception), responce.Exception);
        }
        [Test]
        public void AddUserCafeLinkTestExCafeManager()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var responce = _controller.AddUserCafeLink(cafe.Id, _user.Id);
            Assert.IsInstanceOf(typeof(Exception), responce.Exception);
        }
        [Test]
        public void AddUserCafeLinkTestExCafeUser()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var responce = _controller.AddUserCafeLink(cafe.Id, 50);
            Assert.IsInstanceOf(typeof(Exception), responce.Exception);
        }

        [Test]
        public void EditUserCafeLinkTestExCafe()
        {
            SetUp();
            var responce = _controller.EditUserCafeLink(10, _user.Id);
            Assert.IsInstanceOf(typeof(Exception), responce.Exception);
        }
        [Test]
        public void EditUserCafeLinkTestExUser()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var responce = _controller.EditUserCafeLink(cafe.Id, 50);
            Assert.IsInstanceOf(typeof(Exception), responce.Exception);
        }
        [Test]
        public void EditUserCafeLinkTestExManager()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var responce = _controller.EditUserCafeLink(cafe.Id, _user.Id);
            Assert.IsInstanceOf(typeof(AggregateException), responce.Exception);
        }
        [Test]
        public void GetCafeBusinessHoursTestExManager()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var responce = _controller.GetCafeBusinessHours(cafe.Id);
            Assert.IsInstanceOf(typeof(AggregateException), responce.Exception);
        }
        [Test]
        public void SetCafeBusinessHoursTestExManager()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var hours = CafeFactory.CreateBusinessHoursModel(cafe);
            var responce = _controller.SetCafeBusinessHours(hours);
            Assert.IsInstanceOf(typeof(AggregateException), responce.Exception);
        }
        [Test]
        public void GetCafeByIdIgnoreActivityTestExCafe()
        {
            SetUp();
            var responce = _controller.GetCafeByIdIgnoreActivity(50);
            var result = TransformResult.GetObject<CafeModel>(responce.Result);
            Assert.IsNull(result);

        }
        [Test]
        public void GetCafeInfoTestExCafeManager()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            Assert.Catch(typeof(FaultException<SecurityFault>), () => _controller.GetCafeInfo(cafe.Id));
        }
        [Test]
        public void SetCafeInfoTestExManager()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var info = CafeFactory.CreateCafeInfo(cafe);
            Assert.CatchAsync(async () => { await _controller.SetCafeInfo(info); });
        }
        [Test]
        public void GetCafeNotificationContactByIdTestEx0()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var responce = _controller.GetCafeNotificationContactById(0);
            var result = TransformResult.GetObject<CafeNotificationContactModel>(responce.Result);
            Assert.IsNull(result);
        }
        [Test]
        public void RemoveCafeNotificationContactTestExNoContacts()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var responce = _controller.RemoveCafeNotificationContact(2);
            Assert.IsInstanceOf(typeof(AggregateException), responce.Exception);
        }
        [Test]
        public void RemoveCafeNotificationContactTestExManager()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var info = CafeFactory.CreateCafeInfo(cafe);
            ContextManager.Get().CafeNotificationContact.Add(new CafeNotificationContact()
            {
                CafeId = cafe.Id,
                NotificationContact = Guid.NewGuid().ToString("n") + "@" + Guid.NewGuid().ToString("n") + ".cru",
                NotificationChannelId = (short)NotificationChannelEnum.Email,
                Id = 0
            });
            var responce = _controller.RemoveCafeNotificationContact(0);
            Assert.IsInstanceOf(typeof(AggregateException), responce.Exception);
        }
        [Test]
        public void UpdateCafeNotificationContactExNull()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var notify = CafeNotificationFactory.CreateModel(cafe);
            var responce = _controller.UpdateCafeNotificationContact(notify);
            Assert.IsInstanceOf(typeof(AggregateException), responce.Exception);
        }
        [Test]
        public void UpdateCafeNotificationContactExManager()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var notify = CafeNotificationFactory.CreateNotificationContact(cafe);
            var notifyModel = CafeNotificationFactory.CreateModel(notify);
            var responce = _controller.UpdateCafeNotificationContact(notifyModel);
            Assert.IsInstanceOf(typeof(AggregateException), responce.Exception);
        }

    }
}
