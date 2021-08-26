using Food.Services.Services;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using ITWebNet.Food.Core.DataContracts.Common;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Tests
{
    [TestFixture]
    public class CommonSMSNotificationsTests
    {
        private FakeContext _context;

        [SetUp]
        public void SetUp()
        {
            _context = new FakeContext();
            ITWebNet.FoodService.Food.DbAccessor.Accessor.SetTestingModeOn(_context);
            ContextManager.Set(_context);
        }

        [Test]
        public async Task InformAboutCancelledOrdersTest()
        {
            var userA = UserFactory.CreateUser();
            var ordersA = OrderFactory.CreateFew(3, userA, null, null, userA);
            var userB = UserFactory.CreateUser();
            var ordersB = OrderFactory.CreateFew(3, userB, null, null, userB);
            var userC = UserFactory.CreateUser();
            var ordersC = OrderFactory.CreateFew(3, userC, null, null, userC);

            ordersA[0].PhoneNumber = "+71231231212";
            ordersA[1].PhoneNumber = "+79999999999";
            ordersA[2].PhoneNumber = "+79999999999";

            userA.PhoneNumber = "+70000000000";
            userA.PhoneNumberConfirmed = true;
            userA.SmsNotify = true;
            userB.PhoneNumber = "+71111111111";
            userB.PhoneNumberConfirmed = true;
            userB.SmsNotify = true;
            userC.PhoneNumber = "+72222222222";
            userC.PhoneNumberConfirmed = true;
            userC.SmsNotify = false;

            var service = new Mock<ISmsSender>();
            service
                .Setup(x => x.SmsSend(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(Task.FromResult(new ResponseModel { Status = 0 }));

            bool seenA = false, seenB = false;
            await CommonSmsNotifications.InformAboutCancelledOrders(
                service.Object,
                ordersA.Concat(ordersB).Concat(ordersB).Concat(ordersC),
                (u, os) =>
                {
                    seenA = u.Id == userA.Id ? true : seenA;
                    seenB = u.Id == userB.Id ? true : seenB;

                    if (u.Id == userA.Id)
                    {
                        // Сообщения должны группироваться по номеру телефона, убедиться
                        // что сообщения пользователю userA были корректно сгруппированы.
                        Assert.IsTrue(os.Count() == 2 || os.Count() == 1);

                        var osList = os.ToList();
                        for (var i = 1; i < osList.Count(); i++)
                        {
                            Assert.AreEqual(osList[i - 1].PhoneNumber, osList[i].PhoneNumber);
                        }

                        // Проверить если переданные заказы соответствуют заказам пользователя
                        // по номеру телефона.
                        Assert.IsTrue(os.Select(x => x.PhoneNumber).Intersect(ordersA.Select(x => x.PhoneNumber)).Any());
                    }
                    else
                    {
                        Assert.AreEqual(3, os.Count());
                    }

                    Assert.AreNotEqual(u.Id, userC.Id);
                    return "Test";
                }
            );
            Assert.IsTrue(seenA && seenB);
        }
    }
}
