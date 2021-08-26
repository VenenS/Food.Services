using System;
using System.Linq;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using NUnit.Framework;


namespace ITWebNet.FoodService.Food.DbAccessor.Tests
{
    [TestFixture]
    public class UserRoleTests
    {
        [SetUp]
        public void SetUp()
        {
            _context = new FakeContext();
            ContextManager.Set(_context);
            Accessor.SetTestingModeOn(_context);
        }

        private FakeContext _context;
        private readonly Random _rnd = new Random();

        [Test]
        public void AddUserToRoleTest()
        {
            var user = UserFactory.CreateUser();
            var role = RoleFactory.CreateRole();
            var uir = new UserInRole
            {
                RoleId = role.Id,
                UserId = user.Id
            };
            var result = Accessor.Instance.AddUserToRole(uir);
            Assert.IsTrue(result);
            Assert.IsNotNull(ContextManager.Get().UsersInRoles.FirstOrDefault(e => e.UserId == user.Id && e.RoleId == role.Id));
        }

        [Test]
        public void AddUserToRoleTest_ByRoleName()
        {
            var user = UserFactory.CreateUser();
            var role = RoleFactory.CreateRole();
            var result = Accessor.Instance.AddUserToRole(user.Id, role.RoleName);
            Assert.IsTrue(result);
            Assert.IsNotNull(ContextManager.Get().UsersInRoles.FirstOrDefault(e => e.UserId == user.Id && e.RoleId == role.Id));
        }

        [Test]
        public void EditUserRoleTest()
        {
            var user = UserFactory.CreateUser();
            var link = UserInRolesFactory.CreateRoleForUser(user);
            var uir = new UserInRole
            {
                Id = link.Id,
                RoleId = _rnd.Next(),
                UserId = _rnd.Next()
            };
            var result = Accessor.Instance.EditUserRole(uir);
            Assert.IsTrue(result);
            Assert.IsTrue(link.RoleId == uir.RoleId);
            Assert.IsTrue(link.UserId == uir.UserId);
        }

        [Test]
        public void GetListRoleToUserTest()
        {
            var user = UserFactory.CreateUser();
            var link = UserInRolesFactory.CreateRoleForUser(user);
            UserInRolesFactory.CreateRoleForUser(UserFactory.CreateUser());
            var result = Accessor.Instance.GetListRoleToUser(user.Id);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.First().RoleName == link.Role.RoleName);
        }

        [Test]
        public void GetListUserRoleTest()
        {
            var user = UserFactory.CreateUser();
            var link = UserInRolesFactory.CreateRoleForUser(user);
            //UserInRolesFactory.CreateRoleForUser(user);
            var result = Accessor.Instance.GetListUserRole();
            Assert.IsTrue(result.Any());
            Assert.IsNotNull(result.FirstOrDefault(e => e.Role.RoleName == link.Role.RoleName));
        }

        [Test]
        public void GetListUserRoleTest_OnlyAlive()
        {
            var user = UserFactory.CreateUser();
            var link = UserInRolesFactory.CreateRoleForUser(user);
            var temp = UserInRolesFactory.CreateRoleForUser(user);
            temp.IsDeleted = true;
            var result = Accessor.Instance.GetListUserRole();
            Assert.IsTrue(result.Any());
            Assert.IsNotNull(result.FirstOrDefault(e => e.Role.RoleName == link.Role.RoleName));
            Assert.IsNull(result.FirstOrDefault(e => e.IsDeleted));
        }

        [Test]
        public void RemoveUserRoleTest()
        {
            var user = UserFactory.CreateUser();
            var link = UserInRolesFactory.CreateRoleForUser(user);
            var result = Accessor.Instance.RemoveUserRole(link);
            Assert.IsTrue(result);
            Assert.IsTrue(link.IsDeleted);
        }

        [Test]
        public void RemoveUserRoleTest1()
        {
            var user = UserFactory.CreateUser();
            var link = UserInRolesFactory.CreateRoleForUser(user);
            var result = Accessor.Instance.RemoveUserRole(user.Id, link.Role.RoleName);
            Assert.IsTrue(result);
            Assert.IsTrue(link.IsDeleted);
        }
    }
}