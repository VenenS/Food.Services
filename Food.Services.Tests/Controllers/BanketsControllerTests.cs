using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Food.Data.Entities;
using Food.Services.Controllers;
using Food.Services.Extensions.OrderExtensions;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using Food.Services.Tests.Mocks;
using Food.Services.Tests.Tools;
using ITWebNet.Food.Core.DataContracts.Common;
using ITWebNet.Food.Core.DataContracts.Manager;
using ITWebNet.FoodService.Food.DbAccessor;
using Moq;
using NUnit.Framework;

namespace Food.Services.Tests.Controllers
{
    [TestFixture]
    class BanketsControllerTests
    {
        private FakeContext _context;
        private BanketsController _controller;
        private Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor> _accessor;
        private User _user;

        private void SetUp()
        {
            Food.Services.ShedulerQuartz.Scheduler.Instance = new MockScheduler();
            _accessor = new Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor>();
            _context = new FakeContext();
            ContextManager.Set(_context);
            _controller = new BanketsController(_context, _accessor.Object);
            _user = UserFactory.CreateUser();
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()));
            Thread.CurrentPrincipal = new GenericPrincipal(identity, null);
        }

        [Test]
        public async Task PostTest_Manager()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            //Создаем компанию для заказа
            Company company = CompanyFactory.Create(_user);
            //Создаем банкет
            Banket banket = BanketFactory.Create(_user, company, cafe);
            BanketModel banketModel = banket.GetContract();
            //Создаем банкет, но не на сегодня, чтоб не занять день
            banket.EventDate = DateTime.Now.AddDays(1);
            Data.Entities.ScheduledTask sheduledTask = SheduledTaskFactory.Create(_user, banket);
            var responce = await _controller.Post(banketModel);
            var result = TransformResult.GetObject<BanketModel>(responce);

            Assert.IsNotNull(result);
        }

        [Test]
        public void GetTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            //Создаем компанию для заказа
            Company company = CompanyFactory.Create(_user);
            //Создаем банкет
            Banket banket = BanketFactory.Create(_user, company, cafe);

            var responce = _controller.Get();
            var result = TransformResult.GetObject<IEnumerable<BanketModel>>(responce);

            //Результат должен быть не Null
            Assert.IsNotNull(result);
            //Создан 1 банкет, поэтому Count > 0
            Assert.IsTrue(result.Count() > 0);
        }

        [Test]
        public void GetTest2()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            //Создаем компанию для заказа
            Company company = CompanyFactory.Create(_user);
            //Создаем банкет
            Banket banket = BanketFactory.Create(_user, company, cafe);

            var responce = _controller.Get(banket.Id);
            var result = TransformResult.GetObject<BanketModel>(responce);

            //Результат должен быть не Null
            Assert.IsNotNull(result);

            //Вернули непустую модель
            Assert.IsTrue(result.Id >= 0);
        }

        [Test]
        public void GetBanketsByFilterTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            //Создаем компанию для заказа
            Company company = CompanyFactory.Create(_user);
            List<Banket> bankets = BanketFactory.CreateFew(1000, _user, company, cafe);
            //Создаем фильтр, по которому будет выборка
            BanketsFilterModel banketsFilterModel = new BanketsFilterModel
            {
                //по фильтру берем не все Идшники из заказов
                BanketIds = bankets.Where(b => b.Id > 600 || b.Id < 200).Select(b => b.Id).ToList(),
                CafeId = cafe.Id,
                EndDate = DateTime.Now.AddDays(25),
                SearchType = SearchType.SearchByCafe,
                Search = "",
                SortType = ReportSortType.OrderByDate,
                StartDate = DateTime.Now.AddDays(-25)
            };
            for (int i = 0; i < 100; i++)//Отсеиваем первые 100 банкетов по CafeId
                bankets[i].CafeId = i * 100 + cafe.Id + 1;
            for (int i = 100; i < 200; i++)
            {
                if (i % 2 == 0)
                    bankets[i].EventDate = DateTime.Now.AddDays(-(i % 100));
                else
                    bankets[i].EventDate = DateTime.Now.AddDays(i % 100);
            }

            var responce = _controller.GetBanketsByFilter(banketsFilterModel);
            var result = TransformResult.GetObject<List<BanketModel>>(responce);

            //Результат должен быть не Null
            Assert.IsNotNull(result);

            //всего банкетов 1000, по фильтру отсеятся чуть больше 400, остальные должны попасть
            Assert.IsTrue(result.Count > 400);
        }

        [Test]
        public void GetOrderItemsByUserIdTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            //Создаем компанию для заказа
            Company company = CompanyFactory.Create(_user);
            //СОздаем банкет
            Banket banket = BanketFactory.Create(_user, company, cafe);
            //СОздаем заказы
            List<Order> orders = OrderFactory.CreateFew(100, _user, banket, cafe, _user);
            for (int i = 0; i < orders.Count; i++)
            {
                orders[i].OrderItems = OrderItemFactory.CreateFew(i, _user, null, orders[i]);
            }

            var responce = _controller.GetOrderItemsByUserId(banket.Id, _user.Id);
            var result = TransformResult.GetObject<OrderModel>(responce);

            //Результат должен быть не Null
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetOrdersInBanketTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            //Создаем компанию для заказа
            Company company = CompanyFactory.Create(_user);
            //СОздаем банкет
            Banket banket = BanketFactory.Create(_user, company, cafe);
            //СОздаем заказы
            List<Order> orders = OrderFactory.CreateFew(100, _user, banket, cafe, _user);
            for (int i = 0; i < orders.Count; i++)
            {
                orders[i].OrderItems = OrderItemFactory.CreateFew(i, _user, null, orders[i]);
            }
            orders[0].BanketId = banket.Id + 5;
            orders[1].IsDeleted = true;
            var responce = _controller.GetOrdersInBanket(banket.Id);
            var result = TransformResult.GetObject<IEnumerable<OrderModel>>(responce);

            //Результат должен быть не Null
            Assert.IsNotNull(result);

            //Заказов 100, из них 1 удален, 1 не с тем банкетом, поэтоу должно быть 98
            Assert.IsTrue(result.Count() == 98);
        }

        [Test]
        public void PostOrdersTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            //Создаем компанию для заказа
            Company company = CompanyFactory.Create(_user);
            //СОздаем банкет
            Banket banket = BanketFactory.Create(_user, company, cafe);
            //СОздаем заказы
            Order order = OrderFactory.Create(_user, banket, cafe, _user);
            order.OrderItems = OrderItemFactory.CreateFew(10, _user, null, order);
            var orderModel = order.GetContract();

            var responce = _controller.PostOrders(orderModel);

            Assert.IsNotNull(responce);
        }

        [Test]
        public void UpdateOrderTest()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            //Создаем компанию для заказа
            Company company = CompanyFactory.Create(_user);
            //Создаем банкет
            Banket banket = BanketFactory.Create(_user, company, cafe);
            banket.OrderStartDate = DateTime.Now.AddDays(-1);
            banket.OrderEndDate = DateTime.Now.AddDays(1);
            //Создаем заказы
            List<Order> orders = OrderFactory.CreateFew(100, _user, banket, cafe, _user);
            for (int i = 0; i < orders.Count; i++)
            {
                orders[i].OrderItems = OrderItemFactory.CreateFew(i, _user, null, orders[i]);
            }
            orders[0].BanketId = banket.Id + 5;
            orders[1].IsDeleted = true;
            OrderModel orderModel = orders[2].GetContract();
            var responce = _controller.UpdateOrder(orderModel);
            //Результат должен быть не Null
            Assert.IsNotNull(responce);
        }

        [Test]
        public void DeleteTest_Manager()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            //Создаем компанию для заказа
            Company company = CompanyFactory.Create(_user);
            //Создаем банкет
            Banket banket = BanketFactory.Create(_user, company, cafe);
            banket.OrderStartDate = DateTime.Now.AddDays(-1);
            banket.OrderEndDate = DateTime.Now.AddDays(1);
            //Создаем заказы
            List<Order> orders = OrderFactory.CreateFew(100, _user, banket, cafe, _user);
            for (int i = 0; i < orders.Count; i++)
            {
                orders[i].OrderItems = OrderItemFactory.CreateFew(i, _user, null, orders[i]);
            }
            orders[0].BanketId = banket.Id + 5;
            orders[1].IsDeleted = true;

            //Пробуем удалить банкет
            var responce = _controller.Delete(banket.Id);
            //Результат должен быть не Null
            Assert.IsNotNull(responce);
        }

        [Test]
        public void DeleteOrderItemTest_Manager()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            //Создаем компанию для заказа
            Company company = CompanyFactory.Create(_user);
            //Создаем банкет
            Banket banket = BanketFactory.Create(_user, company, cafe);
            banket.OrderStartDate = DateTime.Now.AddDays(-1);
            banket.OrderEndDate = DateTime.Now.AddDays(1);
            //Создаем заказ
            Order order = OrderFactory.Create(_user, banket, cafe, _user);
            order.OrderItems = OrderItemFactory.CreateFew(10, _user, null, order);

            //Пробуем удалить итем в заказе
            var responce = _controller.DeleteOrderItem(order.OrderItems[0].Id);
            //Результат должен быть не Null
            Assert.IsNotNull(responce);
            order = _context.Orders.FirstOrDefault(o => o.Id == order.Id);
            //После удаление 1 итема заказ не должен быть null
            Assert.IsNotNull(order);
            //Итемов было 10, но 1 удалили, должно быть 9
            Assert.IsTrue(order.OrderItems.Count(oi => !oi.IsDeleted) == 9);
        }

        [Test]
        public void PutTest_Manager()
        {
            SetUp();
            //Создаем кафе
            Cafe cafe = CafeFactory.Create(_user);
            //Делаем пользователя менеджером кафе
            CafeManagerFactory.Create(_user, cafe);
            //Создаем компанию для заказа
            Company company = CompanyFactory.Create(_user);
            //Создаем банкет
            Banket banket = BanketFactory.Create(_user, company, cafe);
            banket.OrderStartDate = DateTime.Now.AddDays(-1);
            banket.OrderEndDate = DateTime.Now.AddDays(1);

            BanketModel banketModel = banket.GetContract();
            banketModel.OrderStartDate = DateTime.Now.AddDays(-2);
            //Пробуем удалить итем в заказе
            var responce = _controller.Put(banketModel);
            //Результат должен быть не Null
            Assert.IsNotNull(responce);
            banket = _context.Bankets.FirstOrDefault(b => b.Id == banket.Id);
            //После изменений банкет не должен пропасть
            Assert.IsNotNull(banket);
            //Проверим изменилось ли время
            Assert.IsTrue(banket.OrderStartDate.Date == DateTime.Now.AddDays(-2).Date);
        }
    }
}
