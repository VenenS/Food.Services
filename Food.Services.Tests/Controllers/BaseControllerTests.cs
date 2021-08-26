using Food.Data.Entities;
using Food.Services.Controllers;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
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
    //TODO: Авторизация через соцсети. Удалить
    /*[TestFixture]
    public class BaseControllerTests
    {
        private FakeContext _context;
        private TestController _controller;
        private Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor> _accessor;
        private readonly Random _random = new Random();
        private User _user;

        [SetUp]
        public void SetUp()
        {
            _accessor = new Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor>();
            _context = new FakeContext();
            _controller = new TestController();
            ContextManager.Set(_context);
            _user = UserFactory.CreateUser();
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()));
            Thread.CurrentPrincipal = new GenericPrincipal(identity, null);
        }

        private class TestController : BaseController
        {
            /////IdentityResult требует подключения Microsoft.AspNet.Identity.Core
            //public virtual IHttpActionResult GetErrorResultTest(IdentityResult result)
            //{ }

            public IList<Claim> GetClaimsTest(string loginProvider, string providerKey, string userName)
            {
                ExternalLoginData loginData = new ExternalLoginData()
                {
                    LoginProvider = loginProvider,
                    ProviderKey = providerKey
                };
                if (userName != null)
                    loginData.UserName = userName;

                return loginData.GetClaims();
            }

            public List<string> FromIdentityTest(ClaimsIdentity identity)
            {
                var loginData = ExternalLoginData.FromIdentity(identity);

                return new List<string>()
                {
                    loginData.LoginProvider,
                    loginData.ProviderKey,
                    loginData.UserName
                };
            }
        }

        [Test]
        public void GetClaimsTest()
        {
            string loginProvider = Guid.NewGuid().ToString("n");
            string providerKey = Guid.NewGuid().ToString("n");

            var response = _controller.GetClaimsTest(loginProvider, providerKey, null);
            Assert.IsTrue(response.Count == 2);
            var result = response.ToList();
            Assert.IsTrue(result[0].Value == providerKey &&
                          result[1].Value == loginProvider);

            string userName = Guid.NewGuid().ToString("n");

            response = _controller.GetClaimsTest(loginProvider, providerKey, userName);
            Assert.IsTrue(response.Count == 3);
            result = response.ToList();
            Assert.IsTrue(result[2].Value == userName);
        }

        [Test]
        public void FromIdentityTest()
        {
            string name = Guid.NewGuid().ToString("n");
            string nameIdentifier = Guid.NewGuid().ToString("n");
            string issuer = Guid.NewGuid().ToString("n");

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, name));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, nameIdentifier, null, issuer));
            ClaimsIdentity identity = new ClaimsIdentity(claims);

            var response = _controller.FromIdentityTest(identity);
            Assert.IsTrue(response[0] == issuer &&
                          response[1] == nameIdentifier &&
                          response[2] == name);
        }
    }*/
}
