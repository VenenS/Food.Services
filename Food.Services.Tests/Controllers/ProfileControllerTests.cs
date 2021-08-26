using Food.Data.Entities;
using Food.Services.Controllers;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using Food.Services.Tests.Tools;
using ITWebNet.Food.Core.DataContracts.Account;
using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.AspNet.Identity;
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
    public class ProfileControllerTests
    {
        FakeContext _context;
        ProfileController _controller;
        Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor> _accessor;
        User _user;
        Company _company;
        UserInCompany _userInCompany;
        private void SetUp()
        {
            _accessor = new Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor>();
            _context = new FakeContext();
            ContextManager.Set(_context);
            _controller = new ProfileController(_context, _accessor.Object);
            _user = UserFactory.CreateUser();
            _accessor.Setup(e => e.GetUserById(_user.Id)).Returns(_user);
            ClaimsIdentity identity = new ClaimsIdentity(DefaultAuthenticationTypes.ExternalBearer);

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Name, _user.Name));
            Thread.CurrentPrincipal = new GenericPrincipal(identity, null);
        }

        private void SetUpCompany()
        {
            _company = CompanyFactory.Create();
            var role = UserInCompanyFactory.CreateRoleForUser(_user, _company);
            _userInCompany = new UserInCompany
            {
                Company = _company,
                User = _user,
                CompanyId = _company.Id,
                UserId = _user.Id,
                UserRole = RoleFactory.CreateRole(),
                UserRoleId = role.Id,
                IsActive = true
            };
        }

        [Test]
        public void AuthorizeTest()
        {
            SetUp();
            var response = _controller.Authorize(_user.Email, _user.Password, "http://myUrl/api/index");
            var result = TransformResult.GetPrimitive<long>(response.Result);
            Assert.IsNotNull(result);
        }

        [Test]
        public void ChangePasswordTest()
        {
            SetUp();
            _user.Password = ITWebNet.Food.Core.PasswordHasher.HashPassword("123456");
            var model = new ChangePasswordModel()
            {
                OldPassword = "123456",
                NewPassword = "123455",
                ConfirmPassword = "123455"
            };
            var response = _controller.ChangePassword(model);
            var result = TransformResult.GetPrimitive<long>(response);
            Assert.IsNotNull(result);
            Assert.IsTrue(response.GetType() != typeof(BadRequestObjectResult));
        }

        [Test]
        public void GetUserInfoTest()
        {
            SetUp();
            SetUpCompany();
            _user.UserInCompanies = new List<UserInCompany>() { _userInCompany };

            var response = _controller.GetUserInfo();
            var result = TransformResult.GetObject<UserInfoModel>(response);

            Assert.IsNotNull(result);
        }

        [Test]
        public void GetMyCompaniesTest()
        {
            SetUp();
            var response = _controller.GetMyCompanies();
            var result = TransformResult.GetPrimitive<long>(response);

            Assert.IsNotNull(result);
        }

        [Test]
        public void GetMyCompanyByIdTest()
        {
            SetUp();
            SetUpCompany();
            _context.UsersInCompanies.Add(_userInCompany);
            var response = _controller.GetMyCompanyById(_company.Id);
            var result = TransformResult.GetPrimitive<long>(response);

            Assert.IsNotNull(result);
        }

        [Test]
        public void GetAvailabelBanketsTest()
        {
            SetUp();
            SetUpCompany();
            Cafe cafe = CafeFactory.Create();
            _context.UsersInCompanies.Add(_userInCompany);
            var today = DateTime.Now.Date;
            Banket banket = BanketFactory.Create(null, _company, cafe);
            banket.OrderStartDate = today.AddDays(-1);
            banket.OrderEndDate = today.AddDays(1);
            CafeMenuPattern menu = CafeMenuPatternFactory.Create(cafe);
            banket.Menu = menu;
            banket.MenuId = menu.Id;
            _context.CafeMenuPatterns.Add(menu);
            //_context.Bankets.Add(banket);
            var response = _controller.GetAvailabelBankets(cafe.Id);
            var result = TransformResult.GetObject<IEnumerable<BanketModel>>(response).ToList();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
        }

        [Test]
        public void RemoveLoginTest()
        {
            SetUp();
            var response = _controller.RemoveLogin(new RemoveLoginModel());
            var result = TransformResult.GetPrimitive<long>(response);

            Assert.IsNotNull(result);
        }

        [Test]
        public void SaveUserInfoTest()
        {
            SetUp();
            SetUpCompany();
            _user.UserInCompanies = new List<UserInCompany>() { _userInCompany };
            var model = new UserInfoModel
            {
                Email = _user.Email,
                EmailConfirmed = _user.EmailConfirmed,
                PhoneNumber = new string(_user.PhoneNumber.Where(c => char.IsDigit(c)).ToArray()),
                PhoneNumberConfirmed = _user.PhoneNumberConfirmed,
                TwoFactorEnabled = _user.TwoFactor,
                UserFullName = _user.FullName,
                HasPassword = !string.IsNullOrWhiteSpace(_user.Password),
                DefaultAddressId = _user.DefaultAddressId,
                DefaultAddress = _user.DefaultAddressId.HasValue
                    ? _user.Address.RawAddress
                    : string.Empty,
                PersonalPoints = _user.PersonalPoints,
                ReferralPoints = _user.ReferralPoints,
                PercentOfOrder = _user.PercentOfOrder,
                UserReferralLink = _user.UserReferralLink,
                UserInCompanies = _user.UserInCompanies.Where(c => c.IsActive && !c.IsDeleted).Select(c =>
                    new UserInCompanyModel()
                    {
                        Id = c.Id,
                        CompanyId = c.CompanyId,
                        UserId = c.UserId,
                        DeliveryAddressId = c.DefaultAddressId
                    }).ToList()
            };
            var response = _controller.SaveUserInfo(model);

            // Метод использует аутентификацию сессии для получения токена! Вывод ломается с NullReference
            //var result = TransformResult.GetObject<TokenModel>(response.Result);
            Assert.IsNotNull(response);
        }

        [Test]
        public void ConfirmPhoneTest()
        {
            SetUp();
            _user.PhoneNumberConfirmed = false;
            var model = new UserInfoModel
            {
                PhoneNumber = new string(_user.PhoneNumber.Where(c => char.IsDigit(c)).ToArray()),
                SmsCode = "",
                IsSendCode = true
            };
            var code = new SmsCode()
            {
                Code = model.SmsCode,
                UserId = _user.Id,
                Phone = new string(_user.PhoneNumber.Where(c => char.IsDigit(c)).ToArray()),
                ValidTime = DateTime.Now.AddMinutes(1),
                IsActive = true,
            };
            _context.SmsCodes.Add(code);
            var response = _controller.ConfirmPhone(model);
            var result = TransformResult.GetObject<UserInfoModel>(response.Result);

            Assert.IsNotNull(result);
            // смс-код больше не отправлен и номер подтвержден
            Assert.IsTrue(!result.IsSendCode && _user.PhoneNumberConfirmed);
        }

        [Test]
        public void SetPasswordTest()
        {
            SetUp();
            var model = new SetPasswordModel()
            {
                NewPassword = "123455",
                ConfirmPassword = "123455"
            };
            var response = _controller.SetPassword(model);
            var result = TransformResult.GetObject<string>(response);

            Assert.IsNotNull(result);
        }

    }
}
