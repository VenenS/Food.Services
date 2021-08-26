using Food.Data.Entities;
using Food.Services.Controllers;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using Food.Services.Tests.Tools;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;

namespace Food.Services.Tests.Controllers
{

    [TestFixture]
    class CafesControllerTests
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
        public void GetTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var cafe2 = CafeFactory.Create();
            cafe2.IsDeleted = true;
            var cafe3 = CafeFactory.Create();
            cafe3.IsActive = false;
            var responce = _controller.Get();
            var result = TransformResult.GetObject<IEnumerable<CafeModel>>(responce).ToList();
            Assert.IsNotNull(result.FirstOrDefault(e => e.Id == cafe.Id));
        }
        [Test]
        public void GetAllTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var cafe2 = CafeFactory.Create();
            cafe2.IsDeleted = true;
            var cafe3 = CafeFactory.Create();
            cafe3.IsActive = false;
            var responce = _controller.GetAll();
            var result = TransformResult.GetObject<IEnumerable<CafeModel>>(responce).ToList();
            Assert.IsTrue(result.Count() == 2);
        }

        [Test]
        public void Geturl()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var responce = _controller.Get(cafe.CleanUrlName);
            var result = TransformResult.GetObject<CafeModel>(responce);
            Assert.IsTrue(result.Id == cafe.Id);
        }
        [Test]
        public void Getid()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var responce = _controller.Get(cafe.Id);
            var result = TransformResult.GetObject<CafeModel>(responce);
            Assert.IsTrue(result.Id == cafe.Id);
        }
        [Test]
        public void GetCafesToCurrentUserTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var cafe1 = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            CafeManagerFactory.Create(_user, cafe1);
            var responce = _controller.GetCafesToCurrentUser();
            var result = TransformResult.GetObject<List<CafeModel>>(responce);
            Assert.IsTrue(result.Count == 2);
        }
        [Test]
        public void AddNewCostOfDeliveryTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var costOfDelivery = CostOfDeliveryFactory.CreateModel(cafe);
            var responce = _controller.AddNewCostOfDelivery(costOfDelivery);
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            var result = TransformResult.GetPrimitive<long>(responce);
            Assert.IsTrue(result >= 0);
        }

        [Test]
        public void GetCostOfDelivery()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var costOfDelivery = CostOfDeliveryFactory.Create(cafe);
            var price = 5;
            var response = _controller.GetCostOfDelivery(cafe.Id, price);
            var result = TransformResult.GetObject<OrderDeliveryPriceModel>(response.Result);
            Assert.IsNotNull(result);
            Assert.Greater(result.TotalPrice, 0);
        }

        [Test]
        public void UpdateCafeTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var cafemodel = CafeFactory.CreateModel(cafe);
            var responce = _controller.UpdateCafe(cafemodel);
            var result = TransformResult.GetObject<CafeModel>(responce);
            Assert.IsTrue(result.Name == cafemodel.Name);
        }

        [Test]
        public void CreateCafeTest()
        {
            SetUp();
            var cafeModel = CafeFactory.CreateModel();
            cafeModel.Name = Guid.NewGuid().ToString("N");
            cafeModel.FullName = Guid.NewGuid().ToString("N");
            var responce = _controller.CreateCafe(cafeModel);
            var result = TransformResult.GetObject<CafeModel>(responce.Result);
            Assert.IsTrue(result.Name == cafeModel.Name);
        }

        [Test]
        public void СheckUniqueName()
        {
            SetUp();
            var cafe = new Cafe()
            {
                IsActive = true,
                IsDeleted = false,
                CafeName = Guid.NewGuid().ToString("N")
            };
            var responce = _controller.СheckUniqueName(cafe.CafeName);
            var result = TransformResult.GetPrimitive<bool>(responce.Result);
            Assert.IsTrue(result);
        }
        [Test]
        public void СheckUniqueNameId()
        {
            SetUp();
            CafeFactory.Create();
            var cafe = new Cafe()
            {
                IsActive = true,
                IsDeleted = false,
                Id = 1,
                CafeName = Guid.NewGuid().ToString("N")
            };
            var responce = _controller.СheckUniqueName(cafe.CafeName, cafe.Id);
            var result = TransformResult.GetPrimitive<bool>(responce.Result);
            Assert.IsTrue(result);
        }

        [Test]
        public void AddNewCafeNotificationContactTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var notify = new CafeNotificationContactModel()
            {
                CafeId = cafe.Id,
                NotificationContact = Guid.NewGuid().ToString("N")
            };
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            var responce = _controller.AddNewCafeNotificationContact(notify);
            var result = TransformResult.GetPrimitive<long>(responce.Result);
            Assert.IsTrue(result > -1);
        }
        [Test]
        public void DeleteCafeIsDeleted()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            cafe.IsDeleted = true;
            var responce = _controller.DeleteCafe(cafe.Id);
            Assert.IsInstanceOf(typeof(OkResult), responce.Result);
        }
        [Test]
        public void DeleteCafe()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var responce = _controller.DeleteCafe(cafe.Id);
            Assert.IsInstanceOf(typeof(OkResult), responce.Result);
        }
        [Test]
        public void GetCafesRangeTest()
        {
            SetUp();
            var cafeList = CafeFactory.CreateFew(15);
            CafeFactory.CreateFewModels(cafeList, 15);
            var start = 5;
            var range = 10;
            _accessor.Setup(e => e.GetCafes()).Returns(cafeList);
            var responce = _controller.GetCafesRange(start, range);
            var result = TransformResult.GetObject<List<CafeModel>>(responce);
            Assert.IsTrue(result.Count() > 0);
        }
        [Test]
        public void GetCafesToUserTest0()
        {
            SetUp();
            var cafeList = CafeFactory.CreateFew(5);
            var responce = _controller.GetCafesToUser(_user.Id);
            var result = TransformResult.GetObject<List<CafeModel>>(responce.Result);
            Assert.IsTrue(result.Count() == 0);
        }
        [Test]
        public void GetCafesToUserTest()
        {
            SetUp();
            var cafe = CafeFactory.CreateFew(3);
            CafeManagerFactory.Create(_user, cafe.FirstOrDefault());
            _accessor.Setup(e => e.GetCafesByUserId(_user.Id)).Returns(cafe);
            var responce = _controller.GetCafesToUser(_user.Id);
            var result = TransformResult.GetObject<List<CafeModel>>(responce.Result);
            Assert.IsTrue(result.Count() > 0);
        }
        [Test]
        public void GetListOfCafeByUserIdTest()
        {
            SetUp();
            var cafe = CafeFactory.CreateFew();
            CafeManagerFactory.Create(_user, cafe.FirstOrDefault());
            _accessor.Setup(e => e.GetListOfCafeByUserId(_user.Id)).Returns(cafe);
            var responce = _controller.GetListOfCafeByUserId(_user.Id);
            var result = TransformResult.GetObject<List<CafeModel>>(responce.Result);
            Assert.IsTrue(result.Count() > 0);
        }
        [Test]
        public void GetListOfCafeByUserIdTest0()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            _accessor.Setup(e => e.GetListOfCafeByUserId(_user.Id)).Returns(new List<Cafe>());
            var responce = _controller.GetListOfCafeByUserId(_user.Id);
            var result = TransformResult.GetObject<List<CafeModel>>(responce.Result);
            Assert.IsTrue(result.Count() == 0);
        }
        [Test]
        public void RemoveUserCafeLinkTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            _accessor.Setup(e => e.RemoveUserCafeLink(It.IsAny<CafeManager>())).Returns(true);
            var responce = _controller.RemoveUserCafeLink(cafe.Id, _user.Id);
            var result = TransformResult.GetPrimitive<bool>(responce.Result);
            Assert.IsTrue(result);
        }
        [Test]
        public void GetCafeManagersTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var user = UserFactory.CreateUser();
            CafeManagerFactory.Create(_user, cafe);
            CafeManagerFactory.Create(user, cafe);
            var responce = _controller.GetCafeManagers(cafe.Id);
            var result = TransformResult.GetObject<IEnumerable<CafeManagerModel>>(responce.Result);
            Assert.IsTrue(result.Count() == 2);
        }
        [Test]
        public void GetCafeManagersTestId()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var user = UserFactory.CreateUser();
            CafeManagerFactory.Create(_user, cafe);
            CafeManagerFactory.Create(user, cafe);
            var responce = _controller.GetCafeManagers(cafe.Id, _user.Id);
            var result = TransformResult.GetObject<CafeManagerModel>(responce.Result);
            Assert.NotNull(result);
        }
        [Test]
        public void GetCafeManagersTestId0()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var user = UserFactory.CreateUser();
            CafeManagerFactory.Create(user, cafe);
            var responce = _controller.GetCafeManagers(cafe.Id, _user.Id);
            Assert.IsInstanceOf(typeof(NotFoundResult), responce.Result);
        }
        [Test]
        public void GetManagedCafesTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var anotherCafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            CafeManagerFactory.Create(_user, anotherCafe);
            var ListCafe = new List<Cafe>();
            ListCafe.Add(cafe);
            ListCafe.Add(anotherCafe);
            _accessor.Setup(e => e.GetManagedCafes(_user.Id)).Returns(ListCafe);
            var responce = _controller.GetManagedCafes();
            Assert.IsTrue(responce.Count() > 0);
        }
        [Test]
        public void GetManagedCaffeeByMailTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var ListCafes = new List<Cafe>();
            ListCafes.Add(cafe);
            CafeManagerFactory.Create(_user, cafe);
            _accessor.Setup(e => e.GetManagedCafes(_user.Id)).Returns(ListCafes);
            var responce = _controller.GetManagedCaffeeByMail(_user.Email);
            var result = TransformResult.GetObject<List<CafeModel>>(responce.Result);
            Assert.IsNotEmpty(result);

        }
        [Test]
        public void PostCafeManagerTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var role = RoleFactory.CreateRole(EnumUserRole.Manager);
            UserInRolesFactory.CreateRoleForUser(_user, role);
            var model = CafeManagerFactory.CreateModel(cafe, _user.Id);
            _accessor.Setup(e => e.AddUserCafeLink(It.IsAny<CafeManager>(), _user.Id)).Returns(true);
            var responce = _controller.PostCafeManager(model);
            Assert.IsInstanceOf(typeof(OkResult), responce.Result);
        }

        //-----------------------------------------------------------------------------------------------
        //
        //BUG?
        //UserRole.cs/RemoveUserRole
        //var role = fc.Roles.FirstOrDefault(o => !o.IsDeleted && o.RoleName.Trim().ToLower() == roleName);
        //role = null---------------------------------------------(o.RoleName.Trim().ToLower = EnumUserRole.Manager.ToLower()) == (roleName = EnumUserRole.Manager)
        //[Test]
        //public void DeleteCafeManagerTest()
        //{
        //    SetUp();
        //    var cafe = CafeFactory.Create();
        //    CafeManagerFactory.Create(_user, cafe);
        //    var role = RoleFactory.CreateRole(EnumUserRole.Manager);
        //    UserInRolesFactory.CreateRoleForUser(_user, role);
        //    var responce = _controller.DeleteCafeManager(cafe.Id, _user.Id);
        //    Assert.IsInstanceOf(typeof(BadRequestErrorMessageResult), responce.Result);
        //}
        //
        //------------------------------------------------------------------------------------------------



        [Test]
        public void IsUserOfManagerCafeTestFalse()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(false);
            var responce = _controller.IsUserOfManagerCafe(cafe.Id);
            Assert.IsFalse(responce);
        }
        [Test]
        public void IsUserOfManagerCafeTestTrue()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            var responce = _controller.IsUserOfManagerCafe(cafe.Id);
            Assert.IsTrue(responce);
        }
        [Test]
        public void AddCafeMenuPatternTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var pattern = CafeMenuPatternFactory.CreateModel(cafe);
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            var responce = _controller.AddCafeMenuPattern(pattern);
            Assert.IsInstanceOf(typeof(OkResult), responce.Result);
        }

        [Test]
        public void GetCafeMenuPatternById()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var pattern = CafeMenuPatternFactory.Create(cafe);
            var responce = _controller.GetCafeMenuPatternById(cafe.Id, pattern.Id);
            var result = TransformResult.GetObject<CafeMenuPatternModel>(responce.Result);
            Assert.IsTrue(result.Name != null);
        }
        [Test]
        public void GetCafeMenuPatternById0()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var patternId = 10;
            var responce = _controller.GetCafeMenuPatternById(cafe.Id, patternId);
            var result = TransformResult.GetObject<CafeMenuPatternModel>(responce.Result);
            Assert.IsTrue(result.Name == null);
        }
        [Test]
        public void GetBankets()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var banket = BanketFactory.CreateWithMenu(_user, null, cafe);
            var responce = _controller.GetBankets(cafe.Id);
            var result = TransformResult.GetObject<IEnumerable<BanketModel>>(responce.Result);
            Assert.IsNotEmpty(result);
        }
        [Test]
        public void GetMenuPatternsByCafeId()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var pattern = CafeMenuPatternFactory.Create(cafe);
            var responce = _controller.GetMenuPatternsByCafeId(cafe.Id);
            var result = TransformResult.GetObject<IEnumerable<CafeMenuPatternModel>>(responce.Result);
            Assert.IsNotEmpty(result);
        }

        [Test]
        public void GetCompanyOrderSchedules()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var order = new CompanyOrderSchedule()
            {
                Cafe = cafe,
                CafeId = cafe.Id,
                IsActive = true,
                IsDeleted = false
            };
            ContextManager.Get().CompanyOrderSchedules.Add(order);
            var responce = _controller.GetCompanyOrderSchedules(cafe.Id);
            var result = TransformResult.GetObject<IEnumerable<CompanyOrderScheduleModel>>(responce.Result);
            Assert.IsNotEmpty(result);
        }
        [Test]
        public void GetCompanyOrderSchedulesNull()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var responce = _controller.GetCompanyOrderSchedules(cafe.Id);
            var result = TransformResult.GetObject<IEnumerable<CompanyOrderScheduleModel>>(responce.Result);
            Assert.IsEmpty(result);
        }


        [Test]
        public void GetDiscountTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var discount = DiscountFactory.Create(_user, cafe);
            var responce = _controller.GetDiscount(cafe.Id, DateTime.MinValue.Millisecond, 0);
            var result = TransformResult.GetPrimitive<int>(responce.Result);
            Assert.IsTrue(result > 0);
        }
        [Test]
        public void GetDiscountTestNull()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var discount = DiscountFactory.Create(_user, cafe);
            var responce = _controller.GetDiscount(cafe.Id, DateTime.MinValue.Millisecond);
            var result = TransformResult.GetPrimitive<int>(responce.Result);
            Assert.IsTrue(result == 0);
        }

        [Test]
        public void GetMenuByPatternIdTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var pattern = CafeMenuPatternFactory.CreateWithDishes(cafe);
            var responce = _controller.GetMenuByPatternId(cafe.Id, pattern.Id);
            var result = TransformResult.GetObject<List<CafeMenuModel>>(responce.Result);
            Assert.IsNotEmpty(result);
        }

        [Test]
        public void GetMenuFilteredMenuByPatternIdTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var pattern = CafeMenuPatternFactory.CreateWithDishes(cafe);
            var dish = pattern.Dishes[0];
            var responce = _controller.GetMenuFilteredMenuByPatternId(cafe.Id, pattern.Id, dish.Name);
            var result = TransformResult.GetObject<IEnumerable<CafeMenuModel>>(responce.Result);
            Assert.IsNotEmpty(result);
        }

        [Test]
        public void RemoveCafeMenuPatternTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var pattern = CafeMenuPatternFactory.CreateWithDishes(cafe);
            CafeManagerFactory.Create(_user, cafe);
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            var responce = _controller.RemoveCafeMenuPattern(cafe.Id, pattern.Id);
            Assert.IsInstanceOf(typeof(OkResult), responce.Result);
        }

        [Test]
        public void UpdateCafeMenuPatternTestDishes()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var pattern = CafeMenuPatternFactory.CreateWithDishes(cafe);
            var patternModel = CafeMenuPatternFactory.CreateModel(cafe);
            patternModel.IsBanket = false;
            patternModel.PatternToDate = DateTime.Today.AddDays(1);
            patternModel.Name = pattern.Name;
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            var responce = _controller.UpdateCafeMenuPattern(patternModel);
            Assert.IsInstanceOf(typeof(OkResult), responce.Result);
        }
        [Test]
        public void UpdateCafeMenuPatternTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var pattern = CafeMenuPatternFactory.CreateWithDishes(cafe);
            var patternModel = CafeMenuPatternFactory.CreateModelWithDishes(cafe, pattern.Dishes);
            patternModel.IsBanket = false;
            patternModel.PatternToDate = DateTime.Today.AddDays(1);
            patternModel.Name = pattern.Name;
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            var responce = _controller.UpdateCafeMenuPattern(patternModel);
            Assert.IsInstanceOf(typeof(OkResult), responce.Result);
        }

        [Test]
        public void EditCostOfDeliveryTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var costOfDelivery = CostOfDeliveryFactory.Create(cafe);
            double PriceFrom = 10;
            double PriceTo = 100;
            double DeliveryPrice = 10;
            var responce = _controller.EditCostOfDelivery(new CostOfDeliveryModel() { Id = costOfDelivery.Id, OrderPriceFrom = PriceFrom, OrderPriceTo = PriceTo, DeliveryPrice = DeliveryPrice });
            var result = TransformResult.GetPrimitive<bool>(responce.Result);
            Assert.IsTrue(result);
        }
        [Test]
        public void GetListOfCostOfDeliveryTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var costOfDelivery = CostOfDeliveryFactory.CreateFew(cafe);
            var responce = _controller.GetListOfCostOfDelivery(cafe.Id, null);
            var result = TransformResult.GetObject<List<CostOfDeliveryModel>>(responce.Result);
            Assert.IsTrue(result.Count() > 0);
        }
        [Test]
        public void GetListOfCostOfDeliveryTest0()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var responce = _controller.GetListOfCostOfDelivery(cafe.Id, null);
            var result = TransformResult.GetObject<List<CostOfDeliveryModel>>(responce.Result);
            Assert.IsTrue(result.Count() == 0);
        }
        [Test]
        public void GetListOfCostOfDeliveryTestNotNull()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var costOfDelivery = CostOfDeliveryFactory.Create(cafe, 5, 10);
            var responce = _controller.GetListOfCostOfDelivery(cafe.Id, 10);
            var result = TransformResult.GetObject<List<CostOfDeliveryModel>>(responce.Result);
            Assert.IsTrue(result.Count() > 0);
        }
        [Test]
        public void GetListOfCostOfDeliveryTestNotNull0()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var responce = _controller.GetListOfCostOfDelivery(cafe.Id, 10);
            var result = TransformResult.GetObject<List<CostOfDeliveryModel>>(responce.Result);
            Assert.IsTrue(result.Count() == 0);
        }

        [Test]
        public void RemoveCostOfDeliveryTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var costOfDelivery = CostOfDeliveryFactory.Create(cafe);
            var responce = _controller.RemoveCostOfDelivery(costOfDelivery.Id);
            var result = TransformResult.GetPrimitive<bool>(responce.Result);
            Assert.IsTrue(result);
        }

        [Test]
        public void AddUserCafeLinkTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            _accessor.Setup(e => e.AddUserCafeLink(It.IsAny<CafeManager>(), _user.Id)).Returns(true);
            var responce = _controller.AddUserCafeLink(cafe.Id, _user.Id);
            var result = TransformResult.GetPrimitive<bool>(responce.Result);
            Assert.IsTrue(result);
        }


        //[Test]
        //public void EditUserCafeLinkTest()
        //{
        //    SetUp();
        //    var cafe = CafeFactory.Create();
        //    var responce = _controller.EditUserCafeLink(cafe.Id, _user.Id);
        //    var result = TransformResult.GetPrimitive<bool>(responce.Result);
        //    Assert.IsFalse(result);
        //}

        [Test]
        public void GetCafeBusinessHoursTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            _accessor.Setup(e => e.IsUserManagerOfCafeIgnoreActivity(_user.Id, cafe.Id)).Returns(true);
            var responce = _controller.GetCafeBusinessHours(cafe.Id);
            var result = TransformResult.GetObject<CafeBusinessHoursModel>(responce.Result);
            Assert.IsNotNull(result);
        }

        [Test]
        public void SetCafeBusinessHoursTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var hours = CafeFactory.CreateBusinessHoursModel(cafe);
            _accessor.Setup(e => e.IsUserManagerOfCafeIgnoreActivity(_user.Id, cafe.Id)).Returns(true);
            var responce = _controller.SetCafeBusinessHours(hours);
            Assert.IsInstanceOf(typeof(OkResult), responce.Result);
        }
        [Test]
        public void GetCafeByIdIgnoreActivityTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var responce = _controller.GetCafeByIdIgnoreActivity(cafe.Id);
            _accessor.Setup(e => e.GetCafeByIdIgnoreActivity(cafe.Id)).Returns(cafe);
            var result = TransformResult.GetObject<CafeModel>(responce.Result);
            Assert.IsNotNull(result);

        }

        [Test]
        public void GetCafeInfoTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            _accessor.Setup(e => e.GetCafeById(cafe.Id)).Returns(cafe);
            var responce = _controller.GetCafeInfo(cafe.Id);
            var result = TransformResult.GetObject<CafeInfoModel>(responce);
            Assert.IsNotNull(result);
        }

        [Test]
        public void SetCafeInfoTest()
        {

            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var info = CafeFactory.CreateCafeInfo(cafe);
            _accessor.Setup(e => e.IsUserManagerOfCafeIgnoreActivity(_user.Id, cafe.Id)).Returns(true);
            _accessor.Setup(e => e.GetCafeByIdIgnoreActivity(cafe.Id)).Returns(cafe);
            Assert.That(() => _controller.SetCafeInfo(info), Throws.Nothing);
            Assert.That(() => _controller.SetCafeInfo(info), Throws.Nothing);
        }

        [Test]
        public void GetCafeNotificationContactByIdTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var info = CafeFactory.CreateCafeInfo(cafe);
            Assert.That(() => _controller.SetCafeInfo(info), Throws.Nothing);
            var id = 0;
            var responce = _controller.GetCafeNotificationContactById(id);
            var result = TransformResult.GetObject<CafeNotificationContactModel>(responce.Result);
            Assert.IsNotNull(result);
        }
        [Test]
        public void GetNotificationContactsToCafeTest0()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var responce = _controller.GetNotificationContactsToCafe(cafe.Id, 0);
            var result = TransformResult.GetObject<List<CafeNotificationContactModel>>(responce.Result);
            Assert.IsNotNull(result);
        }

        [Test]
        public void RemoveCafeNotificationContactTest()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var info = CafeFactory.CreateCafeInfo(cafe);
            Assert.That(() => _controller.SetCafeInfo(info), Throws.Nothing);
            var notify = _context.CafeNotificationContact.FirstOrDefault(e => e.CafeId == cafe.Id);
            var responce = _controller.RemoveCafeNotificationContact(notify.Id);
            var result = TransformResult.GetPrimitive<bool>(responce.Result);
            Assert.IsTrue(result);
        }

        [Test]
        public void UpdateCafeNotificationContact()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var notify = CafeNotificationFactory.CreateNotificationContact(cafe);
            var notifyModel = CafeNotificationFactory.CreateModel(notify);
            var responce = _controller.UpdateCafeNotificationContact(notifyModel);
            var result = TransformResult.GetPrimitive<bool>(responce.Result);
            Assert.IsTrue(result);
        }
    }
}
