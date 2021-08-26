using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using NUnit.Framework;
using System.Linq;

namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    [SingleThreaded]
    class ExtTests
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

        /// <summary>
        /// Успешный поиск расширения отчета
        /// </summary>
        [Test]
        public void GetExt_Test()
        {
            var lstExt = ExtFactory.CreateFew(count: 3);
            var checkExt = lstExt.Last();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetExt(checkExt.Id);
            //
            Assert.NotNull(response);
            Assert.IsTrue(response.Id == checkExt.Id);
        }

        /// <summary>
        /// Неудачный поиск расширения отчета
        /// </summary>
        [Test]
        public void GetExt_Test2()
        {
            var lstExt = ExtFactory.CreateFew(count: 3);
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetExt(-1);
            //
            Assert.IsNull(response);
        }

        [Test]
        public void AddExt_Test()
        {
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.AddExt(ExtFactory.Create(saveDB: false));
            //
            Assert.IsTrue(response);
            Assert.IsTrue(_context.Ext.Count() == 1);
        }

        [Test]
        public void EditExt_Test()
        {
            var ext = ExtFactory.Create();
            var editExt = ExtFactory.Create(saveDB:false);
            editExt.Name = "NewExt";
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.EditExt(editExt);
            //
            Assert.IsTrue(response);
            Assert.IsTrue(ext.Name == editExt.Name);
        }
    }
}
