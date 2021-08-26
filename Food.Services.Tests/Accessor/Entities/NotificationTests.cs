using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using ITWebNet.FoodService.Food.DbAccessor;
using NUnit.Framework;
using System.Linq;

namespace Food.Services.Tests.Accessor.Entities
{
    [TestFixture]
    [SingleThreaded]
    class NotificationTests
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
        public void GetNotificationById_Test()
        {
            var lstNotifications = NotificationFactory.CreateFew(count: 3);
            var checkNotififcation = lstNotifications.Last();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetNotificationById(checkNotififcation.Id);
            //
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Id == checkNotififcation.Id);
        }

        [Test]
        public void GetNotificationsToCafe_Test()
        {
            var cafe = CafeFactory.Create();

            var lstNotifications = NotificationFactory.CreateFew(count: 3, cafe: cafe);
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetNotificationsToCafe(cafe.Id);
            //
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Count == lstNotifications.Count);
        }

        [Test]
        public void GetNotificationsToUser_Test()
        {
            var user = UserFactory.CreateUser();
            var lstNotifications = NotificationFactory.CreateFew(count: 3, user: user);
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetNotificationsToUser(user.Id);
            //
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Count == lstNotifications.Count);
        }

        [Test]
        public void GetNotificationHistory_Test()
        {
            var lstNotifications = NotificationFactory.CreateFew(count: 3);
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.GetNotificationHistory(null, NotificationChannelEnum.Default, NotificationTypeEnum.Default);
            //
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Count == lstNotifications.Count);
        }

        [Test]
        public void AddNotification_Test()
        {
            var notification = NotificationFactory.Create();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.AddNotification(notification);
            //
            Assert.IsTrue(response > 0);
        }

        [Test]
        public void RemoveNotification_Test()
        {
            var lstNotifications = NotificationFactory.CreateFew(count:3);
            var delNotification = lstNotifications.Last();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.RemoveNotification(delNotification.Id);
            //
            Assert.IsTrue(response);
        }

        [Test]
        public void UpdateNotification_Test()
        {
            var lstNotifications = NotificationFactory.CreateFew(count: 3);
            var updateNotification = lstNotifications.Last();
            //
            var response = ITWebNet.FoodService.Food.DbAccessor.Accessor.Instance.UpdateNotification(updateNotification);
            //
            Assert.IsTrue(response);
        }
    }
}
