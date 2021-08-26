using ITWebNet.FoodService.Food.DbAccessor;
using System;
using System.Linq;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using NUnit.Framework;

namespace AccessorTests.Entites
{
    [TestFixture]
    public class UserCompanyTests
    {
        private FakeContext _context;
        private readonly Random _random = new Random();

        private void SetUp()
        {
            _context = new FakeContext();
            Accessor.SetTestingModeOn(_context);
            ContextManager.Set(_context);
        }

        [Test]
        public void GetListOfCompanysByUserId_Success()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            var result = Accessor.Instance.GetListOfCompanysByUserId(links.UserId);
            Assert.IsTrue(result.Count == 1);
            Assert.True(result.First().Name == links.Company.Name);
        }

        [Test]
        public void GetListOfCompanysByUserId_Only_Active()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            links.IsActive = false;
            var result = Accessor.Instance.GetListOfCompanysByUserId(links.UserId);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetListOfCompanysByUserId_Only_Alive()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            links.IsDeleted = true;
            var result = Accessor.Instance.GetListOfCompanysByUserId(links.UserId);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetListOfCompanysByUserId_Adequate_StartDate()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            links.StartDate = DateTime.MaxValue;
            var result = Accessor.Instance.GetListOfCompanysByUserId(links.UserId);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetListOfCompanysByUserId_Adequate_EndDate()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            links.EndDate = DateTime.MinValue;
            var result = Accessor.Instance.GetListOfCompanysByUserId(links.UserId);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetListOfUserByCompanyId_Success()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            var result = Accessor.Instance.GetListOfUserByCompanyId(links.CompanyId);
            Assert.IsTrue(result.Count == 1);
            Assert.True(result.First().Name == links.User.Name);
        }

        [Test]
        public void GetListOfUserByCompanyId_Only_Active()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            links.IsActive = false;
            var result = Accessor.Instance.GetListOfUserByCompanyId(links.CompanyId);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetListOfUserByCompanyId_Only_Alive()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            links.IsDeleted = true;
            var result = Accessor.Instance.GetListOfUserByCompanyId(links.CompanyId);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetListOfUserByCompanyId_Adequate_StartDate()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            links.StartDate = DateTime.MaxValue;
            var result = Accessor.Instance.GetListOfUserByCompanyId(links.CompanyId);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetListOfUserByCompanyId_Adequate_EndDate()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            links.EndDate = DateTime.MinValue;
            var result = Accessor.Instance.GetListOfUserByCompanyId(links.CompanyId);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void CanUserReadInfoAboutCompanyOrders_Success()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            links.UserType = EnumUserType.Manager;
            var result = Accessor.Instance.CanUserReadInfoAboutCompanyOrders(links.UserId, links.CompanyId);
            Assert.IsTrue(result == true);
        }

        [Test]
        public void CanUserReadInfoAboutCompanyOrders_Only_Active()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            links.UserType = EnumUserType.Manager;
            links.IsActive = false;
            var result = Accessor.Instance.CanUserReadInfoAboutCompanyOrders(links.UserId, links.CompanyId);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void CanUserReadInfoAboutCompanyOrders_Only_Alive()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            links.UserType = EnumUserType.Manager;
            links.IsDeleted = true;
            var result = Accessor.Instance.CanUserReadInfoAboutCompanyOrders(links.UserId, links.CompanyId);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void CanUserReadInfoAboutCompanyOrders_Only_Manager()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            links.UserType = EnumUserType.SomeType;
            var result = Accessor.Instance.CanUserReadInfoAboutCompanyOrders(links.UserId, links.CompanyId);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void CanUserReadInfoAboutCompanyOrders_Only_Actual_StartDate()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            links.UserType = EnumUserType.Manager;
            links.StartDate = DateTime.MaxValue;
            var result = Accessor.Instance.CanUserReadInfoAboutCompanyOrders(links.UserId, links.CompanyId);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void CanUserReadInfoAboutCompanyOrders_Only_Actual_EndDate()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            links.UserType = EnumUserType.Manager;
            links.EndDate = DateTime.MinValue;
            var result = Accessor.Instance.CanUserReadInfoAboutCompanyOrders(links.UserId, links.CompanyId);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void AddUserToCompanyInRole_Already_Exists()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            links.IsDeleted = true;
            links.IsActive = false;
            var result = Accessor.Instance.AddUserToCompanyInRole(links);
            Assert.IsTrue(result == true);
            Assert.IsTrue(links.IsActive == true);
            Assert.IsTrue(links.IsDeleted == false);
        }

        [Test]
        public void AddUserToCompanyInRole_Success()
        {
            SetUp();
            var user = UserFactory.CreateUser();
            var company = CompanyFactory.Create();
            var links = new UserInCompany();
            links.Id = -1;
            links.CompanyId = company.Id;
            links.UserId = user.Id;
            var result = Accessor.Instance.AddUserToCompanyInRole(links);
            Assert.IsTrue(result == true);
            Assert.IsTrue(links.Id >= 0);
        }

        [Test]
        public void EditUserCompanyLink_No_Such_Link()
        {
            SetUp();
            var result = Accessor.Instance.EditUserCompanyLink(_random.Next(), _random.Next(), _random.Next(), _random.Next());
            Assert.IsTrue(result == false);
        }

        [Test]
        public void EditUserCompanyLink_Only_Alive()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            links.IsDeleted = true;
            var result = Accessor.Instance.EditUserCompanyLink(_random.Next(), links.CompanyId, links.UserId, _random.Next());
            Assert.IsTrue(result == false);
        }

        [Test]
        public void EditUserCompanyLink_Only_Active()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            links.IsActive = false;
            var result = Accessor.Instance.EditUserCompanyLink(_random.Next(), links.CompanyId, links.UserId, _random.Next());
            Assert.IsTrue(result == false);
        }

        [Test]
        public void EditUserCompanyLink_Success()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            var author = _random.Next();
            var company = CompanyFactory.Create();
            var result = Accessor.Instance.EditUserCompanyLink(company.Id, links.CompanyId, links.UserId, author);
            Assert.IsTrue(result == true);
            Assert.IsTrue(links.CompanyId == company.Id);
            Assert.IsTrue(links.LastUpdateBy == author);
            Assert.IsTrue(links.LastUpdateDate.Value.ToString("g") == DateTime.Now.ToString("g"));
        }

        [Test]
        public void RemoveUserCompanyLink_No_Such_Link()
        {
            SetUp();
            var links = new UserInCompany
            {
                UserId = _random.Next(),
                CompanyId = _random.Next()
            };
            var result = Accessor.Instance.RemoveUserCompanyLink(links);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void RemoveUserCompanyLink_Only_Alive()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            links.IsDeleted = true;
            var result = Accessor.Instance.RemoveUserCompanyLink(links);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void RemoveUserCompanyLink_Only_Active()
        {
            SetUp();
            var links = UserInCompanyFactory.CreateRoleForUser();
            links.IsActive = false;
            var result = Accessor.Instance.RemoveUserCompanyLink(links);
            Assert.IsTrue(result == false);
        }

        [Test]
        public void RemoveUserCompanyLink_Success()
        {
            SetUp();
            var link = UserInCompanyFactory.CreateRoleForUser();
            var model = UserInCompanyFactory.Clone(link);
            model.LastUpdateBy = _random.Next();
            var result = Accessor.Instance.RemoveUserCompanyLink(model);
            Assert.IsTrue(result == true);
            Assert.IsTrue(link.IsActive == false);
            Assert.IsTrue(link.IsDeleted == true);
            Assert.IsTrue(link.LastUpdateBy == model.LastUpdateBy);
            Assert.IsTrue(link.LastUpdateDate.Value.ToString("g") == DateTime.Now.ToString("g"));
        }
    }
}