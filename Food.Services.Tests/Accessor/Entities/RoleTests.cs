using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using NUnit.Framework;
using System.Linq;

namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    class RoleTests
    {
        FakeContext _context;
        User _user;

        [SetUp]
        public void SetUp()
        {
            _context = new FakeContext();
            ContextManager.Set(_context);
            ITWebNet.FoodService.Food.DbAccessor.Accessor.SetTestingModeOn(_context);
            _user = UserFactory.CreateUser();
        }

        [Test]
        public void GetRoles_Test()
        {
            var lstRoles = RoleFactory.CreateFewRoles(count: 3);
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetRoles();
            //
            Assert.IsTrue(lstRoles.Count == response.Count);
        }

        [Test]
        public void GetRoleByName_Test()
        {
            var lstRoles = RoleFactory.CreateFewRoles(count: 3);
            var checkRole = lstRoles.Last();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetRoleByName(checkRole.RoleName);
            //
            Assert.IsNotNull(response);
            Assert.IsTrue(response.RoleName == checkRole.RoleName);
        }

        /// <summary>
        /// Успешное получение роли по идентификатору
        /// </summary>
        [Test]
        public void GetRoleById_Test1()
        {
            var lstRoles = RoleFactory.CreateFewRoles(count: 3);
            var checkRole = lstRoles.Last();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetRoleById(checkRole.Id);
            //
            Assert.IsNotNull(response);
            Assert.IsTrue(response.RoleName == checkRole.RoleName);
        }

        /// <summary>
        /// Попытка получить удаленную роль
        /// </summary>
        [Test]
        public void GetRoleById_Test2()
        {
            var lstRoles = RoleFactory.CreateFewRoles(count: 3);
            var checkRole = lstRoles.Last();
            checkRole.IsDeleted = true;
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetRoleById(checkRole.Id);
            //
            Assert.IsNull(response);
        }

        /// <summary>
        /// Попытка получить удаленную роль
        /// </summary>
        [Test]
        public void GetRolesByUserId_Test1()
        {
            var checkRoles = UserInRolesFactory.CreateRoleForUser(_user);
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetRolesByUserId(_user.Id);
            //
            Assert.IsTrue(response.Count == 1);
        }
    }
}
