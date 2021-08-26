using System;
using System.Linq;
using Food.Data.Entities;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using NUnit.Framework;


namespace ITWebNet.FoodService.Food.DbAccessor.Tests
{
    [TestFixture]
    public class CafeNotificationContactTests
    {
        [SetUp]
        public void SetUp()
        {
            _context = new FakeContext();
            ContextManager.Set(_context);
            Accessor.SetTestingModeOn(_context);
        }

        private FakeContext _context;
        private readonly Random _rnd = new Random();

        [Test]
        public void GetCafeNotificationContactByIdTest()
        {
            var contact = CafeNotificationContactFactory.Create();
            var result = Accessor.Instance.GetCafeNotificationContactById(contact.Id);
            Assert.IsTrue(contact == result);
        }

        [Test]
        public void GetCafeNotificationContactByIdTest_Not_Exists()
        {
            var result = Accessor.Instance.GetCafeNotificationContactById(_rnd.Next(9999,int.MaxValue));
            Assert.IsNull(result);
        }

        [Test]
        public void GetCafeNotificationContactByCafeIdTest()
        {
            var cafe = CafeFactory.Create();
            var contacts = CafeNotificationContactFactory.CreateFew(cafe: cafe);
            var result = Accessor.Instance.GetCafeNotificationContactByCafeId(cafe.Id, null);
            Assert.IsTrue(result.Sum(e => e.Id) == contacts.Sum(e => e.Id));
        }

        [Test]
        public void GetCafeNotificationContactByCafeIdTest_With_Channel()
        {
            var cafe = CafeFactory.Create();
            var channel = NotificationChannelFactory.Create();
            var contacts = CafeNotificationContactFactory.CreateFew(cafe: cafe, notificationChannel:channel);
            CafeNotificationContactFactory.Create(cafe: cafe);
            var result = Accessor.Instance.GetCafeNotificationContactByCafeId(cafe.Id,(NotificationChannelEnum?) channel.Id);
            Assert.IsTrue(result.Sum(e => e.Id) == contacts.Sum(e => e.Id));
        }

        [Test]
        public void AddCafeNotificationContactTest()
        {
            var conact = new CafeNotificationContact
            {
                Id = _rnd.Next(9999, int.MaxValue)
            };
            var result = Accessor.Instance.AddCafeNotificationContact(conact);
            Assert.IsTrue(conact.Id == result);
        }

        [Test]
        public void RemoveCafeNotificationContactTest()
        {
            var contact = CafeNotificationContactFactory.Create();
            var result = Accessor.Instance.RemoveCafeNotificationContact(contact.Id);
            Assert.IsTrue(result);
            Assert.IsTrue(contact.IsDeleted);
        }

        [Test]
        public void RemoveCafeNotificationContactTest_Already_Deleted()
        {
            var contact = CafeNotificationContactFactory.Create();
            contact.IsDeleted = true;
            var result = Accessor.Instance.RemoveCafeNotificationContact(contact.Id);
            Assert.IsFalse(result);
        }

        [Test]
        public void UpdateCafeNotificationContactTest()
        {
            var contact = CafeNotificationContactFactory.Create();
            var model = new CafeNotificationContact
            {
                Id = contact.Id,
                CafeId = contact.CafeId,
                NotificationChannelId = (short)_rnd.Next(),
                NotificationContact = Guid.NewGuid().ToString("N")
            };
            var result = Accessor.Instance.UpdateCafeNotificationContact(model);
            Assert.IsTrue(contact.NotificationChannelId == model.NotificationChannelId);
            Assert.IsTrue(contact.NotificationContact == model.NotificationContact);
        }
    }
}