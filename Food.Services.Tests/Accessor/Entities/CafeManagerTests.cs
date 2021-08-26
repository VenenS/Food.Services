using System;
using System.Linq;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using ITWebNet.FoodService.Food.DbAccessor;
using NUnit.Framework;
// ReSharper disable PossibleInvalidOperationException


namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    public class CafeManagerTests
    {
        [SetUp]
        public void SetUp()
        {
            _context = new FakeContext();
            ContextManager.Set(_context);
            ITWebNet.FoodService.Food.DbAccessor.Accessor.SetTestingModeOn(_context);
            _user = UserFactory.CreateUser();
            _rnd = new Random();
        }

        private FakeContext _context;
        private User _user;
        private Random _rnd;

        [Test]
        public void AddUserCafeLinkTest()
        {
            var user = UserFactory.CreateUser();
            var cafe = CafeFactory.Create();
            var link = new CafeManager {UserId = user.Id, CafeId = cafe.Id};
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.AddUserCafeLink(link, _user.Id);
            Assert.IsTrue(result);
            var created = ContextManager.Get().CafeManagers
                .FirstOrDefault(e => e.CafeId == link.CafeId && e.UserId == link.UserId);
            Assert.IsNotNull(created);
            //А почему записи не далаются при создании?
            // Assert.IsTrue(created.CreatedBy == _user.Id);
            // Assert.IsTrue(created.CreationDate.Value.Date == DateTime.Now.Date);
        }

        [Test]
        public void AddUserCafeLinkTest_Already_Exists()
        {
            var exists = CafeManagerFactory.Create();
            var link = new CafeManager {UserId = exists.UserId, CafeId = exists.CafeId};
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.AddUserCafeLink(link, _user.Id);
            Assert.IsTrue(result);
            var created = ContextManager.Get().CafeManagers
                .Where(e => e.CafeId == link.CafeId && e.UserId == link.UserId);
            Assert.IsTrue(created.Count() == 1);
            Assert.IsTrue(created.First().LastUpdateBy == _user.Id);
            Assert.IsTrue(created.First().LastUpdateDate.Value.Date == DateTime.Now.Date);
        }

        [Test]
        public void EditUserCafeLinkTest()
        {
            var exists = CafeManagerFactory.Create();
            var link = new CafeManager {UserId = exists.UserId, CafeId = exists.CafeId};
            link.LastUpdateBy = _user.Id;
            link.UserRoleId = Guid.NewGuid().ToString("N");
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.EditUserCafeLink(link);
            Assert.IsTrue(result);
            Assert.IsTrue(exists.LastUpdateBy == _user.Id);
            Assert.IsTrue(exists.UserRoleId == link.UserRoleId);
            Assert.IsTrue(exists.LastUpdateDate.Value.Date == DateTime.Now.Date);
        }

        [Test]
        public void EditUserCafeLinkTest_Not_Exists()
        {
            var link = new CafeManager {UserId = _rnd.Next(), CafeId = _rnd.Next()};
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.EditUserCafeLink(link);
            Assert.IsFalse(result);
        }

        [Test]
        public void GetListOfCafeByUserIdTest()
        {
            var user = UserFactory.CreateUser();
            var link = CafeManagerFactory.CreateFew(user: user);
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetListOfCafeByUserId(user.Id);
            Assert.IsTrue(result.Sum(e => e.Id) == link.Sum(e => e.CafeId));
        }

        [Test]
        public void IsUserManagerOfCafeIgnoreActivityTest_Cafe_is_Deleted()
        {
            var user = UserFactory.CreateUser();
            var cafe = CafeFactory.Create();
            cafe.IsDeleted = true;
            CafeManagerFactory.Create(user, cafe);
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.IsUserManagerOfCafeIgnoreActivity(user.Id, cafe.Id);
            Assert.IsFalse(result);
        }

        [Test]
        public void IsUserManagerOfCafeIgnoreActivityTest_False()
        {
            var user = UserFactory.CreateUser();
            var cafe = CafeFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.IsUserManagerOfCafeIgnoreActivity(user.Id, cafe.Id);
            Assert.IsFalse(result);
        }

        [Test]
        public void IsUserManagerOfCafeIgnoreActivityTest_True()
        {
            var user = UserFactory.CreateUser();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(user, cafe);
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.IsUserManagerOfCafeIgnoreActivity(user.Id, cafe.Id);
            Assert.IsTrue(result);
        }

        [Test]
        public void IsUserManagerOfCafeTest_Cafe_is_Deleted()
        {
            var user = UserFactory.CreateUser();
            var cafe = CafeFactory.Create();
            cafe.IsDeleted = true;
            CafeManagerFactory.Create(user, cafe);
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.IsUserManagerOfCafe(user.Id, cafe.Id);
            Assert.IsFalse(result);
        }

        [Test]
        public void IsUserManagerOfCafeTest_False()
        {
            var user = UserFactory.CreateUser();
            var cafe = CafeFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.IsUserManagerOfCafe(user.Id, cafe.Id);
            Assert.IsFalse(result);
        }

        [Test]
        public void IsUserManagerOfCafeTest_True()
        {
            var user = UserFactory.CreateUser();
            var cafe = CafeFactory.Create();
            CafeManagerFactory.Create(user, cafe);
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.IsUserManagerOfCafe(user.Id, cafe.Id);
            Assert.IsTrue(result);
        }

        [Test]
        public void RemoveUserCafeLinkTest()
        {
            var exists = CafeManagerFactory.Create();
            var link = new CafeManager {UserId = exists.UserId, CafeId = exists.CafeId};
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RemoveUserCafeLink(link);
            Assert.IsTrue(result);
            Assert.IsTrue(exists.IsDeleted);
        }

        [Test]
        public void RemoveUserCafeLinkTest_Already_Deleted()
        {
            var exists = CafeManagerFactory.Create();
            exists.IsDeleted = true;
            var link = new CafeManager {UserId = exists.UserId, CafeId = exists.CafeId};
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RemoveUserCafeLink(link);
            Assert.IsFalse(result);
        }

        [Test]
        public void RemoveUserCafeLinkTest_Not_Exists()
        {
            var link = new CafeManager {UserId = _rnd.Next(), CafeId = _rnd.Next()};
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RemoveUserCafeLink(link);
            Assert.IsFalse(result);
        }
    }
}