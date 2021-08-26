using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using NUnit.Framework;
using System.Linq;

namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    [SingleThreaded]
    class CompanyCuratorTests
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
        public void AddCompanyCurator_Test()
        {
            var ccModel = CompanyCuratorFactory.CreateAccessorModel(CompanyCuratorFactory.Create());
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.AddCompanyCurator(ccModel, _user.Id);
            var lstCC = _context.CompanyCurators.ToList();
            //
            Assert.IsTrue(response);
            Assert.IsTrue(lstCC.Count == 1);
        }

        [Test]
        public void DeleteCompanyCurator_Test()
        {
            var lstCC = CompanyCuratorFactory.CreateFew(count: 3, saveDB: true);
            var firstCC = lstCC.First();
            _context.SaveChanges();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.DeleteCompanyCurator(firstCC.CompanyId, firstCC.UserId, _user.Id);
            //
            Assert.IsTrue(response);
            Assert.IsTrue(firstCC.IsDeleted);
        }

        /// <summary>
        /// Пользователь является куратором
        /// </summary>
        [Test]
        public void IsUserCuratorOfCafe_Test()
        {
            var lstCC = CompanyCuratorFactory.CreateFew(count: 3, saveDB: true);
            var firstCC = lstCC.First();
            _context.SaveChanges();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.IsUserCuratorOfCafe(firstCC.UserId, firstCC.CompanyId);
            //
            Assert.IsTrue(response);
        }

        /// <summary>
        /// Пользователь НЕ является куратором
        /// </summary>
        [Test]
        public void IsUserCuratorOfCafe_Test2()
        {
            var lstCC = CompanyCuratorFactory.CreateFew(count: 3, saveDB: true);
            var firstCC = lstCC.First();
            _context.SaveChanges();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.IsUserCuratorOfCafe(firstCC.UserId+1, firstCC.CompanyId);
            //
            Assert.IsFalse(response);
        }

        /// <summary>
        /// Пользователь имеет курирущую компанию
        /// </summary>
        [Test]
        public void GetCurationCompany_Test()
        {
            var lstCC = CompanyCuratorFactory.CreateFew(count: 3, saveDB: true);
            var firstCC = lstCC.First();
            _context.SaveChanges();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCurationCompany(firstCC.UserId);
            //
            Assert.IsNotNull(response);
        }

        /// <summary>
        /// Пользователь НЕ имеет курирущую компанию
        /// </summary>
        [Test]
        public void GetCurationCompany_Test2()
        {
            var lstCC = CompanyCuratorFactory.CreateFew(count: 3, saveDB: true);
            var firstCC = lstCC.First();
            _context.SaveChanges();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCurationCompany(firstCC.UserId + 1);
            //
            Assert.IsNull(response);
        }

        /// <summary>
        /// Пользователь привязан как куратор к компании
        /// </summary>
        [Test]
        public void GetCompanyCurator_Test()
        {
            var lstCC = CompanyCuratorFactory.CreateFew(count: 3, saveDB: true);
            var firstCC = lstCC.First();
            _context.SaveChanges();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCompanyCurator(firstCC.UserId, firstCC.CompanyId);
            //
            Assert.IsNotNull(response);
        }

        /// <summary>
        /// Пользователь НЕ привязан как куратор к компании
        /// </summary>
        [Test]
        public void GetCompanyCurator_Test2()
        {
            var lstCC = CompanyCuratorFactory.CreateFew(count: 3, saveDB: true);
            var firstCC = lstCC.First();
            _context.SaveChanges();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCompanyCurator(firstCC.UserId + 1, firstCC.CompanyId);
            //
            Assert.IsNull(response);
        }
    }
}
