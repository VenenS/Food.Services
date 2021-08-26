using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using NUnit.Framework;
using System.Linq;

namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    [SingleThreaded]
    class LogMessageTests
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
        public void WriteShedulerError()
        {
            string text = "Мое сообщение";
            //
            ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.WriteShedulerError(text);
            var message = _context.LogMessages.First();
            //
            Assert.IsNotNull(message);
            Assert.IsTrue(message.Text == text);
        }
    }
}
