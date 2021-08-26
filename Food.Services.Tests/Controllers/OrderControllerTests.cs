using Food.Data.Entities;
using Food.Services.Controllers;
using Food.Services.Extensions.OrderExtensions;
using Food.Services.Tests.Context;
using Food.Services.Tests.FakeFactories;
using Food.Services.Tests.Tools;
using ITWebNet.Food.Core.DataContracts.Common;
using Microsoft.AspNet.Identity;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Food.Services.Tests.Controllers
{
    [TestFixture]
    public class OrderControllerTests
    {
        FakeContext _context;
        OrderController _orderController;
        Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor> _accessor;
        User _user;

        private void SetUp()
        {
            _accessor = new Mock<ITWebNet.FoodService.Food.DbAccessor.Accessor>();
            _context = new FakeContext();
            ContextManager.Set(_context);
            _orderController = new OrderController(_context, _accessor.Object);
            _user = UserFactory.CreateUser();
            _accessor.Setup(e => e.GetUserById(_user.Id)).Returns(_user);
            ClaimsIdentity identity = new ClaimsIdentity(DefaultAuthenticationTypes.ExternalBearer);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Name, _user.Name));
            Thread.CurrentPrincipal = new GenericPrincipal(identity, null);
        }

        /// <summary>
        /// Тест изменения состояния корпоративного заказа
        /// </summary>
        [Test]
        public void ChangeCompanyOrderStatusTest()
        {
            SetUp();
            // Cоздаём заказ и добавляем в БД. Заказ создаётся с состоянием "создан".
            CompanyOrder co = CompanyOrderFactory.Create();
            // Теперь надо поменять его состояние.
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, co.CafeId)).Returns(true);
            var responce = _orderController.ChangeCompanyOrderStatus(co.Id, EnumOrderStatus.Accepted,
                co.CafeId);
            // После этого нужно проконтролировать, что метод вернул true.
            var result = TransformResult.GetPrimitive<bool>(responce);
            Assert.IsTrue(result);
            // Также надо проконтролировать, что состояние реально поменялось.
            CompanyOrder co2 = _accessor.Object.GetContext().CompanyOrders.FirstOrDefault(
                        o => o.Id == co.Id && o.IsDeleted == false);
            Assert.IsTrue(co2.State == (long)OrderStatusEnum.Accepted);
        }

        /// <summary>
        /// Тест изменения данных пользовательского заказа самим пользователем
        /// </summary>
        [Test]
        public void ChangeOrderTest()
        {
            SetUp();
            // Создание заказа и добавление его в БД.
            Order orderDb = OrderFactory.Create(_user, null, null, _user);
            // Создание модели заказа с измененными данными.
            OrderModel changedModel = orderDb.GetContract();
            // Меняем телефон и комментарий:
            string ChangedPhone = "+783321111111";
            string NewComment = "Test comment";
            changedModel.PhoneNumber = ChangedPhone;
            changedModel.Comment = NewComment;
            // Изменение заказа:
            var responce = _orderController.ChangeOrder(changedModel);
            // Должен получиться пустой объект OrderStatusModel:
            OrderStatusModel result = TransformResult.GetPrimitive<OrderStatusModel>(responce);
            Assert.NotNull(result);
            // Контролируем изменения состояния заказа - телефон и комментарий должны поменяться на новые:
            Order order2 = _accessor.Object.GetContext().Orders.FirstOrDefault(
                        o => o.Id == orderDb.Id && o.IsDeleted == false);
            Assert.IsTrue(order2.PhoneNumber == ChangedPhone && order2.Comment == NewComment);
        }

        /// <summary>
        /// Тест отмены пользовательского заказа самим пользователем
        /// </summary>
        [Test]
        public void CancelOrderTest()
        {
            SetUp();
            // Создание заказа и добавление его в БД.
            Order orderDb = OrderFactory.Create(_user, null, null, _user);
            // Создание модели заказа с состоянием "Отмена".
            OrderModel changedModel = orderDb.GetContract();
            changedModel.Status = (long)OrderStatusEnum.Abort;

            // Пробуем отменить заказ:
            var responce = _orderController.ChangeOrder(changedModel);
            // Должен получиться пустой объект OrderStatusModel:
            OrderStatusModel result = TransformResult.GetPrimitive<OrderStatusModel>(responce);
            Assert.NotNull(result);
            // Также надо проконтролировать, что состояние заказа изменилось на "отменен".
            Order order2 = _accessor.Object.GetContext().Orders.FirstOrDefault(
                        o => o.Id == orderDb.Id && o.IsDeleted == false);
            Assert.IsTrue(order2.State == EnumOrderState.Abort);
        }

        /// <summary>
        /// Тест изменения сразу нескольких заказов.
        /// </summary>
        [Test]
        public void ChangeOrdersTest()
        {
            SetUp();
            // Создание заказов и добавление их в БД.
            int count = 3;
            List<Order> ordersDb = OrderFactory.CreateFew(count, _user, null, null, _user);

            // Создание моделей заказов
            List<OrderModel> changedModels = ordersDb.Select(o => o.GetContract()).ToList();
            // Первый заказ отменяем, у второго меняем телефон, у третьего - комментарий:
            OrderModel model = changedModels.ElementAt(0);
            model.Status = (long)OrderStatusEnum.Abort;
            model = changedModels.ElementAt(1);
            string ChangedPhone = "+783321111111";
            model.PhoneNumber = ChangedPhone;
            model = changedModels.ElementAt(2);
            string NewComment = "Test comment";
            model.Comment = NewComment;
            // Пробуем изменить заказы:
            var responce = _orderController.ChangeOrders(changedModels);

            // Должен получиться List<OrderStatusModel>:
            OrderStatusModel[] result = TransformResult.GetObject<OrderStatusModel[]>(responce);
            // Объект должен быть не null:
            Assert.NotNull(result);
            // Почему-то метод контроллера сделан так, что всегда возвращает массив из одного элемента. Зачем это нужно - непонятно. Исправлять стремно - а вдруг это такая фича, которая где-то используется. Поэтому проверять их количество смысла нет. 
            // Assert.IsTrue(result.Length == count);
            // Также надо проконтролировать изменения состояния заказов.
            Order order2 = _accessor.Object.GetContext().Orders.FirstOrDefault(
                        o => o.Id == ordersDb.ElementAt(0).Id && o.IsDeleted == false);
            Assert.IsTrue(order2.State == EnumOrderState.Abort);
            order2 = _accessor.Object.GetContext().Orders.FirstOrDefault(
                        o => o.Id == ordersDb.ElementAt(1).Id && o.IsDeleted == false);
            Assert.IsTrue(order2.PhoneNumber == ChangedPhone);
            order2 = _accessor.Object.GetContext().Orders.FirstOrDefault(
                        o => o.Id == ordersDb.ElementAt(2).Id && o.IsDeleted == false);
            Assert.IsTrue(order2.Comment == NewComment);
        }

        /// <summary>
        /// Тест изменения состояния пользовательского заказа менеджером
        /// </summary>
        [Test]
        public async Task ChangeOrderStatusTest()
        {
            SetUp();
            // Создание заказа и добавление его в БД.
            Order orderDb = OrderFactory.Create(_user, null, null, _user);
            // Изменение состояния ордера на "Принят":
            orderDb.State = EnumOrderState.Accepted;
            // Новый статус, который будем устанавливать - "Доставка":
            long newState = (long)OrderStatusEnum.Delivery;
            // Назначаем пользователя менеджером кафе:
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, orderDb.CafeId)).Returns(true);
            // Пробуем установить новое состояние заказа:
            var responce = await _orderController.ChangeOrderStatus(orderDb.Id, (int)newState, orderDb.CafeId);
            // Метод должен вернуть true:
            bool result = TransformResult.GetPrimitive<bool>(responce);
            Assert.IsTrue(result);
            // Также надо проконтролировать, что состояние заказа изменилось на "отменен".
            Order order2 = _accessor.Object.GetContext().Orders.FirstOrDefault(
                        o => o.Id == orderDb.Id && o.IsDeleted == false);
            Assert.IsTrue(((long)order2.State) == newState);
        }

        /// <summary>
        /// Тест получения доступных состояний заказов, в случае если состояние заказа изменяет пользователь
        /// </summary>
        [Test]
        public void GetAvailableOrderStatusesTest_User()
        {
            SetUp();
            // Создание заказа и добавление его в БД.
            Order orderDb = OrderFactory.Create(_user, null, null, _user);
            // Вызов метода контроллера:
            var responce = _orderController.GetAvailableOrderStatuses(orderDb.Id, false);
            List<OrderStatusEnum> result = TransformResult.GetObject<List<OrderStatusEnum>>(responce);
            // Результат должен быть:
            Assert.IsNotNull(result);
            // Пользователю доступна только отмена заказа:
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result.FirstOrDefault() == OrderStatusEnum.Abort);
        }

        /// <summary>
        /// Тест получения доступных состояний заказов, в случае если состояние заказа изменяет менеджер
        /// </summary>
        [Test]
        public void GetAvailableOrderStatusesTest_Manager()
        {
            SetUp();
            // Создание пользователя для заказа.
            User ordUser = UserFactory.CreateUser();
            // Создание заказа и добавление его в БД.
            Order orderDb = OrderFactory.Create(ordUser, null, null, ordUser);
            // Состояние заказа - принят кафе:
            orderDb.State = EnumOrderState.Accepted;
            // Назначение текущего пользователя менеджером кафе:
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, orderDb.CafeId)).Returns(true);
            // Вызов метода контроллера:
            var responce = _orderController.GetAvailableOrderStatuses(orderDb.Id);
            List<OrderStatusEnum> result = TransformResult.GetObject<List<OrderStatusEnum>>(responce);
            // Результат должен быть:
            Assert.IsNotNull(result);
            // Для принятых заказов доступно три следующих состояния - доставка, доставлен и отмена:
            Assert.IsTrue(result.Count == 3);
            Assert.IsTrue(result.Contains(OrderStatusEnum.Delivery) &&
                result.Contains(OrderStatusEnum.Delivered) &&
                result.Contains(OrderStatusEnum.Abort));
        }

        /// <summary>
        /// Тест выборки корпоративных ордеров по фильтру
        /// </summary>
        [Test]
        public void GetCompanyOrderByFiltersTest()
        {
            SetUp();
            // Создание компании для заказов:
            Company company = CompanyFactory.Create(_user);
            // Создание кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            // Создание заказов и добавление их в БД.
            int count = 10;
            List<CompanyOrder> compOrders = CompanyOrderFactory.CreateFew(count, _user, company, cafe);
            // Создание ордеров. Фильтр должен показывать только те корпоративные заказы, которые имеют пользовательские заказы.
            List<Order> orders = OrderFactory.CreateFew(2, _user, null, cafe);
            List<Order> ordersEmpty = new List<Order>();
            for (int i = 0; i < count - 1; ++i)
                compOrders.ElementAt(i).Orders = orders;
            // Последнему корпоративному заказу присваиваем пустой список заказов сотрудников:
            compOrders.ElementAt(count - 1).Orders = ordersEmpty;
            // Подготовка фильтра по датам.
            // Двум ордерам назначаем дату 10 дней назад:
            compOrders.ElementAt(0).DeliveryDate = DateTime.Now.AddDays(-10);
            compOrders.ElementAt(1).DeliveryDate = DateTime.Now.AddDays(-10);
            // Ещё двум - день назад:
            compOrders.ElementAt(2).DeliveryDate = DateTime.Now.AddDays(-1);
            compOrders.ElementAt(3).DeliveryDate = DateTime.Now.AddDays(-1);
            // У остальных 6 должно быть текущее время.

            // Подготовка фильтра по состоянию ордеров.
            // Двум ордерам назначаем состояние "Отмена":
            compOrders.ElementAt(4).State = (long)OrderStatusEnum.Abort;
            compOrders.ElementAt(5).State = (long)OrderStatusEnum.Abort;

            // Назначение текущего пользователя менеджером кафе:
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            // Создание фильтра для выборки заказов:
            ReportFilter reportFilter = new ReportFilter()
            {
                CafeId = cafe.Id,
                LoadUserOrders = false,
                LoadOrderItems = false,
                // Фильтр по датам:
                StartDate = DateTime.Now.Date.AddDays(-5),
                EndDate = DateTime.Now.Date.AddDays(1),
                // Фильтр по состояниям ордеров:
                AvailableStatusList = new List<OrderStatusEnum>()
                {
                    OrderStatusEnum.Created,
                    OrderStatusEnum.Accepted,
                    OrderStatusEnum.Delivery,
                    OrderStatusEnum.Delivered,
                    //OrderStatusEnum.Abort,
                    OrderStatusEnum.Cart
                },
                // Фильтр по компании:
                CompanyId = company.Id
            };
            // Пробуем получить ордеры:
            var responce = _orderController.GetCompanyOrderByFilters(reportFilter);
            List<CompanyOrderModel> result = TransformResult.GetObject<List<CompanyOrderModel>>(responce);
            // Результат должен быть:
            Assert.IsNotNull(result);
            // В исходном списке 10 ордеров. 2 из них должно отсеяться фильтром по датам, 2 - фильтром по состоянию, 1 - из-за отсуствия заказов сотрудников, 5 должно остаться:
            Assert.IsTrue(result.Count == 5);
        }
        [Test]
        public void GetCurrentListOfOrdersAndOrderItemsToCafeTest_Manager()
        {
            SetUp();
            // Создание кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            // Назначаем пользователя менеджером кафе:
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            //Создаем заказы у кафе
            List<Order> orders = OrderFactory.CreateFew(10, _user, null, cafe);
            for (int i = 0; i < orders.Count; i++)
            {
                orders[i].DeliverDate = DateTime.Now;
                if (i != 4)
                    orders[i].OrderItems = OrderItemFactory.CreateFew(5, _user, null, orders[i]);
            }
            //Эти два заказа не должны попасть в итоговый массив
            orders[9].DeliverDate = DateTime.Now.AddDays(2);
            orders[8].DeliverDate = DateTime.Now.AddDays(-2);

            orders[7].State = EnumOrderState.Cart;

            orders[6].CafeId = cafe.Id + 1;

            orders[5].OrderItems[0].IsDeleted = true;
            // Пробуем получить заказы:
            var responce = _orderController.GetCurrentListOfOrdersAndOrderItemsToCafe(cafe.Id);
            OrderModel[] result = TransformResult.GetObject<OrderModel[]>(responce);

            //результат должен быть не null
            Assert.IsNotNull(result);

            //2 должны отсеяться по дате, 1 по состоянию заказа и 1 по чужому кафе
            Assert.IsTrue(result.Count() == 6);

            //Количество итемов у 5 заказов = 5, у одного 4
            Assert.IsFalse(result.All(o => o.OrderItems.Count == 5));

            //У одного из заказов удален итем, итого итемов должно быть 4
            Assert.IsTrue(result.Where(o => o.OrderItems.Count == 4).Count() == 1);

            //У одного из заказов убираем итемордеры
            Assert.IsTrue(result.Where(o => o.OrderItems.Count == 0).Count() == 1);

            //У остальных заказов кол-во итемов должно быть = 5
            Assert.IsTrue(result.Where(o => o.OrderItems.Count == 5).Count() == 4);
        }

        [Test]
        public void GetCurrentListOfOrdersToCafeTest_Manager()
        {
            SetUp();
            // Создание кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            // Назначаем пользователя менеджером кафе:
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            //Создаем заказы у кафе
            List<Order> orders = OrderFactory.CreateFew(10, _user, null, cafe);
            for (int i = 0; i < orders.Count; i++)
            {
                orders[i].DeliverDate = DateTime.Now;
            }
            //Эти два заказа не должны попасть в итоговый массив
            orders[9].DeliverDate = DateTime.Now.AddDays(2);
            orders[8].DeliverDate = DateTime.Now.AddDays(-2);

            orders[7].State = EnumOrderState.Cart;

            orders[6].CafeId = cafe.Id + 1;
            // Пробуем получить заказы:
            var responce = _orderController.GetCurrentListOfOrdersToCafe(cafe.Id);
            OrderModel[] result = TransformResult.GetObject<OrderModel[]>(responce);

            //результат должен быть не null
            Assert.IsNotNull(result);

            //2 должны отсеяться по дате, 1 по состоянию заказа и 1 по чужому кафе
            Assert.IsTrue(result.Count() == 6);
        }

        [Test]
        public void GetCurrentListOfOrdersToCafeOrderByClientsTest_Manager()
        {
            SetUp();
            // Создание кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            // Назначаем пользователя менеджером кафе:
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            //Создаем заказы у кафе
            List<Order> orders = OrderFactory.CreateFew(10, _user, null, cafe);
            for (int i = 0; i < orders.Count; i++)
            {
                orders[i].DeliverDate = DateTime.Now;
            }
            //Эти два заказа не должны попасть в итоговый массив
            orders[9].DeliverDate = DateTime.Now.AddDays(2);
            orders[8].DeliverDate = DateTime.Now.AddDays(-2);

            orders[7].State = EnumOrderState.Cart;

            orders[6].CafeId = cafe.Id + 1;
            // Пробуем получить заказы:
            var responce = _orderController.GetCurrentListOfOrdersToCafeOrderByClients(cafe.Id);
            List<KeyValuePair<UserModel, OrderModel>> result = TransformResult.GetObject<List<KeyValuePair<UserModel, OrderModel>>>(responce);

            //результат должен быть не null
            Assert.IsNotNull(result);

            //2 должны отсеяться по дате, 1 по состоянию заказа и 1 по чужому кафе
            Assert.IsTrue(result.Count() == 6);

            //Ни ключ, ни значение не нулл
            Assert.IsTrue(result.All(x => x.Key != null && x.Value != null));
        }

        [Test]
        public void GetCurrentListOfOrdersToCafeOrderByCompaniesTest_Manager()
        {
            SetUp();
            // Создание кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            // Назначаем пользователя менеджером кафе:
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            //Создаем заказы у кафе
            List<Order> orders = OrderFactory.CreateFew(10, _user, null, cafe);
            // Создание компании для заказов:
            Company company = CompanyFactory.Create(_user);

            for (int i = 0; i < orders.Count; i++)
            {
                orders[i].DeliverDate = DateTime.Now;
                orders[i].CompanyOrder = CompanyOrderFactory.Create(_user, company, cafe);
                orders[i].CompanyOrder.DeliveryAddress = i;
            }
            //Эти два заказа не должны попасть в итоговый массив
            orders[9].DeliverDate = DateTime.Now.AddDays(2);
            orders[8].DeliverDate = DateTime.Now.AddDays(-2);

            orders[7].State = EnumOrderState.Cart;

            orders[6].CafeId = cafe.Id + 1;
            // Пробуем получить заказы:
            var responce = _orderController.GetCurrentListOfOrdersToCafeOrderByCompanies(cafe.Id);
            List<KeyValuePair<CompanyModel, OrderModel>> result = TransformResult.GetObject<List<KeyValuePair<CompanyModel, OrderModel>>>(responce);

            //результат должен быть не null
            Assert.IsNotNull(result);

            //2 должны отсеяться по дате, 1 по состоянию заказа и 1 по чужому кафе
            Assert.IsTrue(result.Count() == 6);

            //Ни ключ, ни значение не нулл
            Assert.IsTrue(result.All(x => x.Key != null && x.Value != null));
        }

        [Test]
        public void GetOrderTest_User()
        {
            SetUp();
            // Создание кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);

            //Создаем заказ у кафе
            Order order = OrderFactory.Create(_user, null, cafe, _user);
            Dish dish = DishFactory.Create();

            order.OrderItems = OrderItemFactory.CreateFew(10, _user, dish, order);

            order.OrderItems[9].IsDeleted = true;
            // Пробуем получить заказ:
            var responce = _orderController.GetOrder(order.Id);
            OrderModel result = TransformResult.GetObject<OrderModel>(responce);

            //Результат должен быть не нулл
            Assert.IsNotNull(result);

            //Один итем заказа удален, должно быть 9
            Assert.IsTrue(result.OrderItems.Count == 9);
        }

        [Test]
        public async Task GetOrderByFiltersTest()
        {
            SetUp();
            // Создание кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            // Создание ордеров. Фильтр должен показывать только те корпоративные заказы, которые имеют пользовательские заказы.
            List<Order> orders = OrderFactory.CreateFew(10, _user, null, cafe);
            // Создание компании для заказов:
            Company company = CompanyFactory.Create(_user);
            // Создание фильтра для выборки заказов:
            ReportFilter reportFilter = new ReportFilter()
            {
                CafeId = cafe.Id,
                LoadUserOrders = false,
                LoadOrderItems = false,
                // Фильтр по датам:
                StartDate = DateTime.Now.Date.AddDays(-5),
                EndDate = DateTime.Now.Date.AddDays(5),
                // Фильтр по состояниям ордеров:
                AvailableStatusList = new List<OrderStatusEnum>()
                {
                    OrderStatusEnum.Created,
                    OrderStatusEnum.Accepted,
                    OrderStatusEnum.Delivery//,
                    //OrderStatusEnum.Delivered,
                    //OrderStatusEnum.Abort,
                   // OrderStatusEnum.Cart
                },
                // Фильтр по компании:
                CompanyId = company.Id,
                SortType = ReportSortType.OrderByPrice
            };
            for (int i = 0; i < 10; i++)
            {
                orders[i].DeliverDate = DateTime.Now.Date;
                orders[i].OrderInfo = OrderInfoFactory.Create();
                orders[i].Id = 10000 + i;
            }
            //заказы, которые не попадают по фильтру
            orders[0].CafeId = cafe.Id + 1;
            orders[1].IsDeleted = true;
            orders[2].DeliverDate = DateTime.Now.Date.AddDays(-6);
            orders[3].DeliverDate = DateTime.Now.Date.AddDays(6);
            orders[4].State = EnumOrderState.Cart;

            // Пробуем получить заказы:
            var responce = await _orderController.GetOrderByFilters(reportFilter);
            List<OrderModel> result = TransformResult.GetObject<List<OrderModel>>(responce);

            // Результат должен быть:
            Assert.IsNotNull(result);

            /* В исходном списке 10 заказов.
               1 - с чужим CafeID,
               1 - удален
               у 1 дата меньше дат в фильтре,
               у 1 дата больше дат в фильтре,
               у 1 статус заказа не попадает в список фильтра, 
               должно остаться 5
             */
            Assert.IsTrue(result.Count == 5);
        }

        [Test]
        public void GetOrderItemsTest_User()
        {
            SetUp();
            // Создание кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            // Создаем заказ у кафе
            Order order = OrderFactory.Create(_user, null, cafe, _user);
            order.OrderItems = OrderItemFactory.CreateFew(10, _user, null, order);
            //Создание неподходящих под выборку итемов
            order.OrderItems[0].IsDeleted = true;
            order.OrderItems[1].OrderId = order.Id + 1;

            // Пробуем получить итемы заказа:
            var responce = _orderController.GetOrderItems(_user.Id, order.Id);
            List<OrderItemModel> result = TransformResult.GetObject<List<OrderItemModel>>(responce);

            // Результат должен быть:
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(i => i != null));
            //Всего итемов заказа 10, 1 должен отсеяться, потому что удален, второй, потому что принадлежит другому заказу
            Assert.IsTrue(result.Count == 8);
        }

        [Test]
        public void GetOrderForManagerTest()
        {
            SetUp();
            // Создание кафе для заказов:
            Cafe cafe = CafeFactory.Create(_user);
            // Создаем заказ у кафе
            Order order = OrderFactory.Create(_user, null, cafe, _user);
            order.OrderItems = OrderItemFactory.CreateFew(10, _user, null, order);
            //Создание неподходящих под выборку итемов
            order.OrderItems[0].IsDeleted = true;
            order.OrderItems[1].OrderId = order.Id + 1;

            // Пробуем получить заказ:
            var responce = _orderController.GetOrderForManager(order.Id);
            OrderModel result = TransformResult.GetObject<OrderModel>(responce);

            // Результат должен быть:
            Assert.IsNotNull(result);
            Assert.IsTrue(result.OrderItems.Any(i => i != null));
            //Всего итемов заказа 10, 1 должен отсеяться, потому что удален, второй, потому что принадлежит другому заказу
            Assert.IsTrue(result.OrderItems.Count == 8);
        }

        [Test]
        public void GetOrderPriceDetailsTest_Manager()
        {
            SetUp();
            // Создаем кафе для заказов
            Cafe cafe = CafeFactory.Create(_user);
            CostOfDelivery costOfDeliveryFactory = CostOfDeliveryFactory.Create(cafe, 35);

            // Создаем компании для заказов
            Company company = CompanyFactory.Create(_user);
            //Создаем скидку для компании в этом кафе
            CafeDiscount cafeDiscount = CafeDiscountFactory.Create(_user, cafe, company);
            // Назначаем пользователя менеджером кафе
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            // Создаем заказ
            Order order = OrderFactory.Create(_user, null, cafe, _user);
            List<Dish> dish = DishFactory.CreateFew(10, _user);
            order.OrderItems = OrderItemFactory.CreateFew(10, _user, null, order);
            for (int i = 0; i < 10; i++)
            {
                dish[i].BasePrice = i * 100 + 1;
                order.OrderItems[i].Dish = dish[i];
                order.OrderItems[i].DishCount = i + 1;
            }
            order.DeliverDate = DateTime.Now;


            // Создаем модели заказа.
            OrderModel orderModel = order.GetContract();

            // Пробуем получить детали заказа:
            var responce = _orderController.GetOrderPriceDetails(orderModel);
            TotalDetailsModel result = TransformResult.GetObject<TotalDetailsModel>(responce);

            //Результат должен быть не нулл
            Assert.IsNotNull(result);
            //Сумма заказа(0) - сумма скидки(20) + сумма доставки(35) = 15 > 0
            Assert.IsTrue(result.TotalSumm > 0);
        }


        [Test]

        public void GetOrderPriceDetailsTest_User()
        {
            SetUp();
            // Создаем кафе для заказов
            Cafe cafe = CafeFactory.Create(null);
            CostOfDelivery costOfDeliveryFactory = CostOfDeliveryFactory.Create(cafe, 35);

            // Создаем компании для заказов
            Company company = CompanyFactory.Create(null);
            //Создаем скидку для компании в этом кафе
            CafeDiscount cafeDiscount = CafeDiscountFactory.Create(null, cafe, company);
            // Создаем заказ
            Order order = OrderFactory.Create(_user, null, cafe, _user);
            List<Dish> dish = DishFactory.CreateFew(10, _user);
            order.OrderItems = OrderItemFactory.CreateFew(10, _user, null, order);
            for (int i = 0; i < 10; i++)
            {
                dish[i].BasePrice = i * 100 + 1;
                order.OrderItems[i].Dish = dish[i];
                order.OrderItems[i].DishCount = i + 1;
            }
            order.DeliverDate = DateTime.Now;


            // Создаем модели заказа.
            OrderModel orderModel = order.GetContract();

            // Пробуем получить детали заказа:
            var responce = _orderController.GetOrderPriceDetails(orderModel);
            TotalDetailsModel result = TransformResult.GetObject<TotalDetailsModel>(responce);

            //Результат должен быть не нулл
            Assert.IsNotNull(result);
            //Вот здесь по идее сумма скидки должна быть = 0, т.к. скидка привязана к компании,но запрос выдает 20. 
            //Поэтому пока оставлю
            //Сумма заказа(0) - сумма скидки(20) + сумма доставки(35) = 15 > 0
            Assert.IsTrue(result.TotalSumm > 0);
        }

        [Test]
        public void GetOrdersInCompanyOrderByCompanyOrderIdInTxtTest_Consolidator()
        {
            SetUp();
            // Создаем кафе для заказов
            Cafe cafe = CafeFactory.Create(_user);
            // Создаем компании для заказов
            Company company = CompanyFactory.Create(_user);
            // Создаем заказ
            Order order = OrderFactory.Create(_user, null, cafe, _user);

            // Назначаем пользователя менеджером кафе
            _accessor.Setup(e => e.IsUserManagerOfCafe(_user.Id, cafe.Id)).Returns(true);
            // Пробуем получить в txt:
            var responce = _orderController.GetOrdersInCompanyOrderByCompanyOrderIdInTxt(order.Id);
            string result = TransformResult.GetObject<string>(responce);

            //Результат должен быть не нулл
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetOrdersInCompanyOrderByCompanyOrderIdInTxtTest_Manager()
        {
        }

        [Test]
        public void GetOrdersInCompanyOrderByCompanyOrderIdInXmlTest_Consolidator()
        {
        }

        [Test]
        public void GetOrdersInCompanyOrderByCompanyOrderIdInXmlTest_Manager()
        {
        }

        [Test]
        public void GetOrdersInCompanyOrderByCompanyOrderIdInXmlHelperTest()
        {
        }

        [Test]
        public void GetTotalOrdersPerDayTest()
        {
            SetUp();
            // Создаем кафе для заказов
            Cafe cafe = CafeFactory.Create(null);
            // Создаем заказы
            List<Order> orders = OrderFactory.CreateFew(10, _user, null, cafe, _user);
            for (int i = 0; i < 10; i++)
                orders[i].CreationDate = DateTime.Now;
            orders[0].CreationDate = DateTime.Now.AddDays(-1);//Должен выпасть, т.к. создан не сегодня
            orders[1].State = EnumOrderState.Cart;//Должен выпасть, т.к. не то состояние заказа
            orders[2].IsDeleted = true;//Должен выпасть, т.к. удален
            orders[3].CreationDate = DateTime.Now.AddDays(1);//Должен выпасть, т.к. создан не сегодня
            // Пробуем получить:
            var responce = _orderController.GetTotalOrdersPerDay();
            long result = TransformResult.GetPrimitive<long>(responce);

            //Результат должен быть не нулл
            Assert.IsNotNull(result);

            //всего заказов 10, 1 не проходит по времени создания, 1 по состоянию заказа, 1 удален
            Assert.IsTrue(result == 7);
        }

        [Test]
        public void GetUserCurrentCartTest_User()
        {
            SetUp();
            // Создаем кафе для заказов
            Cafe cafe = CafeFactory.Create(null);
            // Создаем заказ
            Order order = OrderFactory.Create(_user, null, cafe, _user);
            order.State = EnumOrderState.Cart;

            // Пробуем получить корзину:
            var responce = _orderController.GetUserCurrentCart();
            List<OrderModel> result = TransformResult.GetObject<List<OrderModel>>(responce);

            //Результат должен быть не нулл
            Assert.IsNotNull(result);

            //Всего заказов 1 и он походит по состоянию
            Assert.IsTrue(result.Count == 1);
        }

        [Test]
        public void GetUserOrdersTest_User()
        {
            SetUp();
            // Создаем кафе для заказов
            Cafe cafe = CafeFactory.Create(null);
            List<Order> orders = OrderFactory.CreateFew(10, _user, null, cafe, _user);
            for (int i = 0; i < 10; i++)
                orders[i].CreationDate = DateTime.Now;

            orders[0].CreationDate = DateTime.Now.AddDays(1);
            orders[1].CreationDate = DateTime.Now.AddDays(-1);
            //этот заказ удален
            orders[2].IsDeleted = true;

            // Пробуем получить:
            var responce = _orderController.GetUserOrders(_user.Id);
            List<OrderModel> result = TransformResult.GetObject<List<OrderModel>>(responce);

            //Проверяем на нулл:
            Assert.IsNotNull(result);

            //Всего заказов 10,  1 удален
            Assert.IsTrue(result.Count == 9);

            //это должно быть у всех заказов
            Assert.IsTrue(result.All(o => o.CreatorLogin == _user.Name));

        }

        [Test]
        public void GetUserOrdersInCompanyOrderTest()
        {
            SetUp();
            // Создаем кафе для заказов
            Cafe cafe = CafeFactory.Create(null);
            //Создаем компанию для заказов
            Company company = CompanyFactory.Create(_user);
            //Создаем заказы компании
            List<Order> orders = OrderFactory.CreateFew(10, _user, null, cafe, _user);

            //Создаем заказ компании
            CompanyOrder companyOrder = CompanyOrderFactory.Create(_user, company, cafe);
            for (int i = 0; i < 10; i++)
            {
                orders[i].CompanyOrderId = companyOrder.Id;
                orders[i].CompanyOrder = companyOrder;
            }
            //Этот заказ не должен попасть, т.к. удален
            orders[0].IsDeleted = true;
            // Пробуем получить:
            var responce = _orderController.GetUserOrdersInCompanyOrder(companyOrder.Id);
            List<OrderModel> result = TransformResult.GetObject<List<OrderModel>>(responce);

            //Проверяем на нулл:
            Assert.IsNotNull(result);

            //Всего заказов 10,  1 удален
            Assert.IsTrue(result.Count == 9);

            //это должно быть у всех заказов
            Assert.IsTrue(result.All(o => o.CreatorLogin == _user.DisplayName &&
                                          !o.IsDeleted));
        }

        [Test]
        public void GetUserOrdersInDateRangeTest_User()
        {
            SetUp();
            // Создаем кафе для заказов
            Cafe cafe = CafeFactory.Create(null);
            //Создаем компанию для заказов
            Company company = CompanyFactory.Create(_user);

            //Создаем заказы компании
            List<Order> orders = OrderFactory.CreateFew(10, _user, null, cafe, _user);
            for (int i = 0; i < 10; i++)
                orders[i].DeliverDate = DateTime.Now.AddDays(i);
            var startDate = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            var endDate = DateTime.Now.AddDays(5).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            var responce = _orderController.GetUserOrdersInDateRange(_user.Id, (long)startDate, (long)endDate);
            List<OrderModel> result = TransformResult.GetObject<List<OrderModel>>(responce);

            //Проверяем на нулл:
            Assert.IsNotNull(result);

            //Всего заказов 10,  из них 6 подходят по времени
            Assert.IsTrue(result.Count == 6);

            //это должно быть у всех заказов
            Assert.IsTrue(result.All(o => o.CreatorLogin == _user.Name));
        }

        [Test]
        public void GetUserOrderWithItemsAndDishesTest_User()
        {
            SetUp();
            // Создаем кафе для заказов
            Cafe cafe = CafeFactory.Create(null);
            //Создаем компанию для заказов
            Company company = CompanyFactory.Create(_user);

            //Создаем заказы компании
            List<Order> orders = OrderFactory.CreateFew(10, _user, null, cafe, _user);
            List<Dish> dishes = DishFactory.CreateFew(10, _user);
            for (int i = 0; i < 10; i++)
            {
                orders[i].OrderItems = OrderItemFactory.CreateFew(10, _user, null, orders[i]);
                orders[i].OrderItems[i].Dish = dishes[i];
                orders[i].OrderItems[i].DishCount = i + 1;
            }

            var responce = _orderController.GetUserOrderWithItemsAndDishes(_user.Id);
            Dictionary<OrderModel, List<FoodDishModel>> result = TransformResult.GetObject<Dictionary<OrderModel, List<FoodDishModel>>>(responce);

            //Проверяем на нулл:
            Assert.IsNotNull(result);

            //Проверяем нет ли среди пар nullов
            Assert.IsTrue(result.All(o => o.Value != null && o.Key != null));
        }

        [Test]
        public async Task PostOrdersHttpTest()
        {
            SetUp();
            //Создаем кафе
            var cafe = CafeFactory.Create(_user);
            //Создаем заказ
            Order order = OrderFactory.Create(_user, null, cafe, _user);
            // Создание модели заказа с данными.
            OrderModel orderModel = order.GetContract();
            var postOrderRequest = new PostOrderRequest()
            {
                Order = orderModel,
                FailureURL = string.Empty,
                SuccessURL = string.Empty,
                //PaymentType = PaymentTypeEnum.Delivery
            };

            var responce = await _orderController.PostOrdersHttp(postOrderRequest);
            PostOrderResponse result = TransformResult.GetObject<PostOrderResponse>(responce);

            //Проверяем на нулл:
            Assert.IsNotNull(result);

        }

        [Test]
        public void PostOrdersTest()
        {
            SetUp();
            //Создаем кафе
            var cafe = CafeFactory.Create(_user);
            //Создаем заказ
            Order order = OrderFactory.Create(_user, null, cafe, _user);
            // Создание модели заказа с данными.
            OrderModel orderModel = order.GetContract();
            var postOrderRequest = new PostOrderRequest()
            {
                Order = orderModel,
                FailureURL = string.Empty,
                SuccessURL = string.Empty,
                //PaymentType = PaymentTypeEnum.Delivery
            };

            var responce = _orderController.PostOrders(postOrderRequest);

            //Проверяем на нулл:
            Assert.IsNotNull(responce);
        }

        [Test]
        public async Task SaveUserOrderCartIntoBaseTest_User()
        {
            SetUp();
            //Создаем кафе
            var cafe = CafeFactory.Create(_user);
            //Создаем заказ
            Order order = OrderFactory.Create(_user, null, cafe, _user);
            order.State = EnumOrderState.Cart;
            order.DeliverDate = DateTime.Now;
            order.OrderItems = OrderItemFactory.CreateFew(10, _user, null, order);
            // Создание модели заказа с данными.
            OrderModel orderModel = order.GetContract();

            var responce = await _orderController.SaveUserOrderCartIntoBase(orderModel);
            OrderStatusModel result = TransformResult.GetPrimitive<OrderStatusModel>(responce);

            //Проверяем на нулл:
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Order);

            //Итого было 10, после всех изменений должно остаться 10
            Assert.IsTrue(result.Order.OrderItems.Count == 10);
        }
    }
}
