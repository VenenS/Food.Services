using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using NUnit.Framework;
using System.Linq;

namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    [SingleThreaded]
    class CostOfDeliveryTests
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
        public void GetListOfDeliveryCosts_Test()
        {
            var cafe = CafeFactory.Create();
            CostOfDeliveryFactory.CreateFew(cafe, count: 3);
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetListOfDeliveryCosts(cafe.Id);
            //
            Assert.IsTrue(response.Count == 3);
        }

        [Test]
        public void GetListOfDeliveryCosts2_Test()
        {
            var cafe = CafeFactory.Create();
            CostOfDeliveryFactory.CreateFew(cafe, count: 3);
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetListOfDeliveryCosts(cafe.Id, -100);
            //
            Assert.NotNull(response);
        }

        [Test]
        public void AddNewCostOfDelivery_Test()
        {
            var entity = CostOfDeliveryFactory.Create(CafeFactory.Create());
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.AddNewCostOfDelivery(entity);
            //
            Assert.NotZero(response);
        }

        /// <summary>
        /// Успешное редактирование стоимости доставки
        /// </summary>
        [Test]
        public void EditCostOfDelivery_Test()
        {
            var entity = CostOfDeliveryFactory.Create(CafeFactory.Create());
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.EditCostOfDelivery(entity);
            //
            Assert.IsTrue(response);
        }

        /// <summary>
        /// Ошибка при редактировании стоимости доставки
        /// </summary>
        [Test]
        public void EditCostOfDelivery_Test2()
        {
            var entity = CostOfDeliveryFactory.Create(CafeFactory.Create());
            entity.IsDeleted = true;
            _context.SaveChanges();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.EditCostOfDelivery(entity);
            //
            Assert.IsFalse(response);
        }

        /// <summary>
        /// Успешное удаление стоимости доставки
        /// </summary>
        [Test]
        public void RemoveCostOfDelivery_Test()
        {
            var entity = CostOfDeliveryFactory.Create(CafeFactory.Create());
            entity.IsDeleted = false;
            _context.SaveChanges();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RemoveCostOfDelivery(entity.Id);
            //
            Assert.IsTrue(response);
        }

        /// <summary>
        /// Ошибка при удалении стоимости доставки (не существует)
        /// </summary>
        [Test]
        public void RemoveCostOfDelivery_Test2()
        {
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RemoveCostOfDelivery(1);
            //
            Assert.IsFalse(response);
        }

        [Test]
        public void GetCostOfDeliveryById_Test()
        {
            var lstEntities = CostOfDeliveryFactory.CreateFew(CafeFactory.Create(), count: 3);
            var checkEntity = lstEntities.First();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCostOfDeliveryById(checkEntity.Id);
            //
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Id == checkEntity.Id);
        }
    }
}
