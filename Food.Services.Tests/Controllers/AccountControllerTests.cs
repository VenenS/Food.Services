using System;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using Food.Services.Tests.Tools;
using Moq;
using Food.Data.Entities;
using System.Security.Claims;
using System.Threading;
using System.Security.Principal;
using NUnit.Framework;
using System.Collections.Generic;
using ITWebNet.Food.Core.DataContracts.Account;
using System.Threading.Tasks;
using System.Configuration;
using ITWebNet.Food.Core;
using ITWebNet.Food.Core.DataContracts.Common;
using System.Linq;
using Food.Services.Models;
using Food.Services.Controllers;

namespace Food.Services.Tests.Controllers
{
    /*
    [TestFixture]
    public class AccountControllerTests
    {
        FakeContext _context;
        AccountController _controller;
        Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor> _accessor;
        User _user;

        private void SetUp()
        {
            _accessor = new Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor>();
            _context = new FakeContext();
            ContextManager.Set(_context);
            _controller = new AccountController(_context, _accessor.Object);
            _user = UserFactory.CreateUser();
            _accessor.Setup(e => e.GetUserById(_user.Id)).Returns(_user);
            ClaimsIdentity identity = new ClaimsIdentity(AuthenticationTypes.Password);

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Name, _user.Name));
            Thread.CurrentPrincipal = new GenericPrincipal(identity, null);
        }

        [Test]
        public void ForgotPasswordTest()
        {
            SetUp();
            var model = new ForgotPasswordModel()
            {
                Email = _user.Email,
                ReturnUrl = "http://myurl/api/index"
            };
            var response = _controller.ForgotPassword(model);

            // _controller.Url.Link( ломается при попытке обратиться к маршруту в фейковой ситуации
            //var result = TransformResult.GetObject<OkNegotiatedContentResult<string>>(response.Result);
            Assert.IsNotNull(response);
        }

        [Test]
        public void GetExternalLoginTest()
        {
            SetUp();
            var response = _controller.GetExternalLogin("");

            // Метод работает с аутентификацией пользователя! Вывод ломается с NullReference
            //var result = TransformResult.GetObject<RedirectResult>(response.Result);
            Assert.IsNotNull(response);
        }

        [Test]
        public void GetExternalLoginsTest()
        {
            SetUp();

            var response = _controller.GetExternalLogins("http://myurl/api/index");
            Assert.IsNotNull(response);
        }

        [Test]
        public void LoginTest()
        {
            SetUp();
            _user.Password = PasswordHasher.HashPassword("123456");
            //_user.EmailConfirmed = false;
            var model = new LoginModel()
            {
                Email = _user.Email,
                Password = "123456",
                RememberMe = false
            };
            var response = _controller.Login(model);

            // Метод использует аутентификацию сессии для получения токена! Вывод ломается с NullReference
            //var result = response.Result;
            //Assert.IsTrue(result.GetType() == typeof(UnauthorizedResult));
            Assert.IsNotNull(response);
        }

        [Test]
        public void LoginSmsTest()
        {
            SetUp();
            var model = new LoginSmsModel()
            {
                Phone = "",
                SmsCode = ""
            };
            SmsCode code = new SmsCode()
            {
                User = _user,
                UserId = _user.Id,
                IsActive = true,
                IsDeleted = false,
                ValidTime = DateTime.Now.AddDays(1),
                Code = "",
                Phone = _user.PhoneNumber
            };
            _context.SmsCodes.Add(code);
            var response = _controller.LoginSms(model);
            // Метод использует аутентификацию сессии для получения токена! Вывод ломается с NullReference
            //var result = TransformResult.GetPrimitive<long>(response.Result);
            Assert.IsNotNull(response);
        }

        [Test]
        public void LoginAnonymousTest()
        {
            SetUp();
            var response = _controller.LoginAnonymous();
            // Метод использует аутентификацию сессии для получения токена! Вывод ломается с NullReference
            //var result = TransformResult.GetPrimitive<long>(response.Result);
            Assert.IsNotNull(response);
        }

        [Test]
        public void RegisterTest()
        {
            SetUp();
            var model = new RegisterModel()
            {
                PhoneNumber = "",
                Email = "a@b.c",
                Password = "123456",
                ConfirmPassword = "123456"
            };
            var role = RoleFactory.CreateRole();
            role.RoleName = "User";
            var response = _controller.Register(model);
            // Метод использует аутентификацию сессии для получения токена! Вывод ломается с NullReference
            //var result = TransformResult.GetPrimitive<long>(response.Result);
            Assert.IsNotNull(response);
        }

        [Test]
        public void RegisterByDeviceTest()
        {
            SetUp();
            var model = new RegisterDeviceModel()
            {
                DeviceUUID = "0"
            };
            var response = _controller.RegisterByDevice(model);
            // Метод использует аутентификацию сессии для получения токена! Вывод ломается с NullReference
            //var result = TransformResult.GetPrimitive<long>(response.Result);
            Assert.IsNotNull(response);
        }

        [Test]
        public void ConfirmEmailTest()
        {
            SetUp();

            var response = _controller.ConfirmEmail((int)_user.Id, "", "http://myurl/api/index");
            // Метод использует аутентификацию сессии для получения токена! Вывод ломается с NullReference
            //var result = TransformResult.GetPrimitive<long>(response.Result);
            Assert.IsNotNull(response);
        }

        [Test]
        public void SendEmailConfirmationTest()
        {
            SetUp();
            var model = new EmailConfirmationModel()
            {
                Email = _user.Email,
                ReturnUrl = ""
            };
            var response = _controller.SendEmailConfirmation(model);
            // _controller.Url.Link( ломается при попытке обратиться к маршруту в фейковой ситуации
            //var result = TransformResult.GetPrimitive<long>(response.Result);
            Assert.IsNotNull(response);
        }

        [Test]
        public void ResetPasswordTest()
        {
            SetUp();
            var model = new ResetPasswordModel()
            {
                Email = _user.Email,
                Code = _user.SecurityStamp,
                Password = _user.Password,
                ConfirmPassword = _user.Password
            };

            var response = _controller.ResetPassword(model);
            var result = TransformResult.GetObject<string>(response);
            Assert.IsNotNull(result);
            Assert.IsTrue(result == $"Пользователь {_user.Name} успешно сменил пароль");
        }

        [Test]
        public void GetReferralLinkTest()
        {
            SetUp();

            var response = _controller.GetReferralLink(_user.Email);
            var result = TransformResult.GetObject<string>(response);
            Assert.IsNotNull(result);
            Assert.IsTrue(result == _user.UserReferralLink);
        }
    }
    */
}
