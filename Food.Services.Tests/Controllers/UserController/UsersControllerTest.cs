using Food.Data.Entities;
using Food.Services.Controllers;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using Food.Services.Tests.Tools;
using ITWebNet.Food.Core.DataContracts.Admin;
using ITWebNet.Food.Core.DataContracts.Common;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;

namespace Food.Services.Tests.Controllers.UserController
{
    [TestFixture]
    public class UserControllerTest
    {
        private FakeContext _context;
        private UsersController _controller;
        private Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor> _accessor;
        private User _user;

        private void SetUp()
        {
            _accessor = new Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor>();
            _context = new FakeContext();
            ContextManager.Set(_context);
            _controller = new UsersController(_context, _accessor.Object);
            _user = UserFactory.CreateUser();
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()));
            Thread.CurrentPrincipal = new GenericPrincipal(identity, null);
        }

        [Test]
        public void GetUsersTest()
        {
            SetUp();
            var responce = _controller.GetUsers();
            var result = TransformResult.GetObject<List<UserModel>>(responce);
            Assert.IsTrue(result.Any());
        }
        [Test]
        public void GetAdminUsersTest()
        {
            SetUp();
            var responce = _controller.GetAdminUsers();
            var result = TransformResult.GetObject<List<UserAdminModel>>(responce);
            Assert.IsTrue(result.Count() > 0);
        }
        [Test]
        public void GetUsersWithoutCuratorsTest()
        {
            SetUp();
            var responce = _controller.GetUsersWithoutCurators();
            var result = TransformResult.GetObject<List<UserModel>>(responce);
            Assert.IsTrue(result.Count() > 0);
        }
        //[Test]
        //public void AddUserReferralLinkTest()
        //{
        //    SetUp();
        //    UserReferral refLink = new UserReferral();
        //    refLink.Id = _random.Next();
        //    refLink.ParentId = _user.Id;
        //    refLink.RefId = _random.Next();
        //    refLink.IsActive = true;
        //    refLink.IsDeleted = false;
        //    ContextManager.Get().UserReferral.Add(refLink);
        //    var responce = _controller.AddUserReferralLink(refLink.ParentId.Value, refLink.RefId);
        //}
        [Test]
        public void AddUserToRoleTest()
        {
            SetUp();
            var role = RoleFactory.CreateRole();
            var responce = _controller.AddUserToRole(role.Id, _user.Id);
            var result = TransformResult.GetPrimitive<bool>(responce);
            Assert.IsTrue(result);
        }
        [Test]
        public void AddUserToRoleTestEx()
        {
            SetUp();
            var role = RoleFactory.CreateRole();
            var userInRole = UserInRolesFactory.CreateRoleForUser(_user, role);
            ContextManager.Get().UsersInRoles.Add(userInRole);
            Assert.Catch<Exception>(() => _controller.AddUserToRole(role.Id, _user.Id));
            Assert.Catch<Exception>(() => _controller.AddUserToRole(50, _user.Id));
            Assert.Catch<Exception>(() => _controller.AddUserToRole(role.Id, 50));

        }
        [Test]
        public void EditUserTest()
        {
            SetUp();
            var user = new UserWithLoginModel()
            {
                Password = _user.Password,
                DisplayName = _user.DisplayName,
                UserFirstName = _user.FirstName,
                PhoneNumber = new string(_user.PhoneNumber.Where(c => char.IsDigit(c)).ToArray())
            };
            var responce = _controller.EditUser(user);
            var result = TransformResult.GetPrimitive<bool>(responce);
            Assert.IsTrue(result);

        }
        [Test]
        public void EditUserPointsByLoginTestAddPoint()
        {
            SetUp();
            var responce = _controller.EditUserPointsByLogin(_user.Name, 1, 10);
            var result = TransformResult.GetPrimitive<bool>(responce);
            Assert.IsTrue(result);
        }
        [Test]
        public void EditUserPointsByLoginTestSubPointEx()
        {
            SetUp();
            var responce = _controller.EditUserPointsByLogin(_user.Name, 1, -10);
            var result = TransformResult.GetPrimitive<bool>(responce);
            Assert.IsFalse(result);
        }
        [Test]
        public void EditUserPointsByLoginTestSubPoint()
        {
            SetUp();
            _user.PersonalPoints = 20;
            var responce = _controller.EditUserPointsByLogin(_user.Name, 1, -10);
            var result = TransformResult.GetPrimitive<bool>(responce);
            Assert.IsTrue(result);
        }
        [Test]
        public void EditUserPointsByLoginAndTotalPriceTest()
        {
            SetUp();
            _user.PercentOfOrder = 50;
            var responce = _controller.EditUserPointsByLoginAndTotalPrice(_user.Name, 10);
            var result = TransformResult.GetPrimitive<bool>(responce);
            Assert.IsTrue(result);
        }
        [Test]
        public void EditUserPointsByLoginAndTotalPriceTestEx()
        {
            SetUp();
            var responce = _controller.EditUserPointsByLoginAndTotalPrice(_user.Name, -10);
            var result = TransformResult.GetPrimitive<bool>(responce);
            Assert.IsFalse(result);
        }
        [Test]
        public void EditUserRoleTestEx()
        {
            SetUp();
            var role = RoleFactory.CreateRole();
            var roleModel = new UserRoleModel
            {
                RoleId = role.Id,
                UserId = _user.Id
            };
            UserInRolesFactory.CreateRoleForUser(_user, role);
            Assert.Catch<Exception>(() => _controller.EditUserRole(roleModel));
        }
        [Test]
        public void EditUserRoleTest()
        {
            SetUp();
            var role = RoleFactory.CreateRole();
            var roleModel = new UserRoleModel()
            {
                RoleId = role.Id,
                UserId = _user.Id
            };
            UserInRolesFactory.CreateRoleForUser(_user);
            var responce = _controller.EditUserRole(roleModel);
            var result = TransformResult.GetPrimitive<bool>(responce);
            Assert.IsTrue(result);
        }
        [Test]
        public void GetFullListOfUsers()
        {
            SetUp();
            var responce = _controller.GetFullListOfUsers();
            var result = TransformResult.GetObject<List<UserWithLoginModel>>(responce);
            Assert.IsTrue(result.Count() > 0);
        }
        [Test]
        public void GetListRoleToUserTest()
        {
            SetUp();
            for (var i = 0; i < 5; i++)
            {
                UserInRolesFactory.CreateRoleForUser(_user);
            }
            var responce = _controller.GetListRoleToUser(_user.Id);
            var result = TransformResult.GetObject<List<RoleModel>>(responce);
            Assert.IsTrue(result.Count() > 0);
        }
        [Test]
        public void GetListUserRoleTest()
        {
            SetUp();
            UserInRolesFactory.CreateRoleForUser(_user);
            var responce = _controller.GetListUserRole();
            var result = TransformResult.GetObject<List<UserRoleModel>>(responce);
            Assert.IsTrue(result.Count() > 0);
        }
        [Test]
        public void GetRolesTest()
        {
            SetUp();
            RoleFactory.CreateFewRoles(5);
            var responce = _controller.GetRoles();
            var result = TransformResult.GetObject<List<RoleModel>>(responce);
            Assert.IsTrue(result.Count() > 0);
        }
        [Test]
        public void GetUserByLoginTest()
        {
            SetUp();
            _user.AccessFailedCount = 0;
            var responce = _controller.GetUserByLogin(_user.Name);
            var result = TransformResult.GetObject<UserModel>(responce);
            Assert.IsTrue(result != null);

            responce = _controller.GetUserByLogin("does not exist");
            result = TransformResult.GetObject<UserModel>(responce);
            Assert.IsNull(result);
        }
        [Test]
        public void GetUserByRoleIdTest()
        {
            SetUp();
            RoleFactory.CreateRole();
            var role = UserInRolesFactory.CreateRoleForUser(_user);
            var responce = _controller.GetUserByRoleId(role.RoleId);
            var result = TransformResult.GetObject<List<UserWithLoginModel>>(responce);
            Assert.IsTrue(result.Count() > 0);
        }
        [Test]
        public void GetUserByReferralLinkTest()
        {
            SetUp();

            var result = _controller.GetUserByReferralLink("https://google.com");
            Assert.IsNull(TransformResult.GetObject<UserModel>(result));

            _user.UserReferralLink = "https://www.google.com";
            result = _controller.GetUserByReferralLink(_user.UserReferralLink);
            Assert.NotNull(TransformResult.GetObject<UserModel>(result));
        }
        [Test]
        public void GetUserPointsByLoginTest()
        {
            SetUp();
            _user.PersonalPoints = 10;
            var responce = _controller.GetUserPointsByLogin(_user.Name);
            var result = TransformResult.GetPrimitive<double>(responce);
            Assert.IsTrue(result > 0);
        }
        //[Test]
        //public void GetUserReferralsTest()
        //{
        //    SetUp();
        //    UserReferral refLink = new UserReferral()
        //    {
        //        Id = 1,
        //        IsActive = true,
        //        IsDeleted = false,
        //        RefId = _user.Id,
        //        Level = 1

        //    };
        //    ContextManager.Get().UserReferral.Add(refLink);
        //    int[] m = { 1 };
        //    var responce = _controller.GetUserReferrals(_user.Id, m);

        //}
        [Test]
        public void GetUsersByCafeOrCompanyTestCafe()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(_user, cafe);
            var responce = _controller.GetUsersByCafeOrCompany(cafe.Id, 1);
            var result = TransformResult.GetObject<List<UserWithLoginModel>>(responce);
            Assert.IsTrue(result.Count() > 0);
        }
        [Test]
        public void GetUsersByCafeOrCompanyTestCompany()
        {
            SetUp();
            var company = CompanyFactory.Create();
            var role = UserInCompanyFactory.CreateRoleForUser(_user, company);
            var userInCompany = new UserInCompany
            {
                Company = company,
                User = _user,
                CompanyId = company.Id,
                UserId = _user.Id,
                UserRole = RoleFactory.CreateRole(),
                UserRoleId = role.Id,
                IsActive = true
            };
            ContextManager.Get().UsersInCompanies.Add(userInCompany);
            var responce = _controller.GetUsersByCafeOrCompany(company.Id, 2);
            var result = TransformResult.GetObject<List<UserWithLoginModel>>(responce);
            Assert.IsTrue(result.Count() > 0);
        }
        [Test]
        public void RemoveUserRoleTest()
        {
            SetUp();
            var role = RoleFactory.CreateRole();
            var roleModel = new UserRoleModel()
            {
                RoleId = role.Id,
                UserId = _user.Id
            };
            UserInRolesFactory.CreateRoleForUser(_user, role);
            var responce = _controller.RemoveUserRole(roleModel);
            var result = TransformResult.GetPrimitive<bool>(responce);
            Assert.IsTrue(result);
        }


    }

}