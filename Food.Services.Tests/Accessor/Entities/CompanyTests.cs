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
    public class CompanyTests
    {
        [SetUp]
        public void SetUp()
        {
            _context = new FakeContext();
            ContextManager.Set(_context);
            ITWebNet.FoodService.Food.DbAccessor.Accessor.SetTestingModeOn(_context);
            _user = UserFactory.CreateUser();
        }

        private FakeContext _context;
        private User _user;

        [Test]
        public void AddCompanyTest()
        {
            var model = new Company {Name = Guid.NewGuid().ToString("N")};
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.AddCompany(model);
            Assert.IsTrue(result);
            Assert.IsNotNull(ContextManager.Get().Companies.FirstOrDefault(e => e.Name == model.Name));
        }

        [Test]
        public void EditCompanyTest()
        {
            var company = CompanyFactory.Create();
            var changes = new Company {FullName = Guid.NewGuid().ToString("N"), Id = company.Id};
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.EditCompany(changes);
            Assert.IsTrue(changes.FullName == company.FullName);
            Assert.IsTrue(result);
            Assert.IsTrue(company.LastUpdDate.Value.Date == DateTime.Now.Date);
        }

        [Test]
        public void GetCompaniesTest()
        {
            var companies = CompanyFactory.CreateFew();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCompanies();
            Assert.IsTrue(result.Count >= companies.Count);
        }

        [Test]
        public void GetCompaniesTest_NotDeleted()
        {
            var companies = CompanyFactory.CreateFew();
            companies.First().IsDeleted = true;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCompanies();
            Assert.IsNull(result.FirstOrDefault(e => e.IsDeleted));
        }

        [Test]
        public void GetCompaniesTest_OnlyActive()
        {
            var companies = CompanyFactory.CreateFew();
            companies.First().IsActive = false;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCompanies();
            Assert.IsNull(result.FirstOrDefault(e => !e.IsActive));
        }

        [Test]
        public void GetCompanyByIdTest()
        {
            var company = CompanyFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCompanyById(company.Id);
            Assert.IsTrue(company == result);
        }

        [Test]
        public void GetCompanysTest()
        {
            var companies = CompanyFactory.CreateFew();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCompanys();
            Assert.IsTrue(result.Count >= companies.Count);
        }

        [Test]
        public void GetCompanysTest_NotDeleted()
        {
            var companies = CompanyFactory.CreateFew();
            companies.First().IsDeleted = true;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCompanys();
            Assert.IsNull(result.FirstOrDefault(e => e.IsDeleted));
        }

        [Test]
        public void GetCompanysTest_OnlyActive()
        {
            var companies = CompanyFactory.CreateFew();
            companies.First().IsActive = false;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCompanys();
            Assert.IsNull(result.FirstOrDefault(e => !e.IsActive));
        }

        [Test]
        public void GetInactiveCompanies_OnlyDeleted()
        {
            var companies = CompanyFactory.CreateFew();
            companies.First().IsActive = false;
            companies.First().IsDeleted = true;
            companies.Last().IsDeleted = true;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetInactiveCompanies();
            Assert.IsNull(result.FirstOrDefault(e => !(!e.IsActive && e.IsDeleted)));
        }

        [Test]
        public void GetInactiveCompanies_OnlyInactiveAndDeleted()
        {
            var companies = CompanyFactory.CreateFew();
            companies.First().IsActive = false;
            companies.First().IsDeleted = true;
            companies.Last().IsActive = false;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetInactiveCompanies();
            Assert.IsNull(result.FirstOrDefault(e => !(!e.IsActive && e.IsDeleted)));
        }

        [Test]
        public void RemoveCompanyTest()
        {
            var company = CompanyFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RemoveCompany(company.Id, _user.Id);
            Assert.IsTrue(company.IsDeleted);
            Assert.IsTrue(result);
            Assert.IsTrue(company.LastUpdateByUserId == _user.Id);
            Assert.IsTrue(company.LastUpdDate.Value.Date == DateTime.Now.Date);
        }

        [Test]
        public void RestoreCompanyTest()
        {
            var company = CompanyFactory.Create();
            company.IsActive = false;
            company.IsDeleted = true;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RestoreCompany(company.Id, _user.Id);
            Assert.IsTrue(result);
            Assert.IsTrue(company.IsActive);
            Assert.IsFalse(company.IsDeleted);
        }
    }
}