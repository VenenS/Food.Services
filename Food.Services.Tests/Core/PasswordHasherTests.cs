using System;
using ITWebNet.Food.Core;
using NUnit.Framework;

namespace Food.Services.Tests.Core
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class PasswordHasherTests
    {
        [Test]
        public void HashPassword_Success()
        {
            var pass = Guid.NewGuid().ToString("N");
            var secured = PasswordHasher.HashPassword(pass);
            Assert.IsTrue(!string.IsNullOrEmpty(secured));
        }

        [Test]
        public void VerifyHashedPasswordTest()
        {
            var pass = Guid.NewGuid().ToString("N");
            var secured = PasswordHasher.HashPassword(pass);
            var decoded = PasswordHasher.VerifyHashedPassword(secured, pass);
            Assert.IsTrue(decoded);
        }
    }
}