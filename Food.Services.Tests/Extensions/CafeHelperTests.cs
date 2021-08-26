using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using Food.Services.Extensions;

namespace Food.Services.Tests
{
    [TestFixture]
    class CafeHelperTests
    {
        FakeContext _context;

        [SetUp]
        public void SetUp()
        {
            _context = new FakeContext();
            ContextManager.Set(_context);
            ITWebNet.FoodService.Food.DbAccessor.Accessor.SetTestingModeOn(_context);
        }

        [Test]
        public void IsNewUserCafeLinkAvailableTest()
        {
            var user = UserFactory.CreateUser();
            var cafe = CafeFactory.Create(user);
            var anotherCafe = CafeFactory.Create(user);

            var existingLink = CafeManagerFactory.Create(user, cafe);
            Assert.Catch(() => CafeServiceHelper.IsNewUserCafeLinkAvailable(existingLink));

            var nonExistentLink = new CafeManager
            {
                UserId = user.Id,
                CafeId = anotherCafe.Id,
                Cafe = cafe,
                User = user,
                CreationDate = DateTime.Now,
                IsDeleted = false
            };
            Assert.AreEqual(CafeServiceHelper.IsNewUserCafeLinkAvailable(nonExistentLink), true);
        }
    }
}
