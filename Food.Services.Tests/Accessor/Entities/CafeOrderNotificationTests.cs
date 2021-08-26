using System;
using System.Linq;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using ITWebNet.FoodService.Food.DbAccessor;
using NUnit.Framework;

namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    public class CafeOrderNotificationTests
    {
        public void SetUp()
        {
            _context = new FakeContext();
            ContextManager.Set(_context);
            ITWebNet.FoodService.Food.DbAccessor.Accessor.SetTestingModeOn(_context);
        }

        private FakeContext _context;
        private readonly Random _rnd = new Random();

        [Test]
        public void GetCafeIdWithNewOrdersTest_Success()
        {
            SetUp();
            var notification = CafeOrderNotificationFactory.Create();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafeIdWithNewOrders(notification.UserId);
            Assert.IsTrue(result == notification.CafeId);
        }

        [Test]
        public void GetCafeIdWithNewOrdersTest_Date_Is_In_Future()
        {
            SetUp();
            var notification = CafeOrderNotificationFactory.Create();
            notification.DeliverDate = DateTime.MaxValue;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafeIdWithNewOrders(notification.UserId);
            Assert.IsTrue(result < 0);
        }

        [Test]
        public void GetCafeIdWithNewOrdersTest_Wrong_UserId()
        {
            SetUp();
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetCafeIdWithNewOrders(_rnd.Next(999, int.MaxValue));
            Assert.IsTrue(result < 0);
        }

        [Test]
        public void StopNotifyUserTest_Success()
        {
            SetUp();
            var user = UserFactory.CreateUser();
            CafeOrderNotificationFactory.CreateFew(user:user);
            ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.StopNotifyUser(user.Id);
            Assert.IsNull(ContextManager.Get().CafeOrderNotifications.FirstOrDefault(e => e.UserId == user.Id));
        }

        [Test]
        public void StopNotifyUserTest_Future_Notifs_Should_Be_Alive()
        {
            SetUp();
            var user = UserFactory.CreateUser();
            var notifs = CafeOrderNotificationFactory.CreateFew(user: user);
            notifs.First().DeliverDate = DateTime.MaxValue;;
            ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.StopNotifyUser(user.Id);
            Assert.IsTrue(ContextManager.Get().CafeOrderNotifications.Count(e => e.UserId == user.Id) == 1);
        }

        [Test]
        public void SetOrdersOfCafeViewedTest_Success()
        {
            var cafe = CafeFactory.Create();
            CafeOrderNotificationFactory.CreateFew(cafe: cafe);
            ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.SetOrdersOfCafeViewed(cafe.Id);
            Assert.IsNull(ContextManager.Get().CafeOrderNotifications.FirstOrDefault(e => e.CafeId == cafe.Id));
        }

        [Test]
        public void SetOrdersOfCafeViewedTest_Future_Notifs_Should_Be_Alive()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var notifs = CafeOrderNotificationFactory.CreateFew(cafe: cafe);
            notifs.First().DeliverDate = DateTime.MaxValue; ;
            ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.SetOrdersOfCafeViewed(cafe.Id);
            Assert.IsTrue(ContextManager.Get().CafeOrderNotifications.Count(e => e.CafeId == cafe.Id) == 1);
        }

        [Test]
        public void NewOrderForNotificationTest_Right_Return()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var managers = CafeManagerFactory.CreateFew(cafe:cafe);
            CafeManagerFactory.Create(); //проверим, что лишних не уведомляет
            var date = DateTime.Now.Date;
            var result = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.NewOrderForNotification(cafe.Id, date);
            var resultNames = "";
            result.ForEach(e => resultNames += e);
            var managerNames = "";
            managers.ForEach(e => managerNames += e.User.Name);
            Assert.IsTrue(resultNames == managerNames);
        }

        [Test]
        public void NewOrderForNotificationTest_Write_To_Base_Success()
        {
            SetUp();
            var cafe = CafeFactory.Create();
            var managers = CafeManagerFactory.CreateFew(cafe: cafe);
            var date = DateTime.Now.Date;
            ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.NewOrderForNotification(cafe.Id, date);
            var countCreatedNotifications = ContextManager.Get().CafeOrderNotifications.Count(e => e.CafeId == cafe.Id);
            Assert.IsTrue(countCreatedNotifications == managers.Count);
        }
    }
}